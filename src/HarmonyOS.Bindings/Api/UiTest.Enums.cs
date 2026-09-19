using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// MatchPattern 枚举
/// </summary>
public enum MatchPattern
{
    Equals = 0,
    Contains = 1,
    StartsWith = 2,
    EndsWith = 3,
    RegExp = 4,
    RegExpIcase = 5
}

/// <summary>
/// WindowMode 枚举
/// </summary>
public enum WindowMode
{
    Fullscreen = 0,
    Primary = 1,
    Secondary = 2,
    Floating = 3
}

/// <summary>
/// ResizeDirection 枚举
/// </summary>
public enum ResizeDirection
{
    Left = 0,
    Right = 1,
    Up = 2,
    Down = 3,
    LeftUp = 4,
    LeftDown = 5,
    RightUp = 6,
    RightDown = 7
}

/// <summary>
/// DisplayRotation 枚举
/// </summary>
public enum DisplayRotation
{
    Rotation0 = 0,
    Rotation90 = 1,
    Rotation180 = 2,
    Rotation270 = 3
}

/// <summary>
/// WindowChangeType 枚举
/// </summary>
public enum WindowChangeType
{
    WindowUndefined = 0,
    WindowAdded = 1,
    WindowRemoved = 2,
    WindowBoundsChanged = 3
}

/// <summary>
/// ComponentEventType 枚举
/// </summary>
public enum ComponentEventType
{
    ComponentUndefined = 0,
    ComponentClicked = 1,
    ComponentLongClicked = 2,
    ComponentScrollStart = 3,
    ComponentScrollEnd = 4,
    ComponentTextChanged = 5
}

/// <summary>
/// UiDirection 枚举
/// </summary>
public enum UiDirection
{
    Left = 0,
    Right = 1,
    Up = 2,
    Down = 3
}

/// <summary>
/// MouseButton 枚举
/// </summary>
public enum MouseButton
{
    MouseButtonLeft = 0,
    MouseButtonRight = 1,
    MouseButtonMiddle = 2
}

/// <summary>
/// PenKey 枚举
/// </summary>
public enum PenKey
{
    Handwriting = 0,
    Smart = 1,
    AirMouse = 2
}

/// <summary>
/// PenKeyOperation 枚举
/// </summary>
public enum PenKeyOperation
{
    Click = 0,
    DoubleClick = 1
}

/// <summary>
/// PenMode 枚举
/// </summary>
public enum PenMode
{
    Handwriting = 0,
    AirMouse = 1
}