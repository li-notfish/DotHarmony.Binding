using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DeviceTypes 枚举
/// </summary>
public enum DeviceTypes
{
    [Description("default")]
    TYPE_DEFAULT,
    [Description("phone")]
    TYPE_PHONE,
    [Description("tablet")]
    TYPE_TABLET,
    [Description("2in1")]
    TYPE_2IN1,
    [Description("tv")]
    TYPE_TV,
    [Description("wearable")]
    TYPE_WEARABLE,
    [Description("car")]
    TYPE_CAR
}

/// <summary>
/// PerformanceClassLevel 枚举
/// </summary>
public enum PerformanceClassLevel
{
    CLASS_LEVEL_HIGH,
    CLASS_LEVEL_MEDIUM,
    CLASS_LEVEL_LOW
}