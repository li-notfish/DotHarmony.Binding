using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// MultimodalInputMouseEventAction 枚举
/// </summary>
public enum MultimodalInputMouseEventAction
{
    Cancel = 0,
    Move = 1,
    ButtonDown = 2,
    ButtonUp = 3,
    AxisBegin = 4,
    AxisUpdate = 5,
    AxisEnd = 6,
    ActionDown = 7,
    ActionUp = 8
}

/// <summary>
/// MultimodalInputMouseEventButton 枚举
/// </summary>
public enum MultimodalInputMouseEventButton
{
    Left = 0,
    Middle = 1,
    Right = 2,
    Side = 3,
    Extra = 4,
    Forward = 5,
    Back = 6,
    Task = 7
}

/// <summary>
/// Axis 枚举
/// </summary>
public enum Axis
{
    ScrollVertical = 0,
    ScrollHorizontal = 1,
    Pinch = 2
}

/// <summary>
/// ToolType 枚举
/// </summary>
public enum ToolType
{
    Unknown = 0,
    Mouse = 1,
    Joystick = 2,
    Touchpad = 3
}