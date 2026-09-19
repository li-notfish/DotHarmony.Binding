using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ExecuteMode 枚举
/// </summary>
public enum ExecuteMode
{
    UiAbilityForeground = 0,
    UiAbilityBackground = 1,
    UiExtensionAbility = 2
}

/// <summary>
/// QueryType 枚举
/// </summary>
public enum QueryType
{
    [Description("all")]
    All,
    [Description("byProperty")]
    ByProperty
}

/// <summary>
/// ReturnMode 枚举
/// </summary>
public enum ReturnMode
{
    Callback = 0,
    Function = 1
}