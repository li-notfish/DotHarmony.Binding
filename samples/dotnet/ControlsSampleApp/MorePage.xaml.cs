using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public partial class MorePage : ContentPage
{
    public MorePage() => InitializeComponent();

    private void OnFlyoutClicked(object? sender, EventArgs e)
        => Window.Page = new NavigationPage(new FlyoutDemoPage());

    private void OnShellClicked(object? sender, EventArgs e)
        => Window.Page = new ShellDemoPage();
}
