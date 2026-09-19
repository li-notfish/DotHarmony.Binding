using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// EnterpriseNetworkManagerDirection 枚举
/// </summary>
public enum EnterpriseNetworkManagerDirection
{
    Input = 0,
    Output = 1,
    Forward = 2
}

/// <summary>
/// EnterpriseNetworkManagerAction 枚举
/// </summary>
public enum EnterpriseNetworkManagerAction
{
    Allow = 0,
    Deny = 1,
    Reject = 2
}

/// <summary>
/// EnterpriseNetworkManagerProtocol 枚举
/// </summary>
public enum EnterpriseNetworkManagerProtocol
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