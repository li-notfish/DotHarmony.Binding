// 平台启动代码：当前 OpenHarmony ILC 只导出入口程序集内的 [UnmanagedCallersOnly] 方法，
// 因此每个 libapp.so 应用都需要这份薄转发层；真实实现见 HarmonyOS.Bindings/Hosting。
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using global::HarmonyOS.Bindings.Hosting;

namespace ControlsSampleApp.Platforms.HarmonyOS;

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

internal static class Bootstrap
{
    [ModuleInitializer]
    internal static void Init() => Program.Register();
}
