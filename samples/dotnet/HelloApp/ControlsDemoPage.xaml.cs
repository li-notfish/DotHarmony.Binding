// ControlsDemoPage 的 code-behind：展示新加的 10 种 MAUI 控件在鸿蒙上的渲染
using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class ControlsDemoPage : ContentPage
{
    public ControlsDemoPage()
    {
        InitializeComponent();
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
