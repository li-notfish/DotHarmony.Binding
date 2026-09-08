// ControlsDemoPage 的 code-behind：展示新加的 10 种 MAUI 控件在鸿蒙上的渲染
using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class ControlsDemoPage : ContentPage
{
    public ControlsDemoPage()
    {
        InitializeComponent();
    }

    private void OnPopClicked(object? sender, EventArgs e)
    {
        HarmonyOS.Maui.Hosting.HarmonyNavigation.Pop();
    }
}
