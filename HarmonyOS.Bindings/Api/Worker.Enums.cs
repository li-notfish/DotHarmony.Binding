using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ThreadWorkerPriority 枚举
/// </summary>
public enum ThreadWorkerPriority
{
    High = 0,
    Medium = 1,
    Low = 2,
    Idle = 3,
    Deadline = 4,
    Vip = 5
}