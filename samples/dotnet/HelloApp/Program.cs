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
        // M1-XAML：UI 由 MainPage.xaml 声明（XamlC 编译期生成控件树，NativeAOT 零反射），
        // 渲染经 HarmonyOS.Maui Handlers 映射到 ArkUI 原生节点。
        MauiHarmonyHost.Run(() => new MainPage());
    }
}
