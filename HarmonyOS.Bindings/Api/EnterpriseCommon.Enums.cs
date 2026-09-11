using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ManagedPolicy 枚举
/// </summary>
public enum ManagedPolicy
{
    Default = 0,
    Disallow = 1,
    ForceOpen = 2
}

/// <summary>
/// Result 枚举
/// </summary>
public enum Result
{
    Success = 0,
    [Description("-1")]
    Fail
}

/// <summary>
/// StartupScene 枚举
/// </summary>
public enum StartupScene
{
    UserSetup = 0,
    Ota = 1,
    DeviceProvision = 2
}