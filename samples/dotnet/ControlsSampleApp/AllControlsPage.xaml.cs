using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlsSampleApp;

public partial class AllControlsPage : ContentPage
{
    public AllControlsPage()
    {
        InitializeComponent();

        DemoPicker.ItemsSource = new[] { "Alpha", "Beta", "Gamma" };
        DemoPicker.SelectedIndex = 0;
        var items = new[] { "One", "Two", "Three", "Four", "Five" };
        DemoCollection.ItemsSource = items;
        DemoCarousel.ItemsSource = items;
        DemoIndicator.ItemsSource = items;
        DemoList.ItemsSource = items;
        DemoWebView.Source = new HtmlWebViewSource
        {
            Html = "<html><body style='font-family:sans-serif'><h1>WebView</h1><p>Local HTML content.</p></body></html>",
        };
        DemoGraphicsView.Drawable = new SampleDrawable();

        DemoRefresh.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(RefreshView.IsRefreshing) && DemoRefresh.IsRefreshing)
            {
                await Task.Delay(500);
                DemoRefresh.IsRefreshing = false;
            }
        };
    }

    private sealed class SampleDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.DodgerBlue;
            canvas.FillEllipse(20, 20, 90, 90);
            canvas.StrokeColor = Colors.DarkGreen;
            canvas.StrokeSize = 4;
            canvas.DrawLine(130, 30, 300, 110);
            canvas.FillColor = Colors.Orange;
            canvas.FillRoundedRectangle(20, 80, 260, 40, 10);
        }
    }
}
