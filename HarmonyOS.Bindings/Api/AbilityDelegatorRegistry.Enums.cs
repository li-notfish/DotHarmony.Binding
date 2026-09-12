using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// AbilityLifecycleState 枚举
/// </summary>
public enum AbilityLifecycleState
{
    Uninitialized = 0,
    Create = 1,
    Foreground = 2,
    Background = 3,
    Destroy = 4
}