using HarmonyOS.Maui.Blazor;
using Microsoft.Maui.Controls;

namespace BlazorHybridApp;

/// <summary>
/// 内嵌模式演示：BlazorWebView 与原生 MAUI 控件同页混排。
///
/// 与官方 .NET MAUI 的 BlazorWebView 语义对齐——Web 参与 MAUI 布局（这里占 Grid 的 star 行），
/// 原生控件占据剩余空间并独立响应点击。实现路径：宿主 Index.ets 用 BuilderNode 把 Web 包成节点，
/// 经 OH_ArkUI_GetNodeHandleFromNapiValue 映射为 ArkUI_NodeHandle 后由 handler 挂进 MAUI 节点树。
/// </summary>
public sealed class BlazorApp : Application
{
    public BlazorApp()
    {
        var webView = new HarmonyBlazorWebView
        {
            HostPage = "wwwroot/index.html",
            StartPath = "/counter"
        };
        webView.RootComponents.Add(new HarmonyRootComponent
        {
            Selector = "#app",
            ComponentType = typeof(Routes)
        });

        // 原生 MAUI 控件：与 Web 并列于同一页面，验证布局分区与命中测试互不干扰
        var statusLabel = new Label
        {
            Text = "原生控件区（与 Blazor WebView 混排）",
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
        };
        var nativeButton = new Button
        {
            Text = "原生按钮（点击计数 0）",
            BackgroundColor = Color.FromArgb("#5B8DEF"),
            TextColor = Colors.White,
        };

        var clickCount = 0;
        nativeButton.Clicked += (_, _) =>
        {
            clickCount++;
            nativeButton.Text = $"原生按钮（点击计数 {clickCount}）";
            statusLabel.Text = $"原生控件区 · 已点击 {clickCount} 次（Web 区域不受影响）";
        };

        MainPage = new ContentPage
        {
            Content = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),   // Web 占据剩余空间
                    new RowDefinition(GridLength.Auto),   // 原生控件区
                },
                Children = { webView, statusLabel, nativeButton },
            },
        };
        Grid.SetRow(statusLabel, 1);
        Grid.SetRow(nativeButton, 1);
        statusLabel.Margin = new Thickness(0, 8, 0, 4);
        nativeButton.Margin = new Thickness(12, 0, 12, 12);
    }
}
