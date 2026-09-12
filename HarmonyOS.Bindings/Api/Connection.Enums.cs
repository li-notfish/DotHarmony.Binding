using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// BluetoothTransport 枚举
/// </summary>
public enum BluetoothTransport
{
    TransportBrEdr = 0,
    TransportLe = 1,
    TransportDual = 2,
    TransportUnknown = 3
}

/// <summary>
/// ScanMode 枚举
/// </summary>
public enum ScanMode
{
    ScanModeNone = 0,
    ScanModeConnectable = 1,
    ScanModeGeneralDiscoverable = 2,
    ScanModeLimitedDiscoverable = 3,
    ScanModeConnectableGeneralDiscoverable = 4,
    ScanModeConnectableLimitedDiscoverable = 5
}

/// <summary>
/// BondState 枚举
/// </summary>
public enum BondState
{
    BondStateInvalid = 0,
    BondStateBonding = 1,
    BondStateBonded = 2
}

/// <summary>
/// DeviceChargeState 枚举
/// </summary>
public enum DeviceChargeState
{
    DeviceNormalChargeNotCharged = 0,
    DeviceNormalChargeInCharging = 1,
    DeviceSuperChargeNotCharged = 2,
    DeviceSuperChargeInCharging = 3
}

/// <summary>
/// UnbondCause 枚举
/// </summary>
public enum UnbondCause
{
    UserRemoved = 0,
    RemoteDeviceDown = 1,
    AuthFailure = 2,
    AuthRejected = 3,
    InternalError = 4
}

/// <summary>
/// HashAlgorithmType 枚举
/// </summary>
public enum HashAlgorithmType
{
    HashAlgorithmSha256 = 0
}

/// <summary>
/// AclState 枚举
/// </summary>
public enum AclState
{
    StateConnected = 0,
    StateDisconnected = 1
}