using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// FloatViewTemplateType 枚举
/// </summary>
public enum FloatViewTemplateType
{
    RoundedRectangle = 0,
    HorizontalBar = 1
}

/// <summary>
/// FloatViewState 枚举
/// </summary>
public enum FloatViewState
{
    Started = 1,
    Hidden = 2,
    Stopped = 3,
    InSidebar = 4,
    InFloatingBall = 5,
    Error = 6
}