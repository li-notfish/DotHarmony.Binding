#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HarmonyOS.Bindings.Api.Web;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;

namespace HarmonyOS.Maui.Web;

public sealed class HarmonyArkWebViewSurface : IHarmonyWebViewSurface, IDisposable
{
    public event Action? ProxyReady;

    public event Action<string>? PageEnded;

    public event Func<string, bool>? NavigationStarting;

    public bool IsProxyReady { get; private set; }

    private readonly WebviewController _controller;
    private GCHandle _messageHandle;
    private IntPtr _platformNode;      // BuilderNode.getFrameNode() 映射得到的 ArkUI_NodeHandle
    private IntPtr _attachedParent;    // 当前挂载的父节点（0 表示未挂载）
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

    /// <summary>
    /// 宿主把 BuilderNode(Web) 的 FrameNode 映射成节点句柄后交给 surface，
    /// 供 <see cref="TryAttachPlatformNode"/> 在 handler 连接时挂进 MAUI 布局。
    /// </summary>
    internal void SetPlatformNode(IntPtr nodeHandle) => _platformNode = nodeHandle;

    public bool TryAttachPlatformNode(IntPtr parentHandle)
    {
        if (_platformNode == IntPtr.Zero || parentHandle == IntPtr.Zero)
            return false;
        if (_attachedParent != IntPtr.Zero)
            return _attachedParent == parentHandle;

        try
        {
            var status = ArkUINativeApi.AddChild(parentHandle, _platformNode);
            if (status != 0)
            {
                HiLog.Error("HarmonyWeb", $"[node] AddChild failed: {status}");
                return false;
            }
            _attachedParent = parentHandle;
            HiLog.Info("HarmonyWeb", $"[node] web node 0x{_platformNode:X} attached to 0x{parentHandle:X}");
            return true;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"[node] attach failed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>把 MAUI 布局算出的尺寸写到 Web 节点上（NODE_WIDTH/HEIGHT，vp）。</summary>
    public unsafe void SetPlatformNodeSize(float widthVp, float heightVp)
    {
        if (_platformNode == IntPtr.Zero || widthVp <= 0 || heightVp <= 0)
            return;
        try
        {
            // 外部节点（BuilderNode 产出）没有 ArkUINodeBase 包装，直接用属性写入。
            var width = stackalloc ArkUI_NumberValue[1];
            width[0] = ArkUIValue.F(widthVp);
            var wItem = new ArkUI_AttributeItem { value = width, size = 1 };
            ArkUINativeApi.SetAttribute(_platformNode, ArkUI_NodeAttributeType.NODE_WIDTH, &wItem);

            var height = stackalloc ArkUI_NumberValue[1];
            height[0] = ArkUIValue.F(heightVp);
            var hItem = new ArkUI_AttributeItem { value = height, size = 1 };
            ArkUINativeApi.SetAttribute(_platformNode, ArkUI_NodeAttributeType.NODE_HEIGHT, &hItem);
        }
        catch (Exception ex)
        {
            HiLog.Warn("HarmonyWeb", $"[node] size sync failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    public void DetachPlatformNode()
    {
        if (_attachedParent == IntPtr.Zero || _platformNode == IntPtr.Zero)
            return;
        try
        {
            var status = ArkUINativeApi.RemoveChild(_attachedParent, _platformNode);
            if (status != 0)
                HiLog.Warn("HarmonyWeb", $"[node] RemoveChild failed: {status}");
        }
        catch (Exception ex)
        {
            HiLog.Warn("HarmonyWeb", $"[node] detach failed: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            _attachedParent = IntPtr.Zero;
        }
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

    internal void RaisePageEnded(string? url)
    {
        PageEnded?.Invoke(url ?? string.Empty);
    }

    /// <summary>返回 true 表示拦截导航（不加载）。</summary>
    internal bool RaiseNavigationStarting(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        var handlers = NavigationStarting;
        if (handlers is null)
            return false;
        foreach (var handler in handlers.GetInvocationList())
        {
            if (handler is Func<string, bool> func && func(url))
                return true;
        }
        return false;
    }

    public void Dispose()
    {
        if (_messageHandle.IsAllocated)
            _messageHandle.Free();
    }
}
