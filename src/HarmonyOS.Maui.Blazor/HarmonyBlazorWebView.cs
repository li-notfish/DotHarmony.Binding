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
public sealed class HarmonyBlazorWebView : View
{
    private readonly JSComponentConfigurationStore _jsComponents = new();
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

    public IFileProvider CreateFileProvider(string contentRootDir)
        => new HarmonyBlazorFileProvider(contentRootDir);

    internal void RaiseBlazorWebViewInitializing(HarmonyBlazorWebViewInitializingEventArgs args)
        => BlazorWebViewInitializing?.Invoke(this, args);

    internal void RaiseBlazorWebViewInitialized(HarmonyBlazorWebViewInitializedEventArgs args)
        => BlazorWebViewInitialized?.Invoke(this, args);

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
