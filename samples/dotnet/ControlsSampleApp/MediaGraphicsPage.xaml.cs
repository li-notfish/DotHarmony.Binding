using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlsSampleApp;

public partial class MediaGraphicsPage : ContentPage
{
    private readonly PaintCanvas _canvas = new();

    public MediaGraphicsPage()
    {
        InitializeComponent();

        DemoGraphicsView.Drawable = _canvas;

        void PaintAt(TouchEventArgs e)
        {
            if (e.Touches is { Length: > 0 })
            {
                _canvas.AddPoint(e.Touches[0]);
                DemoGraphicsView.Invalidate();
                GraphicsStatus.Text = $"Touches: {_canvas.Count} (last {e.Touches[0].X:F0}, {e.Touches[0].Y:F0})";
            }
        }
        DemoGraphicsView.StartInteraction += (_, e) => PaintAt(e);
        DemoGraphicsView.DragInteraction += (_, e) => PaintAt(e);

        DemoWebView.Navigated += (_, _) =>
        {
            WebStatus.Text = "Inline HTML rendered";
            ActivityLog.Record("Media: WebView navigated");
        };
        DemoWebView.Source = new HtmlWebViewSource
        {
            Html = "<!doctype html><html><body style='font-family:sans-serif;background:#F0FDFA'>" +
                   "<h3 style='color:#0F766E'>WebView says hi</h3>" +
                   "<p>This page is inline HTML rendered inside the app.</p></body></html>",
        };
    }

    private sealed class PaintCanvas : IDrawable
    {
        private readonly List<PointF> _points = new();

        public int Count => _points.Count;

        public void AddPoint(PointF p)
        {
            _points.Add(p);
            if (_points.Count > 300)
                _points.RemoveAt(0);
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(dirtyRect);

            canvas.StrokeColor = Color.FromArgb("#CBD5E1");
            canvas.StrokeSize = 1;
            for (float x = 0; x < dirtyRect.Width; x += 24)
            {
                canvas.DrawLine(x, 0, x, dirtyRect.Height);
            }
            for (float y = 0; y < dirtyRect.Height; y += 24)
            {
                canvas.DrawLine(0, y, dirtyRect.Width, y);
            }

            canvas.FillColor = Color.FromArgb("#2563EB");
            foreach (var p in _points)
            {
                canvas.FillCircle(p, 6);
            }
        }
    }
}
