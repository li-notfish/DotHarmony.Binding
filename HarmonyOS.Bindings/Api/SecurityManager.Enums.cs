using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PermissionManagedState 枚举
/// </summary>
public enum PermissionManagedState
{
    [Description("-1")]
    Denied,
    Granted = 0,
    Default = 1
}

/// <summary>
/// PasswordAlgs 枚举
/// </summary>
public enum PasswordAlgs
{
    ScryptHkdfAes = 0,
    ScryptHkdfSm4 = 1
}

/// <summary>
/// ClipboardPolicy 枚举
/// </summary>
public enum ClipboardPolicy
{
    Default = 0,
    InApp = 1,
    LocalDevice = 2,
    CrossDevice = 3
}