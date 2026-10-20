#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Controls;

namespace HarmonyOS.Maui.Blazor;

/// <summary>
/// 鸿蒙 Blazor Hybrid 宿主视图。API 形状对齐官方 Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebView：
/// HostPage / StartPath / RootComponents / JSComponents / CreateFileProvider / UrlLoading /
/// BlazorWebViewInitializing / BlazorWebViewInitialized / AddRootComponentAsync / RemoveRootComponentAsync /
/// WebResourceRequested。
/// </summary>
[ContentProperty(nameof(RootComponents))]
public sealed class HarmonyBlazorWebView : WebView, IHarmonyBlazorWebView
{
    private readonly JSComponentConfigurationStore _jsComponents = new();
    // TryDispatchAsync 所需的调度上下文与 scoped services（由 handler 在管理器中转注入）
    private Microsoft.AspNetCore.Components.Dispatcher? _dispatchDispatcher;
    private IServiceProvider? _dispatchServices;
    private readonly HarmonyRootComponentsCollection _rootComponents;

    public static readonly BindableProperty HostPageProperty = BindableProperty.Create(
        nameof(HostPage), typeof(string), typeof(HarmonyBlazorWebView), null);

    /// <summary>官方同样只把 StartPath 声明为 BindableProperty（HostPage 在官方是普通属性）；
    /// 这里两者都提供 BindableProperty，以便 XAML 绑定与 PropertyMapper 变更通知。</summary>
    public static readonly BindableProperty StartPathProperty = BindableProperty.Create(
        nameof(StartPath), typeof(string), typeof(HarmonyBlazorWebView), "/");

    public HarmonyBlazorWebView()
    {
        _rootComponents = new HarmonyRootComponentsCollection(_jsComponents);
    }

    // ---- 官方对齐事件（实例级） ----

    /// <summary>WebView 初始化前触发：可配置 User-Agent / 背景色。</summary>
    public event EventHandler<HarmonyBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing;

    /// <summary>WebView 初始化完成后触发。</summary>
    public event EventHandler<HarmonyBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized;

    /// <summary>导航发生且需要决策时触发。未订阅时外部 URL 默认交给系统浏览器。</summary>
    public event EventHandler<HarmonyUrlLoadingEventArgs>? UrlLoading;

    /// <summary>Web 资源请求（scheme handler 命中前）触发；返回 true 表示已接管。</summary>
    public event EventHandler<HarmonyWebResourceRequestedEventArgs>? WebResourceRequested;

    public string? HostPage
    {
        get => (string?)GetValue(HostPageProperty);
        set => SetValue(HostPageProperty, value);
    }

    public static readonly BindableProperty WebTagProperty = BindableProperty.Create(
        nameof(WebTag), typeof(string), typeof(HarmonyBlazorWebView), HarmonyOS.Maui.Web.HarmonyWebRuntime.DefaultTag);

    public string StartPath
    {
        get => (string?)GetValue(StartPathProperty) ?? "/";
        set => SetValue(StartPathProperty, value);
    }

    /// <summary>
    /// 实例标识：对应宿主 ArkTS 侧的 webTag。宿主为每个 BlazorWebView 提供一个 BuilderNode 时，
    /// 用不同的 tag 区分各自的 surface（ArkWeb NDK API 按 tag 寻址）。默认 harmony-web。
    /// </summary>
    public string WebTag
    {
        get => (string?)GetValue(WebTagProperty) ?? HarmonyOS.Maui.Web.HarmonyWebRuntime.DefaultTag;
        set => SetValue(WebTagProperty, value);
    }

    public HarmonyRootComponentsCollection RootComponents => _rootComponents;

    public JSComponentConfigurationStore JSComponents => _jsComponents;

    /// <summary>
    /// 在 Blazor 的调度上下文里执行操作并传入 scoped services（对齐官方 TryDispatchAsync）。
    /// 供原生 UI 代码访问 NavigationManager 等 scoped 服务；
    /// 返回 false 表示 WebView 尚未初始化到可分发状态。
    /// </summary>
    public Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        var services = _dispatchServices;
        var dispatcher = _dispatchDispatcher;
        if (services is null || dispatcher is null)
            return Task.FromResult(false);

        return ExecuteAsync(dispatcher, services, workItem);
    }

    private static async Task<bool> ExecuteAsync(
        Microsoft.AspNetCore.Components.Dispatcher dispatcher,
        IServiceProvider services,
        Action<IServiceProvider> workItem)
    {
        await dispatcher.InvokeAsync(() => workItem(services)).ConfigureAwait(false);
        return true;
    }

    /// <summary>动态添加根组件（等价官方 AddRootComponentAsync）。</summary>
    public Task AddRootComponentAsync(Type componentType, string selector, IDictionary<string, object?>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);

        var component = new HarmonyRootComponent
        {
            ComponentType = componentType,
            Selector = selector,
            Parameters = parameters,
        };
        _rootComponents.Add(component);
        return Task.CompletedTask;
    }

    /// <summary>动态移除根组件（等价官方 RemoveRootComponentAsync）。</summary>
    public Task RemoveRootComponentAsync(string selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);

        for (var i = _rootComponents.Count - 1; i >= 0; i--)
        {
            if (string.Equals(_rootComponents[i].Selector, selector, StringComparison.Ordinal))
                _rootComponents.RemoveAt(i);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 为本地静态内容返回 Cache-Control 头（对齐官方 .NET 10 的 StaticContentCacheControlProvider）。
    /// 返回 null/空表示保持默认 no-store。仅对本地（app origin）资源生效。
    /// </summary>
    public Func<HarmonyStaticContentRequest, string?>? StaticContentCacheControlProvider { get; set; }

    public IFileProvider CreateFileProvider(string contentRootDir)
        => new HarmonyBlazorFileProvider(contentRootDir);

    // ---- IHarmonyBlazorWebView 显式实现 ----
    // 接口方法名与事件同名，只能显式实现；对外仍以事件形式订阅。

    void IHarmonyBlazorWebView.UrlLoading(HarmonyUrlLoadingEventArgs args)
        => UrlLoading?.Invoke(this, args);

    void IHarmonyBlazorWebView.BlazorWebViewInitializing(HarmonyBlazorWebViewInitializingEventArgs args)
        => BlazorWebViewInitializing?.Invoke(this, args);

    void IHarmonyBlazorWebView.BlazorWebViewInitialized(HarmonyBlazorWebViewInitializedEventArgs args)
        => BlazorWebViewInitialized?.Invoke(this, args);

    internal void RaiseBlazorWebViewInitializing(HarmonyBlazorWebViewInitializingEventArgs args)
        => BlazorWebViewInitializing?.Invoke(this, args);

    internal void RaiseBlazorWebViewInitialized(HarmonyBlazorWebViewInitializedEventArgs args)
        => BlazorWebViewInitialized?.Invoke(this, args);

    /// <summary>由 handler 在创建 WebViewManager 后调用，供 TryDispatchAsync 使用。</summary>
    internal void AttachDispatchContext(
        Microsoft.AspNetCore.Components.Dispatcher dispatcher,
        IServiceProvider services)
    {
        _dispatchDispatcher = dispatcher;
        _dispatchServices = services;
    }

    internal void RaiseUrlLoading(HarmonyUrlLoadingEventArgs args)
        => UrlLoading?.Invoke(this, args);

    internal bool RaiseWebResourceRequested(HarmonyWebResourceRequestedEventArgs args)
    {
        var handler = WebResourceRequested;
        if (handler is null)
            return false;
        handler(this, args);
        return args.Handled;
    }
}

/// <summary>本地静态内容请求（对齐官方 BlazorWebViewStaticContentRequest）。</summary>
public sealed class HarmonyStaticContentRequest
{
    public HarmonyStaticContentRequest(Uri uri, string contentType)
    {
        ArgumentNullException.ThrowIfNull(uri);
        Uri = uri;
        ContentType = contentType;
    }

    /// <summary>请求的绝对 URI。</summary>
    public Uri Uri { get; }

    /// <summary>已解析的内容类型。</summary>
    public string ContentType { get; }
}

/// <summary>WebResourceRequested 事件参数。</summary>
public sealed class HarmonyWebResourceRequestedEventArgs : EventArgs
{
    public HarmonyWebResourceRequestedEventArgs(HarmonyOS.Maui.Web.HarmonyWebRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Request = request;
    }

    public HarmonyOS.Maui.Web.HarmonyWebRequest Request { get; }

    /// <summary>处理器可写：设为 true 并赋值 Response 表示已接管该请求。</summary>
    public bool Handled { get; set; }

    public HarmonyOS.Maui.Web.HarmonyWebResponse? Response { get; set; }
}
