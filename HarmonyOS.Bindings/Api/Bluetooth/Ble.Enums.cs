using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// GattWriteType 枚举
/// </summary>
public enum GattWriteType
{
    Write = 1,
    WriteNoResponse = 2
}

/// <summary>
/// ScanDuty 枚举
/// </summary>
public enum ScanDuty
{
    ScanModeLowPower = 0,
    ScanModeBalanced = 1,
    ScanModeLowLatency = 2
}

/// <summary>
/// MatchMode 枚举
/// </summary>
public enum MatchMode
{
    MatchModeAggressive = 1,
    MatchModeSticky = 2
}

/// <summary>
/// AdvertisingState 枚举
/// </summary>
public enum AdvertisingState
{
    Started = 1,
    Enabled = 2,
    Disabled = 3,
    Stopped = 4
}

/// <summary>
/// PhyType 枚举
/// </summary>
public enum PhyType
{
    PhyLe1M = 1,
    PhyLeAllSupported = 255
}

/// <summary>
/// ScanReportMode 枚举
/// </summary>
public enum ScanReportMode
{
    Normal = 1,
    Batch = 2,
    FenceSensitivityLow = 10,
    FenceSensitivityHigh = 11
}

/// <summary>
/// ScanReportType 枚举
/// </summary>
public enum ScanReportType
{
    OnFound = 1,
    OnLost = 2,
    OnBatch = 3
}

/// <summary>
/// BleProfile 枚举
/// </summary>
public enum BleProfile
{
    Gatt = 1,
    GattClient = 2,
    GattServer = 3
}

/// <summary>
/// ConnectionParam 枚举
/// </summary>
public enum ConnectionParam
{
    LowPower = 1,
    Balanced = 2,
    High = 3
}

/// <summary>
/// GattDisconnectReason 枚举
/// </summary>
public enum GattDisconnectReason
{
    ConnTimeout = 1,
    ConnTerminatePeerUser = 2,
    ConnTerminateLocalHost = 3,
    ConnUnknown = 4
}

/// <summary>
/// BlePhy 枚举
/// </summary>
public enum BlePhy
{
    BlePhy1M = 1,
    BlePhy2M = 2,
    BlePhyCoded = 3
}

/// <summary>
/// CodedPhyMode 枚举
/// </summary>
public enum CodedPhyMode
{
    BlePhyCodedS2 = 1,
    BlePhyCodedS8 = 2
}