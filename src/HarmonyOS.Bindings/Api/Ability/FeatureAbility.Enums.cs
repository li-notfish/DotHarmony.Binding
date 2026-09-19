using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// AbilityWindowConfiguration 枚举
/// </summary>
public enum AbilityWindowConfiguration
{
    WindowModeUndefined = 0,
    WindowModeFullscreen = 1,
    WindowModeSplitPrimary = 100,
    WindowModeSplitSecondary = 101,
    WindowModeFloating = 102
}

/// <summary>
/// AbilityStartSetting 枚举
/// </summary>
public enum AbilityStartSetting
{
    [Description("abilityBounds")]
    BoundsKey,
    [Description("windowMode")]
    WindowModeKey,
    [Description("displayId")]
    DisplayIdKey
}

/// <summary>
/// ErrorCode 枚举
/// </summary>
public enum ErrorCode
{
    NoError = 0,
    [Description("-1")]
    InvalidParameter,
    [Description("-2")]
    AbilityNotFound,
    [Description("-3")]
    PermissionDeny
}

/// <summary>
/// DataAbilityOperationType 枚举
/// </summary>
public enum DataAbilityOperationType
{
    TypeInsert = 1,
    TypeUpdate = 2,
    TypeDelete = 3,
    TypeAssert = 4
}