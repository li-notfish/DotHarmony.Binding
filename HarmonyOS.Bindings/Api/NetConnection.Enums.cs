using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// TcpState 枚举
/// </summary>
public enum TcpState
{
    TcpEstablished = 1,
    TcpSynSent = 2,
    TcpSynRecv = 3,
    TcpFinWait1 = 4,
    TcpFinWait2 = 5,
    TcpTimeWait = 6,
    TcpClose = 7,
    TcpCloseWait = 8,
    TcpLastAck = 9,
    TcpListen = 10,
    TcpClosing = 11
}

/// <summary>
/// ConversionProcess 枚举
/// </summary>
public enum ConversionProcess
{
    NoConfiguration = 0,
    AllowUnassigned = 1,
    UseStd3AsciiRules = 2
}

/// <summary>
/// FamilyType 枚举
/// </summary>
public enum FamilyType
{
    FamilyTypeAll = 0,
    FamilyTypeIpv4 = 1,
    FamilyTypeIpv6 = 2
}

/// <summary>
/// NetCap 枚举
/// </summary>
public enum NetCap
{
    NetCapabilityMms = 0,
    NetCapabilityNotMetered = 11,
    NetCapabilityInternet = 12,
    NetCapabilityNotVpn = 15,
    NetCapabilityValidated = 16,
    NetCapabilityPortal = 17,
    NetCapabilityCheckingConnectivity = 31
}

/// <summary>
/// NetBearType 枚举
/// </summary>
public enum NetBearType
{
    BearerCellular = 0,
    BearerWifi = 1,
    BearerBluetooth = 2,
    BearerEthernet = 3,
    BearerVpn = 4
}

/// <summary>
/// Socks5DnsStrategy 枚举
/// </summary>
public enum Socks5DnsStrategy
{
    SystemMode = 0,
    ProxyMode = 1
}

/// <summary>
/// NetConnectionProtocolType 枚举
/// </summary>
public enum NetConnectionProtocolType
{
    ProtoTypeTcp = 6,
    ProtoTypeUdp = 17
}

/// <summary>
/// PacketsType 枚举
/// </summary>
public enum PacketsType
{
    NetconnPacketsIcmp = 0,
    NetconnPacketsUdp = 1
}