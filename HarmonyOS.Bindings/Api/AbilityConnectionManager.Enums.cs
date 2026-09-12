using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ConnectErrorCode 枚举
/// </summary>
public enum ConnectErrorCode
{
    ConnectedSessionExists = 0,
    PeerAppRejected = 1,
    LocalWifiNotOpen = 2,
    PeerWifiNotOpen = 3,
    PeerAbilityNoOncollaborate = 4,
    SystemInternalError = 5
}

/// <summary>
/// StartOptionParams 枚举
/// </summary>
public enum StartOptionParams
{
    StartInForeground = 0
}

/// <summary>
/// CollaborateEventType 枚举
/// </summary>
public enum CollaborateEventType
{
    SendFailure = 0,
    ColorSpaceConversionFailure = 1
}

/// <summary>
/// DisconnectReason 枚举
/// </summary>
public enum DisconnectReason
{
    PeerAppCloseCollaboration = 0,
    PeerAppExit = 1,
    NetworkDisconnected = 2
}

/// <summary>
/// CollaborationKeys 枚举
/// </summary>
public enum CollaborationKeys
{
    [Description("ohos.collaboration.key.peerInfo")]
    PeerInfo,
    [Description("ohos.collaboration.key.connectOptions")]
    ConnectOptions,
    [Description("ohos.collaboration.key.abilityCollaborateType")]
    CollaborateType
}

/// <summary>
/// CollaborationValues 枚举
/// </summary>
public enum CollaborationValues
{
    [Description("ohos.collaboration.value.abilityCollab")]
    AbilityCollaborationTypeDefault,
    [Description("ohos.collaboration.value.connectProxy")]
    AbilityCollaborationTypeConnectProxy
}