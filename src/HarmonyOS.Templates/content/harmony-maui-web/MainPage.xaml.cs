using Microsoft.Maui.Controls;

namespace HarmonyWebApp1;

public partial class MainPage : ContentPage
{
    private int _clicks;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnTapClicked(object? sender, EventArgs e)
    {
        _clicks++;
        TapButton.Text = $"Clicked {_clicks}x";
    }
}
