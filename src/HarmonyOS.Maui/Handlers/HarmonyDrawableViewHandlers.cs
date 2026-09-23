// 自绘类视图 Handler：GraphicsView（IDrawable）与 Shape（Controls 形状基类，覆盖
// Ellipse/Rectangle/Line/Path/Polyline/Polygon/RoundRectangle 等）。
// 绘制通道：ArkCustomDrawNode（ARKUI_NODE_CUSTOM + NODE_ON_DRAW）→ HarmonyDrawingCanvas
// （MAUI ICanvas → OH_Drawing），回调内以 vp 作画（节点层已把 px 空间画布 Scale(density)）。
#nullable enable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkCustomDrawNode = HarmonyOS.ArkUI.ArkCustomDrawNode;
using MGraphicsView = Microsoft.Maui.Controls.GraphicsView;
using MShape = Microsoft.Maui.Controls.Shapes.Shape;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// GraphicsView → ArkCustomDrawNode：Drawable.Draw(ICanvas, RectF) 经 OH_Drawing 落地。
/// MAUI 的 GraphicsView.Invalidate() 经 UpdateValue(Drawable) 触发 Mapper → MarkDirty 重绘。
/// </summary>
public class HarmonyGraphicsViewHandler : HarmonyViewHandler<MGraphicsView, ArkCustomDrawNode>
{
    public static PropertyMapper<MGraphicsView, HarmonyGraphicsViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(MGraphicsView.Drawable)] = MapInvalidate,
        [nameof(VisualElement.BackgroundColor)] = MapInvalidate,
    };

    // 画布适配器复用（ON_DRAW 每帧 Bind 新画布指针，避免热路径分配）
    private HarmonyDrawingCanvas? _canvas;

    public HarmonyGraphicsViewHandler() : base(Mapper) { }

    protected override ArkCustomDrawNode CreatePlatformView() => new();

    protected override void ConnectHandler(ArkCustomDrawNode platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SetDrawCallback(OnDraw);
        platformView.Invalidate();
    }

    protected override void DisconnectHandler(ArkCustomDrawNode platformView)
    {
        platformView.SetDrawCallback(null);
        _canvas?.Dispose(); // 画布持有的 Pen/Brush 原生对象显式释放
        _canvas = null;
        base.DisconnectHandler(platformView);
    }

    public static void MapInvalidate(HarmonyGraphicsViewHandler handler, MGraphicsView view)
        => handler.PlatformView.Invalidate();

    private void OnDraw(Bindings.NativeNode.OHDrawingCanvas nativeCanvas, float width, float height)
    {
        var drawable = VirtualView?.Drawable;
        if (drawable is null)
            return;
        (_canvas ??= new HarmonyDrawingCanvas(nativeCanvas)).Bind(nativeCanvas);
        _canvas.ResetState();
        drawable.Draw(_canvas, new RectF(0, 0, width, height));
    }
}

/// <summary>
/// Shape（Controls 形状基类）→ ArkCustomDrawNode：经官方 Microsoft.Maui.Graphics.ShapeDrawable
/// （IShapeView 协议：GetPath + Fill/Stroke 属性驱动 ICanvas）绘制——形状几何与描边语义全部复用官方实现，
/// 覆盖 Ellipse/Rectangle/Line/Path/Polyline/Polygon/RoundRectangle 等。
/// 限制：文本通道未接（DrawString no-op）；渐变画刷在画布层暂按纯色处理。
/// </summary>
public class HarmonyShapeHandler : HarmonyViewHandler<MShape, ArkCustomDrawNode>
{
    public static PropertyMapper<MShape, HarmonyShapeHandler> Mapper = new(ViewMapper)
    {
        // 形状属性变化时重绘（ShapeDrawable.Draw 读取的就是这些属性）
        [nameof(MShape.Fill)] = MapInvalidate,
        [nameof(MShape.Stroke)] = MapInvalidate,
        [nameof(MShape.StrokeThickness)] = MapInvalidate,
        [nameof(MShape.StrokeDashArray)] = MapInvalidate,
        [nameof(MShape.StrokeDashOffset)] = MapInvalidate,
        [nameof(MShape.StrokeLineCap)] = MapInvalidate,
        [nameof(MShape.StrokeLineJoin)] = MapInvalidate,
        [nameof(MShape.StrokeMiterLimit)] = MapInvalidate,
        [nameof(MShape.Aspect)] = MapInvalidate,
    };

    // 官方 ShapeDrawable 承载形状绘制（WeakReference 持有 IShapeView，随属性变化实时读取）
    private readonly Microsoft.Maui.Graphics.ShapeDrawable _drawable = new();
    private HarmonyDrawingCanvas? _canvas;

    public HarmonyShapeHandler() : base(Mapper) { }

    protected override ArkCustomDrawNode CreatePlatformView() => new();

    protected override void ConnectHandler(ArkCustomDrawNode platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SetDrawCallback(OnDraw);
        platformView.Invalidate();
    }

    protected override void DisconnectHandler(ArkCustomDrawNode platformView)
    {
        platformView.SetDrawCallback(null);
        _canvas?.Dispose(); // 同上：Pen/Brush 原生对象显式释放
        _canvas = null;
        base.DisconnectHandler(platformView);
    }

    public static void MapInvalidate(HarmonyShapeHandler handler, MShape view)
        => handler.PlatformView.Invalidate();

    private void OnDraw(Bindings.NativeNode.OHDrawingCanvas nativeCanvas, float width, float height)
    {
        if (VirtualView is null)
            return;
        _drawable.UpdateShapeView(VirtualView);
        (_canvas ??= new HarmonyDrawingCanvas(nativeCanvas)).Bind(nativeCanvas);
        _canvas.ResetState();
        _drawable.Draw(_canvas, new RectF(0, 0, width, height));
    }
}
