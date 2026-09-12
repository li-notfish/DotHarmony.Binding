using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DataType 枚举
/// </summary>
public enum DataType : long
{
    MediaData = 1,
    AllData = 4294967295
}

/// <summary>
/// AccessStatus 枚举
/// </summary>
public enum AccessStatus
{
    [Description("-1")]
    AccessDenied,
    AccessGranted = 0
}

/// <summary>
/// ReleaseStatus 枚举
/// </summary>
public enum ReleaseStatus
{
    [Description("-1")]
    ReleaseDenied,
    ReleaseGranted = 0
}

/// <summary>
/// KeyStatus 枚举
/// </summary>
public enum KeyStatus
{
    [Description("-2")]
    KeyNotExist,
    [Description("-1")]
    KeyReleased,
    KeyExist = 0
}