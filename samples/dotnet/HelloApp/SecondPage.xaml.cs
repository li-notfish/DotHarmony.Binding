using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class SecondPage : ContentPage
{
    private readonly int _visit;

    public SecondPage(int visit)
    {
        InitializeComponent();
        _visit = visit;
        VisitLabel.Text = $"visit #{_visit}";
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        HarmonyOS.Maui.Hosting.HarmonyNavigation.Pop();
    }
}
