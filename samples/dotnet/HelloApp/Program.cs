// HelloApp：.NET (NativeAOT) 在鸿蒙上的最小 UI 应用
// 构建产物 libapp.so 由 HarmonyHost 的 C shim dlopen 并调用 HarmonyInit / HarmonyBuildUI。
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.Api;
using HarmonyOS.Bindings.Hosting;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.ArkUI;

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

        Host.RootBuilder = contentHandle =>
        {
            // M0 验证：经 napi 服务通道读取 @ohos.deviceInfo（失败时把诊断写上屏）
            string deviceLine;
            try
            {
                deviceLine = $"{DeviceInfo.Brand} {DeviceInfo.ProductModel} · {DeviceInfo.OsFullName}";
            }
            catch (Exception ex)
            {
                deviceLine = "deviceInfo FAILED: " + ex.Message;
            }

            var root = new Column
            {
                JustifyContent = ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_CENTER,
                AlignItems = ArkUI_HorizontalAlignment.ARKUI_HORIZONTAL_ALIGNMENT_CENTER,
                Width = 320,
                Height = 240,
                Padding = 12,
            };
            root.SetBackgroundColor(245, 246, 248);

            var title = new Text
            {
                Content = "Hello .NET on HarmonyOS!",
                TextAlign = ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_CENTER,
            };
            title.SetBackgroundColor(255, 255, 255);
            title.Margin = 8;
            title.Padding = 8;

            var deviceText = new Text
            {
                Content = deviceLine,
                TextAlign = ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_CENTER,
            };
            deviceText.SetBackgroundColor(255, 255, 255);
            deviceText.Margin = 8;
            deviceText.Padding = 8;

            var button = new Button
            {
                Label = "Tap me (from .NET)",
            };
            button.Click += ev =>
            {
                clicks++;
                title.Content = $"Click@({ev.ClickX:F1},{ev.ClickY:F1})vp device={ev.ClickDevice}";
                button.Label = $"Clicked {clicks}x";
            };

            root.AddChild(title);
            root.AddChild(deviceText);
            root.AddChild(button);

            // 挂载到 ArkTS ContentSlot 提供的 NodeContent
            Host.AttachRoot(contentHandle, root);
        };
    }
}
