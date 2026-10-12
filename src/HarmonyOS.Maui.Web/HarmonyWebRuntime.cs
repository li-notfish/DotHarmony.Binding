#nullable enable
using System;
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
}
