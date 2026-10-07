using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public partial class LayoutsPage : ContentPage
{
    public LayoutsPage()
    {
        InitializeComponent();
        DemoRefresh.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(RefreshView.IsRefreshing) && DemoRefresh.IsRefreshing)
            {
                await Task.Delay(500);
                DemoRefresh.IsRefreshing = false;
            }
        };
    }
}
