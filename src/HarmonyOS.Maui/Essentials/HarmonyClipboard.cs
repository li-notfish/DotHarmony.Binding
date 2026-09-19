// IClipboard 鸿蒙实现：@ohos.pasteboard 系统剪贴板（ApiDemo/2.10 已实测该模块的 Promise/同步混合通道）。
// HasText/写入走同步 API（JS 线程内联，无死锁窗口）；GetTextAsync 走 promise 异步与 MAUI 语义对齐。
// READ_PASTEBOARD（API 26 起 user_grant）状态机：
//   - 每次读都经 getSelfPermissionStatus 重查授权状态（同步、廉价）——用户在系统设置里改权限后
//     下一次读即生效；
//   - 未授权且本会话尚未弹过窗时，经宿主导出的 globalThis.abilityContext 发
//     requestPermissionsFromUser 弹窗，授权后重试一次读；
//   - 被拒时系统返回空 PasteData 壳而非抛错（读请求不落到剪贴板服务），故空结果触发预检；
//     旧版本系统以异常形态拒绝，异常路径同样接弹窗（见 GetTextAsync catch）。
// ClipboardContentChanged：SystemPasteboard.Update（on('update')）远端变更事件转发；
// 同时使 HasText 的回退缓存失效——否则其他 app 改写剪贴板后 HasText 仍按旧缓存回答。
#nullable enable
using Microsoft.Maui.ApplicationModel.DataTransfer;
using HarmonyOS.Interop;
using HPasteboard = HarmonyOS.Bindings.Api.Pasteboard;
using HPasteData = HarmonyOS.Bindings.Api.PasteData;
using HSystemPasteboard = HarmonyOS.Bindings.Api.SystemPasteboard;
using HAccessCtrl = HarmonyOS.Bindings.Api.AbilityAccessCtrl;
using HPermissionStatus = HarmonyOS.ArkUI.PermissionStatus;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyClipboard : IClipboard
{
    static readonly string[] ReadPermissions = ["ohos.permission.READ_PASTEBOARD"];

    readonly HSystemPasteboard _pasteboard;
    bool _dialogShown;      // 授权弹窗每次会话至多一次；状态重查不受此限制
    string? _lastKnownText; // 最近写入/读取的文本；远端 update 事件使其失效

    public HarmonyClipboard()
    {
        _pasteboard = HPasteboard.GetSystemPasteboard();
        _pasteboard.Update += OnPasteboardUpdated;
    }

    private void OnPasteboardUpdated()
    {
        // 远端（其他 app/设备）改写：本地缓存不再可信
        _lastKnownText = null;
        ClipboardContentChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasText
    {
        get
        {
            try
            {
                if (_pasteboard.HasDataSync())
                {
                    var data = _pasteboard.GetDataSync();
                    if (data.GetRecordCount() > 0)
                    {
                        var text = data.GetPrimaryText();
                        if (!string.IsNullOrEmpty(text))
                        {
                            _lastKnownText = text;
                            return true;
                        }
                    }
                }
                // 同步读被权限拒（空壳）或剪贴板确为空：回退到最近已知状态
                return _lastKnownText is not null;
            }
            catch (Exception ex) when (ex is ArkTSException or NapiException)
            {
                // 同步 getter 无法发起授权弹窗；未授权时按 MAUI 语义回退到最近已知状态
                HiLog.Warn("Essentials", $"clipboard HasText denied: {ex.Message}");
                return _lastKnownText is not null;
            }
        }
    }

    public Task SetTextAsync(string? text)
    {
        var data = HPasteboard.CreatePlainTextData(text ?? string.Empty);
        _pasteboard.SetDataSync(data);
        _lastKnownText = text ?? string.Empty;
        return Task.CompletedTask;
    }

    public async Task<string?> GetTextAsync()
    {
        HPasteData data;
        try
        {
            data = await ReadDataAsync().ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is ArkTSException or NapiException)
        {
            // 旧版本系统权限拒绝以异常形态出现：弹窗授权后重试一次；用户拒绝则重试仍抛，透传调用方
            await RequestReadPermissionAsync().ConfigureAwait(true);
            data = await _pasteboard.GetDataAsync().ConfigureAwait(true);
        }
        try
        {
            if (data.GetRecordCount() > 0)
            {
                var text = data.GetPrimaryText();
                _lastKnownText = text;
                return text;
            }
            return null;
        }
        catch (NapiException)
        {
            // getPrimaryText 对无文本记录返回 undefined
            return null;
        }
    }

    async Task<HPasteData> ReadDataAsync()
    {
        var data = await _pasteboard.GetDataAsync().ConfigureAwait(true);
        if (data.GetRecordCount() == 0 && await EnsureReadPermissionAsync().ConfigureAwait(true))
            data = await _pasteboard.GetDataAsync().ConfigureAwait(true);
        return data;
    }

    /// <summary>
    /// true = 弹窗授权已发出且应重试读取；false = 已授权（空结果属剪贴板为空的正常语义）、
    /// 本会话已弹过窗（不再打扰，用户改系统设置后下次读经状态重查自然生效）或无法申请。
    /// </summary>
    async Task<bool> EnsureReadPermissionAsync()
    {
        var context = NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8);
        if (context == IntPtr.Zero)
        {
            HiLog.Warn("Essentials",
                "host did not export globalThis.abilityContext; clipboard read stays denied");
            return false;
        }
        var atManager = HAccessCtrl.CreateAtManager();
        HPermissionStatus status;
        try
        {
            status = atManager.GetSelfPermissionStatus(ReadPermissions[0]);
        }
        catch (NapiException)
        {
            // getSelfPermissionStatus 不可用（低版本系统）：无法预判，依赖异常触发路径
            return false;
        }
        if (status == HPermissionStatus.Granted)
            return false;
        if (_dialogShown)
            return false;
        _dialogShown = true;
        await atManager.RequestPermissionsFromUserAsync(context, ReadPermissions).ConfigureAwait(true);
        HiLog.Info("Essentials", "READ_PASTEBOARD permission requested from user");
        return true;
    }

    async Task RequestReadPermissionAsync()
    {
        var context = NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8);
        if (context == IntPtr.Zero)
        {
            HiLog.Warn("Essentials",
                "host did not export globalThis.abilityContext; clipboard read stays denied");
            return;
        }
        if (!_dialogShown)
        {
            _dialogShown = true;
            var atManager = HAccessCtrl.CreateAtManager();
            await atManager.RequestPermissionsFromUserAsync(context, ReadPermissions).ConfigureAwait(true);
            HiLog.Info("Essentials", "READ_PASTEBOARD permission requested from user");
        }
    }

    public event EventHandler<EventArgs>? ClipboardContentChanged;
}
