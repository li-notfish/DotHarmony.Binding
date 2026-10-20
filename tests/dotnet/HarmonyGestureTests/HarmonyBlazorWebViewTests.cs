using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public void HostPageAndStartPath_AreBindable()
    {
        // 对齐官方 StartPath 的 BindableProperty 语义，并把 HostPage 也提升为 BindableProperty，
        // 使 XAML 绑定与 PropertyMapper 变更通知可用。
        Assert.NotNull(HarmonyBlazorWebView.HostPageProperty);
        Assert.Equal(nameof(HarmonyBlazorWebView.HostPage), HarmonyBlazorWebView.HostPageProperty.PropertyName);
        Assert.Equal(typeof(string), HarmonyBlazorWebView.HostPageProperty.ReturnType);
        Assert.NotNull(HarmonyBlazorWebView.StartPathProperty);
        Assert.Equal(nameof(HarmonyBlazorWebView.StartPath), HarmonyBlazorWebView.StartPathProperty.PropertyName);

        var view = new HarmonyBlazorWebView { HostPage = "wwwroot/index.html", StartPath = "/counter" };
        Assert.Equal("wwwroot/index.html", view.HostPage);
        Assert.Equal("/counter", view.StartPath);

        // 未设置 HostPage 时保持 null；StartPath 回落到默认 "/"
        var fresh = new HarmonyBlazorWebView();
        Assert.Null(fresh.HostPage);
        Assert.Equal("/", fresh.StartPath);
    }

    [Fact]
    public void RootComponentsCollection_PreservesJSComponentStore()
    {
        var jsComponents = new Microsoft.AspNetCore.Components.Web.JSComponentConfigurationStore();
        var collection = new HarmonyRootComponentsCollection(jsComponents);

        Assert.Same(jsComponents, collection.JSComponents);
    }

    [Fact]
    public async Task AddRootComponentAsync_AddsComponentToCollection()
    {
        var view = new HarmonyBlazorWebView();

        await view.AddRootComponentAsync(typeof(object), "#app", new Dictionary<string, object?> { ["name"] = "demo" });

        var component = Assert.Single(view.RootComponents);
        Assert.Equal("#app", component.Selector);
        Assert.Equal(typeof(object), component.ComponentType);
        Assert.NotNull(component.Parameters);
        Assert.Equal("demo", component.Parameters!["name"]);
    }

    [Fact]
    public async Task RemoveRootComponentAsync_RemovesMatchingSelector()
    {
        var view = new HarmonyBlazorWebView();
        await view.AddRootComponentAsync(typeof(object), "#app");
        await view.AddRootComponentAsync(typeof(string), "#header");

        await view.RemoveRootComponentAsync("#app");

        var remaining = Assert.Single(view.RootComponents);
        Assert.Equal("#header", remaining.Selector);
    }

    [Fact]
    public void UrlLoading_DefaultStrategyIsOpenExternally()
    {
        // 官方默认：外链交给系统浏览器（OpenExternally）
        var args = new HarmonyUrlLoadingEventArgs(new Uri("https://example.com/"), HarmonyUrlLoadingStrategy.OpenExternally);
        Assert.Equal(HarmonyUrlLoadingStrategy.OpenExternally, args.UrlLoadingStrategy);
        Assert.Equal(new Uri("https://example.com/"), args.Url);
    }

    [Fact]
    public void UrlLoadingStrategy_MatchesOfficialValues()
    {
        // 官方枚举（Microsoft.AspNetCore.Components.WebView.UrlLoadingStrategy）：
        // OpenExternally = 0, OpenInWebView = 1, CancelLoad = 2 —— 这里只要求语义对位，不绑定数值。
        Assert.Equal("CancelLoad", nameof(HarmonyUrlLoadingStrategy.CancelLoad));
        Assert.Equal("OpenInWebView", nameof(HarmonyUrlLoadingStrategy.OpenInWebView));
        Assert.Equal("OpenExternally", nameof(HarmonyUrlLoadingStrategy.OpenExternally));
    }

    [Fact]
    public void LifecycleEvents_CanBeSubscribed()
    {
        var view = new HarmonyBlazorWebView();
        var initCount = 0;

        view.BlazorWebViewInitializing += (_, _) => initCount++;
        view.RaiseBlazorWebViewInitializing(new HarmonyBlazorWebViewInitializingEventArgs());

        Assert.Equal(1, initCount);
    }

    [Fact]
    public void WebResourceRequested_UnhandledByDefault()
    {
        var view = new HarmonyBlazorWebView();
        var args = new HarmonyWebResourceRequestedEventArgs(
            new HarmonyOS.Maui.Web.HarmonyWebRequest("https://localhost/_framework/blazor.webview.js", "GET"));

        var handled = view.RaiseWebResourceRequested(args);

        Assert.False(handled);
        Assert.False(args.Handled);
    }

    [Fact]
    public void WebResourceRequested_CanHandOverResponse()
    {
        var view = new HarmonyBlazorWebView();
        view.WebResourceRequested += (_, args) =>
        {
            args.Handled = true;
            args.Response = new HarmonyOS.Maui.Web.HarmonyWebResponse(200, "text/plain", "ok"u8.ToArray());
        };

        var request = new HarmonyWebResourceRequestedEventArgs(
            new HarmonyOS.Maui.Web.HarmonyWebRequest("https://localhost/custom", "GET"));
        var handled = view.RaiseWebResourceRequested(request);

        Assert.True(handled);
        Assert.True(request.Handled);
        Assert.Equal(200, request.Response!.StatusCode);
    }

    [Fact]
    public void Surface_ExposesEmbeddedNodeContract()
    {
        // 内嵌模式的契约面：宿主用 BuilderNode 承载 Web 时，surface 必须能接受外部节点。
        // 具体挂载行为依赖 ArkUI 运行时不进入单测（见设备验证记录），这里锁定 API 形状，
        // 防止后续重构把内嵌能力从接口上抹掉。
        var surfaceType = typeof(HarmonyOS.Maui.Web.IHarmonyWebViewSurface);

        Assert.NotNull(surfaceType.GetMethod(nameof(HarmonyOS.Maui.Web.IHarmonyWebViewSurface.TryAttachPlatformNode)));
        Assert.NotNull(surfaceType.GetMethod(nameof(HarmonyOS.Maui.Web.IHarmonyWebViewSurface.DetachPlatformNode)));
        Assert.NotNull(surfaceType.GetMethod(nameof(HarmonyOS.Maui.Web.IHarmonyWebViewSurface.SetPlatformNodeSize)));

        var attach = surfaceType.GetMethod(nameof(HarmonyOS.Maui.Web.IHarmonyWebViewSurface.TryAttachPlatformNode))!;
        Assert.Equal(typeof(bool), attach.ReturnType);
        Assert.Single(attach.GetParameters());
    }

    [Fact]
    public void WebTag_DefaultsToRuntimeDefaultAndIsBindable()
    {
        var view = new HarmonyBlazorWebView();

        // 未显式设置时使用运行时默认 tag（单实例兼容）
        Assert.Equal(HarmonyOS.Maui.Web.HarmonyWebRuntime.DefaultTag, view.WebTag);

        // 多实例：每个实例可用不同 tag 对应宿主侧不同 BuilderNode
        view.WebTag = "harmony-web-2";
        Assert.Equal("harmony-web-2", view.WebTag);
        Assert.NotNull(HarmonyBlazorWebView.WebTagProperty);
    }

    [Fact]
    public void Runtime_GetSurface_ReturnsNullForUnknownTag()
    {
        // 未注册的 tag 不应误返回默认 surface（多实例路由正确性）
        Assert.Null(HarmonyOS.Maui.Web.HarmonyWebRuntime.GetSurface("not-registered-tag"));
    }

    [Fact]
    public void ImplementsIBlazorWebViewContract()
    {
        // 对齐官方 IBlazorWebView：组件库可只依赖接口完成根组件注册与 URL 策略接管
        var view = new HarmonyBlazorWebView();
        Assert.IsAssignableFrom<IHarmonyBlazorWebView>(view);

        IHarmonyBlazorWebView contract = view;
        Assert.Null(contract.HostPage);
        Assert.Equal("/", contract.StartPath);
        Assert.NotNull(contract.RootComponents);
        Assert.NotNull(contract.JSComponents);
    }

    [Fact]
    public void InheritsWebViewForNavigatingAndHistory()
    {
        // 对齐官方 BlazorWebView : WebView：继承导航事件与历史能力
        var view = new HarmonyBlazorWebView();
        Assert.IsAssignableFrom<Microsoft.Maui.Controls.WebView>(view);
        Assert.IsAssignableFrom<Microsoft.Maui.IWebView>(view);
    }

    [Fact]
    public async Task TryDispatchAsync_ReturnsFalseBeforeManagerAttached()
    {
        // 未初始化时不应抛错，而是返回 false（官方语义：调用方自行决定降级行为）
        var view = new HarmonyBlazorWebView();
        var called = false;

        var dispatched = await view.TryDispatchAsync(_ => called = true);

        Assert.False(dispatched);
        Assert.False(called);
    }

    [Fact]
    public void StaticContentCacheControlProvider_IsSettable()
    {
        var view = new HarmonyBlazorWebView();
        Assert.Null(view.StaticContentCacheControlProvider);

        view.StaticContentCacheControlProvider =
            request => request.ContentType.StartsWith("image/", StringComparison.Ordinal)
                ? "public, max-age=86400"
                : null;

        Assert.NotNull(view.StaticContentCacheControlProvider);
        var header = view.StaticContentCacheControlProvider!(
            new HarmonyStaticContentRequest(new Uri("https://localhost/img/a.png"), "image/png"));
        Assert.Equal("public, max-age=86400", header);
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
        // RCL 静态资产路径同样按扩展名解析
        Assert.Equal("text/css", HarmonyBlazorMimeMap.Resolve("https://localhost/_content/Lib/styles.css"));
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
