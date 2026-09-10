// ControlsDemoPage 的 code-behind：展示新加的 10 种 MAUI 控件在鸿蒙上的渲染
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace HelloApp;

public partial class ControlsDemoPage : ContentPage
{
    public ControlsDemoPage()
    {
        InitializeComponent();
        TestPicker.ItemsSource = new List<string> { "苹果", "香蕉", "樱桃", "蜜桃" };
        TestPicker.SelectedIndex = 0;
        TestCollection.ItemsSource = new List<string> { "条目一", "条目二", "条目三", "条目四" };
        TestCarousel.ItemsSource = new List<string> { "第 1 页", "第 2 页", "第 3 页" };
        // 流式图片（1x1 红 PNG → MemoryStream → 落盘 file://）
        const string redPng = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";
        TestStreamImage.Source = Microsoft.Maui.Controls.ImageSource.FromStream(
            () => new MemoryStream(Convert.FromBase64String(redPng)));
        // RefreshView 演示：下拉触发后 1.5s 复位（经 PropertyChanged，不依赖 Controls 内部事件流）
        TestRefresh.PropertyChanged += async (sender, e) =>
        {
            if (e.PropertyName == nameof(RefreshView.IsRefreshing) && TestRefresh.IsRefreshing)
            {
                await Task.Delay(1500);
                TestRefresh.IsRefreshing = false;
            }
        };
    }

    private void OnPopClicked(object? sender, EventArgs e)
    {
        HarmonyOS.Maui.Hosting.HarmonyNavigation.Pop();
    }
}
