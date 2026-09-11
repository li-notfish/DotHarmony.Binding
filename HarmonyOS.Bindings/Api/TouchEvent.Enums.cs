using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// TouchEventAction 枚举
/// </summary>
public enum TouchEventAction
{
    Cancel = 0,
    Down = 1,
    Move = 2,
    Up = 3,
    PullDown = 4,
    PullMove = 5,
    PullUp = 6
}

/// <summary>
/// TouchEventToolType 枚举
/// </summary>
public enum TouchEventToolType
{
    Finger = 0,
    Pen = 1,
    Rubber = 2,
    Brush = 3,
    Pencil = 4,
    Airbrush = 5,
    Mouse = 6,
    Lens = 7
}

/// <summary>
/// TouchEventSourceType 枚举
/// </summary>
public enum TouchEventSourceType
{
    TouchScreen = 0,
    Pen = 1,
    TouchPad = 2
}