#nullable enable
using System;
using HarmonyOS.Bindings.NativeNode;
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

    public static PropertyMapper<HarmonyBlazorWebView, HarmonyBlazorWebViewHandler> Mapper =
        new(HarmonyViewMapper.Base)
        {
            [nameof(HarmonyBlazorWebView.HostPage)] = MapHostPage,
            [nameof(HarmonyBlazorWebView.RootComponents)] = MapRootComponents,
            [nameof(HarmonyBlazorWebView.StartPath)] = MapHostPage,
        };

    public HarmonyBlazorWebViewHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView() => new();

    protected override void DisconnectHandler(ArkStack platformView)
    {
        if (_surface is not null)
        {
            _surface.PageEnded -= OnPageEnded;
            _surface.NavigationStarting -= OnNavigationStarting;
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

        _surface = HarmonyWebRuntime.GetSurface(HarmonyWebRuntime.DefaultTag)
            ?? throw new InvalidOperationException(
                "Harmony WebView surface is not initialized. Ensure the host Index.ets passes WebviewController via passWebView.");

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

        _manager.Navigate(VirtualView.StartPath);

        VirtualView.RaiseBlazorWebViewInitialized(new HarmonyBlazorWebViewInitializedEventArgs
        {
            WebView = _surface,
        });
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
