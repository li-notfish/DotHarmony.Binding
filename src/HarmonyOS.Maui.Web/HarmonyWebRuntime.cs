#nullable enable
using System;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.Hosting;
using HarmonyOS.Interop;

namespace HarmonyOS.Maui.Web;

public static class HarmonyWebRuntime
{
    public static IHarmonyWebViewSurface? Current { get; private set; }

    public static int InitializeCore(IntPtr env, IntPtr webViewValue)
    {
        try
        {
            // 单实例 surface：第二个 passWebView 会覆盖全局入口导致消息/拦截串台，直接拒绝并报错
            if (Current is not null)
            {
                HiLog.Error("HarmonyWeb", "InitializeCore: surface already initialized; multiple WebView hosts are not supported.");
                return -2;
            }
            Host.InitializeCore(env);
            var surface = new HarmonyArkWebViewSurface(webViewValue);
            surface.InitializeAsync().GetAwaiter().GetResult();
            Current = surface;
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"InitializeCore: {ex.GetType().Name}: {ex.Message}");
            return -1;
        }
    }

    public static IntPtr InterceptRequestCore(IntPtr env, IntPtr urlValue, IntPtr createResponseValue)
    {
        try
        {
            Host.InitializeCore(env);
            var url = NodeApi.ToString(urlValue);
            if (string.IsNullOrWhiteSpace(url))
                return IntPtr.Zero;

            var response = Current?.InterceptRequest(new HarmonyWebRequest(url, "GET"));
            if (response is null)
                return IntPtr.Zero;

            return NodeApi.CallFunction(
                createResponseValue,
                response.StatusCode,
                "OK",
                response.ContentType,
                NapiArg.OfArrayBuffer(response.Body));
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"InterceptRequestCore: {ex.GetType().Name}: {ex.Message}");
            return IntPtr.Zero;
        }
    }

    public static int ReceiveMessageCore(IntPtr env, IntPtr messageValue)
    {
        try
        {
            Host.InitializeCore(env);
            var message = NodeApi.ToString(messageValue);
            if (message is null)
                return -1;

            if (Current is HarmonyArkWebViewSurface surface)
                surface.ReceiveMessage(message);
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"ReceiveMessageCore: {ex.GetType().Name}: {ex.Message}");
            return -1;
        }
    }

    /// <summary>ArkWeb JS proxy 生效通知（经 NDK valid 回调 + TSFN 回到 env 线程后调用）。</summary>
    public static int NotifyProxyReadyCore(IntPtr env)
    {
        try
        {
            Host.InitializeCore(env);
            if (Current is HarmonyArkWebViewSurface surface)
                surface.RaiseProxyReady();
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"NotifyProxyReadyCore: {ex.GetType().Name}: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// NDK scheme 拦截入口（C 侧 OnRequestStart 经 TSFN 投递到 env 线程后调用）。
    /// 处理器内部允许 NAPI（rawfile 走 ResourceManager）；完成后经 HarmonyHostSink 回写 NDK 响应。
    /// </summary>
    public static unsafe int InterceptRequestNativeCore(IntPtr env, int requestId, sbyte* urlPtr)
    {
        try
        {
            Host.InitializeCore(env);
            var url = Marshal.PtrToStringUTF8((IntPtr)urlPtr);
            if (string.IsNullOrEmpty(url))
            {
                HarmonyHostSink.Fail(requestId);
                return -1;
            }

            var response = Current?.InterceptRequest(new HarmonyWebRequest(url, "GET"));
            if (response is null)
            {
                // 无注册处理器（未来得及初始化或非 Blazor 场景）：按 404 完成，请求不得悬空
                HarmonyHostSink.Respond(requestId, 404, "Not Found", "text/plain", "Not Found"u8);
                return 0;
            }
            HarmonyHostSink.Respond(requestId, response.StatusCode,
                response.StatusCode == 200 ? "OK" : "Not Found",
                response.ContentType, response.Body);
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"InterceptRequestNative: {ex.GetType().Name}: {ex.Message}");
            HarmonyHostSink.Fail(requestId);
            return -1;
        }
    }
}
