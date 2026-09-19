#nullable enable
using System;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>
/// OH_Drawing_Canvas 包装类：NODE_ON_DRAW 回调拿到的原生画布只读视图 +
/// 绘制原语（描边用 Pen，填充用 Brush，由调用方 Attach/Detach）。
/// </summary>
public sealed unsafe class OHDrawingCanvas
{
    private readonly OH_Drawing_Canvas* _canvas;

    internal OHDrawingCanvas(OH_Drawing_Canvas* canvas) => _canvas = canvas;

    public void Save() => OHDrawingApi.OH_Drawing_CanvasSave(_canvas);

    public void Restore() => OHDrawingApi.OH_Drawing_CanvasRestore(_canvas);

    /// <summary>画布矩阵缩放（vp→px 预缩放入口，见 ArkCustomDrawNode）</summary>
    public void Scale(float sx, float sy) => OHDrawingApi.OH_Drawing_CanvasScale(_canvas, sx, sy);

    /// <summary>绕点旋转（度，顺时针）</summary>
    public void Rotate(float degrees, float px, float py)
        => OHDrawingApi.OH_Drawing_CanvasRotate(_canvas, degrees, px, py);

    /// <summary>平移</summary>
    public void Translate(float dx, float dy) => OHDrawingApi.OH_Drawing_CanvasTranslate(_canvas, dx, dy);

    public void AttachPen(OHDrawingPen pen) => OHDrawingApi.OH_Drawing_CanvasAttachPen(_canvas, pen.Native);

    public void DetachPen() => OHDrawingApi.OH_Drawing_CanvasDetachPen(_canvas);

    public void AttachBrush(OHDrawingBrush brush) => OHDrawingApi.OH_Drawing_CanvasAttachBrush(_canvas, brush.Native);

    public void DetachBrush() => OHDrawingApi.OH_Drawing_CanvasDetachBrush(_canvas);

    /// <summary>画线（坐标系为画布当前矩阵）</summary>
    public void DrawLine(float x1, float y1, float x2, float y2)
        => OHDrawingApi.OH_Drawing_CanvasDrawLine(_canvas, x1, y1, x2, y2);

    /// <summary>描边路径</summary>
    public void DrawPath(OHDrawingPath path) => OHDrawingApi.OH_Drawing_CanvasDrawPath(_canvas, path.Native);

    /// <summary>画圆（描边，需先 AttachPen）</summary>
    public void DrawCircle(float cx, float cy, float radius)
    {
        var point = OHDrawingApi.OH_Drawing_PointCreate(cx, cy);
        OHDrawingApi.OH_Drawing_CanvasDrawCircle(_canvas, point, radius);
        OHDrawingApi.OH_Drawing_PointDestroy(point);
    }

    /// <summary>描边矩形</summary>
    public void DrawRect(float x, float y, float width, float height)
        => DrawRectInternal(x, y, width, height, rounded: false, 0, 0);

    /// <summary>描边圆角矩形（xRad/yRad 为圆角半径）</summary>
    public void DrawRoundRect(float x, float y, float width, float height, float xRad, float yRad)
        => DrawRectInternal(x, y, width, height, rounded: true, xRad, yRad);

    /// <summary>填充矩形（需先 AttachBrush）</summary>
    public void FillRect(float x, float y, float width, float height)
        => DrawRectInternal(x, y, width, height, rounded: false, 0, 0);

    /// <summary>填充圆角矩形（需先 AttachBrush）</summary>
    public void FillRoundRect(float x, float y, float width, float height, float xRad, float yRad)
        => DrawRectInternal(x, y, width, height, rounded: true, xRad, yRad);

    /// <summary>以画布当前颜色覆盖整个画布</summary>
    public void DrawColor(uint argb)
        => OHDrawingApi.OH_Drawing_CanvasDrawColor(_canvas, argb, OHDrawingApi.BlendModeSrcOver);

    /// <summary>用指定颜色清空画布</summary>
    public void Clear(uint argb) => OHDrawingApi.OH_Drawing_CanvasClear(_canvas, argb);

    /// <summary>矩形裁剪（INTERSECT 语义）</summary>
    public void ClipRect(float x, float y, float width, float height, bool antialias = true)
    {
        var rect = OHDrawingApi.OH_Drawing_RectCreate(x, y, x + width, y + height);
        OHDrawingApi.OH_Drawing_CanvasClipRect(_canvas, rect, OHDrawingApi.ClipOpIntersect, antialias);
        OHDrawingApi.OH_Drawing_RectDestroy(rect);
    }

    /// <summary>路径裁剪（INTERSECT 语义）</summary>
    public void ClipPath(OHDrawingPath path, bool antialias = true)
        => OHDrawingApi.OH_Drawing_CanvasClipPath(_canvas, path.Native, OHDrawingApi.ClipOpIntersect, antialias);

    private void DrawRectInternal(float x, float y, float width, float height, bool rounded, float xRad, float yRad)
    {
        var rect = OHDrawingApi.OH_Drawing_RectCreate(x, y, x + width, y + height);
        if (rounded)
        {
            var roundRect = OHDrawingApi.OH_Drawing_RoundRectCreate(rect, xRad, yRad);
            OHDrawingApi.OH_Drawing_CanvasDrawRoundRect(_canvas, roundRect);
            OHDrawingApi.OH_Drawing_RoundRectDestroy(roundRect);
        }
        else
        {
            OHDrawingApi.OH_Drawing_CanvasDrawRect(_canvas, rect);
        }
        OHDrawingApi.OH_Drawing_RectDestroy(rect);
    }
}