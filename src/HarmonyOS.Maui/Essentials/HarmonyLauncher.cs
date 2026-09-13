// ILauncher 鸿蒙实现：want/startAbility 通道（AppInfo.ShowSettingsUI 已走通同一通道）。
// CanOpenAsync 经 @ohos.bundle.bundleManager.canOpenLink（API 12 起；自定义 scheme 需应用
// 在 app.json5 声明 querySchemes 白名单，未声明时返回 false）；OpenAsync/TryOpenAsync 经 startAbility。
// OpenAsync(OpenFileRequest) 的文件路径经 @ohos.file.fileuri.getUriFromPath 折算 file://。
#nullable enable
using Microsoft.Maui.ApplicationModel;
using HBundleManager = HarmonyOS.Bindings.Api.BundleManager;
using HFileuri = HarmonyOS.Bindings.Api.Fileuri;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyLauncher : ILauncher
{
    public Task<bool> CanOpenAsync(Uri uri) =>
        Task.FromResult(HBundleManager.CanOpenLink(uri.ToString()));

    public Task<bool> OpenAsync(Uri uri) => StartAbilityAsync(uri);

    public Task<bool> OpenAsync(OpenFileRequest request) =>
        OpenAsync(new Uri(HFileuri.GetUriFromPath(request.File!.FullPath)));

    public Task<bool> TryOpenAsync(Uri uri) => StartAbilityAsync(uri);

    // 共享的 want 通道：want = {uri} + startAbility（异步，promise 续体在 JS 线程恢复）
    internal static async Task<bool> StartAbilityAsync(Uri uri)
    {
        try
        {
            var want = NativeValue.From(new Dictionary<string, object?>
            {
                ["uri"] = uri.ToString(),
            });
            await NodeApi.CallMethodAsync<object?>(HarmonyPreferences.Context, "startAbility", want);
            return true;
        }
        catch (Exception ex)
        {
            HiLog.Warn("Essentials", $"[launcher] open {uri} failed: {ex.Message}");
            return false;
        }
    }
}
