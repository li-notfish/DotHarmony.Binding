using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// USBRequestTargetType 枚举
/// </summary>
public enum USBRequestTargetType
{
    UsbRequestTargetDevice = 0,
    UsbRequestTargetInterface = 1,
    UsbRequestTargetEndpoint = 2,
    UsbRequestTargetOther = 3
}

/// <summary>
/// USBControlRequestType 枚举
/// </summary>
public enum USBControlRequestType
{
    UsbRequestTypeStandard = 0,
    UsbRequestTypeClass = 1,
    UsbRequestTypeVendor = 2
}

/// <summary>
/// USBRequestDirection 枚举
/// </summary>
public enum USBRequestDirection
{
    UsbRequestDirToDevice = 0,
    UsbRequestDirFromDevice = 128
}