using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// AppPreloadType 枚举
/// </summary>
public enum AppPreloadType
{
    Unspecified = 0,
    TypeCreateProcess = 1,
    TypeCreateAbilityStage = 2,
    TypeCreateWindowStage = 3,
    TypeCreateBackgroundAbility = 4
}