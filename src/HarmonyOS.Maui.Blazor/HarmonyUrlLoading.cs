#nullable enable
using System;

namespace HarmonyOS.Maui.Blazor;

/// <summary>
/// 导航策略，取值与官方 Microsoft.AspNetCore.Components.WebView.UrlLoadingStrategy 对齐
/// （转储自 Microsoft.AspNetCore.Components.WebView.Maui 10.0.20：OpenExternally, OpenInWebView, CancelLoad）。
/// </summary>
public enum HarmonyUrlLoadingStrategy
{
    /// <summary>取消本次导航（等价官方 CancelLoad）。</summary>
    CancelLoad = 0,

    /// <summary>在 WebView 内加载（等价官方 OpenInWebView）。</summary>
    OpenInWebView = 1,

    /// <summary>交给系统浏览器打开（等价官方 OpenExternally）。</summary>
    OpenExternally = 2,
}

/// <summary>UrlLoading 事件参数（形状对齐官方 UrlLoadingEventArgs: Url + UrlLoadingStrategy）。</summary>
public sealed class HarmonyUrlLoadingEventArgs : EventArgs
{
    public HarmonyUrlLoadingEventArgs(Uri url, HarmonyUrlLoadingStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(url);
        Url = url;
        UrlLoadingStrategy = strategy;
    }

    public Uri Url { get; }

    /// <summary>处理器可写：决定该导航如何处理。默认值由触发方给出。</summary>
    public HarmonyUrlLoadingStrategy UrlLoadingStrategy { get; set; }
}

/// <summary>
/// BlazorWebView 初始化前事件参数：宿主可在此完成 Web 引擎级配置。
/// 对应官方事件在 WKWebView/CoreWebView2 上暴露的配置对象；鸿蒙侧按 ArkWeb 可配项对齐。
/// </summary>
public sealed class HarmonyBlazorWebViewInitializingEventArgs : EventArgs
{
    /// <summary>自定义 User-Agent；null 表示沿用默认。</summary>
    public string? UserAgent { get; set; }

    /// <summary>背景色（ARGB），null 表示不设置。</summary>
    public uint? BackgroundColor { get; set; }

    /// <summary>是否启用 DOM 存储（对应 ArkWeb domStorageAccess）。null 表示沿用默认。</summary>
    public bool? DomStorageAccess { get; set; }

    /// <summary>是否启用文件访问（对应 ArkWeb fileAccess）。null 表示沿用默认。</summary>
    public bool? FileAccess { get; set; }

    /// <summary>
    /// 是否开启 ArkWeb 调试（Chrome DevTools 可连）。仅开发期使用，默认关闭。
    /// </summary>
    public bool? WebDebuggingAccess { get; set; }

    /// <summary>
    /// 是否允许混合内容（对应 ArkWeb mixedMode）。
    /// null 表示沿用默认；true 允许 https 页面加载 http 资源。
    /// </summary>
    public bool? AllowMixedContent { get; set; }
}

/// <summary>BlazorWebView 初始化完成事件参数。</summary>
public sealed class HarmonyBlazorWebViewInitializedEventArgs : EventArgs
{
    /// <summary>平台 WebView 控制器（可用作进阶互通）。null 表示宿主未暴露。</summary>
    public object? WebView { get; init; }
}
