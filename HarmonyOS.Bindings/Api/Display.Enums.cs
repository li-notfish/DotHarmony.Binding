using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// FoldStatus 枚举
/// </summary>
public enum FoldStatus
{
    FOLD_STATUS_UNKNOWN = 0,
    FOLD_STATUS_EXPANDED = 1,
    FOLD_STATUS_FOLDED = 2,
    FOLD_STATUS_HALF_FOLDED = 3,
    FOLD_STATUS_EXPANDED_WITH_SECOND_EXPANDED = 11,
    FOLD_STATUS_EXPANDED_WITH_SECOND_HALF_FOLDED = 21,
    FOLD_STATUS_FOLDED_WITH_SECOND_HALF_FOLDED = 22,
    FOLD_STATUS_HALF_FOLDED_WITH_SECOND_HALF_FOLDED = 23,
    FOLD_STATUS_FOLDED_WITH_SECOND_EXPANDED = 12,
    FOLD_STATUS_HALF_FOLDED_WITH_SECOND_EXPANDED = 13
}

/// <summary>
/// FoldDisplayMode 枚举
/// </summary>
public enum FoldDisplayMode
{
    FOLD_DISPLAY_MODE_UNKNOWN = 0,
    FOLD_DISPLAY_MODE_FULL = 1,
    FOLD_DISPLAY_MODE_MAIN = 2,
    FOLD_DISPLAY_MODE_SUB = 3,
    FOLD_DISPLAY_MODE_COORDINATION
}

/// <summary>
/// DisplayState 枚举
/// </summary>
public enum DisplayState
{
    STATE_UNKNOWN = 0,
    STATE_OFF = 1,
    STATE_ON = 2,
    STATE_DOZE = 3,
    STATE_DOZE_SUSPEND = 4,
    STATE_VR = 5,
    STATE_ON_SUSPEND = 6
}

/// <summary>
/// Orientation 枚举
/// </summary>
public enum Orientation
{
    PORTRAIT = 0,
    LANDSCAPE = 1,
    PORTRAIT_INVERTED = 2,
    LANDSCAPE_INVERTED = 3
}

/// <summary>
/// ScreenShape 枚举
/// </summary>
public enum ScreenShape
{
    ROUND = 1,
    RECTANGLE = 0
}

/// <summary>
/// DisplaySourceMode 枚举
/// </summary>
public enum DisplaySourceMode
{
    MAIN = 1,
    NONE = 0,
    EXTEND = 3,
    MIRROR = 2,
    ALONE = 4
}

/// <summary>
/// CornerType 枚举
/// </summary>
public enum CornerType
{
    TOP_LEFT = 0,
    TOP_RIGHT = 1,
    BOTTOM_RIGHT = 2,
    BOTTOM_LEFT = 3
}