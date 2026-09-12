using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ResourceManagerDirection 枚举
/// </summary>
public enum ResourceManagerDirection
{
    DirectionVertical = 0,
    DirectionHorizontal = 1
}

/// <summary>
/// ResourceManagerDeviceType 枚举
/// </summary>
public enum ResourceManagerDeviceType
{
    DeviceTypePhone = 0,
    DeviceTypeTablet = 1,
    DeviceTypeCar = 2,
    DeviceTypePc = 3,
    DeviceTypeTV = 4,
    DeviceTypeWearable = 6,
    DeviceType2In1 = 7
}

/// <summary>
/// ResourceManagerScreenDensity 枚举
/// </summary>
public enum ResourceManagerScreenDensity
{
    ScreenSdpi = 120,
    ScreenMdpi = 160,
    ScreenLdpi = 240,
    ScreenXldpi = 320,
    ScreenXxldpi = 480,
    ScreenXxxldpi = 640
}

/// <summary>
/// ResourceManagerColorMode 枚举
/// </summary>
public enum ResourceManagerColorMode
{
    Dark = 0,
    Light = 1
}