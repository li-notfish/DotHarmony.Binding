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

        DemoCollection.SelectionChanged += (_, e) =>
        {
            var selected = e.CurrentSelection.FirstOrDefault() as string;
            if (selected is null)
                return;

            CollectionStatus.Text = $"Selected: {selected}";
            ActivityLog.Record($"Items: selected “{selected}”");
        };

        DemoCarousel.PositionChanged += (_, e) =>
            CarouselStatus.Text = $"Position: {e.CurrentPosition}";

        DemoList.ItemTapped += (_, e) =>
        {
            ListStatus.Text = $"Tapped: {e.Item}";
            ActivityLog.Record($"Items: row “{e.Item}” tapped");
            DemoList.SelectedItem = null;
        };
    }
}
