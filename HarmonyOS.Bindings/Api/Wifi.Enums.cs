using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// WifiWifiSecurityType 枚举
/// </summary>
public enum WifiWifiSecurityType
{
    WifiSecTypeInvalid = 0,
    WifiSecTypeOpen = 1,
    WifiSecTypeWep = 2,
    WifiSecTypePsk = 3,
    WifiSecTypeSae = 4
}

/// <summary>
/// ConnState 枚举
/// </summary>
public enum ConnState
{
    Scanning,
    Connecting,
    Authenticating,
    ObtainingIpaddr,
    Connected,
    Disconnecting,
    Disconnected,
    Unknown
}

/// <summary>
/// P2pConnectState 枚举
/// </summary>
public enum P2pConnectState
{
    Disconnected = 0,
    Connected = 1
}

/// <summary>
/// P2pDeviceStatus 枚举
/// </summary>
public enum P2pDeviceStatus
{
    Connected = 0,
    Invited = 1,
    Failed = 2,
    Available = 3,
    Unavailable = 4
}

/// <summary>
/// GroupOwnerBand 枚举
/// </summary>
public enum GroupOwnerBand
{
    GoBandAuto = 0,
    GoBand2Ghz = 1,
    GoBand5Ghz = 2
}