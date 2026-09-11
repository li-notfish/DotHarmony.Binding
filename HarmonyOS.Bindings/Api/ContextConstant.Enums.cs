using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// AreaMode 枚举
/// </summary>
public enum AreaMode
{
    El1 = 0,
    El2 = 1,
    El3 = 2,
    El4 = 3,
    El5 = 4
}

/// <summary>
/// ProcessMode 枚举
/// </summary>
public enum ProcessMode
{
    NewProcessAttachToParent = 1,
    NewProcessAttachToStatusBarItem = 2,
    AttachToStatusBarItem = 3
}

/// <summary>
/// StartupVisibility 枚举
/// </summary>
public enum StartupVisibility
{
    StartupHide = 0,
    StartupShow = 1
}

/// <summary>
/// Scenarios 枚举
/// </summary>
public enum Scenarios
{
    ScenarioMoveMissionToFront = 1,
    ScenarioShowAbility = 2,
    ScenarioBackToCallerAbilityWithResult = 4
}

/// <summary>
/// ContextType 枚举
/// </summary>
public enum ContextType
{
    ApplicationContext = 0,
    AbilityStageContext = 1,
    UiabilityContext = 2,
    FormExtensionContext = 3,
    AppServiceExtensionContext = 4
}