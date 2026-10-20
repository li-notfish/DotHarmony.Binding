#nullable enable
using System;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;
using HarmonyOS.Maui.Handlers;
using HarmonyOS.Maui.Hosting;
using HarmonyOS.Maui.Web;
using ArkStack = HarmonyOS.ArkUI.Stack;

namespace HarmonyOS.Maui.Blazor;

public sealed class HarmonyBlazorWebViewHandler : HarmonyViewHandler<HarmonyBlazorWebView, ArkStack>
{
    private HarmonyBlazorWebViewManager? _manager;
    private HarmonyRootComponentsCollection? _rootComponents;
    private IHarmonyWebViewSurface? _surface;
    private string? _pendingTag;

    public static PropertyMapper<HarmonyBlazorWebView, HarmonyBlazorWebViewHandler> Mapper =
        new(HarmonyViewMapper.Base)
        {
            [nameof(HarmonyBlazorWebView.HostPage)] = MapHostPage,
            [nameof(HarmonyBlazorWebView.RootComponents)] = MapRootComponents,
            [nameof(HarmonyBlazorWebView.StartPath)] = MapHostPage,
            // 继承 WebView 后 Source 可用：与 Blazor 导航联动（同源走 Router，异源交给 UrlLoading）
            [nameof(Microsoft.Maui.Controls.WebView.Source)] = MapSource,
        };

    public HarmonyBlazorWebViewHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView()
    {
        var container = new ArkStack();
        // 容器尺寸由 MAUI 布局驱动；Web 是外部 BuilderNode 节点，不会自动跟随，
        // 需在容器尺寸变化时显式同步（模式同 HarmonyContentViewHandler.OnSizeChange）。
        container.SizeChange += OnContainerSizeChange;
        return container;
    }

    private void OnContainerSizeChange(ArkUINodeEvent e)
    {
        if (e.SizeChangeWidth <= 0 || e.SizeChangeHeight <= 0)
            return;
        _surface?.SetPlatformNodeSize(e.SizeChangeWidth, e.SizeChangeHeight);
    }

    protected override void DisconnectHandler(ArkStack platformView)
    {
        HarmonyWebRuntime.SurfaceRegistered -= OnSurfaceRegistered;
        HarmonyWebRuntime.PlatformNodeBound -= OnPlatformNodeBound;
        _pendingTag = null;
        if (_surface is not null)
        {
            _surface.PageEnded -= OnPageEnded;
            _surface.PageStarted -= OnPageStarted;
            _surface.NavigationStarting -= OnNavigationStarting;
            _surface.DetachPlatformNode();
            _surface = null;
        }

        if (_manager is not null)
        {
            var manager = _manager;
            _manager = null;
            _ = manager.DisposeAsync().AsTask();
        }
        if (_rootComponents is not null)
            _rootComponents.CollectionChanged -= OnRootComponentsChanged;
        base.DisconnectHandler(platformView);
    }

    private static void MapHostPage(HarmonyBlazorWebViewHandler handler, HarmonyBlazorWebView view)
        => handler.StartWebViewCoreIfPossible();

    private static void MapRootComponents(HarmonyBlazorWebViewHandler handler, HarmonyBlazorWebView view)
        => handler.StartWebViewCoreIfPossible();

    /// <summary>
    /// Source 联动：Blazor 应用的导航由 Router 管理，直接改 WebView 的 URL 会绕过它。
    /// 因此这里只处理「应用内路径」——转成 Blazor 的 NavigationManager.NavigateTo；
    /// 绝对外部 URL 交给 UrlLoading 策略（与点击外链一致）。
    /// </summary>
    private static void MapSource(HarmonyBlazorWebViewHandler handler, HarmonyBlazorWebView view)
    {
        var source = view.Source;
        if (source is not UrlWebViewSource urlSource || string.IsNullOrWhiteSpace(urlSource.Url))
            return;

        var url = urlSource.Url!;
        if (!handler.StartWebViewCoreIfPossible(alreadyStarted: true))
            return;

        handler.NavigateViaBlazor(url);
    }

    private void NavigateViaBlazor(string url)
    {
        var manager = _manager;
        if (manager is null)
            return;

        // 相对路径或同源绝对 URL → Blazor 内部路由；其余交给导航策略
        var isAppRelative = url.StartsWith('/') || (!Uri.TryCreate(url, UriKind.Absolute, out _));
        if (isAppRelative)
        {
            _ = manager.NavigateViaNavigationManagerAsync(url);
            return;
        }

        HiLog.Info("HarmonyWeb", $"[blazor-handler] Source '{url}' is external; deferring to UrlLoading");
    }

    private bool StartWebViewCoreIfPossible(bool alreadyStarted = false)
    {
        if (alreadyStarted)
            return _manager is not null;
        if (_manager is not null || VirtualView is null)
            return _manager is not null;
        if (string.IsNullOrWhiteSpace(VirtualView.HostPage))
            return false;

        // 多实例：按实例 WebTag 取 surface；缺失时请求宿主按需创建（ArkTS 侧 BuilderNode）。
        // 创建是异步的，未就绪则挂到 surface 注册回调上等待。
        var tag = VirtualView.WebTag;
        _surface = HarmonyWebRuntime.EnsureSurface(tag);
        if (_surface is null)
        {
            HarmonyWebRuntime.SurfaceRegistered += OnSurfaceRegistered;
            _pendingTag = tag;
            HiLog.Info("HarmonyWeb", $"[blazor-handler] waiting for surface '{tag}'");
            return false;
        }

        var contentRoot = System.IO.Path.GetDirectoryName(VirtualView.HostPage) ?? "wwwroot";
        var hostPageRelativePath = System.IO.Path.GetFileName(VirtualView.HostPage);
        var fileProvider = VirtualView.CreateFileProvider(contentRoot);
        var services = MauiContext?.Services
            ?? throw new InvalidOperationException("MauiContext services are not available.");

        // 初始化前事件：允许宿主配置 UA
        var initializing = new HarmonyBlazorWebViewInitializingEventArgs();
        VirtualView.RaiseBlazorWebViewInitializing(initializing);
        if (!string.IsNullOrEmpty(initializing.UserAgent))
            _surface.SetCustomUserAgent(initializing.UserAgent);
        if (initializing.WebDebuggingAccess == true)
            _surface.SetWebDebuggingAccess(true);

        var blazorDispatcher = new HarmonyBlazorDispatcher(HarmonyDispatcher.Instance);
        VirtualView.AttachDispatchContext(blazorDispatcher, services);

        _manager = new HarmonyBlazorWebViewManager(
            _surface,
            services,
            blazorDispatcher,
            fileProvider,
            VirtualView.JSComponents,
            hostPageRelativePath,
            VirtualView)
        {
            StartUrl = VirtualView.StartPath,
        };

        _rootComponents = VirtualView.RootComponents;
        _rootComponents.CollectionChanged += OnRootComponentsChanged;

        // 启动确定性：主框架加载完成（PageEnded）即触发 Blazor.start，
        // __bwv 握手作为自确认；不再依赖 500ms 固定延迟。
        _surface.PageEnded += OnPageEnded;
        _surface.PageStarted += OnPageStarted;
        _surface.NavigationStarting += OnNavigationStarting;

        // 把 BuilderNode 的 Web 节点挂进 handler 的平台容器，使 Web 参与 MAUI 布局
        // （替代宿主页全屏覆盖）。宿主未提供节点时保持全屏模式，行为与之前一致。
        TryAttachToPlatformContainer();

        // 先注册完根组件再导航：否则 Router 尚未挂载，StartPath 会渲染成 "Page not found"。
        // 多实例下这个竞争必然发生（第二个 WebView 启动更慢），单实例只是碰巧躲过。
        _ = RegisterRootComponentsThenNavigateAsync();

        VirtualView.RaiseBlazorWebViewInitialized(new HarmonyBlazorWebViewInitializedEventArgs
        {
            WebView = _surface,
        });
        return true;
    }

    /// <summary>
    /// 把 Web 节点挂进 handler 的平台容器。节点由宿主异步绑定（factory 创建路径），
    /// 首次可能尚不可用；绑定完成后 <see cref="HarmonyWebRuntime.PlatformNodeBound"/> 会再次触发。
    /// </summary>
    private void TryAttachToPlatformContainer()
    {
        if (_surface is null || PlatformView is not ArkUINodeBase container)
            return;
        if (!_surface.TryAttachPlatformNode(container.Handle))
        {
            HiLog.Info("HarmonyWeb", $"[blazor-handler] node pending for tag '{VirtualView?.WebTag}'");
            HarmonyWebRuntime.PlatformNodeBound -= OnPlatformNodeBound;
            HarmonyWebRuntime.PlatformNodeBound += OnPlatformNodeBound;
            return;
        }
        HarmonyWebRuntime.PlatformNodeBound -= OnPlatformNodeBound;
        HiLog.Info("HarmonyWeb", $"[blazor-handler] web node attached (tag '{VirtualView?.WebTag}')");
    }

    private void OnPlatformNodeBound(string tag)
    {
        if (VirtualView is null || !string.Equals(tag, VirtualView.WebTag, StringComparison.Ordinal))
            return;
        TryAttachToPlatformContainer();
    }

    private async Task RegisterRootComponentsThenNavigateAsync()
    {
        var manager = _manager;
        var view = VirtualView;
        if (manager is null || view is null)
            return;

        try
        {
            foreach (var component in view.RootComponents)
                await component.AddToWebViewManagerAsync(manager).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"[blazor-handler] root component registration failed: {ex.GetType().Name}: {ex.Message}");
        }

        if (_manager == manager)
            _manager.Navigate(view.StartPath);
    }

    private void OnSurfaceRegistered(string tag)
    {
        if (_pendingTag is null || !string.Equals(tag, _pendingTag, StringComparison.Ordinal))
            return;
        HarmonyWebRuntime.SurfaceRegistered -= OnSurfaceRegistered;
        _pendingTag = null;
        // surface 就绪后继续初始化（回到本方法，此时 GetSurface 能取到）
        StartWebViewCoreIfPossible();
    }

    private void OnPageEnded(string url)
    {
        // 继承 WebView 后：把页面加载完成映射到 WebView.Navigated
        RaiseNavigated(url);
        UpdateHistoryState();
        if (_manager is null)
            return;
        _ = _manager.OnPageLoadedAsync(url);
    }

    private void OnPageStarted(string url) => RaiseNavigating(url);

    /// <summary>
    /// 把 ArkWeb 的历史状态回写到 WebView.CanGoBack/CanGoForward。
    /// 这两个属性是只读 BindableProperty（内部以 BindablePropertyKey 支撑），
    /// 平台层必须经 SetValue(key, ...) 更新，否则调用方始终读到默认 false。
    /// </summary>
    private void UpdateHistoryState()
    {
        if (VirtualView is not WebView webView || _surface is null)
            return;

        var canGoBack = _surface.CanGoBack;
        var canGoForward = _surface.CanGoForward;

        if (webView.CanGoBack != canGoBack)
            SetReadOnlyBindableProperty(webView, CanGoBackKey, canGoBack);
        if (webView.CanGoForward != canGoForward)
            SetReadOnlyBindableProperty(webView, CanGoForwardKey, canGoForward);
    }

    // CanGoBack/CanGoForward 是只读 BindableProperty：平台层只能经内部 BindablePropertyKey 写入。
    // 键是 internal 静态字段，首次使用时反射解析并缓存（AOT 下 Suppress trimming 已由 Unconditional 覆盖）。
    private static readonly BindablePropertyKey? CanGoBackKey =
        ResolvePropertyKey(typeof(WebView), "CanGoBackPropertyKey");

    private static readonly BindablePropertyKey? CanGoForwardKey =
        ResolvePropertyKey(typeof(WebView), "CanGoForwardPropertyKey");

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "WebView's key fields are part of the Controls assembly we always ship against.")]
    private static BindablePropertyKey? ResolvePropertyKey(Type type, string fieldName)
        => type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.GetValue(null) as BindablePropertyKey;

    private static void SetReadOnlyBindableProperty(BindableObject target, BindablePropertyKey? key, bool value)
    {
        if (key is null)
            return;
        target.SetValue(key, value);
    }

    private void RaiseNavigating(string url)
    {
        if (VirtualView is not WebView webView)
            return;
        var navigating = (Microsoft.Maui.Controls.IWebViewController?)webView;
        navigating?.SendNavigating(new WebNavigatingEventArgs(
            WebNavigationEvent.NewPage, new UrlWebViewSource { Url = url }, url));
    }

    private void RaiseNavigated(string url)
    {
        if (VirtualView is not WebView webView)
            return;
        var navigating = (Microsoft.Maui.Controls.IWebViewController?)webView;
        navigating?.SendNavigated(new WebNavigatedEventArgs(
            WebNavigationEvent.NewPage, new UrlWebViewSource { Url = url }, url, WebNavigationResult.Success));
    }

    private bool OnNavigationStarting(string url)
    {
        if (_manager is null || VirtualView is null)
            return false;
        return _manager.ShouldCancelNavigation(url, VirtualView);
    }

    private void OnRootComponentsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_manager is null)
            return;
        foreach (HarmonyRootComponent item in e.NewItems ?? Array.Empty<HarmonyRootComponent>())
            _ = item.AddToWebViewManagerAsync(_manager);
        foreach (HarmonyRootComponent item in e.OldItems ?? Array.Empty<HarmonyRootComponent>())
            _ = item.RemoveFromWebViewManagerAsync(_manager);
    }
}
