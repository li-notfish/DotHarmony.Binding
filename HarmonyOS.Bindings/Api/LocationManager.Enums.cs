using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LocationPolicy 枚举
/// </summary>
public enum LocationPolicy
{
    DefaultLocationService = 0,
    DisallowLocationService = 1,
    ForceOpenLocationService = 2
}