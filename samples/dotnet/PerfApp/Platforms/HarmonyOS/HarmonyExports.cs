// 平台启动代码：当前 OpenHarmony ILC 只导出入口程序集内的 [UnmanagedCallersOnly] 方法，
// 因此每个 libapp.so 应用都需要这份薄转发层；真实实现见 HarmonyOS.Bindings/Hosting。
using System.Runtime.InteropServices;
using global::HarmonyOS.Bindings.Hosting;
using global::HarmonyOS.Maui.Hosting;

namespace PerfApp.Platforms.HarmonyOS;

internal static class NativeExports
{
    [UnmanagedCallersOnly(EntryPoint = "HarmonyInit")]
    private static int HarmonyInit(nint env)
    {
        PerfClock.Start();
        return Host.InitializeCore(env);
    }

    [UnmanagedCallersOnly(EntryPoint = "HarmonyBuildUI")]
    private static int HarmonyBuildUI(nint env, nint nodeContentValue)
        => Host.BuildUICore(env, nodeContentValue);
}
