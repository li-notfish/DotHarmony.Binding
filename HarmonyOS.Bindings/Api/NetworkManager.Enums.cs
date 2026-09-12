using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// NetworkManagerDirection 枚举
/// </summary>
public enum NetworkManagerDirection
{
    Input = 0,
    Output = 1,
    Forward = 2
}

/// <summary>
/// NetworkManagerAction 枚举
/// </summary>
public enum NetworkManagerAction
{
    Allow = 0,
    Deny = 1,
    Reject = 2
}

/// <summary>
/// NetworkManagerProtocol 枚举
/// </summary>
public enum NetworkManagerProtocol
{
    All = 0,
    Tcp = 1,
    Udp = 2,
    Icmp = 3
}

/// <summary>
/// LogType 枚举
/// </summary>
public enum LogType
{
    Nflog = 0
}

/// <summary>
/// IpSetMode 枚举
/// </summary>
public enum IpSetMode
{
    Static = 0,
    Dhcp = 1
}