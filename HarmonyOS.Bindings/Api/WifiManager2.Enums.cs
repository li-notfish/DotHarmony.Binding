using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DeviceAddressType 枚举
/// </summary>
public enum DeviceAddressType
{
    RandomDeviceAddress,
    RealDeviceAddress
}

/// <summary>
/// WifiManager2EapMethod 枚举
/// </summary>
public enum WifiManager2EapMethod
{
    EapNone,
    EapPeap,
    EapTls,
    EapTtls,
    EapPwd,
    EapSim,
    EapAka,
    EapAkaPrime,
    EapUnauthTls
}

/// <summary>
/// WifiManager2Phase2Method 枚举
/// </summary>
public enum WifiManager2Phase2Method
{
    Phase2None,
    Phase2Pap,
    Phase2Mschap,
    Phase2Mschapv2,
    Phase2Gtc,
    Phase2Sim,
    Phase2Aka,
    Phase2AkaPrime
}

/// <summary>
/// WifiCategory 枚举
/// </summary>
public enum WifiCategory
{
    Default = 1,
    Wifi6 = 2,
    Wifi6Plus = 3,
    Wifi7 = 4,
    Wifi7Plus = 5
}

/// <summary>
/// WifiLinkType 枚举
/// </summary>
public enum WifiLinkType
{
    DefaultLink = 0,
    Wifi7SingleLink = 1,
    Wifi7Mlsr = 2,
    Wifi7Emlsr = 3,
    Wifi7Str = 4
}

/// <summary>
/// WifiChannelWidth 枚举
/// </summary>
public enum WifiChannelWidth
{
    Width20Mhz = 0,
    Width40Mhz = 1,
    Width80Mhz = 2,
    Width160Mhz = 3,
    Width80MhzPlus = 4,
    WidthInvalid
}

/// <summary>
/// WifiManager2WifiSecurityType 枚举
/// </summary>
public enum WifiManager2WifiSecurityType
{
    WifiSecTypeInvalid = 0,
    WifiSecTypeOpen = 1,
    WifiSecTypeWep = 2,
    WifiSecTypePsk = 3,
    WifiSecTypeSae = 4,
    WifiSecTypeEap = 5,
    WifiSecTypeEapSuiteB = 6,
    WifiSecTypeOwe = 7,
    WifiSecTypeWapiCert = 8,
    WifiSecTypeWapiPsk = 9
}

/// <summary>
/// WifiCapability 枚举
/// </summary>
public enum WifiCapability
{
    WifiAutoEnable = 0
}

/// <summary>
/// WapiPskType 枚举
/// </summary>
public enum WapiPskType
{
    WapiPskAscii = 0,
    WapiPskHex = 1
}

/// <summary>
/// WifiBandType 枚举
/// </summary>
public enum WifiBandType
{
    WifiBandNone,
    WifiBand2G,
    WifiBand5G,
    WifiBand6G,
    WifiBand60G
}

/// <summary>
/// WifiStandard 枚举
/// </summary>
public enum WifiStandard
{
    WifiStandardUndefined,
    WifiStandard11A,
    WifiStandard11B,
    WifiStandard11G,
    WifiStandard11N,
    WifiStandard11Ac,
    WifiStandard11Ax,
    WifiStandard11Ad
}

/// <summary>
/// WifiManager2ConnState 枚举
/// </summary>
public enum WifiManager2ConnState
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
/// WifiManager2P2pConnectState 枚举
/// </summary>
public enum WifiManager2P2pConnectState
{
    Disconnected = 0,
    Connected = 1
}

/// <summary>
/// WifiManager2P2pDeviceStatus 枚举
/// </summary>
public enum WifiManager2P2pDeviceStatus
{
    Connected = 0,
    Invited = 1,
    Failed = 2,
    Available = 3,
    Unavailable = 4
}

/// <summary>
/// WifiManager2GroupOwnerBand 枚举
/// </summary>
public enum WifiManager2GroupOwnerBand
{
    GoBandAuto = 0,
    GoBand2Ghz = 1,
    GoBand5Ghz = 2
}