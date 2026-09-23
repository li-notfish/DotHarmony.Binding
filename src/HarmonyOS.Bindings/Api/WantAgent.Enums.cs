using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// WantAgentWantAgentFlags 枚举
/// </summary>
public enum WantAgentWantAgentFlags
{
    OneTimeFlag = 0,
    NoBuildFlag,
    CancelPresentFlag,
    UpdatePresentFlag,
    ConstantFlag,
    ReplaceElement,
    ReplaceAction,
    ReplaceUri,
    ReplaceEntities,
    ReplaceBundle
}

/// <summary>
/// WantAgentOperationType 枚举
/// </summary>
public enum WantAgentOperationType
{
    UnknownType = 0,
    StartAbility,
    StartAbilities,
    StartService,
    SendCommonEvent
}