#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.Hosting;
using HarmonyOS.Bindings.NativeNode;
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

    /// <summary>surface 注册完成通知（多实例时 handler 据此继续初始化）。</summary>
    public static event Action<string>? SurfaceRegistered;

    /// <summary>surface 完成平台节点绑定（内嵌模式就绪）——handler 可据此重试挂载。</summary>
    public static event Action<string>? PlatformNodeBound;

    /// <summary>默认 tag 的 surface（单实例时代的兼容读取口）。</summary>
    public static IHarmonyWebViewSurface? Current
    {
        get
        {
            lock (Sync)
                return Surfaces.TryGetValue(DefaultTag, out var surface) ? surface : null;
        }
    }

    /// <summary>
    /// 按 webTag 取 surface；未注册时先请求宿主按需创建（多实例）。
    /// 创建是异步的（ArkTS 侧 BuilderNode → onControllerAttached → passWebView），
    /// 因此返回值可能仍为 null，调用方可稍后重试 <see cref="GetSurface"/>。
    /// </summary>
    public static IHarmonyWebViewSurface? EnsureSurface(string? webTag)
    {
        var existing = GetSurface(webTag);
        if (existing is not null)
            return existing;

        var tag = string.IsNullOrEmpty(webTag) ? DefaultTag : webTag;
        if (HarmonyWebViewFactorySink.IsAvailable)
            HarmonyWebViewFactorySink.TryCreate(tag);
        else
            HiLog.Warn("HarmonyWeb", $"[factory] no on-demand host; surface '{tag}' must be pre-created");

        return GetSurface(tag);
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
            SurfaceRegistered?.Invoke(tag);
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"InitializeCore: {ex.GetType().Name}: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// 内嵌模式初始化：宿主用 BuilderNode 把 Web 包成节点后传入 FrameNode，
    /// surface 保存对应节点句柄，供 handler 挂进 MAUI 布局容器。
    /// </summary>
    public static int InitializeWithNodeCore(IntPtr env, IntPtr webViewValue, string? webTag, IntPtr frameNodeValue)
    {
        var result = InitializeCore(env, webViewValue, webTag);
        if (result != 0)
            return result;

        try
        {
            if (frameNodeValue == IntPtr.Zero)
                return result;

            var tag = string.IsNullOrEmpty(webTag) ? DefaultTag : webTag;
            var surface = GetSurface(tag) as HarmonyArkWebViewSurface;
            if (surface is null)
                return result;

            var node = ArkUINativeApi.GetNodeHandleFromNapiValue(env, frameNodeValue);
            surface.SetPlatformNode(node);
            HiLog.Info("HarmonyWeb", $"[node] surface '{tag}' bound to web node 0x{node:X} (embedded mode)");
            // 节点可能晚于 handler 的首次挂载尝试（factory 异步创建），通知其重试
            PlatformNodeBound?.Invoke(tag);
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"InitializeWithNodeCore: {ex.GetType().Name}: {ex.Message}");
            return -4;
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
        => ReceiveMessageCore(env, messageValue, IntPtr.Zero);

    /// <summary>带 webTag 的消息入口：多实例时按 tag 路由到对应 surface。</summary>
    public static int ReceiveMessageCore(IntPtr env, IntPtr messageValue, IntPtr tagValue)
    {
        try
        {
            Host.InitializeCore(env);
            var message = NodeApi.ToString(messageValue);
            if (message is null)
                return -1;

            // tag 以 napi_value 传入（ArkTS 侧 bridge 的 webTag 字段）；缺省回落默认/唯一 surface。
            var tag = tagValue == IntPtr.Zero ? null : NodeApi.ToString(tagValue);
            var surface = string.IsNullOrEmpty(tag)
                ? (HarmonyArkWebViewSurface?)(Current ?? SingleSurface())
                : (HarmonyArkWebViewSurface?)GetSurface(tag);
            surface?.ReceiveMessage(message);
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
        => InterceptRequestNativeCore(env, requestId, urlPtr, null);

    /// <summary>带 webTag 的拦截入口：多实例时按 tag 精确路由到对应 surface。</summary>
    public static unsafe int InterceptRequestNativeCore(IntPtr env, int requestId, sbyte* urlPtr, string? webTag)
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

            var surface = string.IsNullOrEmpty(webTag)
                ? ResolveSurfaceForRequest(url)
                : (HarmonyArkWebViewSurface?)GetSurface(webTag);
            var response = surface?.InterceptRequest(new HarmonyWebRequest(url, "GET"));
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
