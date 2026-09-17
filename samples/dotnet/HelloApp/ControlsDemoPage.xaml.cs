using System.Threading.Tasks;
// ControlsDemoPage 的 code-behind：展示新加的 10 种 MAUI 控件在鸿蒙上的渲染
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace HelloApp;

public partial class ControlsDemoPage : ContentPage
{
    public ControlsDemoPage()
    {
        InitializeComponent();
        TestPicker.ItemsSource = new List<string> { "苹果", "香蕉", "樱桃", "蜜桃" };
        TestPicker.SelectedIndex = 0;
        TestCollection.ItemsSource = Enumerable.Range(1, 200).Select(i => $"条目 {i}");
        TestCarousel.ItemsSource = new List<string> { "第 1 页", "第 2 页", "第 3 页" };
        // CarouselView 小项验证：位置回传（滑动/程序化 → PositionChanged → Label）+ 程序化跳转
        TestCarousel.PositionChanged += (s, e) => CarouselPosLabel.Text = $"carousel pos: {e.CurrentPosition}";
        CarouselJumpBtn.Clicked += (s, e) => TestCarousel.Position = 2;
        // Drag & Drop 小项验证：源 DragStarting 填充文本 → 目标 Drop 读回（UDMF 文本载荷）
        ((DragGestureRecognizer)DragSourceLabel.GestureRecognizers[0]).DragStarting +=
            (s, e) => e.Data.Text = "hello harmony";
        ((DropGestureRecognizer)DropTargetLabel.GestureRecognizers[0]).Drop += async (s, e) =>
            DropStatusLabel.Text = $"drop: {await e.Data.GetTextAsync()}";
        // Shape/自绘验证：GraphicsView IDrawable → ICanvas → OH_Drawing
        TestGraphicsView.Drawable = new DemoDrawable();
        // 流式图片（1x1 红 PNG → MemoryStream → 落盘 file://）
        const string redPng = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";
        TestStreamImage.Source = Microsoft.Maui.Controls.ImageSource.FromStream(
            () => new MemoryStream(Convert.FromBase64String(redPng)));
        // RefreshView 演示：下拉触发后 1.5s 复位（经 PropertyChanged，不依赖 Controls 内部事件流）
        TestRefresh.PropertyChanged += async (sender, e) =>
        {
            if (e.PropertyName == nameof(RefreshView.IsRefreshing) && TestRefresh.IsRefreshing)
            {
                await Task.Delay(1500);
                TestRefresh.IsRefreshing = false;
            }
        };
    }

    /// <summary>自绘演示：圆/线/弧/圆角矩形（vp 坐标，每帧重绘）</summary>
    private sealed class DemoDrawable : Microsoft.Maui.Graphics.IDrawable
    {
        public void Draw(Microsoft.Maui.Graphics.ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Microsoft.Maui.Graphics.Colors.Red;
            canvas.FillEllipse(10, 10, 50, 50);
            canvas.StrokeColor = Microsoft.Maui.Graphics.Colors.Blue;
            canvas.StrokeSize = 3;
            canvas.DrawLine(70, 20, 200, 60);
            canvas.StrokeColor = Microsoft.Maui.Graphics.Colors.Green;
            canvas.DrawArc(10, 70, 60, 60, 0, 270, clockwise: true, closed: false);
            canvas.FillColor = Microsoft.Maui.Graphics.Colors.Orange;
            canvas.FillRoundedRectangle(90, 70, 120, 25, 10);
        }
    }

    private void OnPopClicked(object? sender, EventArgs e)
    {
        // NavigationPage 协议内返回（与 Navigation.PushAsync 配对）
        Navigation.PopAsync().FireAndForgetNavigation();
    }
}
