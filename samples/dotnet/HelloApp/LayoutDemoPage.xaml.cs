using System.Threading.Tasks;
// LayoutDemoPage 的 code-behind：验证 MAUI 托管布局
// （Grid + AbsoluteLayout + 单元格对齐 + ZIndex 叠放 + Auto 轨道内容自适应）
using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class LayoutDemoPage : ContentPage
{
    bool _blueRaised;

    public LayoutDemoPage()
    {
        InitializeComponent();
    }

    private void OnPopClicked(object? sender, EventArgs e)
    {
        // NavigationPage 协议内返回（与 Navigation.PushAsync 配对）
        Navigation.PopAsync().FireAndForgetNavigation();
    }

    private void OnToggleZIndex(object? sender, EventArgs e)
    {
        _blueRaised = !_blueRaised;
        ZBlue.ZIndex = _blueRaised ? 10 : 0;
        ZStatusLabel.Text = _blueRaised
            ? "blue ZIndex = 10 (over red edge)"
            : "blue ZIndex = 0 (under red edge)";
    }

    private void OnGrowText(object? sender, EventArgs e)
    {
        // Auto 行高度应随 Label 宽度增长而增大（AREA_CHANGE 触发重排）
        AutoLabel.Text += " + more text that keeps growing";
    }
}
