using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Devices;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;
using ArkCustomDrawNode = HarmonyOS.ArkUI.ArkCustomDrawNode;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyActivityIndicatorHandler :
    HarmonyViewHandler<IActivityIndicator, ArkCustomDrawNode>, IActivityIndicatorHandler
{
    private const int AnimationIntervalMs = 16;
    private const float StrokeThicknessVp = 4f;

    private HarmonyDrawingCanvas? _canvas;
    private Timer? _animationTimer;
    private float _angle;

    public static PropertyMapper<IActivityIndicator, HarmonyActivityIndicatorHandler> Mapper =
        new(HarmonyViewMapper.Base)
        {
            [nameof(IActivityIndicator.Color)] = MapColor,
            [nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
        };

    public HarmonyActivityIndicatorHandler() : base(Mapper) { }

    protected override ArkCustomDrawNode CreatePlatformView() => new();

    protected override void ConnectHandler(ArkCustomDrawNode platformView)
    {
        var density = (float)DeviceDisplay.MainDisplayInfo.Density;
        platformView.SetDefaultMeasuredSize(40f, 40f, density);
        base.ConnectHandler(platformView);
        platformView.SetDrawCallback(OnDraw);
        platformView.Invalidate();
        if (VirtualView.IsRunning)
            StartAnimation();
    }

    protected override void DisconnectHandler(ArkCustomDrawNode platformView)
    {
        StopAnimation();
        platformView.SetDrawCallback(null);
        _canvas?.Dispose();
        _canvas = null;
        base.DisconnectHandler(platformView);
    }

    public static void MapColor(HarmonyActivityIndicatorHandler handler, IActivityIndicator view)
        => handler.PlatformView.Invalidate();

    public static void MapIsRunning(HarmonyActivityIndicatorHandler handler, IActivityIndicator view)
    {
        if (view.IsRunning)
            handler.StartAnimation();
        else
            handler.StopAnimation();
        handler.PlatformView.Invalidate();
    }

    private void StartAnimation()
    {
        if (_animationTimer is not null)
            return;

        _animationTimer = new Timer(
            _ => MainThreadDispatcher.Post(AdvanceAnimation),
            null,
            0,
            AnimationIntervalMs);
    }

    private void StopAnimation()
    {
        _animationTimer?.Dispose();
        _animationTimer = null;
    }

    private void AdvanceAnimation()
    {
        if (_animationTimer is null || PlatformView.Handle.IsNull)
            return;

        _angle = (_angle + 12f) % 360f;
        PlatformView.Invalidate();
    }

    private void OnDraw(Bindings.NativeNode.OHDrawingCanvas nativeCanvas, float width, float height)
    {
        var view = VirtualView;
        if (!view.IsRunning)
            return;

        var size = MathF.Min(width, height);
        var stroke = MathF.Min(StrokeThicknessVp, size / 6f);
        var diameter = MathF.Max(0, size - stroke * 2);
        var x = (width - diameter) / 2f;
        var y = (height - diameter) / 2f;

        (_canvas ??= new HarmonyDrawingCanvas(nativeCanvas)).Bind(nativeCanvas);
        _canvas.ResetState();
        _canvas.StrokeColor = view.Color ?? HarmonyControlDefaults.ActivityIndicatorColorDefault;
        _canvas.StrokeThickness = stroke;
        _canvas.DrawArc(x, y, diameter, diameter, _angle, 270f, clockwise: true, close: false);
    }

    IActivityIndicator IActivityIndicatorHandler.VirtualView => VirtualView;
    object IActivityIndicatorHandler.PlatformView => PlatformView;
}
