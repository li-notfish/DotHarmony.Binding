using HarmonyOS.Maui.Blazor;
using HarmonyOS.Maui.Web;
using Microsoft.Maui.Controls;

namespace BlazorHybridApp;

/// <summary>
/// 多实例验证页（临时探针）：同一页面并排放两个 BlazorWebView，各自不同 WebTag。
/// 宿主对第二个 tag 会经 registerWebViewFactory 按需创建 BuilderNode。
/// </summary>
public sealed class MultiInstancePage : ContentPage
{
    public MultiInstancePage()
    {
        var first = BlazorApp.CreateBlazorWebView(HarmonyWebRuntime.DefaultTag);
        var second = BlazorApp.CreateBlazorWebView("harmony-web-2");

        var label1 = new Label { Text = "实例 A (harmony-web)", FontSize = 12, HorizontalTextAlignment = TextAlignment.Center };
        var label2 = new Label { Text = "实例 B (harmony-web-2)", FontSize = 12, HorizontalTextAlignment = TextAlignment.Center };

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            },
            Children = { first, label1, second, label2 },
        };
        Grid.SetRow(first, 0);
        Grid.SetRow(label1, 1);
        Grid.SetRow(second, 2);
        Grid.SetRow(label2, 3);
    }
}
