using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ProxyTypes 枚举
/// </summary>
public enum ProxyTypes
{
    None = 0,
    Socks5 = 1
}

/// <summary>
/// Protocol 枚举
/// </summary>
public enum Protocol
{
    [Description("TLSv1.2")]
    TlSv12,
    [Description("TLSv1.3")]
    TlSv13
}