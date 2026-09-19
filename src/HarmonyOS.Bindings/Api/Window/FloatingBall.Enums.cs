using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// FloatingBallState 枚举
/// </summary>
public enum FloatingBallState
{
    Started = 1,
    Stopped = 2
}

/// <summary>
/// FloatingBallTemplate 枚举
/// </summary>
public enum FloatingBallTemplate
{
    Static = 1,
    Normal = 2,
    Emphatic = 3,
    Simple = 4
}

/// <summary>
/// FloatingBallTextUpdateAnimationType 枚举
/// </summary>
public enum FloatingBallTextUpdateAnimationType
{
    AnimationNone = 0,
    AnimationOpacity = 1
}