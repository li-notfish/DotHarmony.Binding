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

/// <summary>
/// WorkerPriority 枚举
/// </summary>
public enum WorkerPriority
{
    Immediate = 1,
    High = 2,
    Low = 3,
    Idle = 4
}