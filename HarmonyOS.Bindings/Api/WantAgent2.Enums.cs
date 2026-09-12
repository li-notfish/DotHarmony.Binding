using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// WantAgent2WantAgentFlags 枚举
/// </summary>
public enum WantAgent2WantAgentFlags
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
/// WantAgent2OperationType 枚举
/// </summary>
public enum WantAgent2OperationType
{
    UnknownType = 0,
    StartAbility,
    StartAbilities,
    StartService,
    SendCommonEvent
}