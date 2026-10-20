#nullable enable
using System;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;

namespace HarmonyOS.Maui.Blazor;

/// <summary>
/// Blazor Hybrid 宿主视图的契约，形状对齐官方
/// Microsoft.AspNetCore.Components.WebView.Maui.IBlazorWebView。
///
/// 面向组件库/框架代码：第三方只需要依赖接口即可完成根组件注册、URL 策略接管等，
/// 而不必直接引用具体控件类型。
/// </summary>
public interface IHarmonyBlazorWebView
{
    /// <summary>Blazor 应用入口页（相对宿内容根，如 "wwwroot/index.html"）。</summary>
    string? HostPage { get; }

    /// <summary>Blazor 路由上下文中的初始导航路径。</summary>
    string StartPath { get; set; }

    /// <summary>根组件集合（集合本身可观察，增删会同步到 WebViewManager）。</summary>
    HarmonyRootComponentsCollection RootComponents { get; }

    /// <summary>注册为 JS 组件的配置存储。</summary>
    JSComponentConfigurationStore JSComponents { get; }

    /// <summary>为指定内容根创建静态文件提供者。</summary>
    IFileProvider CreateFileProvider(string contentRootDir);

    /// <summary>导航决策回调（等价官方 IBlazorWebView.UrlLoading）。</summary>
    void UrlLoading(HarmonyUrlLoadingEventArgs args);

    /// <summary>WebView 初始化前回调（等价官方 IBlazorWebView.BlazorWebViewInitializing）。</summary>
    void BlazorWebViewInitializing(HarmonyBlazorWebViewInitializingEventArgs args);

    /// <summary>WebView 初始化完成回调（等价官方 IBlazorWebView.BlazorWebViewInitialized）。</summary>
    void BlazorWebViewInitialized(HarmonyBlazorWebViewInitializedEventArgs args);
}
