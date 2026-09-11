using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LockReason 枚举
/// </summary>
public enum LockReason
{
    SimNone = 0,
    SimPin = 1,
    SimPuk = 2,
    SimPnPin = 3,
    SimPnPuk = 4,
    SimPuPin = 5,
    SimPuPuk = 6,
    SimPpPin = 7,
    SimPpPuk = 8,
    SimPcPin = 9,
    SimPcPuk = 10,
    SimSimPin = 11,
    SimSimPuk = 12
}