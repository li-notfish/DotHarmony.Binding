#nullable enable
using System.Threading.Tasks;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
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

    private IHarmonyWebViewSurface? Surface => HarmonyWebRuntime.Current;

    protected override ArkStack CreatePlatformView() => new();

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
        => _ = handler.Surface?.RunScriptAsync(args as string ?? string.Empty);
}
