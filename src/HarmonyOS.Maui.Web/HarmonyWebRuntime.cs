#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.Hosting;
using HarmonyOS.Interop;

namespace HarmonyOS.Maui.Web;

/// <summary>
/// 宿主 Web 通道的 .NET 入口。多实例：surface 按 webTag 注册（NDK ArkWeb 的 scheme handler /
/// proxy-valid 回调本就按 webTag 寻址，见 host Index.ets 与 napi_init.c）。
/// 未传 tag 的旧导出（single-instance 时代的 HarmonyWebInit）等价于默认 tag <see cref="DefaultTag"/>。
/// </summary>
public static class HarmonyWebRuntime
{
    /// <summary>默认 webTag：与宿主 Index.ets 的单 Web 组件一致（历史单实例通道）。</summary>
    public const string DefaultTag = "harmony-web";

    private static readonly object Sync = new();
    private static readonly Dictionary<string, HarmonyArkWebViewSurface> Surfaces = new(StringComparer.Ordinal);

    /// <summary>默认 tag 的 surface（单实例时代的兼容读取口）。</summary>
    public static IHarmonyWebViewSurface? Current
    {
        get
        {
            lock (Sync)
                return Surfaces.TryGetValue(DefaultTag, out var surface) ? surface : null;
        }
    }

    /// <summary>按 webTag 取 surface；未注册返回 null。</summary>
    public static IHarmonyWebViewSurface? GetSurface(string? webTag)
    {
        lock (Sync)
            return Surfaces.TryGetValue(string.IsNullOrEmpty(webTag) ? DefaultTag : webTag, out var surface)
                ? surface
                : null;
    }

    public static int InitializeCore(IntPtr env, IntPtr webViewValue)
        => InitializeCore(env, webViewValue, null);

    /// <summary>
    /// 注册一个 Web surface。webTag 为空时落到 <see cref="DefaultTag"/>。
    /// 同 tag 重复注册返回 -2（NDK 全局入口会被覆盖，消息/拦截会串台）。
    /// </summary>
    public static int InitializeCore(IntPtr env, IntPtr webViewValue, string? webTag)
    {
        try
        {
            var tag = string.IsNullOrEmpty(webTag) ? DefaultTag : webTag;
            lock (Sync)
            {
                if (Surfaces.ContainsKey(tag))
                {
                    HiLog.Error("HarmonyWeb",
                        $"InitializeCore: surface '{tag}' already initialized; duplicate webTag rejected.");
                    return -2;
                }
            }

            Host.InitializeCore(env);
            var surface = new HarmonyArkWebViewSurface(webViewValue);
            surface.InitializeAsync().GetAwaiter().GetResult();

            lock (Sync)
                Surfaces[tag] = surface;
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"InitializeCore: {ex.GetType().Name}: {ex.Message}");
            return -1;
        }
    }

    /// <summary>注销 surface（Web 组件销毁时由宿主调用），释放 JS proxy 回调句柄。</summary>
    public static int ShutdownCore(IntPtr env, string? webTag)
    {
        try
        {
            var tag = string.IsNullOrEmpty(webTag) ? DefaultTag : webTag;
            HarmonyArkWebViewSurface? surface;
            lock (Sync)
            {
                if (!Surfaces.Remove(tag, out surface))
                    return 0;
            }
            surface.Dispose();
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"ShutdownCore: {ex.GetType().Name}: {ex.Message}");
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

            var response = ResolveSurfaceForRequest(url)?.InterceptRequest(new HarmonyWebRequest(url, "GET"));
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

            // 单 surface 时代消息无 tag 信息：优先默认 tag，其次唯一注册的 surface。
            ((HarmonyArkWebViewSurface?)(Current ?? SingleSurface()))?.ReceiveMessage(message);
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
        => NotifyProxyReadyCore(env, null);

    public static int NotifyProxyReadyCore(IntPtr env, string? webTag)
    {
        try
        {
            Host.InitializeCore(env);
            ((HarmonyArkWebViewSurface?)(string.IsNullOrEmpty(webTag) ? Current ?? SingleSurface() : GetSurface(webTag)))?.RaiseProxyReady();
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"NotifyProxyReadyCore: {ex.GetType().Name}: {ex.Message}");
            return -1;
        }
    }

    /// <summary>主框架加载完成（Web 组件 onPageEnd）：Blazor 启动的确定性信号。</summary>
    public static int NotifyPageEndedCore(IntPtr env, IntPtr urlValue)
        => NotifyPageEndedCore(env, urlValue, null);

    public static int NotifyPageEndedCore(IntPtr env, IntPtr urlValue, string? webTag)
    {
        try
        {
            Host.InitializeCore(env);
            var url = urlValue == IntPtr.Zero ? null : NodeApi.ToString(urlValue);
            var surface = ResolveSurface(webTag) as HarmonyArkWebViewSurface;
            surface?.RaisePageEnded(url);
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"NotifyPageEndedCore: {ex.GetType().Name}: {ex.Message}");
            return -1;
        }
    }

    /// <summary>导航拦截（Web 组件 onLoadIntercept）：返回 1 表示拦截，0 表示放行。</summary>
    public static int ShouldInterceptNavigationCore(IntPtr env, IntPtr urlValue)
        => ShouldInterceptNavigationCore(env, urlValue, null);

    public static int ShouldInterceptNavigationCore(IntPtr env, IntPtr urlValue, string? webTag)
    {
        try
        {
            Host.InitializeCore(env);
            var url = NodeApi.ToString(urlValue);
            if (string.IsNullOrEmpty(url))
                return 0;
            var surface = ResolveSurface(webTag) as HarmonyArkWebViewSurface;
            return surface is not null && surface.RaiseNavigationStarting(url) ? 1 : 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"ShouldInterceptNavigationCore: {ex.GetType().Name}: {ex.Message}");
            return 0;
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

            var response = ResolveSurfaceForRequest(url)?.InterceptRequest(new HarmonyWebRequest(url, "GET"));
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

    private static IHarmonyWebViewSurface? SingleSurface()
    {
        lock (Sync)
            return Surfaces.Count == 1 ? Surfaces.Values.First() : null;
    }

    private static IHarmonyWebViewSurface? ResolveSurface(string? webTag)
        => string.IsNullOrEmpty(webTag) ? Current ?? SingleSurface() : GetSurface(webTag);

    private static HarmonyArkWebViewSurface? ResolveSurfaceForRequest(string url)
    {
        // 单实例/app origin 统一走默认 surface；多实例时按 webTag 语义留待宿主携带 tag（见 ShutdownCore/Notify* 重载）。
        return (HarmonyArkWebViewSurface?)(Current ?? SingleSurface());
    }
}
