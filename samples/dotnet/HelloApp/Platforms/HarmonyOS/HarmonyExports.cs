// 平台启动代码（对齐 MAUI Platforms/Android/MainActivity 的摆放惯例）：
// NativeAOT 共享库导出层 + 模块初始化器。
// 已实测：当前 OpenHarmony ILC 工具链只导出入口程序集内的 [UnmanagedCallersOnly] 方法
// （库内定义不会进入 app.so 动态符号表），因此每个 libapp.so 应用都需要这份薄转发层
// （Host 的真实实现见 HarmonyOS.Bindings/Hosting）。
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using global::HarmonyOS.Bindings.Hosting;

namespace HelloApp.Platforms.HarmonyOS;

internal static class NativeExports
{
    [UnmanagedCallersOnly(EntryPoint = "HarmonyInit")]
    private static int HarmonyInit(nint env) => Host.InitializeCore(env);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyBuildUI")]
    private static int HarmonyBuildUI(nint env, nint nodeContentValue)
        => Host.BuildUICore(env, nodeContentValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyPopPage")]
    private static int HarmonyPopPage(nint env)
        => global::HarmonyOS.Maui.Hosting.HarmonyNavigation.OnBackRequested() ? 1 : 0;
}

// 主题回调无需导出：HarmonyOS.Maui 在 RunApplication 时经 libentry 的
// HarmonyHostSetThemeChangedCallback 导出符号主动注册函数指针（见 MauiHarmonyHost）。

internal static class Bootstrap
{
    // NativeAOT 模块加载时自动执行，早于宿主对 HarmonyBuildUI 的任何调用
    [ModuleInitializer]
    internal static void Init() => Program.Register();
}
