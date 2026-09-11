using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Protocol 枚举
/// </summary>
public enum Protocol
{
    Gatt = 0,
    Spp = 1,
    Opp = 2
}

/// <summary>
/// TransferPolicy 枚举
/// </summary>
public enum TransferPolicy
{
    SendOnly = 0,
    ReceiveOnly = 1,
    ReceiveSend = 2
}