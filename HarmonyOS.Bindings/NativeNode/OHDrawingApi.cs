#nullable enable
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>OH_Drawing 2D 绘制 API 互操作层（libnative_drawing.so，drawing_*.h）</summary>
internal static unsafe partial class OHDrawingApi
{
    private const string DrawingLib = "libnative_drawing.so";

    // ───────────────────────── 画布（drawing_canvas.h）─────────────────────────

    [LibraryImport(DrawingLib)]
    internal static partial OH_Drawing_Canvas* OH_Drawing_CanvasCreate();

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasDestroy(OH_Drawing_Canvas* canvas);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasSave(OH_Drawing_Canvas* canvas);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasRestore(OH_Drawing_Canvas* canvas);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasScale(OH_Drawing_Canvas* canvas, float sx, float sy);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasRotate(OH_Drawing_Canvas* canvas, float degrees, float px, float py);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasTranslate(OH_Drawing_Canvas* canvas, float dx, float dy);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasAttachPen(OH_Drawing_Canvas* canvas, OH_Drawing_Pen* pen);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasDetachPen(OH_Drawing_Canvas* canvas);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasAttachBrush(OH_Drawing_Canvas* canvas, OH_Drawing_Brush* brush);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasDetachBrush(OH_Drawing_Canvas* canvas);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasDrawPath(OH_Drawing_Canvas* canvas, OH_Drawing_Path* path);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasDrawLine(
        OH_Drawing_Canvas* canvas, float xStart, float yStart, float xEnd, float yEnd);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasDrawCircle(
        OH_Drawing_Canvas* canvas, OH_Drawing_Point* center, float radius);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasDrawRect(OH_Drawing_Canvas* canvas, OH_Drawing_Rect* rect);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasDrawRoundRect(
        OH_Drawing_Canvas* canvas, OH_Drawing_RoundRect* roundRect);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasDrawColor(OH_Drawing_Canvas* canvas, uint color, int blendMode);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasClear(OH_Drawing_Canvas* canvas, uint color);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasClipRect(
        OH_Drawing_Canvas* canvas, OH_Drawing_Rect* rect, int clipOp, [MarshalAs(UnmanagedType.U1)] bool antialias);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_CanvasClipPath(
        OH_Drawing_Canvas* canvas, OH_Drawing_Path* path, int clipOp, [MarshalAs(UnmanagedType.U1)] bool antialias);

    // ───────────────────────── 路径（drawing_path.h）─────────────────────────

    [LibraryImport(DrawingLib)]
    internal static partial OH_Drawing_Path* OH_Drawing_PathCreate();

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PathDestroy(OH_Drawing_Path* path);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PathMoveTo(OH_Drawing_Path* path, float x, float y);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PathLineTo(OH_Drawing_Path* path, float x, float y);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PathQuadTo(
        OH_Drawing_Path* path, float ctrlX, float ctrlY, float endX, float endY);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PathCubicTo(
        OH_Drawing_Path* path, float ctrlX1, float ctrlY1, float ctrlX2, float ctrlY2, float endX, float endY);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PathArcTo(
        OH_Drawing_Path* path, float x1, float y1, float x2, float y2, float startAngle, float sweepAngle);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PathClose(OH_Drawing_Path* path);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PathReset(OH_Drawing_Path* path);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PathSetFillType(OH_Drawing_Path* path, int fillType);
}
/// <summary>OH_Drawing 画笔/画刷/几何/颜色互操作（同库 libnative_drawing.so）</summary>
internal static unsafe partial class OHDrawingApi
{
    [LibraryImport(DrawingLib)]
    internal static partial OH_Drawing_Pen* OH_Drawing_PenCreate();

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PenDestroy(OH_Drawing_Pen* pen);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PenSetColor(OH_Drawing_Pen* pen, uint color);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PenSetWidth(OH_Drawing_Pen* pen, float width);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PenSetCap(OH_Drawing_Pen* pen, int capStyle);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PenSetJoin(OH_Drawing_Pen* pen, int joinStyle);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PenSetAntiAlias(OH_Drawing_Pen* pen, [MarshalAs(UnmanagedType.U1)] bool isAntiAlias);

    [LibraryImport(DrawingLib)]
    internal static partial OH_Drawing_Brush* OH_Drawing_BrushCreate();

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_BrushDestroy(OH_Drawing_Brush* brush);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_BrushSetColor(OH_Drawing_Brush* brush, uint color);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_BrushSetAntiAlias(OH_Drawing_Brush* brush, [MarshalAs(UnmanagedType.U1)] bool isAntiAlias);

    [LibraryImport(DrawingLib)]
    internal static partial OH_Drawing_Rect* OH_Drawing_RectCreate(float left, float top, float right, float bottom);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_RectDestroy(OH_Drawing_Rect* rect);

    [LibraryImport(DrawingLib)]
    internal static partial OH_Drawing_RoundRect* OH_Drawing_RoundRectCreate(OH_Drawing_Rect* rect, float xRad, float yRad);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_RoundRectDestroy(OH_Drawing_RoundRect* roundRect);

    [LibraryImport(DrawingLib)]
    internal static partial OH_Drawing_Point* OH_Drawing_PointCreate(float x, float y);

    [LibraryImport(DrawingLib)]
    internal static partial void OH_Drawing_PointDestroy(OH_Drawing_Point* point);

    [LibraryImport(DrawingLib)]
    internal static partial uint OH_Drawing_ColorSetArgb(uint a, uint r, uint g, uint b);

    /// <summary>OH_Drawing_BlendMode.SRC_OVER（DrawColor 用）</summary>
    internal const int BlendModeSrcOver = 3;

    /// <summary>OH_Drawing_CanvasClipOp.INTERSECT</summary>
    internal const int ClipOpIntersect = 1;

    /// <summary>OH_Drawing_PathFillType.WINDING</summary>
    internal const int PathFillTypeWinding = 0;
}

// ───────────────────────── ABI 不透明类型 ─────────────────────────

/// <summary>OH_Drawing_Canvas（不透明）</summary>
public struct OH_Drawing_Canvas { }

/// <summary>OH_Drawing_Path（不透明）</summary>
public struct OH_Drawing_Path { }

/// <summary>OH_Drawing_Pen（不透明）</summary>
public struct OH_Drawing_Pen { }

/// <summary>OH_Drawing_Brush（不透明）</summary>
public struct OH_Drawing_Brush { }

/// <summary>OH_Drawing_Rect（不透明）</summary>
public struct OH_Drawing_Rect { }

/// <summary>OH_Drawing_RoundRect（不透明）</summary>
public struct OH_Drawing_RoundRect { }

/// <summary>OH_Drawing_Point（不透明）</summary>
public struct OH_Drawing_Point { }

/// <summary>
/// ArkUI_DrawContext（opaque，[OH_ArkUI_DrawContext_GetCanvas]/[OH_ArkUI_DrawContext_GetSize] 访问器
/// 见 ArkUICustomEventApi.cs——头文件不公开字段布局，勿直接按结构体读）。
/// </summary>
public struct ArkUI_DrawContext { }
