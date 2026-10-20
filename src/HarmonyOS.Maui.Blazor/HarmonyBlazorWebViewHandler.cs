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

    private void StartWebViewCoreIfPossible()
    {
        if (_manager is not null || VirtualView is null)
            return;
        if (string.IsNullOrWhiteSpace(VirtualView.HostPage))
            return;

        // 多实例：按实例 WebTag 取 surface；缺失时请求宿主按需创建（ArkTS 侧 BuilderNode）。
        // 创建是异步的，未就绪则挂到 surface 注册回调上等待。
        var tag = VirtualView.WebTag;
        _surface = HarmonyWebRuntime.EnsureSurface(tag);
        if (_surface is null)
        {
            HarmonyWebRuntime.SurfaceRegistered += OnSurfaceRegistered;
            _pendingTag = tag;
            HiLog.Info("HarmonyWeb", $"[blazor-handler] waiting for surface '{tag}'");
            return;
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

        _manager = new HarmonyBlazorWebViewManager(
            _surface,
            services,
            new HarmonyBlazorDispatcher(HarmonyDispatcher.Instance),
            fileProvider,
            VirtualView.JSComponents,
            hostPageRelativePath,
            VirtualView);

        _rootComponents = VirtualView.RootComponents;
        _rootComponents.CollectionChanged += OnRootComponentsChanged;

        foreach (var component in _rootComponents)
            _ = component.AddToWebViewManagerAsync(_manager);

        // 启动确定性：主框架加载完成（PageEnded）即触发 Blazor.start，
        // __bwv 握手作为自确认；不再依赖 500ms 固定延迟。
        _surface.PageEnded += OnPageEnded;
        _surface.NavigationStarting += OnNavigationStarting;

        // 把 BuilderNode 的 Web 节点挂进 handler 的平台容器，使 Web 参与 MAUI 布局
        // （替代宿主页全屏覆盖）。宿主未提供节点时保持全屏模式，行为与之前一致。
        TryAttachToPlatformContainer();

        _manager.Navigate(VirtualView.StartPath);

        VirtualView.RaiseBlazorWebViewInitialized(new HarmonyBlazorWebViewInitializedEventArgs
        {
            WebView = _surface,
        });
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
        if (_manager is null)
            return;
        _ = _manager.OnPageLoadedAsync(url);
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
