using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SystemLoadLevel 枚举
/// </summary>
public enum SystemLoadLevel
{
    Low = 0,
    Normal = 1,
    Medium = 2,
    High = 3,
    Overheated = 4,
    Warning = 5,
    Emergency = 6,
    Escape = 7
}