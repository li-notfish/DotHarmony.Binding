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

    // webTag 以 napi_value（JS string）传入：C 侧原样转发，这里用 NodeApi 读取。
    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInitTagged")]
    private static int HarmonyWebInitTagged(nint env, nint webViewValue, nint tagValue)
        => HarmonyWebRuntime.InitializeCore(env, webViewValue, global::HarmonyOS.Interop.NodeApi.ToString(tagValue));

    // 内嵌模式：webTag + BuilderNode 的 FrameNode（napi_value，由 C 侧原样转发）
    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebInitWithNode")]
    private static int HarmonyWebInitWithNode(nint env, nint webViewValue, nint tagValue, nint frameNodeValue)
        => HarmonyWebRuntime.InitializeWithNodeCore(
            env,
            webViewValue,
            global::HarmonyOS.Interop.NodeApi.ToString(tagValue),
            // FrameNode 需要 napi_value 原样传入；ToString 对对象返回 null，这里用零表示"无节点"，
            // 真正的节点映射在 InitializeWithNodeCore 内完成（需要 napi_value 而非字符串）。
            frameNodeValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebShutdown")]
    private static int HarmonyWebShutdown(nint env, nint tagValue)
        => HarmonyWebRuntime.ShutdownCore(env, global::HarmonyOS.Interop.NodeApi.ToString(tagValue));

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebPageEnded")]
    private static int HarmonyWebPageEnded(nint env, nint urlValue, nint tagValue)
        => HarmonyWebRuntime.NotifyPageEndedCore(
            env,
            urlValue,
            global::HarmonyOS.Interop.NodeApi.ToString(tagValue));

    [UnmanagedCallersOnly(EntryPoint = "HarmonyWebShouldInterceptNavigation")]
    private static int HarmonyWebShouldInterceptNavigation(nint env, nint urlValue, nint tagValue)
        => HarmonyWebRuntime.ShouldInterceptNavigationCore(
            env,
            urlValue,
            global::HarmonyOS.Interop.NodeApi.ToString(tagValue));

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

internal static class Bootstrap
{
    [ModuleInitializer]
    internal static void Init() => Program.Register();
}
