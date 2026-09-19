using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ApplicationAbilityDelegatorRegistryAbilityLifecycleState 枚举
/// </summary>
public enum ApplicationAbilityDelegatorRegistryAbilityLifecycleState
{
    Uninitialized = 0,
    Create = 1,
    Foreground = 2,
    Background = 3,
    Destroy = 4
}