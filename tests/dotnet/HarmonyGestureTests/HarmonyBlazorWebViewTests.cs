using HarmonyOS.Maui.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

public sealed class HarmonyBlazorWebViewTests
{
    [Fact]
    public void Constructor_InitializesDefaultProperties()
    {
        var view = new HarmonyBlazorWebView();

        Assert.Null(view.HostPage);
        Assert.Equal("/", view.StartPath);
        Assert.NotNull(view.RootComponents);
        Assert.NotNull(view.JSComponents);
    }

    [Fact]
    public void RootComponentsCollection_PreservesJSComponentStore()
    {
        var jsComponents = new Microsoft.AspNetCore.Components.Web.JSComponentConfigurationStore();
        var collection = new HarmonyRootComponentsCollection(jsComponents);

        Assert.Same(jsComponents, collection.JSComponents);
    }

    [Fact]
    public void MimeMap_ResolvesCommonWebAssets()
    {
        Assert.Equal("text/html", HarmonyBlazorMimeMap.Resolve("index.html"));
        Assert.Equal("text/javascript", HarmonyBlazorMimeMap.Resolve("app.js"));
        Assert.Equal("text/css", HarmonyBlazorMimeMap.Resolve("site.css"));
        Assert.Equal("application/json", HarmonyBlazorMimeMap.Resolve("data.json"));
        Assert.Equal("application/wasm", HarmonyBlazorMimeMap.Resolve("runtime.wasm"));
        Assert.Equal("application/octet-stream", HarmonyBlazorMimeMap.Resolve("blob.bin"));
        Assert.Equal("text/javascript", HarmonyBlazorMimeMap.Resolve("app.js?v=1"));
        Assert.Equal("text/html", HarmonyBlazorMimeMap.Resolve("index.html#start"));
    }

    [Fact]
    public void UseHarmonyBlazorWebView_RegistersHandler()
    {
        var services = new ServiceCollection();

        services.UseHarmonyBlazorWebView();

        Assert.NotNull(HarmonyOS.Maui.Handlers.HarmonyHandlerFactory
            .TryResolveRegistered(typeof(HarmonyBlazorWebView)));
        HarmonyOS.Maui.Handlers.HarmonyHandlerFactory.Unregister<HarmonyBlazorWebView>();
    }
}
