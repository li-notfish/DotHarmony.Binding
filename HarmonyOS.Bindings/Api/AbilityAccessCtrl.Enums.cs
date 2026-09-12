using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// GrantStatus 枚举
/// </summary>
public enum GrantStatus
{
    [Description("-1")]
    PermissionDenied,
    PermissionGranted = 0
}

/// <summary>
/// SelectedResult 枚举
/// </summary>
public enum SelectedResult
{
    [Description("-1")]
    Rejected,
    Opened = 0,
    Granted = 1
}

/// <summary>
/// PermissionStateChangeType 枚举
/// </summary>
public enum PermissionStateChangeType
{
    PermissionRevokedOper = 0,
    PermissionGrantedOper = 1
}

/// <summary>
/// PermissionStatus 枚举
/// </summary>
public enum PermissionStatus
{
    [Description("-1")]
    Denied,
    Granted = 0,
    NotDetermined = 1,
    Invalid = 2,
    Restricted = 3
}

/// <summary>
/// SwitchType 枚举
/// </summary>
public enum SwitchType
{
    Camera = 0,
    Microphone = 1,
    Location = 2
}