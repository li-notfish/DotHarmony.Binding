using Microsoft.Maui.Controls;
using WeatherTwentyOne.ViewModels;

namespace WeatherTwentyOne;

public partial class WeatherPage : ContentPage
{
    private readonly WeatherViewModel _viewModel = new();

    public WeatherPage()
    {
        InitializeComponent();
        BindingContext = _viewModel;
    }

    private void OnRefreshClicked(object? sender, EventArgs e)
    {
        _viewModel.Refresh();
    }
}
