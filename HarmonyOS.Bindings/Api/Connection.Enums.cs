using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// TcpState 枚举
/// </summary>
public enum TcpState
{
    TCP_ESTABLISHED = 1,
    TCP_SYN_SENT = 2,
    TCP_SYN_RECV = 3,
    TCP_FIN_WAIT1 = 4,
    TCP_FIN_WAIT2 = 5,
    TCP_TIME_WAIT = 6,
    TCP_CLOSE = 7,
    TCP_CLOSE_WAIT = 8,
    TCP_LAST_ACK = 9,
    TCP_LISTEN = 10,
    TCP_CLOSING = 11
}

/// <summary>
/// ConversionProcess 枚举
/// </summary>
public enum ConversionProcess
{
    NO_CONFIGURATION = 0,
    ALLOW_UNASSIGNED = 1,
    USE_STD3_ASCII_RULES = 2
}

/// <summary>
/// FamilyType 枚举
/// </summary>
public enum FamilyType
{
    FAMILY_TYPE_ALL = 0,
    FAMILY_TYPE_IPV4 = 1,
    FAMILY_TYPE_IPV6 = 2
}

/// <summary>
/// NetCap 枚举
/// </summary>
public enum NetCap
{
    NET_CAPABILITY_MMS = 0,
    NET_CAPABILITY_NOT_METERED = 11,
    NET_CAPABILITY_INTERNET = 12,
    NET_CAPABILITY_NOT_VPN = 15,
    NET_CAPABILITY_VALIDATED = 16,
    NET_CAPABILITY_PORTAL = 17,
    NET_CAPABILITY_CHECKING_CONNECTIVITY = 31
}

/// <summary>
/// NetBearType 枚举
/// </summary>
public enum NetBearType
{
    BEARER_CELLULAR = 0,
    BEARER_WIFI = 1,
    BEARER_BLUETOOTH = 2,
    BEARER_ETHERNET = 3,
    BEARER_VPN = 4
}

/// <summary>
/// Socks5DnsStrategy 枚举
/// </summary>
public enum Socks5DnsStrategy
{
    SYSTEM_MODE = 0,
    PROXY_MODE = 1
}

/// <summary>
/// ProtocolType 枚举
/// </summary>
public enum ProtocolType
{
    PROTO_TYPE_TCP = 6,
    PROTO_TYPE_UDP = 17
}

/// <summary>
/// PacketsType 枚举
/// </summary>
public enum PacketsType
{
    NETCONN_PACKETS_ICMP = 0,
    NETCONN_PACKETS_UDP = 1
}