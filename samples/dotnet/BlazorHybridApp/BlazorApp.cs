using HarmonyOS.Maui.Blazor;
using HarmonyOS.Maui.Web;
using Microsoft.Maui.Controls;

namespace BlazorHybridApp;

/// <summary>
/// 内嵌 + 多实例演示。
///
/// 内嵌：BlazorWebView 参与 MAUI 布局（Grid star 行），原生控件占 Auto 行——与官方 BlazorWebView 语义一致。
/// 多实例：每个实例一个 WebTag，对应宿主侧一个 BuilderNode；宿主对未知 tag 按需创建。
/// 实现路径：宿主 Index.ets 用 BuilderNode 把 Web 包成节点，getFrameNode() 经
/// OH_ArkUI_GetNodeHandleFromNapiValue 映射为 ArkUI_NodeHandle，由 handler 挂进 MAUI 节点树。
/// </summary>
public sealed class BlazorApp : Application
{
    public BlazorApp()
    {
        var webView = CreateBlazorWebView(HarmonyWebRuntime.DefaultTag);
        var statusLabel = new Label
        {
            Text = "原生控件区（与 Blazor WebView 混排）",
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
        };

        MainPage = new ContentPage
        {
            Content = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto),
                },
                Children = { webView, statusLabel },
            },
        };
        Grid.SetRow(statusLabel, 1);
    }

    /// <summary>
    /// 构造一个 BlazorWebView 实例。<paramref name="webTag"/> 对应宿主侧一个 BuilderNode；
    /// 宿主对未知 tag 会经 registerWebViewFactory 按需创建。
    /// </summary>
    public static HarmonyBlazorWebView CreateBlazorWebView(string webTag)
    {
        var view = new HarmonyBlazorWebView
        {
            HostPage = "wwwroot/index.html",
            StartPath = "/counter",
            WebTag = webTag,
        };
        view.RootComponents.Add(new HarmonyRootComponent
        {
            Selector = "#app",
            ComponentType = typeof(Routes),
        });
        return view;
    }
}
