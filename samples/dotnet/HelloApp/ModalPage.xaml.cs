using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class ModalPage : ContentPage
{
    private int _appearing;
    private int _disappearing;

    public ModalPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        _appearing++;
        LifecycleLabel.Text = $"Modal: {_appearing}A / {_disappearing}D";
    }

    protected override void OnDisappearing()
    {
        _disappearing++;
        LifecycleLabel.Text = $"Modal: {_appearing}A / {_disappearing}D";
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        Navigation.PopModalAsync().FireAndForgetNavigation();
    }
}
