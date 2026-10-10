using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public partial class ShellDemoPage : Shell
{
    public ShellDemoPage() => InitializeComponent();

    private void OnBackClicked(object? sender, EventArgs e)
        => Window.Page = new NavigationPage(new MainPage());
}
