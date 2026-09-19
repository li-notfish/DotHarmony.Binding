// HarmonyDrawingCanvas：Microsoft.Maui.Graphics.ICanvas → OH_Drawing 适配器。
// 坐标单位：vp（ArkCustomDrawNode 已把恒等 px 空间画布 Scale(density)，本适配器直接以 vp 作图）。
// 限制：DrawString 暂为 no-op（排版通道未接）；SetShadow no-op；SetFillPaint 仅支持纯色。
#nullable enable
using System.Numerics;
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Text;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyDrawingCanvas : ICanvas, IDisposable
{
    private OHDrawingCanvas _canvas;
    private readonly Bindings.NativeNode.OHDrawingPen _pen = new();
    private readonly Bindings.NativeNode.OHDrawingBrush _brush = new();
    private bool _disposed;

    private Color _fillColor = Colors.Black;
    private Color _strokeColor = Colors.Black;
    private float _strokeThickness = 1f;
    private float _alpha = 1f;

    public HarmonyDrawingCanvas(Bindings.NativeNode.OHDrawingCanvas canvas) => _canvas = canvas;

    /// <summary>
    /// 重新绑定原生画布（ON_DRAW 每帧画布指针可能不同）；
    /// 同帧内复用本适配器与 Pen/Brush 原生对象，避免热路径分配。
    /// </summary>
    internal void Bind(Bindings.NativeNode.OHDrawingCanvas canvas) => _canvas = canvas;

    private uint FillColorArgb => ToArgb(_fillColor);
    private uint StrokeColorArgb => ToArgb(_strokeColor);

    private uint ToArgb(Color c)
    {
        if (c is null) return 0;
        var a = (byte)(c.Alpha * _alpha * 255);
        return Bindings.NativeNode.OHDrawingColorHelper.PackArgb(
            a, (byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255));
    }

    // ───────────────────────── ICanvas 状态 ─────────────────────────

    public float Alpha { get => _alpha; set => _alpha = value; }
    public bool Antialias { get; set; } = true;

    public Color FillColor { get => _fillColor; set => _fillColor = value ?? Colors.Black; }
    public Color StrokeColor { get => _strokeColor; set => _strokeColor = value ?? Colors.Black; }
    public float StrokeThickness { get => _strokeThickness; set => _strokeThickness = value; }

    public LineCap StrokeLineCap { get; set; }
    public LineJoin StrokeLineJoin { get; set; }
    public float MiterLimit { get; set; } = 4f;

    /// <summary>虚线模式：on/off 交替（单值按 MAUI 语义展开为 on=off，best-effort）</summary>
    private float[]? _strokeDashPattern;
    private bool _dashWarned;
    public float[]? StrokeDashPattern
    {
        get => _strokeDashPattern;
        set
        {
            _strokeDashPattern = value;
            if (value != null && !_dashWarned)
            {
                _dashWarned = true;
                Bindings.Runtime.HiLog.Warn("HarmonyHost",
                    "[DrawingCanvas] StrokeDashPattern 暂不支持（Pen 路径特效通道未接），按实线绘制");
            }
        }
    }

    /// <summary>释放原生 Pen/Brush（handler Disconnect 时调用；无 finalizer，显式释放）</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _pen.Dispose();
        _brush.Dispose();
    }

    public void SetToSystemFont() { /* no-op：排版通道未接 */ }
    public IFont? Font { set { /* no-op：排版通道未接 */ } }
    public Color FontColor { set { /* no-op：排版通道未接 */ } }
    public float FontSize { set { /* no-op：排版通道未接 */ } }

    /// <summary>UI 缩放系数（节点绘制已按 vp 换算，恒为 1）</summary>
    public float DisplayScale { get; set; } = 1f;
    public float StrokeSize { set => _strokeThickness = value; }
    public float StrokeDashOffset { set { /* 虚线通道未接，no-op */ } }
    public BlendMode BlendMode { set { /* 仅 SRC_OVER（画布默认），暂不支持切换 */ } }

    public void SaveState() => _canvas.Save();

    /// <summary>恢复最近保存的画布状态（画布状态栈语义恒成功）</summary>
    public bool RestoreState()
    {
        _canvas.Restore();
        return true;
    }

    /// <summary>恢复初始状态：重置状态字段（画布状态栈无法获知栈深，best-effort）</summary>
    public void ResetState()
    {
        _fillColor = Colors.Black;
        _strokeColor = Colors.Black;
        _strokeThickness = 1f;
        _alpha = 1f;
        StrokeDashPattern = null;
    }

    public void SetFillPaint(Paint paint, RectF rectangle)
    {
        if (paint is SolidPaint solid && solid.Color is not null)
            _fillColor = solid.Color;
        // 渐变/图案 Paint 暂不支持
    }

    /// <summary>阴影暂不支持（no-op）</summary>
    public void SetShadow(SizeF offset, float blur, Color color) { }

    // ───────────────────────── 矩形 / 椭圆 / 圆弧 ─────────────────────────

    public void DrawRectangle(float x, float y, float width, float height)
    {
        if (_strokeThickness <= 0) return; // MAUI 语义：线宽 0 = 不描边
        ApplyPen();
        _canvas.DrawRect(x, y, width, height);
        _canvas.DetachPen();
    }

    public void DrawRectangle(RectF rect) => DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);

    public void FillRectangle(float x, float y, float width, float height)
    {
        ApplyBrush();
        _canvas.FillRect(x, y, width, height);
        _canvas.DetachBrush();
    }

    public void DrawEllipse(float x, float y, float width, float height)
    {
        if (_strokeThickness <= 0) return;
        using var path = BuildPath(p => AddEllipseArcs(p, x, y, width, height));
        ApplyPen();
        _canvas.DrawPath(path);
        _canvas.DetachPen();
    }

    public void DrawEllipse(RectF rect) => DrawEllipse(rect.X, rect.Y, rect.Width, rect.Height);

    public void FillEllipse(float x, float y, float width, float height)
    {
        using var path = BuildPath(p => { AddEllipseArcs(p, x, y, width, height); p.Close(); }, winding: true);
        ApplyBrush();
        _canvas.DrawPath(path);
        _canvas.DetachBrush();
    }

    public void FillEllipse(RectF rect) => FillEllipse(rect.X, rect.Y, rect.Width, rect.Height);

    public void DrawArc(float x, float y, float width, float height, float startAngle, float sweepAngle, bool clockwise, bool close)
    {
        if (_strokeThickness <= 0) return;
        var sweep = clockwise ? sweepAngle : -sweepAngle;
        using var path = BuildPath(p => AddArcSweep(p, x, y, x + width, y + height, startAngle, sweep));
        ApplyPen();
        _canvas.DrawPath(path);
        _canvas.DetachPen();
    }

    public void FillArc(float x, float y, float width, float height, float startAngle, float sweepAngle, bool clockwise)
    {
        var sweep = clockwise ? sweepAngle : -sweepAngle;
        using var path = BuildPath(p =>
        {
            AddArcSweep(p, x, y, x + width, y + height, startAngle, sweep);
            p.Close();
        }, winding: true);
        ApplyBrush();
        _canvas.DrawPath(path);
        _canvas.DetachBrush();
    }

    public void DrawLine(float x1, float y1, float x2, float y2)
    {
        if (_strokeThickness <= 0) return;
        ApplyPen();
        _canvas.DrawLine(x1, y1, x2, y2);
        _canvas.DetachPen();
    }

    // ───────────────────────── 路径 / 裁剪 ─────────────────────────

    public void DrawPath(PathF path)
    {
        if (_strokeThickness <= 0) return;
        using var p = ToNativePath(path, winding: false);
        ApplyPen();
        _canvas.DrawPath(p);
        _canvas.DetachPen();
    }

    public void FillPath(PathF path, WindingMode windingMode)
    {
        using var p = ToNativePath(path, winding: windingMode == WindingMode.NonZero);
        ApplyBrush();
        _canvas.DrawPath(p);
        _canvas.DetachBrush();
    }

    public void ClipRectangle(float x, float y, float width, float height)
        => _canvas.ClipRect(x, y, width, height, Antialias);

    public void ClipRectangle(RectF rect) => ClipRectangle(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>反向裁剪（Subtract）暂不支持——退化为 no-op</summary>
    public void SubtractFromClip(float x, float y, float width, float height) { }

    public void SubtractFromClip(RectF rect) => SubtractFromClip(rect.X, rect.Y, rect.Width, rect.Height);

    // ───────────────────────── 文本（暂不支持）─────────────────────────

    /// <summary>文本绘制暂不支持（排版/字体通道未接）——no-op</summary>
    public void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left) { }

    public void DrawString(string value, float x, float y, float width, float height,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment verticalAlignment = VerticalAlignment.Top,
        TextFlow textFlow = TextFlow.ClipBounds, float lineSpacingAdjustment = 0) { }

    public void DrawString(string value, RectF rect,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment verticalAlignment = VerticalAlignment.Top,
        TextFlow textFlow = TextFlow.ClipBounds, float lineSpacingAdjustment = 0) { }

    public void DrawText(IAttributedText? value, float x, float y, float width, float height) { }


    public void FillRectangle(RectF rect) => FillRectangle(rect.X, rect.Y, rect.Width, rect.Height);

    public void DrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
    {
        if (_strokeThickness <= 0) return;
        var r = ClampRadius(cornerRadius, width, height);
        ApplyPen();
        _canvas.DrawRoundRect(x, y, width, height, r, r);
        _canvas.DetachPen();
    }

    public void DrawRoundedRectangle(RectF rect, float cornerRadius)
        => DrawRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, cornerRadius);

    public void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
    {
        var r = ClampRadius(cornerRadius, width, height);
        ApplyBrush();
        _canvas.FillRoundRect(x, y, width, height, r, r);
        _canvas.DetachBrush();
    }

    public void FillRoundedRectangle(RectF rect, float cornerRadius)
        => FillRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, cornerRadius);

    /// <summary>位图绘制暂不支持（图像解码通道未接）——no-op</summary>
    public void DrawImage(Microsoft.Maui.Graphics.IImage image, float x, float y, float width, float height) { }

    // ───────────────────────── 变换 ─────────────────────────

    public void Rotate(float degrees, float x, float y) => _canvas.Rotate(degrees, x, y);

    public void Rotate(float degrees) => _canvas.Rotate(degrees, 0, 0);

    public void Scale(float sx, float sy) => _canvas.Scale(sx, sy);

    public void Translate(float tx, float ty) => _canvas.Translate(tx, ty);

    /// <summary>
    /// 仿射矩阵拼接：分解为平移+旋转+缩放（OH_Drawing 无矩阵对象通道，倾斜分量暂不支持）。
    /// </summary>
    public void ConcatenateTransform(Matrix3x2 transform)
    {
        _canvas.Translate(transform.M31, transform.M32);
        var rotation = MathF.Atan2(transform.M12, transform.M11) * 180f / MathF.PI;
        if (MathF.Abs(rotation) > 0.001f)
            _canvas.Rotate(rotation, 0, 0);
        _canvas.Scale(transform.M11, transform.M22);
    }

    // ───────────────────────── 裁剪（路径） ─────────────────────────

    public void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero)
    {
        using var p = ToNativePath(path, winding: windingMode == WindingMode.NonZero);
        _canvas.ClipPath(p, Antialias);
    }

    // ───────────────────────── 图像 / 文本测量（暂不支持） ─────────────────────────

    /// <summary>文本测量暂不支持（排版通道未接）——估算占位尺寸</summary>
    public SizeF GetStringSize(string value, IFont font, float fontSize)
        => new(value.Length * fontSize * 0.6f, fontSize);

    public SizeF GetStringSize(string value, IFont font, float fontSize,
        HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        => GetStringSize(value, font, fontSize);

    // ───────────────────────── 内部助手 ─────────────────────────

    /// <summary>构建并按需设置填充规则的原生路径（winding=true 即 NonZero）</summary>
    private OHDrawingPath BuildPath(Action<Bindings.NativeNode.OHDrawingPath> build, bool winding = false)
    {
        var path = new Bindings.NativeNode.OHDrawingPath();
        path.SetFillStyle(winding ? WindingNonZero : WindingEvenOdd);
        build(path);
        return path;
    }

    /// <summary>
    /// 全椭圆 = 两个 180° 弧（ArcTo 全圆 sweep 360 退化不落点，Skia 系行为；实测 GraphicsView 红圆缺失）。
    /// </summary>
    private static void AddEllipseArcs(Bindings.NativeNode.OHDrawingPath p, float x, float y, float w, float h)
    {
        p.ArcTo(x, y, x + w, y + h, 0, 180);
        p.ArcTo(x, y, x + w, y + h, 180, 180);
    }

    /// <summary>
    /// 任意弧段：|sweep| > 180° 拆为两段（同上，全圆/半圆以上退化），其余单段直落。
    /// </summary>
    private static void AddArcSweep(Bindings.NativeNode.OHDrawingPath p,
        float x1, float y1, float x2, float y2, float startAngle, float sweepAngle)
    {
        if (MathF.Abs(sweepAngle) <= 180f)
        {
            p.ArcTo(x1, y1, x2, y2, startAngle, sweepAngle);
            return;
        }
        var sign = MathF.Sign(sweepAngle);
        p.ArcTo(x1, y1, x2, y2, startAngle, 180f * sign);
        p.ArcTo(x1, y1, x2, y2, startAngle + 180f * sign, (MathF.Abs(sweepAngle) - 180f) * sign);
    }

    /// <summary>MAUI PathF → 原生路径（Move/Line/Quad/Cubic/Arc/Close 全量映射）</summary>
    private OHDrawingPath ToNativePath(PathF path, bool winding)
    {
        var native = new Bindings.NativeNode.OHDrawingPath();
        native.SetFillStyle(winding ? WindingNonZero : WindingEvenOdd);
        for (var s = 0; s < path.OperationCount; s++)
        {
            var type = path.GetSegmentInfo(s, out var pt, out var arcAngleIndex, out _);
            switch (type)
            {
                case PathOperation.Move:
                    native.MoveTo(path[pt].X, path[pt].Y);
                    break;
                case PathOperation.Line:
                    native.LineTo(path[pt].X, path[pt].Y);
                    break;
                case PathOperation.Quad:
                    native.QuadBezier(path[pt].X, path[pt].Y, path[pt + 1].X, path[pt + 1].Y);
                    break;
                case PathOperation.Cubic:
                    native.CubicBezier(
                        path[pt].X, path[pt].Y,
                        path[pt + 1].X, path[pt + 1].Y,
                        path[pt + 2].X, path[pt + 2].Y);
                    break;
                case PathOperation.Arc:
                    // (pt, pt+1) 为弧所在椭圆外接矩形对角；角度经 GetArcAngle 取起止
                    native.ArcTo(
                        MathF.Min(path[pt].X, path[pt + 1].X), MathF.Min(path[pt].Y, path[pt + 1].Y),
                        MathF.Max(path[pt].X, path[pt + 1].X), MathF.Max(path[pt].Y, path[pt + 1].Y),
                        path.GetArcAngle(arcAngleIndex),
                        path.GetArcAngle(arcAngleIndex + 1) - path.GetArcAngle(arcAngleIndex));
                    break;
                case PathOperation.Close:
                    native.Close();
                    break;
            }
        }
        return native;
    }

    private void ApplyPen()
    {
        _pen.SetColor(StrokeColorArgb);
        _pen.SetWidth(StrokeThickness);
        _pen.SetCap(StrokeLineCap switch
        {
            LineCap.Round => CapRound,
            LineCap.Square => CapSquare,
            _ => CapFlat,
        });
        _pen.SetJoin(StrokeLineJoin switch
        {
            LineJoin.Round => JoinRound,
            LineJoin.Bevel => JoinBevel,
            _ => JoinMiter,
        });
        _pen.SetAntiAlias(Antialias);
        // StrokeDashPattern 虚线：Pen 路径特效通道未接，暂按实线绘制
        _canvas.AttachPen(_pen);
    }

    private void ApplyBrush()
    {
        _brush.SetColor(FillColorArgb);
        _brush.SetAntiAlias(Antialias);
        _canvas.AttachBrush(_brush);
    }

    /// <summary>圆角半径钳制（不超矩形短边一半）——纯逻辑，单测覆盖</summary>
    internal static float ClampRadius(float cornerRadius, float width, float height)
    {
        if (cornerRadius <= 0 || width <= 0 || height <= 0)
            return 0;
        var half = MathF.Min(width, height) / 2f;
        return MathF.Min(cornerRadius, half);
    }

    // OH_Drawing 枚举常量（drawing_pen.h / drawing_path.h）
    private const int CapFlat = 0, CapRound = 1, CapSquare = 2;
    private const int JoinMiter = 0, JoinRound = 1, JoinBevel = 2;
    private const int WindingNonZero = 0, WindingEvenOdd = 1;
}
