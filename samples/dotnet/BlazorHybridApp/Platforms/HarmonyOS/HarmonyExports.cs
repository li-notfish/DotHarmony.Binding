using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using global::HarmonyOS.Bindings.Hosting;
using global::HarmonyOS.Maui.Hosting;
using global::HarmonyOS.Maui.Web;

namespace BlazorHybridApp.Platforms.HarmonyOS;

// Verified: the current OpenHarmony ILC toolchain only exports [UnmanagedCallersOnly]
// methods from the entry assembly (library-defined ones never reach app.so's dynamic
// symbol table), so every libapp.so app needs this thin forwarding layer.
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

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInit")]
    private static int HarmonyWebInit(nint env, nint webViewValue)
        => HarmonyWebRuntime.InitializeCore(env, webViewValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInterceptRequest")]
    private static nint HarmonyWebInterceptRequest(nint env, nint urlValue, nint createResponseValue)
        => HarmonyWebRuntime.InterceptRequestCore(env, urlValue, createResponseValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebReceiveMessage")]
    private static int HarmonyWebReceiveMessage(nint env, nint messageValue)
        => HarmonyWebRuntime.ReceiveMessageCore(env, messageValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebProxyReady")]
    private static int HarmonyWebProxyReady(nint env)
        => HarmonyWebRuntime.NotifyProxyReadyCore(env);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInterceptNative")]
    private static unsafe int HarmonyWebInterceptNative(nint env, int requestId, sbyte* url)
        => HarmonyWebRuntime.InterceptRequestNativeCore(env, requestId, url);
}

internal static class Bootstrap
{
    [ModuleInitializer]
    internal static void Init() => Program.Register();
}
