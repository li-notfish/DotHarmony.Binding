#nullable enable
using System;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>OH_Drawing 颜色打包助手（0xAARRGGBB，可纯逻辑单测）</summary>
public static class OHDrawingColorHelper
{
    /// <summary>ARGB 分量 → OH_Drawing 颜色 u32（0xAARRGGBB）</summary>
    public static uint PackArgb(byte a, byte r, byte g, byte b)
        => (uint)(a << 24 | r << 16 | g << 8 | b);
}

/// <summary>
/// OH_Drawing_Path 包装类：MoveTo/LineTo/贝塞尔/ArcTo/Close 等路径构建原语。
/// 每帧创建的短命路径请及时 Dispose 销毁原生路径。
/// </summary>
public sealed unsafe class OHDrawingPath : IDisposable
{
    private OH_Drawing_Path* _path;

    public OHDrawingPath() => _path = OHDrawingApi.OH_Drawing_PathCreate();

    internal OH_Drawing_Path* Native => _path;

    public OHDrawingPath MoveTo(float x, float y) { OHDrawingApi.OH_Drawing_PathMoveTo(_path, x, y); return this; }

    public OHDrawingPath LineTo(float x, float y) { OHDrawingApi.OH_Drawing_PathLineTo(_path, x, y); return this; }

    public OHDrawingPath QuadBezier(float ctrlX, float ctrlY, float endX, float endY)
    { OHDrawingApi.OH_Drawing_PathQuadTo(_path, ctrlX, ctrlY, endX, endY); return this; }

    public OHDrawingPath CubicBezier(float ctrlX1, float ctrlY1, float ctrlX2, float ctrlY2, float endX, float endY)
    { OHDrawingApi.OH_Drawing_PathCubicTo(_path, ctrlX1, ctrlY1, ctrlX2, ctrlY2, endX, endY); return this; }

    /// <summary>椭圆弧：（x1,y1,x2,y2）为弧所在椭圆的外接矩形，角度单位为度</summary>
    public OHDrawingPath ArcTo(float x1, float y1, float x2, float y2, float startAngle, float sweepAngle)
    { OHDrawingApi.OH_Drawing_PathArcTo(_path, x1, y1, x2, y2, startAngle, sweepAngle); return this; }

    public OHDrawingPath Close() { OHDrawingApi.OH_Drawing_PathClose(_path); return this; }

    public OHDrawingPath Reset() { OHDrawingApi.OH_Drawing_PathReset(_path); return this; }

    /// <summary>设置填充规则（0 = WINDING，1 = EVEN_ODD，对应 OH_Drawing_PathFillType）——填充绘制前必须设置</summary>
    public OHDrawingPath SetFillStyle(int fillStyle)
    { OHDrawingApi.OH_Drawing_PathSetFillType(_path, fillStyle); return this; }

    public void Dispose()
    {
        if (_path != null)
        {
            OHDrawingApi.OH_Drawing_PathDestroy(_path);
            _path = null;
        }
    }
}

/// <summary>OH_Drawing_Pen 包装类（描边状态：颜色/线宽/线帽/连接样式/抗锯齿）</summary>
public sealed unsafe class OHDrawingPen : IDisposable
{
    private OH_Drawing_Pen* _pen;

    public OHDrawingPen() => _pen = OHDrawingApi.OH_Drawing_PenCreate();

    internal OH_Drawing_Pen* Native => _pen;

    /// <summary>颜色（0xAARRGGBB）</summary>
    public void SetColor(uint argb) => OHDrawingApi.OH_Drawing_PenSetColor(_pen, argb);

    /// <summary>线宽（受画布缩放影响，调用方负责单位）</summary>
    public void SetWidth(float width) => OHDrawingApi.OH_Drawing_PenSetWidth(_pen, width);

    /// <summary>线帽：0 = FLAT，1 = ROUND，2 = SQUARE（OH_Drawing_PenCapStyle）</summary>
    public void SetCap(int capStyle) => OHDrawingApi.OH_Drawing_PenSetCap(_pen, capStyle);

    /// <summary>连接样式：0 = MITER，1 = ROUND，2 = BEVEL（OH_Drawing_PenJoinStyle）</summary>
    public void SetJoin(int joinStyle) => OHDrawingApi.OH_Drawing_PenSetJoin(_pen, joinStyle);

    public void SetAntiAlias(bool antialias) => OHDrawingApi.OH_Drawing_PenSetAntiAlias(_pen, antialias);

    public void Dispose()
    {
        if (_pen != null)
        {
            OHDrawingApi.OH_Drawing_PenDestroy(_pen);
            _pen = null;
        }
    }
}

/// <summary>OH_Drawing_Brush 包装类（填充状态：颜色/抗锯齿）</summary>
public sealed unsafe class OHDrawingBrush : IDisposable
{
    private OH_Drawing_Brush* _brush;

    public OHDrawingBrush() => _brush = OHDrawingApi.OH_Drawing_BrushCreate();

    internal OH_Drawing_Brush* Native => _brush;

    /// <summary>颜色（0xAARRGGBB）</summary>
    public void SetColor(uint argb) => OHDrawingApi.OH_Drawing_BrushSetColor(_brush, argb);

    public void SetAntiAlias(bool antialias) => OHDrawingApi.OH_Drawing_BrushSetAntiAlias(_brush, antialias);

    public void Dispose()
    {
        if (_brush != null)
        {
            OHDrawingApi.OH_Drawing_BrushDestroy(_brush);
            _brush = null;
        }
    }
}
