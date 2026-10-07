using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlsSampleApp;

public partial class MediaGraphicsPage : ContentPage
{
    public MediaGraphicsPage()
    {
        InitializeComponent();
        DemoWebView.Source = new HtmlWebViewSource
        {
            Html = "<html><body style='font-family:sans-serif'><h1>WebView</h1><p>Local HTML content.</p></body></html>",
        };
        DemoGraphicsView.Drawable = new SampleDrawable();
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
