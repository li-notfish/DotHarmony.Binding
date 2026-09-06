// HelloApp (M1)：MAUI 控件树（StackLayout/Label/Button）在鸿蒙上的端到端
// 控件全部来自 Microsoft.Maui.Controls（netstandard VirtualView），
// 渲染经 HarmonyOS.Maui 的 Handler 映射到 ArkUI 原生节点（ArkUINodeBase）。
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyDeviceInfo = HarmonyOS.Bindings.Api.DeviceInfo;
using HarmonyOS.Bindings.Hosting;
using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.Controls;

namespace HelloApp;

/// <summary>
/// NativeAOT 共享库导出层。
/// ILC 只导出入口程序集内的 [UnmanagedCallersOnly] 方法，
/// 因此每个 libapp.so 应用都要在此转发到 Hosting.Host（与其他库实现）。
/// </summary>
internal static class NativeExports
{
    [UnmanagedCallersOnly(EntryPoint = "HarmonyInit")]
    private static int HarmonyInit(nint env) => Host.InitializeCore(env);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyBuildUI")]
    private static int HarmonyBuildUI(nint env, nint nodeContentValue)
        => Host.BuildUICore(env, nodeContentValue);
}

internal static class Bootstrap
{
    // NativeAOT 模块加载时自动执行，早于宿主对 HarmonyBuildUI 的任何调用
    [ModuleInitializer]
    internal static void Init() => Program.Register();
}

public static class Program
{
    public static void Register()
    {
        int clicks = 0;

        // M1：纯 MAUI 控件树——没有任何手写 ArkUI 节点代码
        MauiHarmonyHost.Run(() =>
        {
            string deviceLine;
            try
            {
                deviceLine = $"{HarmonyDeviceInfo.Brand} {HarmonyDeviceInfo.ProductModel} · {HarmonyDeviceInfo.OsFullName}";
            }
            catch (Exception ex)
            {
                deviceLine = "deviceInfo FAILED: " + ex.Message;
            }

            var title = new Label
            {
                Text = "Hello MAUI on HarmonyOS!",
                HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.White,
                Padding = 8,
            };

            var device = new Label
            {
                Text = deviceLine,
                HorizontalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.White,
                Padding = 8,
            };

            var button = new Button { Text = "Tap me (MAUI)" };

            button.Clicked += (_, _) =>
            {
                clicks++;
                button.Text = $"MAUI clicked {clicks}x";
            };

            return new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 12,
                Padding = 12,
                BackgroundColor = Colors.LightGray,
                Children = { title, device, button },
            };
        });
    }
}
