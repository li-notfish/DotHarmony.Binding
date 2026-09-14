// 平台启动代码（对齐 HelloApp 的 Platforms/HarmonyOS 惯例）：
// NativeAOT 共享库导出层 + 模块初始化器。
// ILC 只导出入口程序集内的 [UnmanagedCallersOnly] 方法，
// 因此 libapp.so 需要这份薄转发层（Host 见 HarmonyOS.Bindings/Hosting，
// 引擎通道见 HarmonyOS.Bindings/Experimental）。
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.Experimental;
using HarmonyOS.Bindings.Hosting;

namespace EngineLab.Platforms.HarmonyOS;

internal static class NativeExports
{
    [UnmanagedCallersOnly(EntryPoint = "HarmonyInit")]
    private static int HarmonyInit(nint env) => Host.InitializeCore(env);

    // C shim initEngine 通道：引擎桥对象 → ArkTsEngine（存引用 + onEvent 注入 + 冲刷积压指令）
    [UnmanagedCallersOnly(EntryPoint = "HarmonyEngineInit")]
    private static int HarmonyEngineInit(nint env, nint bridgeValue)
        => ArkTsEngine.InitializeCore(env, bridgeValue);
}

internal static class Bootstrap
{
    // NativeAOT 模块加载时自动执行，早于宿主的任何导出调用：
    // 此处构建引擎场景（纯托管入队，无需 env），initEngine 握手时统一冲刷
    [ModuleInitializer]
    internal static void Init() => Program.Register();
}
