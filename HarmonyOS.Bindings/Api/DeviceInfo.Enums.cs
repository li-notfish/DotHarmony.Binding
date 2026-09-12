using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DeviceTypes 枚举
/// </summary>
public enum DeviceTypes
{
    [Description("default")]
    TypeDefault,
    [Description("phone")]
    TypePhone,
    [Description("tablet")]
    TypeTablet,
    [Description("2in1")]
    Type2In1,
    [Description("tv")]
    TypeTV,
    [Description("wearable")]
    TypeWearable,
    [Description("car")]
    TypeCar
}

/// <summary>
/// PerformanceClassLevel 枚举
/// </summary>
public enum PerformanceClassLevel
{
    ClassLevelHigh,
    ClassLevelMedium,
    ClassLevelLow
}