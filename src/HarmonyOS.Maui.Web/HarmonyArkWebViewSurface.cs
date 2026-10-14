#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HarmonyOS.Bindings.Api.Web;
using HarmonyOS.Interop;

namespace HarmonyOS.Maui.Web;

public sealed class HarmonyArkWebViewSurface : IHarmonyWebViewSurface
{
    public event Action? ProxyReady;

    public bool IsProxyReady { get; private set; }

    private readonly WebviewController _controller;
    private GCHandle _messageHandle;
    private Action<string>? _messageHandler;
    private Func<HarmonyWebRequest, HarmonyWebResponse>? _responseFactory;

    public HarmonyArkWebViewSurface(IntPtr controllerHandle)
    {
        _controller = new WebviewController(controllerHandle);
    }

    public Task InitializeAsync()
    {
        var (jsFunc, handle) = NodeApi.CreateCallbackFunction((Action<string>)ReceiveMessage);
        _messageHandle = handle;
        var jsObject = NodeApi.CreateObject();
        NodeApi.SetProperty(jsObject, "postMessage", jsFunc);
        _controller.RegisterJavaScriptProxy(jsObject, "harmony", new[] { "postMessage" });
        return Task.CompletedTask;
    }

    public Task LoadUrlAsync(string url)
    {
        ArgumentNullException.ThrowIfNull(url);
        _controller.LoadUrl(url);
        return Task.CompletedTask;
    }

    public Task LoadDataAsync(string html, string baseUrl)
    {
        ArgumentNullException.ThrowIfNull(html);
        _controller.LoadData(html, "text/html", "utf-8", baseUrl, baseUrl);
        return Task.CompletedTask;
    }

    public Task<string> RunScriptAsync(string script)
    {
        ArgumentNullException.ThrowIfNull(script);
        return _controller.RunJavaScriptAsync(script);
    }

    public void SetCustomUserAgent(string userAgent)
    {
        ArgumentNullException.ThrowIfNull(userAgent);
        // 原生 UA 通道；须在页面加载前生效，由 WebviewController.setCustomUserAgent 保证
        _controller.SetCustomUserAgent(userAgent);
    }

    public void RegisterMessageHandler(Action<string> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _messageHandler = handler;
    }

    public void SetSchemeHandler(string scheme, Func<HarmonyWebRequest, HarmonyWebResponse> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _responseFactory = handler;
        // Requests are intercepted by Index.web.ets and forwarded through HarmonyWebRuntime.
        // Registering WebSchemeHandler here would bypass ArkTS onInterceptRequest and break
        // the rawfile fallback needed before the MAUI Blazor manager exists.
    }

    public void ClearSchemeHandler()
    {
        _responseFactory = null;
    }

    public HarmonyWebResponse? InterceptRequest(HarmonyWebRequest request)
    {
        return _responseFactory?.Invoke(request);
    }

    internal void ReceiveMessage(string message)
    {
        _messageHandler?.Invoke(message);
    }

    internal void RaiseProxyReady()
    {
        IsProxyReady = true;
        ProxyReady?.Invoke();
    }
}
