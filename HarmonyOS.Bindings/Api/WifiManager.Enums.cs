using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// WifiSecurityType 枚举
/// </summary>
public enum WifiSecurityType
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
/// IpType 枚举
/// </summary>
public enum IpType
{
    Static = 0,
    Dhcp = 1,
    Unknown = 2
}

/// <summary>
/// EapMethod 枚举
/// </summary>
public enum EapMethod
{
    EapNone = 0,
    EapPeap = 1,
    EapTls = 2,
    EapTtls = 3,
    EapPwd = 4,
    EapSim = 5,
    EapAka = 6,
    EapAkaPrime = 7,
    EapUnauthTls = 8
}

/// <summary>
/// Phase2Method 枚举
/// </summary>
public enum Phase2Method
{
    Phase2None = 0,
    Phase2Pap = 1,
    Phase2Mschap = 2,
    Phase2Mschapv2 = 3,
    Phase2Gtc = 4,
    Phase2Sim = 5,
    Phase2Aka = 6,
    Phase2AkaPrime = 7
}