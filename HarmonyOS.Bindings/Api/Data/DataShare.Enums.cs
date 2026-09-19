using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ChangeType 枚举
/// </summary>
public enum ChangeType
{
    Insert = 0,
    Delete = 1,
    Update = 2
}

/// <summary>
/// DataProxyErrorCode 枚举
/// </summary>
public enum DataProxyErrorCode
{
    Success = 0,
    UriNotExist = 1,
    NoPermission = 2,
    OverLimit = 3
}

/// <summary>
/// DataProxyType 枚举
/// </summary>
public enum DataProxyType
{
    SharedConfig = 0
}

/// <summary>
/// DataProxyMaxValueLength 枚举
/// </summary>
public enum DataProxyMaxValueLength
{
    MaxLength4K = 4096,
    MaxLength100K = 102400
}