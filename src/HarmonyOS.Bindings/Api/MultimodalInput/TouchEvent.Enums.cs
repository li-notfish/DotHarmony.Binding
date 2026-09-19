using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// MultimodalInputTouchEventAction 枚举
/// </summary>
public enum MultimodalInputTouchEventAction
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
/// MultimodalInputTouchEventToolType 枚举
/// </summary>
public enum MultimodalInputTouchEventToolType
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
/// MultimodalInputTouchEventSourceType 枚举
/// </summary>
public enum MultimodalInputTouchEventSourceType
{
    TouchScreen = 0,
    Pen = 1,
    TouchPad = 2
}