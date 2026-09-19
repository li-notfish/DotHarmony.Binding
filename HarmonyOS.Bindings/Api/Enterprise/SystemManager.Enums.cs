using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PolicyType 枚举
/// </summary>
public enum PolicyType
{
    Default = 0,
    Prohibit = 1,
    UpdateToSpecificVersion = 2,
    Windows = 3,
    Postpone = 4
}

/// <summary>
/// PackageType 枚举
/// </summary>
public enum PackageType
{
    Firmware = 1
}

/// <summary>
/// UpdateStatus 枚举
/// </summary>
public enum UpdateStatus
{
    [Description("-4")]
    NoUpdatePackage,
    [Description("-3")]
    UpdateWaiting,
    [Description("-2")]
    Updating,
    [Description("-1")]
    UpdateFailure,
    UpdateSuccess = 0
}

/// <summary>
/// NearLinkProtocol 枚举
/// </summary>
public enum NearLinkProtocol
{
    Ssap = 0,
    DataTransfer = 1
}

/// <summary>
/// KeyCode 枚举
/// </summary>
public enum KeyCode
{
    Power = 0,
    VolumeUp = 1,
    VolumeDown = 2,
    Back = 3,
    Home = 4,
    Recent = 5
}

/// <summary>
/// KeyPolicy 枚举
/// </summary>
public enum KeyPolicy
{
    Interception = 0,
    Custom = 1
}

/// <summary>
/// KeyAction 枚举
/// </summary>
public enum KeyAction
{
    [Description("-1")]
    Unknown,
    Down = 0,
    Up = 1
}