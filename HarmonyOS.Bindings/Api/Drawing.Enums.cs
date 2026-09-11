using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// BlendMode 枚举
/// </summary>
public enum BlendMode
{
    Clear = 0,
    Src = 1,
    Dst = 2,
    SrcOver = 3,
    DstOver = 4,
    SrcIn = 5,
    DstIn = 6,
    SrcOut = 7,
    DstOut = 8,
    SrcAtop = 9,
    DstAtop = 10,
    Xor = 11,
    Plus = 12,
    Modulate = 13,
    Screen = 14,
    Overlay = 15,
    Darken = 16,
    Lighten = 17,
    ColorDodge = 18,
    ColorBurn = 19,
    HardLight = 20,
    SoftLight = 21,
    Difference = 22,
    Exclusion = 23,
    Multiply = 24,
    Hue = 25,
    Saturation = 26,
    Color = 27,
    Luminosity = 28
}

/// <summary>
/// VertexMode 枚举
/// </summary>
public enum VertexMode
{
    TrianglesVertexmode = 0,
    TrianglesstripVertexmode = 1,
    TrianglesfanVertexmode = 2
}

/// <summary>
/// PathDirection 枚举
/// </summary>
public enum PathDirection
{
    Clockwise = 0,
    CounterClockwise = 1
}

/// <summary>
/// PathFillType 枚举
/// </summary>
public enum PathFillType
{
    Winding = 0,
    EvenOdd = 1,
    InverseWinding = 2,
    InverseEvenOdd = 3
}

/// <summary>
/// PathMeasureMatrixFlags 枚举
/// </summary>
public enum PathMeasureMatrixFlags
{
    GetPositionMatrix = 0,
    GetTangentMatrix = 1,
    GetPositionAndTangentMatrix = 2
}

/// <summary>
/// PathOp 枚举
/// </summary>
public enum PathOp
{
    Difference = 0,
    Intersect = 1,
    Union = 2,
    Xor = 3,
    ReverseDifference = 4
}

/// <summary>
/// PathIteratorVerb 枚举
/// </summary>
public enum PathIteratorVerb
{
    Move = 0,
    Line = 1,
    Quad = 2,
    Conic = 3,
    Cubic = 4,
    Close = 5,
    [Description("CLOSE + 1")]
    Done
}

/// <summary>
/// PointMode 枚举
/// </summary>
public enum PointMode
{
    Points = 0,
    Lines = 1,
    Polygon = 2
}

/// <summary>
/// FilterMode 枚举
/// </summary>
public enum FilterMode
{
    FilterModeNearest = 0,
    FilterModeLinear = 1
}

/// <summary>
/// ShadowFlag 枚举
/// </summary>
public enum ShadowFlag
{
    None = 0,
    TransparentOccluder = 1,
    GeometricOnly = 2,
    All = 3
}

/// <summary>
/// ClipOp 枚举
/// </summary>
public enum ClipOp
{
    Difference = 0,
    Intersect = 1
}

/// <summary>
/// TextEncoding 枚举
/// </summary>
public enum TextEncoding
{
    TextEncodingUtf8 = 0,
    TextEncodingUtf16 = 1,
    TextEncodingUtf32 = 2,
    TextEncodingGlyphId = 3
}

/// <summary>
/// FontEdging 枚举
/// </summary>
public enum FontEdging
{
    Alias = 0,
    AntiAlias = 1,
    SubpixelAntiAlias = 2
}

/// <summary>
/// FontHinting 枚举
/// </summary>
public enum FontHinting
{
    None = 0,
    Slight = 1,
    Normal = 2,
    Full = 3
}

[Flags]
/// <summary>
/// FontMetricsFlags 枚举
/// </summary>
public enum FontMetricsFlags
{
    UnderlineThicknessValid = 1,
    UnderlinePositionValid = 2,
    StrikethroughThicknessValid = 4,
    StrikethroughPositionValid = 8,
    BoundsInvalid = 16
}

/// <summary>
/// RectType 枚举
/// </summary>
public enum RectType
{
    Default = 0,
    Transparent = 1,
    Fixedcolor = 2
}

/// <summary>
/// PathDashStyle 枚举
/// </summary>
public enum PathDashStyle
{
    Translate = 0,
    Rotate = 1,
    Morph = 2
}

/// <summary>
/// DrawingTileMode 枚举
/// </summary>
public enum DrawingTileMode
{
    Clamp = 0,
    Repeat = 1,
    Mirror = 2,
    Decal = 3
}

/// <summary>
/// JoinStyle 枚举
/// </summary>
public enum JoinStyle
{
    MiterJoin = 0,
    RoundJoin = 1,
    BevelJoin = 2
}

/// <summary>
/// CapStyle 枚举
/// </summary>
public enum CapStyle
{
    FlatCap = 0,
    SquareCap = 1,
    RoundCap = 2
}

/// <summary>
/// BlurType 枚举
/// </summary>
public enum BlurType
{
    Normal = 0,
    Solid = 1,
    Outer = 2,
    Inner = 3
}

/// <summary>
/// ScaleToFit 枚举
/// </summary>
public enum ScaleToFit
{
    FillScaleToFit = 0,
    StartScaleToFit = 1,
    CenterScaleToFit = 2,
    EndScaleToFit = 3
}

/// <summary>
/// RegionOp 枚举
/// </summary>
public enum RegionOp
{
    Difference = 0,
    Intersect = 1,
    Union = 2,
    Xor = 3,
    ReverseDifference = 4,
    Replace = 5
}

/// <summary>
/// CornerPos 枚举
/// </summary>
public enum CornerPos
{
    TopLeftPos = 0,
    TopRightPos = 1,
    BottomRightPos = 2,
    BottomLeftPos = 3
}

/// <summary>
/// SrcRectConstraint 枚举
/// </summary>
public enum SrcRectConstraint
{
    Strict = 0,
    Fast = 1
}