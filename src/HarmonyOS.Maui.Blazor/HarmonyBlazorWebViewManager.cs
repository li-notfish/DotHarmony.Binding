#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HarmonyOS.Maui.Web;
using HarmonyOS.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace HarmonyOS.Maui.Blazor;

public sealed class HarmonyBlazorWebViewManager : WebViewManager
{
    public const string AppOrigin = "https://localhost/";
    private const string FrameworkPath = "_framework/";
    private static readonly Uri AppOriginUri = new(AppOrigin);

    private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(10);

    private readonly IHarmonyWebViewSurface _surface;
    private readonly HarmonyBlazorWebView _view;

    private volatile bool _ipcConfirmed;
    private volatile bool _blazorStarted;
    private bool _loadingHostPage;

    public HarmonyBlazorWebViewManager(
        IHarmonyWebViewSurface surface,
        IServiceProvider services,
        Microsoft.AspNetCore.Components.Dispatcher dispatcher,
        IFileProvider fileProvider,
        JSComponentConfigurationStore jsComponents,
        string hostPageRelativePath,
        HarmonyBlazorWebView view)
        : base(services, dispatcher, AppOriginUri, fileProvider, jsComponents, hostPageRelativePath)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(view);
        _surface = surface;
        _view = view;

        // 走自研 AOT 安全 IPC 接收器——官方 WebViewManager.MessageReceived → IpcReceiver 的
        // JsonElement[] 反序列化在 NativeAOT 下依赖本程序集注册的 converter，只作兜底不可靠。
        _surface.RegisterMessageHandler(message => HarmonyIpcReceiver.Dispatch(this, message));
        _surface.SetSchemeHandler("https", HandleSchemeRequest);
    }

    internal IHarmonyWebViewSurface Surface => _surface;

    protected override void NavigateCore(Uri absoluteUri)
    {
        _ = LoadPageAndStartAsync(absoluteUri);
    }

    private async Task LoadPageAndStartAsync(Uri absoluteUri)
    {
        try
        {
            _loadingHostPage = true;
            await _surface.LoadUrlAsync(absoluteUri.ToString()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _loadingHostPage = false;
            HiLog.Error("HarmonyWeb", $"[blazor-manager] load url failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// 主框架页面加载完成（宿主 Web onPageEnd → HarmonyWebRuntime.NotifyPageEndedCore）。
    /// 这是 Blazor 启动的确定性信号：页面脚本（含 blazor.webview.js）已加载完毕。
    /// 反复进入该方法只对宿主页首次导航生效；SPA 内部导航不重载页面。
    /// </summary>
    internal async Task OnPageLoadedAsync(string url)
    {
        if (!_loadingHostPage)
            return;
        _loadingHostPage = false;

        // 同一管理器只启动一次 Blazor 运行时
        if (_blazorStarted)
            return;
        _blazorStarted = true;

        await StartUntilConfirmedAsync().ConfigureAwait(false);
    }

    private async Task StartUntilConfirmedAsync()
    {
        try
        {
            await _surface.RunScriptAsync("Blazor.start();").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"[blazor-manager] start failed: {ex.GetType().Name}: {ex.Message}");
        }

        // __bwv 握手（AttachPage 为首条）确认页面已挂载；未确认时给出明确诊断。
        var deadline = DateTime.UtcNow + HandshakeTimeout;
        while (!_ipcConfirmed && DateTime.UtcNow < deadline)
            await Task.Delay(100).ConfigureAwait(false);

        if (!_ipcConfirmed)
        {
            HiLog.Error("HarmonyWeb",
                "[blazor-manager] Blazor.start sent but no __bwv handshake received within " +
                $"{HandshakeTimeout.TotalSeconds:0}s; page may stay blank");
        }
        else
        {
            HiLog.Info("HarmonyWeb", "[blazor-manager] Blazor started, __bwv handshake confirmed");
        }
    }

    /// <summary>__bwv 任何消息到达即视为 blazor.webview.js 初始化完成。</summary>
    internal void NoteBlazorIpc() => _ipcConfirmed = true;

    protected override void SendMessage(string message)
    {
        var encoded = JavaScriptEncoder.Default.Encode(message);
        _ = SendToPageAsync(encoded);
    }

    private async Task SendToPageAsync(string encoded)
    {
        try
        {
            await _surface.RunScriptAsync($"window.external.dispatchMessage(\"{encoded}\")").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"[blazor-manager] SendMessage failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    internal HarmonyWebResponse HandleSchemeRequest(HarmonyWebRequest request)
    {
        // 1) 宿主可接管任意请求（WebResourceRequested）
        var webResourceArgs = new HarmonyWebResourceRequestedEventArgs(request);
        if (_view.RaiseWebResourceRequested(webResourceArgs) && webResourceArgs.Handled)
            return webResourceArgs.Response ?? new HarmonyWebResponse(204, "text/plain", Array.Empty<byte>());

        // 2) 官方拦截路径：app origin 内的静态内容
        var hasFileExtension = Path.HasExtension(new Uri(request.Url).AbsolutePath);
        if (TryGetResponseContent(
                request.Url,
                allowFallbackOnHostPage: !hasFileExtension,
                out var statusCode,
                out _,
                out var stream,
                out var headers))
        {
            using var ms = stream.CanSeek
                ? new MemoryStream((int)Math.Min(stream.Length, int.MaxValue))
                : new MemoryStream();
            stream.CopyTo(ms);
            var contentType = headers.TryGetValue("Content-Type", out var type)
                ? type
                : HarmonyBlazorMimeMap.Resolve(request.Url);
            return new HarmonyWebResponse(statusCode, contentType, ms.ToArray())
            {
                Headers = headers,
            };
        }

        return new HarmonyWebResponse(404, "text/plain", System.Text.Encoding.UTF8.GetBytes("Not Found"));
    }

    /// <summary>
    /// 导航决策（宿主 onLoadIntercept → 返回 true 表示取消加载）。
    /// 官方语义：非 app origin 的外部 http(s) 链接默认交给系统浏览器（OpenExternally），
    /// 未订阅 UrlLoading 时同样如此；app origin 内部导航放行。
    /// </summary>
    internal bool ShouldCancelNavigation(string url, HarmonyBlazorWebView view)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Blob/data/javascript 等交给 WebView 自己处理
        if (uri.Scheme is not ("http" or "https"))
            return false;

        // app origin（及 _framework）永不拦截
        if (IsAppOrigin(uri))
            return false;

        var strategy = HarmonyUrlLoadingStrategy.OpenExternally;
        var args = new HarmonyUrlLoadingEventArgs(uri, strategy);
        view.RaiseUrlLoading(args);

        switch (args.UrlLoadingStrategy)
        {
            case HarmonyUrlLoadingStrategy.CancelLoad:
                HiLog.Info("HarmonyWeb", $"[blazor-manager] UrlLoading cancelled: {uri}");
                return true;

            case HarmonyUrlLoadingStrategy.OpenExternally:
                HiLog.Info("HarmonyWeb", $"[blazor-manager] UrlLoading opens externally: {uri}");
                OpenExternal(uri);
                return true;

            case HarmonyUrlLoadingStrategy.OpenInWebView:
            default:
                // 非 app origin 无法由 rawfile scheme handler 提供内容，
                // 官方在此场景会放行给平台 WebView 直接加载；鸿蒙侧同样放行。
                HiLog.Info("HarmonyWeb", $"[blazor-manager] UrlLoading allows in-webview navigation: {uri}");
                return false;
        }
    }

    private static bool IsAppOrigin(Uri uri)
        => uri.Scheme == "https" &&
           string.Equals(uri.Host, AppOriginUri.Host, StringComparison.OrdinalIgnoreCase) &&
           uri.Port == AppOriginUri.Port;

    /// <summary>交给系统浏览器打开（等价官方 OpenExternally）。走 abilityContext.startAbility。</summary>
    private static void OpenExternal(Uri uri)
    {
        try
        {
            var global = NodeApi.GetGlobal();
            var abilityContext = NodeApi.GetProperty(global, "abilityContext");
            if (abilityContext == IntPtr.Zero)
            {
                HiLog.Warn("HarmonyWeb", "[blazor-manager] abilityContext unavailable; cannot open externally");
                return;
            }

            var want = NodeApi.CreateObject();
            NodeApi.SetProperty(want, "action", "ohos.want.action.viewData");
            NodeApi.SetProperty(want, "uri", uri.ToString());
            NodeApi.SetProperty(want, "type", "text/plain");
            NodeApi.CallMethodVoid(abilityContext, "startAbility"u8, want);
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"[blazor-manager] open external failed: {ex.GetType().Name}: {ex.Message}");
        }
    }
}

internal static class HarmonyBlazorMimeMap
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html",
        [".htm"] = "text/html",
        [".js"] = "text/javascript",
        [".mjs"] = "text/javascript",
        [".css"] = "text/css",
        [".json"] = "application/json",
        [".wasm"] = "application/wasm",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".svg"] = "image/svg+xml",
        [".ico"] = "image/x-icon",
        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
        [".ttf"] = "font/ttf",
        [".map"] = "application/json",
        [".txt"] = "text/plain",
        [".dat"] = "application/octet-stream",
        [".blat"] = "application/octet-stream",
        [".pdb"] = "application/octet-stream",
        [".dll"] = "application/octet-stream",
    };

    public static string Resolve(string url)
    {
        var path = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.AbsolutePath : url;
        // 非绝对 URL（如 "app.js?v=1"）也要剥离 query/fragment 再取扩展名
        var cut = path.IndexOfAny(new[] { '?', '#' });
        if (cut >= 0)
            path = path[..cut];
        var ext = Path.GetExtension(path);
        return Map.TryGetValue(ext, out var type) ? type : "application/octet-stream";
    }
}
