// IShare 鸿蒙实现：文本经 startAbility({action:'ohos.want.action.sendData', type:'text/plain',
// parameters:{text}})（want 契约，接收方按 action 路由）；文件分享需跨应用 URI 授权通道
// （ability.params.stream fd 传递），留待立项——抛 FeatureNotSupportedException 并记录原因。
#nullable enable
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Maui.Essentials;

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

    public Task RequestAsync(ShareFileRequest request) => FilesNotSupported();

    public Task RequestAsync(ShareMultipleFilesRequest request) => FilesNotSupported();

    private static Task FilesNotSupported()
    {
        HiLog.Warn("Essentials", "share files requires cross-app URI grant channel (pending)");
        throw new FeatureNotSupportedException("File sharing on HarmonyOS needs the cross-app URI grant channel (pending)");
    }
}
