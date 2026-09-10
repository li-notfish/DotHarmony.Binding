using System.Threading.Tasks;
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
        // NavigationPage 协议内返回（与 Navigation.PushAsync 配对）
        Navigation.PopAsync().FireAndForgetNavigation();
    }
}
