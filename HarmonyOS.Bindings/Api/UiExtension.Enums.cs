using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// EventFlag 枚举
/// </summary>
public enum EventFlag
{
    EventPanGestureLeft = 1,
    EventPanGestureRight = 2,
    EventLongPress = 512,
    EventPanGestureUp = 4,
    EventClick = 256,
    EventNone = 0,
    EventPanGestureDown = 8
}

/// <summary>
/// RectChangeReason 枚举
/// </summary>
public enum RectChangeReason
{
    HostWindowRectChange = 1
}