using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CallTransferType 枚举
/// </summary>
public enum CallTransferType
{
    TransferTypeUnconditional = 0,
    TransferTypeBusy = 1,
    TransferTypeNoReply = 2,
    TransferTypeNotReachable = 3
}

/// <summary>
/// CallCallState 枚举
/// </summary>
public enum CallCallState
{
    [Description("-1")]
    CallStateUnknown,
    CallStateIdle = 0,
    CallStateRinging = 1,
    CallStateOffhook = 2,
    CallStateAnswered = 3
}

/// <summary>
/// TelCallState 枚举
/// </summary>
public enum TelCallState
{
    [Description("-1")]
    TelCallStateUnknown,
    TelCallStateIdle = 0,
    TelCallStateRinging = 1,
    TelCallStateOffhook = 2,
    TelCallStateAnswered = 3,
    TelCallStateConnected = 4
}

/// <summary>
/// CCallState 枚举
/// </summary>
public enum CCallState
{
    [Description("-1")]
    CcallStateUnknown,
    CcallStateActive = 0,
    CcallStateHolding = 1,
    CcallStateDialing = 2,
    CcallStateAlerting = 3,
    CcallStateIncoming = 4,
    CcallStateWaiting = 5,
    CcallStateDisconnected = 6,
    CcallStateDisconnecting = 7,
    CcallStateIdle = 8,
    CcallStateAnswered = 9
}

/// <summary>
/// TransferStatus 枚举
/// </summary>
public enum TransferStatus
{
    TransferDisable = 0,
    TransferEnable = 1
}