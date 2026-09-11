using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// RadioTechnology 枚举
/// </summary>
public enum RadioTechnology
{
    RadioTechnologyUnknown = 0,
    RadioTechnologyGsm = 1,
    RadioTechnology1Xrtt = 2,
    RadioTechnologyWcdma = 3,
    RadioTechnologyHspa = 4,
    RadioTechnologyHspap = 5,
    RadioTechnologyTdScdma = 6,
    RadioTechnologyEvdo = 7,
    RadioTechnologyEhrpd = 8,
    RadioTechnologyLte = 9,
    RadioTechnologyLteCa = 10,
    RadioTechnologyIwlan = 11,
    RadioTechnologyNr = 12
}

/// <summary>
/// NetworkType 枚举
/// </summary>
public enum NetworkType
{
    NetworkTypeUnknown = 0,
    NetworkTypeGsm = 1,
    NetworkTypeCdma = 2,
    NetworkTypeWcdma = 3,
    NetworkTypeTdscdma = 4,
    NetworkTypeLte = 5,
    NetworkTypeNr = 6
}

/// <summary>
/// RegState 枚举
/// </summary>
public enum RegState
{
    RegStateNoService = 0,
    RegStateInService = 1,
    RegStateEmergencyCallOnly = 2,
    RegStatePowerOff = 3
}

/// <summary>
/// NsaState 枚举
/// </summary>
public enum NsaState
{
    NsaStateNotSupport = 1,
    NsaStateNoDetect = 2,
    NsaStateConnectedDetect = 3,
    NsaStateIdleDetect = 4,
    NsaStateDualConnected = 5,
    NsaStateSaAttached = 6
}

/// <summary>
/// NetworkSelectionMode 枚举
/// </summary>
public enum NetworkSelectionMode
{
    NetworkSelectionUnknown = 0,
    NetworkSelectionAutomatic = 1,
    NetworkSelectionManual = 2
}