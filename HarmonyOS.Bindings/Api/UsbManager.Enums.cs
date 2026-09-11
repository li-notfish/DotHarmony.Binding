using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// UsbPolicy 枚举
/// </summary>
public enum UsbPolicy
{
    ReadWrite = 0,
    ReadOnly = 1,
    Disabled = 2
}

/// <summary>
/// Descriptor 枚举
/// </summary>
public enum Descriptor
{
    Interface = 0,
    Device = 1
}