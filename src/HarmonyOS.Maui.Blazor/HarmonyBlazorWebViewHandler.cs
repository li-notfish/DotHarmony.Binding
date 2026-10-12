#nullable enable
using System;
using System.Threading.Tasks;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Maui.Handlers;
using HarmonyOS.Maui.Hosting;
using HarmonyOS.Maui.Web;
using HarmonyOS.Interop;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Dispatching;
using ArkStack = HarmonyOS.ArkUI.Stack;

namespace HarmonyOS.Maui.Blazor;

public sealed class HarmonyBlazorWebViewHandler : HarmonyViewHandler<HarmonyBlazorWebView, ArkStack>
{
    private HarmonyBlazorWebViewManager? _manager;
    private HarmonyRootComponentsCollection? _rootComponents;

    public static PropertyMapper<HarmonyBlazorWebView, HarmonyBlazorWebViewHandler> Mapper =
        new(HarmonyViewMapper.Base)
        {
            [nameof(HarmonyBlazorWebView.HostPage)] = MapHostPage,
            [nameof(HarmonyBlazorWebView.RootComponents)] = MapRootComponents,
        };

    public HarmonyBlazorWebViewHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView() => new();

    protected override void DisconnectHandler(ArkStack platformView)
    {
        if (_manager is not null)
        {
            _ = _manager.DisposeAsync().AsTask();
            _manager = null;
        }
        if (_rootComponents is not null)
            _rootComponents.CollectionChanged -= OnRootComponentsChanged;
        base.DisconnectHandler(platformView);
    }

    private static void MapHostPage(HarmonyBlazorWebViewHandler handler, HarmonyBlazorWebView view)
    {
        handler.StartWebViewCoreIfPossible();
    }

    private static void MapRootComponents(HarmonyBlazorWebViewHandler handler, HarmonyBlazorWebView view)
    {
        handler.StartWebViewCoreIfPossible();
    }

    private void StartWebViewCoreIfPossible()
    {
        if (_manager is not null || VirtualView is null)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(VirtualView.HostPage))
            return;
        if (HarmonyWebRuntime.Current is null)
            throw new InvalidOperationException("Harmony WebView surface is not initialized.");

        var contentRoot = System.IO.Path.GetDirectoryName(VirtualView.HostPage) ?? "wwwroot";
        var hostPageRelativePath = System.IO.Path.GetFileName(VirtualView.HostPage);
        var fileProvider = VirtualView.CreateFileProvider(contentRoot);
        var services = MauiContext?.Services
            ?? throw new InvalidOperationException("MauiContext services are not available.");
        var dispatcher = HarmonyDispatcher.Instance;
        _manager = new HarmonyBlazorWebViewManager(
            HarmonyWebRuntime.Current,
            services,
            new HarmonyBlazorDispatcher(dispatcher),
            fileProvider,
            VirtualView.JSComponents,
            hostPageRelativePath);

        _rootComponents = VirtualView.RootComponents;
        _rootComponents.CollectionChanged += OnRootComponentsChanged;

        foreach (var component in _rootComponents)
            _ = component.AddToWebViewManagerAsync(_manager);

        _manager.Navigate(VirtualView.StartPath);
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
