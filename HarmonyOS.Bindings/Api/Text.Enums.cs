using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// TextAlign 枚举
/// </summary>
public enum TextAlign
{
    Left = 0,
    Right = 1,
    Center = 2,
    Justify = 3,
    Start = 4,
    End = 5
}

/// <summary>
/// TextVerticalAlign 枚举
/// </summary>
public enum TextVerticalAlign
{
    Baseline = 0,
    Bottom = 1,
    Center = 2,
    Top = 3
}

/// <summary>
/// TextDirection 枚举
/// </summary>
public enum TextDirection
{
    Rtl = 0,
    Ltr = 1
}

/// <summary>
/// BreakStrategy 枚举
/// </summary>
public enum BreakStrategy
{
    Greedy = 0,
    HighQuality = 1,
    Balanced = 2
}

/// <summary>
/// WordBreak 枚举
/// </summary>
public enum WordBreak
{
    Normal = 0,
    BreakAll = 1,
    BreakWord = 2,
    BreakHyphen = 3
}

/// <summary>
/// TextDecorationType 枚举
/// </summary>
public enum TextDecorationType
{
    None = 0,
    Underline = 1,
    Overline = 2,
    LineThrough = 4
}

/// <summary>
/// TextDecorationStyle 枚举
/// </summary>
public enum TextDecorationStyle
{
    Solid = 0,
    Double = 1,
    Dotted = 2,
    Dashed = 3,
    Wavy = 4
}

/// <summary>
/// FontWeight 枚举
/// </summary>
public enum FontWeight
{
    W100 = 0,
    W200 = 1,
    W300 = 2,
    W400 = 3,
    W500 = 4,
    W600 = 5,
    W700 = 6,
    W800 = 7,
    W900 = 8
}

/// <summary>
/// FontStyle 枚举
/// </summary>
public enum FontStyle
{
    Normal = 0,
    Italic = 1,
    Oblique = 2
}

/// <summary>
/// FontWidth 枚举
/// </summary>
public enum FontWidth
{
    UltraCondensed = 1,
    ExtraCondensed = 2,
    Condensed = 3,
    SemiCondensed = 4,
    Normal = 5,
    SemiExpanded = 6,
    Expanded = 7,
    ExtraExpanded = 8,
    UltraExpanded = 9
}

[Flags]
/// <summary>
/// TextHeightBehavior 枚举
/// </summary>
public enum TextHeightBehavior
{
    All = 0,
    DisableFirstAscent = 1,
    DisableLastAscent = 2,
    [Description("0x1 | 0x2")]
    DisableAll
}

/// <summary>
/// TextBaseline 枚举
/// </summary>
public enum TextBaseline
{
    Alphabetic = 0,
    Ideographic = 1
}

/// <summary>
/// EllipsisMode 枚举
/// </summary>
public enum EllipsisMode
{
    Start = 0,
    Middle = 1,
    End = 2,
    MultilineStart = 3,
    MultilineMiddle = 4
}

/// <summary>
/// LineHeightStyle 枚举
/// </summary>
public enum LineHeightStyle
{
    FontSize = 0,
    FontHeight = 1
}

/// <summary>
/// TextBadgeType 枚举
/// </summary>
public enum TextBadgeType
{
    TextBadgeNone = 0,
    TextSuperscript = 1,
    TextSubscript = 2
}

/// <summary>
/// PlaceholderAlignment 枚举
/// </summary>
public enum PlaceholderAlignment
{
    OffsetAtBaseline = 0,
    AboveBaseline = 1,
    BelowBaseline = 2,
    TopOfRowBox = 3,
    BottomOfRowBox = 4,
    CenterOfRowBox = 5,
    FollowParagraph = 6
}

[Flags]
/// <summary>
/// SystemFontType 枚举
/// </summary>
public enum SystemFontType
{
    All = 1,
    Generic = 2,
    Stylish = 4,
    Installed = 8,
    Customized = 16
}

/// <summary>
/// RectWidthStyle 枚举
/// </summary>
public enum RectWidthStyle
{
    Tight = 0,
    Max = 1
}

/// <summary>
/// RectHeightStyle 枚举
/// </summary>
public enum RectHeightStyle
{
    Tight = 0,
    Max = 1,
    IncludeLineSpaceMiddle = 2,
    IncludeLineSpaceTop = 3,
    IncludeLineSpaceBottom = 4,
    Strut = 5
}

/// <summary>
/// Affinity 枚举
/// </summary>
public enum Affinity
{
    Upstream = 0,
    Downstream = 1
}

/// <summary>
/// TextHighContrast 枚举
/// </summary>
public enum TextHighContrast
{
    TextFollowSystemHighContrast = 0,
    TextAppDisableHighContrast = 1,
    TextAppEnableHighContrast = 2
}

/// <summary>
/// TextUndefinedGlyphDisplay 枚举
/// </summary>
public enum TextUndefinedGlyphDisplay
{
    UseDefault = 0,
    UseTofu = 1
}

/// <summary>
/// TextProcessState 枚举
/// </summary>
public enum TextProcessState
{
    Init = 0,
    Indexed = 1,
    Shaped = 2,
    LineBroken = 3,
    Formatted = 4,
    Paint = 5,
    UpdateAttribute = 6
}

/// <summary>
/// TextDisplayState 枚举
/// </summary>
public enum TextDisplayState
{
    Unknown = 0,
    All = 1,
    Clip = 2,
    Omitted = 3
}