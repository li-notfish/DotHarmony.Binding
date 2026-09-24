using Microsoft.Maui.Controls;

namespace HarmonyMauiApp;

public partial class MainPage : ContentPage
{
    private int _count;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object? sender, EventArgs e)
    {
        _count++;
        CounterBtn.Text = $"Clicked {_count} time(s)";
    }
}
