using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PointerStyle 枚举
/// </summary>
public enum PointerStyle
{
    Default = 0,
    East = 1,
    West = 2,
    South = 3,
    North = 4,
    WestEast = 5,
    NorthSouth = 6,
    NorthEast = 7,
    NorthWest = 8,
    SouthEast = 9,
    SouthWest = 10,
    NorthEastSouthWest = 11,
    NorthWestSouthEast = 12,
    Cross = 13,
    CursorCopy = 14,
    CursorForbid = 15,
    ColorSucker = 16,
    HandGrabbing = 17,
    HandOpen = 18,
    HandPointing = 19,
    Help = 20,
    Move = 21,
    ResizeLeftRight = 22,
    ResizeUpDown = 23,
    ScreenshotChoose = 24,
    ScreenshotCursor = 25,
    TextCursor = 26,
    ZoomIn = 27,
    ZoomOut = 28,
    MiddleBtnEast = 29,
    MiddleBtnWest = 30,
    MiddleBtnSouth = 31,
    MiddleBtnNorth = 32,
    MiddleBtnNorthSouth = 33,
    MiddleBtnNorthEast = 34,
    MiddleBtnNorthWest = 35,
    MiddleBtnSouthEast = 36,
    MiddleBtnSouthWest = 37,
    MiddleBtnNorthSouthWestEast = 38,
    HorizontalTextCursor = 39,
    CursorCross = 40,
    CursorCircle = 41,
    Loading = 42,
    Running = 43,
    MiddleBtnEastWest = 44,
    RunningLeft = 45,
    RunningRight = 46,
    AechDeveloperDefinedIcon = 47,
    ScreenrecorderCursor = 48,
    LaserCursor = 49,
    LaserCursorDot = 50,
    LaserCursorDotRed = 51,
    [Description("-100")]
    DeveloperDefinedIcon
}

/// <summary>
/// PrimaryButton 枚举
/// </summary>
public enum PrimaryButton
{
    Left = 0,
    Right = 1
}

/// <summary>
/// RightClickType 枚举
/// </summary>
public enum RightClickType
{
    TouchpadRightButton = 1,
    TouchpadLeftButton = 2,
    TouchpadTwoFingerTap = 3,
    TouchpadTwoFingerTapOrRightButton = 4,
    TouchpadTwoFingerTapOrLeftButton = 5
}