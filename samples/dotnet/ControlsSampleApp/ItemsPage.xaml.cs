using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public partial class ItemsPage : ContentPage
{
    public ItemsPage()
    {
        InitializeComponent();
        var items = new[] { "One", "Two", "Three", "Four", "Five" };
        DemoCollection.ItemsSource = items;
        DemoCarousel.ItemsSource = items;
        DemoIndicator.ItemsSource = items;
        DemoList.ItemsSource = items;
    }
}
