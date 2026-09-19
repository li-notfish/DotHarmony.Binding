// IShare 鸿蒙实现：文本经 startAbility({action:'ohos.want.action.sendData', type:'text/plain',
// parameters:{text}})（want 契约，接收方按 action 路由）；文件分享经跨应用 URI 授权通道——
// @ohos.file.fs openSync(READ_ONLY) → fd → want { uri:'fd://fd', type:mime,
// parameters:{'ability.params.stream':fd} }（OHOS 文件分享契约，接收方经 uri/params.stream 读 fd）。
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using HarmonyOS.Interop;

namespace HarmonyOS.Essentials;

public class HarmonyShare : IShare
{
    public Task RequestAsync(ShareTextRequest request)
    {
        var want = NativeValue.From(new Dictionary<string, object?>
        {
            ["action"] = "ohos.want.action.sendData",
            ["type"] = "text/plain",
            ["parameters"] = new Dictionary<string, object?>
            {
                ["text"] = request.Text ?? string.Empty,
            },
        });
        return NodeApi.CallMethodAsync<object?>(HarmonyPreferences.Context, "startAbility", want);
    }

    public async Task RequestAsync(ShareFileRequest request)
    {
        int fd = OpenReadOnly(request.File.FullPath);
        try
        {
            var want = NativeValue.From(new Dictionary<string, object?>
            {
                ["uri"] = $"fd://{fd}",
                ["type"] = request.File.ContentType,
                ["parameters"] = new Dictionary<string, object?>
                {
                    // ability.params.stream：跨应用 fd 授权通道（值为 fd 的字符串形式）
                    ["ability.params.stream"] = fd.ToString(),
                },
            });
            // await 恢复走 PromiseTaskBridge 的 TCS 同步续体（JS 线程内联，NapiEnv 可用）——
            // finally 的 closeSync 经 NodeApi 调用需 env；startAbility 完成后系统已 dup 接收方 fd
            await NodeApi.CallMethodAsync<object?>(HarmonyPreferences.Context, "startAbility", want);
        }
        finally
        {
            HarmonyOS.Bindings.Api.File.Fs.CloseSync(fd);
        }
    }

    public async Task RequestAsync(ShareMultipleFilesRequest request)
    {
        if (request.Files.Count == 0)
            throw new ArgumentException("ShareMultipleFilesRequest.Files is empty", nameof(request));
        // 多文件：fd 列表入 ability.params.stream（逗号分隔）+ uri 取首个；接收方按序读
        var fds = new List<int>();
        try
        {
            foreach (var file in request.Files)
                fds.Add(OpenReadOnly(file.FullPath));
            var want = NativeValue.From(new Dictionary<string, object?>
            {
                ["uri"] = $"fd://{fds[0]}",
                ["type"] = request.Files.Count > 0 ? request.Files[0].ContentType : "application/octet-stream",
                ["parameters"] = new Dictionary<string, object?>
                {
                    ["ability.params.stream"] = string.Join(",", fds),
                },
            });
            await NodeApi.CallMethodAsync<object?>(HarmonyPreferences.Context, "startAbility", want);
        }
        finally
        {
            foreach (var fd in fds)
                HarmonyOS.Bindings.Api.File.Fs.CloseSync(fd);
        }
    }

    /// <summary>file.fs openSync(READ_ONLY) → File.fd（fd 由调用方关闭）</summary>
    private static int OpenReadOnly(string fullPath)
    {
        var fileObj = HarmonyOS.Bindings.Api.File.Fs.OpenSync(fullPath, HarmonyOS.Bindings.Api.File.Fs.ReadOnly);
        if (fileObj == IntPtr.Zero)
            throw new FeatureNotSupportedException($"cannot open file for sharing: {fullPath}");
        var fd = NativeValue.ToInt(NodeApi.GetProperty(fileObj, "fd"));
        if (fd <= 0)
            throw new FeatureNotSupportedException($"cannot resolve fd for sharing: {fullPath}");
        return fd;
    }
}
