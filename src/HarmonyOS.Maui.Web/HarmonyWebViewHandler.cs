#nullable enable
using System;
using System.Threading.Tasks;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ArkStack = HarmonyOS.ArkUI.Stack;

namespace HarmonyOS.Maui.Web;

public sealed class HarmonyWebViewHandler : HarmonyViewHandler<WebView, ArkStack>
{
    public static PropertyMapper<WebView, HarmonyWebViewHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(WebView.Source)] = MapSource,
        [nameof(WebView.UserAgent)] = MapUserAgent,
    };

    public static CommandMapper<WebView, HarmonyWebViewHandler> Commands = new(ViewCommandMapper)
    {
        [nameof(Microsoft.Maui.IWebView.GoBack)] = MapGoBack,
        [nameof(Microsoft.Maui.IWebView.GoForward)] = MapGoForward,
        [nameof(Microsoft.Maui.IWebView.Reload)] = MapReload,
        [nameof(Microsoft.Maui.IWebView.Eval)] = MapEval,
        [nameof(Microsoft.Maui.IWebView.EvaluateJavaScriptAsync)] = MapEvaluateJavaScriptAsync,
    };

    public HarmonyWebViewHandler() : base(Mapper, Commands) { }

    private IHarmonyWebViewSurface? Surface => HarmonyWebRuntime.GetSurface(HarmonyWebRuntime.DefaultTag);

    protected override ArkStack CreatePlatformView() => new();

    protected override void ConnectHandler(ArkStack platformView)
    {
        base.ConnectHandler(platformView);

        // 官方契约：Controls.WebView.EvaluateJavaScriptAsync 经 IWebViewController.EvaluateJavaScriptRequested
        // （委托签名 Task<string>(string script)）下发给平台层；平台层必须订阅并返回真实结果。
        if (VirtualView is IWebViewController controller)
            controller.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
    }

    protected override void DisconnectHandler(ArkStack platformView)
    {
        if (VirtualView is IWebViewController controller)
            controller.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
        base.DisconnectHandler(platformView);
    }

    private Task<string> OnEvaluateJavaScriptRequested(string script)
    {
        var surface = Surface;
        if (surface is null)
            return Task.FromResult(string.Empty);
        return surface.RunScriptAsync(script);
    }

    public static void MapSource(HarmonyWebViewHandler handler, WebView view)
    {
        var url = view.Source switch
        {
            UrlWebViewSource urlSource => urlSource.Url ?? string.Empty,
            HtmlWebViewSource htmlSource => $"data:text/html;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(htmlSource.Html ?? string.Empty))}",
            _ => view.Source?.ToString() ?? string.Empty
        };
        _ = handler.Surface?.LoadUrlAsync(url);
    }

    public static void MapUserAgent(HarmonyWebViewHandler handler, WebView view)
    {
        if (view.UserAgent is { Length: > 0 } userAgent)
            // 原生 UA 通道：JS 改写 navigator.userAgent 既不转义（注入面）也会被 ArkWeb 内置规则覆盖
            handler.Surface?.SetCustomUserAgent(userAgent);
    }

    public static void MapGoBack(HarmonyWebViewHandler handler, WebView view, object? args)
        => _ = handler.Surface?.RunScriptAsync("history.back()");

    public static void MapGoForward(HarmonyWebViewHandler handler, WebView view, object? args)
        => _ = handler.Surface?.RunScriptAsync("history.forward()");

    public static void MapReload(HarmonyWebViewHandler handler, WebView view, object? args)
        => _ = handler.Surface?.RunScriptAsync("location.reload()");

    public static void MapEval(HarmonyWebViewHandler handler, WebView view, object? args)
        => _ = handler.Surface?.RunScriptAsync(args as string ?? string.Empty);

    public static void MapEvaluateJavaScriptAsync(HarmonyWebViewHandler handler, WebView view, object? args)
    {
        // 结果回传由 ConnectHandler 订阅的 EvaluateJavaScriptRequested 完成；
        // 到这里只需保证脚本被执行（Controls 侧同时会走事件通道，绝大多数情况下此处是 no-op）。
        if (args is string script)
            _ = handler.Surface?.RunScriptAsync(script);
    }
}
