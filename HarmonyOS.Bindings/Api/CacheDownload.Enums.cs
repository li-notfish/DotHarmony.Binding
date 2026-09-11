using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SslType 枚举
/// </summary>
public enum SslType
{
    [Description("TLS")]
    Tls,
    [Description("TLCP")]
    Tlcp
}

/// <summary>
/// CacheStrategy 枚举
/// </summary>
public enum CacheStrategy
{
    Force = 0,
    Lazy = 1
}

/// <summary>
/// CacheDownloadErrorCode 枚举
/// </summary>
public enum CacheDownloadErrorCode
{
    Others = 255,
    Dns = 0,
    Tcp = 16,
    Ssl = 32,
    Http = 48
}