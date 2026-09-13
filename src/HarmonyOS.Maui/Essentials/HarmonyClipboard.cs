// IClipboard 鸿蒙实现：@ohos.pasteboard 系统剪贴板（ApiDemo/2.10 已实测该模块的 Promise/同步混合通道）。
// HasText/写入走同步 API（JS 线程内联，无死锁窗口）；GetTextAsync 走 promise 异步与 MAUI 语义对齐。
// API 26 起 READ_PASTEBOARD 为 user_grant，且被拒时系统不抛错而是返回空 PasteData 壳
// （recordCount=0，读请求不落到剪贴板服务）：读前经 getSelfPermissionStatus 主动查状态，
// 未授权则经 host 导出的 globalThis.abilityContext 发起 requestPermissionsFromUser 弹窗后重试。
// ClipboardContentChanged 暂不触发：底层 onRemoteUpdate 通道存在，但生成器未为 SystemPasteboard
// 发射 .NET 事件访问器，手动挂回调留待下一批（订阅不抛错，只是永远不触发）。
#nullable enable
using Microsoft.Maui.ApplicationModel.DataTransfer;
using HarmonyOS.Bindings.Runtime;
using HPasteboard = HarmonyOS.Bindings.Api.Pasteboard;
using HPasteData = HarmonyOS.Bindings.Api.PasteData;
using HSystemPasteboard = HarmonyOS.Bindings.Api.SystemPasteboard;
using HAccessCtrl = HarmonyOS.Bindings.Api.AbilityAccessCtrl;
using HPermissionStatus = HarmonyOS.ArkUI.PermissionStatus;
using HAtManager = HarmonyOS.Bindings.Api.AtManager;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyClipboard : IClipboard
{
    static readonly string[] ReadPermissions = ["ohos.permission.READ_PASTEBOARD"];

    readonly HSystemPasteboard _pasteboard;
    bool _permissionRequested;
    string? _lastKnownText; // 最近写入/读取的文本：同步读被权限拒时 HasText 的回退依据

    public HarmonyClipboard()
    {
        _pasteboard = HPasteboard.GetSystemPasteboard();
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
                return !string.IsNullOrEmpty(_lastKnownText);
            }
            catch (Exception ex) when (ex is ArkTSException or NapiException)
            {
                // 同步 getter 无法发起授权弹窗；未授权时按 MAUI 语义回退到最近已知状态
                HiLog.Warn("Essentials", $"clipboard HasText denied: {ex.Message}");
                return !string.IsNullOrEmpty(_lastKnownText);
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
        catch (Exception ex) when (!_permissionRequested && ex is ArkTSException or NapiException)
        {
            // 部分系统版本权限拒绝以异常形态出现：弹窗授权后重试一次；用户拒绝则重试仍抛，透传调用方
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

    /// <summary>true = 授权弹窗已发出且应重试读取；false = 已授权（空结果属正常）或无法申请</summary>
    async Task<bool> EnsureReadPermissionAsync()
    {
        if (_permissionRequested)
            return false;
        _permissionRequested = true;
        var context = NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8);
        if (context == IntPtr.Zero)
        {
            HiLog.Warn("Essentials",
                "host did not export globalThis.abilityContext; clipboard read stays denied");
            return false;
        }
        var atManager = HAccessCtrl.CreateAtManager();
        var permissionName = NativeValue.From(ReadPermissions[0]);
        HPermissionStatus status;
        try
        {
            status = atManager.GetSelfPermissionStatus(permissionName);
        }
        catch (NapiException)
        {
            // getSelfPermissionStatus 不可用（低版本系统）：无法预判，直接走旧异常触发路径
            return false;
        }
        HiLog.Info("Essentials", $"READ_PASTEBOARD status: {status}");
        if (status == HPermissionStatus.Granted)
            return false;
        await RequestPermissionsCoreAsync(atManager, context).ConfigureAwait(true);
        HiLog.Info("Essentials", "READ_PASTEBOARD permission requested from user");
        return true;
    }

    async Task RequestReadPermissionAsync()
    {
        _permissionRequested = true;
        var context = NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8);
        if (context == IntPtr.Zero)
        {
            HiLog.Warn("Essentials",
                "host did not export globalThis.abilityContext; clipboard read stays denied");
            return;
        }
        var atManager = HAccessCtrl.CreateAtManager();
        await RequestPermissionsCoreAsync(atManager, context).ConfigureAwait(true);
        HiLog.Info("Essentials", "READ_PASTEBOARD permission requested from user");
    }

    async Task RequestPermissionsCoreAsync(HAtManager atManager, IntPtr context)
    {
        // requestPermissionsFromUser 的 permissionList 必须是 JS Array<Permissions>：
        // 经 globalThis.Array 构造真数组直传（生成包装的 IntPtr[] 签名无法封送）
        var permissionList = NodeApi.CreateInstance(NodeApi.GetGlobal(), "Array"u8, ReadPermissions[0]);
        await NodeApi.CallMethodAsyncCallback<bool>(
            atManager.PinnedValue, "requestPermissionsFromUser"u8, static _ => true,
            context, permissionList).ConfigureAwait(true);
    }

#pragma warning disable CS0067 // onRemoteUpdate 通道未接线（见文件头说明）
    public event EventHandler<EventArgs>? ClipboardContentChanged;
#pragma warning restore CS0067
}
