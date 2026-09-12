using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// FoldStatus 枚举
/// </summary>
public enum FoldStatus
{
    FoldStatusUnknown = 0,
    FoldStatusExpanded = 1,
    FoldStatusFolded = 2,
    FoldStatusHalfFolded = 3,
    FoldStatusExpandedWithSecondExpanded = 11,
    FoldStatusExpandedWithSecondHalfFolded = 21,
    FoldStatusFoldedWithSecondHalfFolded = 22,
    FoldStatusHalfFoldedWithSecondHalfFolded = 23,
    FoldStatusFoldedWithSecondExpanded = 12,
    FoldStatusHalfFoldedWithSecondExpanded = 13
}

/// <summary>
/// FoldDisplayMode 枚举
/// </summary>
public enum FoldDisplayMode
{
    FoldDisplayModeUnknown = 0,
    FoldDisplayModeFull = 1,
    FoldDisplayModeMain = 2,
    FoldDisplayModeSub = 3,
    FoldDisplayModeCoordination
}

/// <summary>
/// DisplayState 枚举
/// </summary>
public enum DisplayState
{
    StateUnknown = 0,
    StateOff = 1,
    StateOn = 2,
    StateDoze = 3,
    StateDozeSuspend = 4,
    StateVR = 5,
    StateOnSuspend = 6
}

/// <summary>
/// Orientation 枚举
/// </summary>
public enum Orientation
{
    Portrait = 0,
    Landscape = 1,
    PortraitInverted = 2,
    LandscapeInverted = 3
}

/// <summary>
/// ScreenShape 枚举
/// </summary>
public enum ScreenShape
{
    Round = 1,
    Rectangle = 0
}

/// <summary>
/// DisplaySourceMode 枚举
/// </summary>
public enum DisplaySourceMode
{
    Main = 1,
    None = 0,
    Extend = 3,
    Mirror = 2,
    Alone = 4
}

/// <summary>
/// CornerType 枚举
/// </summary>
public enum CornerType
{
    TopLeft = 0,
    TopRight = 1,
    BottomRight = 2,
    BottomLeft = 3
}