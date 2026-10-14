using System.Runtime.InteropServices;
using global::HarmonyOS.Maui.Web;

namespace HarmonyWebApp1.Platforms.HarmonyOS;

internal static class NativeWebExports
{
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
