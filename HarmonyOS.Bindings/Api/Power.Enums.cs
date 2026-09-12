using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DevicePowerMode 枚举
/// </summary>
public enum DevicePowerMode
{
    ModeNormal = 600,
    ModePowerSave,
    ModePerformance,
    ModeExtremePowerSave,
    ModeCustomPowerSave = 650
}

/// <summary>
/// PowerKeyFilteringStrategy 枚举
/// </summary>
public enum PowerKeyFilteringStrategy
{
    DisableLongPressFiltering = 0,
    LongPressFilteringOnce = 1
}