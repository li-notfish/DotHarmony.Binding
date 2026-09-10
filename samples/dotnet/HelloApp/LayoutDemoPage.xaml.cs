using System.Threading.Tasks;
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
        // NavigationPage 协议内返回（与 Navigation.PushAsync 配对）
        Navigation.PopAsync().FireAndForgetNavigation();
    }
}
