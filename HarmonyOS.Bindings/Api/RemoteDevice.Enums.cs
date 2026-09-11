using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PairingReason 枚举
/// </summary>
public enum PairingReason
{
    PairingReasonSuccess = 0,
    PairingReasonFailure = 1,
    PairingReasonAcbConnectionFail = 2,
    PairingReasonExceedAcbMax = 3,
    PairingReasonRemoteCanceled = 4,
    PairingReasonLocalCanceled = 5,
    PairingReasonAuthFail = 6
}

/// <summary>
/// PairingType 枚举
/// </summary>
public enum PairingType
{
    NoPasskeyConfirmation = 0,
    PairingTypePasscode = 1,
    PairingTypeNumberCompare = 2
}

/// <summary>
/// ConnectionReason 枚举
/// </summary>
public enum ConnectionReason
{
    ConnectionSuccess = 0,
    ConnectionFailure = 1,
    ConnectionLocalDisconnect = 2,
    ConnectionRemoteDisconnect = 3,
    ConnectionFailAcbConnection = 4,
    ConnectionFailServiceDiscovery = 5,
    ConnectionFailNoAvailableService = 6,
    ConnectionFailConnectionNumLimited = 7
}