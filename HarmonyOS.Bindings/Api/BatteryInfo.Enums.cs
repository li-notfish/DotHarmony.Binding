using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// BatteryPluggedType 枚举
/// </summary>
public enum BatteryPluggedType
{
    NONE,
    AC,
    USB,
    WIRELESS
}

/// <summary>
/// BatteryChargeState 枚举
/// </summary>
public enum BatteryChargeState
{
    NONE,
    ENABLE,
    DISABLE,
    FULL
}

/// <summary>
/// BatteryHealthState 枚举
/// </summary>
public enum BatteryHealthState
{
    UNKNOWN,
    GOOD,
    OVERHEAT,
    OVERVOLTAGE,
    COLD,
    DEAD
}

/// <summary>
/// BatteryCapacityLevel 枚举
/// </summary>
public enum BatteryCapacityLevel
{
    LEVEL_NONE,
    LEVEL_FULL,
    LEVEL_HIGH,
    LEVEL_NORMAL,
    LEVEL_LOW,
    LEVEL_WARNING,
    LEVEL_CRITICAL,
    LEVEL_SHUTDOWN
}

/// <summary>
/// CommonEventBatteryChangedKey 枚举
/// </summary>
public enum CommonEventBatteryChangedKey
{
    [Description("soc")]
    EXTRA_SOC,
    [Description("chargeState")]
    EXTRA_CHARGE_STATE,
    [Description("healthState")]
    EXTRA_HEALTH_STATE,
    [Description("pluggedType")]
    EXTRA_PLUGGED_TYPE,
    [Description("voltage")]
    EXTRA_VOLTAGE,
    [Description("technology")]
    EXTRA_TECHNOLOGY,
    [Description("temperature")]
    EXTRA_TEMPERATURE,
    [Description("present")]
    EXTRA_PRESENT,
    [Description("capacityLevel")]
    EXTRA_CAPACITY_LEVEL
}