using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// InstanceType 枚举
/// </summary>
public enum InstanceType
{
    Taskpool = 2,
    Worker = 1,
    Main = 0,
    Custom = 3
}

/// <summary>
/// ResourceType 枚举
/// </summary>
public enum ResourceType
{
    PssMemory = 1,
    IonMemory = 2,
    AshmemMemory = 3,
    GpuMemory = 4,
    Fd = 5,
    Thread = 6
}