using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class SecondPage : ContentPage
{
    private readonly int _visit;

    private int _appearing;
    private int _disappearing;

    public SecondPage(int visit)
    {
        InitializeComponent();
        _visit = visit;
        VisitLabel.Text = $"visit #{_visit}";
    }

    protected override void OnAppearing()
    {
        _appearing++;
        LifecycleLabel.Text = $"Second: {_appearing}A / {_disappearing}D";
    }

    protected override void OnDisappearing()
    {
        _disappearing++;
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        // NavigationPage 协议内返回（与 Navigation.PushAsync 配对）
        Navigation.PopAsync().FireAndForgetNavigation();
    }
}
