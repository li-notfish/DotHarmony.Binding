using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// BatteryPluggedType 枚举
/// </summary>
public enum BatteryPluggedType
{
    None,
    Ac,
    Usb,
    Wireless
}

/// <summary>
/// BatteryChargeState 枚举
/// </summary>
public enum BatteryChargeState
{
    None,
    Enable,
    Disable,
    Full
}

/// <summary>
/// BatteryHealthState 枚举
/// </summary>
public enum BatteryHealthState
{
    Unknown,
    Good,
    Overheat,
    Overvoltage,
    Cold,
    Dead
}

/// <summary>
/// BatteryCapacityLevel 枚举
/// </summary>
public enum BatteryCapacityLevel
{
    LevelNone,
    LevelFull,
    LevelHigh,
    LevelNormal,
    LevelLow,
    LevelWarning,
    LevelCritical,
    LevelShutdown
}

/// <summary>
/// CommonEventBatteryChangedKey 枚举
/// </summary>
public enum CommonEventBatteryChangedKey
{
    [Description("soc")]
    ExtraSoc,
    [Description("chargeState")]
    ExtraChargeState,
    [Description("healthState")]
    ExtraHealthState,
    [Description("pluggedType")]
    ExtraPluggedType,
    [Description("voltage")]
    ExtraVoltage,
    [Description("technology")]
    ExtraTechnology,
    [Description("temperature")]
    ExtraTemperature,
    [Description("present")]
    ExtraPresent,
    [Description("capacityLevel")]
    ExtraCapacityLevel
}