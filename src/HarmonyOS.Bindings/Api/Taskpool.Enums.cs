using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// TaskpoolPriority 枚举
/// </summary>
public enum TaskpoolPriority
{
    High = 0,
    Medium = 1,
    Low = 2,
    Idle = 3
}

/// <summary>
/// TaskpoolState 枚举
/// </summary>
public enum TaskpoolState
{
    Waiting = 1,
    Running = 2,
    Canceled = 3
}