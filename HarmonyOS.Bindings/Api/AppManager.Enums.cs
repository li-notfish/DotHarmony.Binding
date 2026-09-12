using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ProcessState 枚举
/// </summary>
public enum ProcessState
{
    StateCreate,
    StateForeground,
    StateActive,
    StateBackground,
    StateDestroy
}