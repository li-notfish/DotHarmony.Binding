using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public partial class FlyoutDemoPage : FlyoutPage
{
    public FlyoutDemoPage() => InitializeComponent();

    private void OnBackClicked(object? sender, EventArgs e)
        => Window.Page = new NavigationPage(new MainPage());
}
