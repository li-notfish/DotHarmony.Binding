// IBrowser 鸿蒙实现：startAbility({uri}) 拉起系统默认浏览器（web URL 系统自动路由）。
// BrowserLaunchMode/BrowserLaunchOptions 的进程内打开等模式无系统通道——统一系统浏览器。
#nullable enable
using Microsoft.Maui.ApplicationModel;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyBrowser : IBrowser
{
    public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) =>
        HarmonyLauncher.StartAbilityAsync(uri);
}
