using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Subclass 枚举
/// </summary>
public enum Subclass
{
    SubclassUncategorized = 0,
    SubclassJoystick = 1,
    SubclassGamepad = 2,
    SubclassRemoteControl = 3,
    SubclassSensingDevice = 4,
    SubclassDigitizerTablet = 5,
    SubclassCardReader = 6,
    SubclassKeyboard = 64,
    SubclassMouse = 128,
    SubclassCombo = 192
}

/// <summary>
/// ReportType 枚举
/// </summary>
public enum ReportType
{
    ReportTypeInput = 1,
    ReportTypeOutput = 2,
    ReportTypeFeature = 3
}

/// <summary>
/// ServiceType 枚举
/// </summary>
public enum ServiceType
{
    ServiceNoTraffic = 0,
    ServiceBestEffort = 1,
    ServiceGuaranteed = 2
}

/// <summary>
/// ErrorReason 枚举
/// </summary>
public enum ErrorReason
{
    RspSuccess = 0,
    RspNotReady = 1,
    RspInvalidReportId = 2,
    RspUnsupportedReq = 3,
    RspInvalidParam = 4,
    RspUnknown = 14
}

/// <summary>
/// ProtocolType 枚举
/// </summary>
public enum ProtocolType
{
    ProtocolBootMode = 0,
    ProtocolReportMode = 1
}