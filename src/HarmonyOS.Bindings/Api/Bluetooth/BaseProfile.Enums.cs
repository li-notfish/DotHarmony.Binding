using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DisconnectCause 枚举
/// </summary>
public enum DisconnectCause
{
    UserDisconnect = 0,
    ConnectFromKeyboard = 1,
    ConnectFromMouse = 2,
    ConnectFromCar = 3,
    TooManyConnectedDevices = 4,
    ConnectFailInternal = 5
}

/// <summary>
/// PanRole 枚举
/// </summary>
public enum PanRole
{
    RolePannap = 0,
    RolePanu = 1
}