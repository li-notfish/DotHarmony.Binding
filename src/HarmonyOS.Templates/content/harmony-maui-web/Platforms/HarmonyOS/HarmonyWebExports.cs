using System.Runtime.InteropServices;
using global::HarmonyOS.Maui.Web;

namespace HarmonyWebApp1.Platforms.HarmonyOS;

internal static class NativeWebExports
{
    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInit")]
    private static int HarmonyWebInit(nint env, nint webViewValue)
        => HarmonyWebRuntime.InitializeCore(env, webViewValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInitTagged")]
    private static int HarmonyWebInitTagged(nint env, nint webViewValue, nint tagValue)
        => HarmonyWebRuntime.InitializeCore(env, webViewValue, global::HarmonyOS.Interop.NodeApi.ToString(tagValue));

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInitWithNode")]
    private static int HarmonyWebInitWithNode(nint env, nint webViewValue, nint tagValue, nint frameNodeValue)
        => HarmonyWebRuntime.InitializeWithNodeCore(
            env,
            webViewValue,
            global::HarmonyOS.Interop.NodeApi.ToString(tagValue),
            frameNodeValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebShutdown")]
    private static int HarmonyWebShutdown(nint env, nint tagValue)
        => HarmonyWebRuntime.ShutdownCore(env, global::HarmonyOS.Interop.NodeApi.ToString(tagValue));

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebPageEnded")]
    private static int HarmonyWebPageEnded(nint env, nint urlValue, nint tagValue)
        => HarmonyWebRuntime.NotifyPageEndedCore(env, urlValue, global::HarmonyOS.Interop.NodeApi.ToString(tagValue));

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebShouldInterceptNavigation")]
    private static int HarmonyWebShouldInterceptNavigation(nint env, nint urlValue, nint tagValue)
        => HarmonyWebRuntime.ShouldInterceptNavigationCore(env, urlValue, global::HarmonyOS.Interop.NodeApi.ToString(tagValue));

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInterceptRequest")]
    private static nint HarmonyWebInterceptRequest(nint env, nint urlValue, nint createResponseValue)
        => HarmonyWebRuntime.InterceptRequestCore(env, urlValue, createResponseValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebReceiveMessage")]
    private static int HarmonyWebReceiveMessage(nint env, nint messageValue, nint tagValue)
        => HarmonyWebRuntime.ReceiveMessageCore(env, messageValue, tagValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebProxyReady")]
    private static int HarmonyWebProxyReady(nint env)
        => HarmonyWebRuntime.NotifyProxyReadyCore(env);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInterceptNative")]
    private static unsafe int HarmonyWebInterceptNative(nint env, int requestId, sbyte* url, sbyte* webTag)
        => HarmonyWebRuntime.InterceptRequestNativeCore(
            env,
            requestId,
            url,
            webTag is null ? null : System.Runtime.InteropServices.Marshal.PtrToStringUTF8((nint)webTag));
}
