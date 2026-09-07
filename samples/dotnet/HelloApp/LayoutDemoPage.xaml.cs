// LayoutDemoPage 的 code-behind：验证 MAUI 托管布局（Grid + AbsoluteLayout）
using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class LayoutDemoPage : ContentPage
{
    public LayoutDemoPage()
    {
        InitializeComponent();
    }

    private void OnPopClicked(object? sender, EventArgs e)
    {
        HarmonyOS.Maui.Hosting.HarmonyNavigation.Pop();
    }
}
