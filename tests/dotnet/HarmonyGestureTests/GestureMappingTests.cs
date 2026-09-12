// 手势层纯逻辑单测（桌面 net10.0，不依赖 ArkUI 原生）：
// 1) Swipe 方向映射；2) 反射桥（TapGestureRecognizer.SendTapped 非公开通道可用性——
//    AOT 场景下靠 DynamicDependency 收根，此处验证方法定位与委托调用全链路）。
using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Xunit;
using GraphicsPoint = Microsoft.Maui.Graphics.Point;
using IElement = Microsoft.Maui.IElement;

namespace HarmonyGestureTests;

public class SwipeDirectionTests
{
    [Theory]
    [InlineData(100, 10, SwipeDirection.Right)]
    [InlineData(-100, 10, SwipeDirection.Left)]
    [InlineData(10, 100, SwipeDirection.Down)]
    [InlineData(10, -100, SwipeDirection.Up)]
    // 主轴平局归 x 轴（|x| >= |y|）
    [InlineData(50, 50, SwipeDirection.Right)]
    [InlineData(-50, 50, SwipeDirection.Left)]
    [InlineData(0, 0, SwipeDirection.Right)]
    public void MapSwipeDirection_MapsByDominantAxis(double x, double y, SwipeDirection expected)
    {
        Assert.Equal(expected, HarmonyGestureManager.MapSwipeDirection(x, y));
    }
}

public class MauiGestureBridgeTests
{
    [Fact]
    public void SendTapped_RaisesTapped_AndExecutesCommand()
    {
        var view = new Label();
        var commandExecuted = false;
        TappedEventArgs? received = null;
        var recognizer = new TapGestureRecognizer
        {
            Command = new Command(() => commandExecuted = true),
        };
        recognizer.Tapped += (_, e) => received = e;

        MauiGestureBridge.SendTapped(recognizer, view, (IElement? _) => new GraphicsPoint(1, 2));

        Assert.NotNull(received);
        Assert.True(commandExecuted);
        Assert.Equal(new GraphicsPoint(1, 2), received.GetPosition(view));
    }

    [Fact]
    public void SendPointerMoved_RaisesPointerMoved()
    {
        var view = new Label();
        var recognizer = new PointerGestureRecognizer();
        var moved = new TaskCompletionSource<bool>();
        recognizer.PointerMoved += (_, _) => moved.TrySetResult(true);

        MauiGestureBridge.SendPointerMoved(recognizer, view, null);

        Assert.True(moved.Task.Wait(TimeSpan.FromSeconds(5)));
    }
}
