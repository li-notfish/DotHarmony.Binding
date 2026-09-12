using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// WantAgentFlags 枚举
/// </summary>
public enum WantAgentFlags
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
/// OperationType 枚举
/// </summary>
public enum OperationType
{
    UnknownType = 0,
    StartAbility,
    StartAbilities,
    StartService,
    SendCommonEvent
}