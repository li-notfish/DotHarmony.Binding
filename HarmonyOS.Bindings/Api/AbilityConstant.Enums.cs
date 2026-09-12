using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LaunchReason 枚举
/// </summary>
public enum LaunchReason
{
    Unknown = 0,
    StartAbility = 1,
    Call = 2,
    Continuation = 3,
    AppRecovery = 4,
    Share = 5,
    AutoStartup = 8,
    InsightIntent = 9,
    PrepareContinuation = 10,
    Preload = 11
}

/// <summary>
/// LastExitReason 枚举
/// </summary>
public enum LastExitReason
{
    Unknown = 0,
    AbilityNotResponding = 1,
    Normal = 2,
    CppCrash = 3,
    JsError = 4,
    AppFreeze = 5,
    PerformanceControl = 6,
    ResourceControl = 7,
    Upgrade = 8,
    UserRequest = 9,
    Signal = 10
}

/// <summary>
/// OnContinueResult 枚举
/// </summary>
public enum OnContinueResult
{
    Agree = 0,
    Reject = 1,
    Mismatch = 2
}

/// <summary>
/// MemoryLevel 枚举
/// </summary>
public enum MemoryLevel
{
    MemoryLevelModerate = 0,
    MemoryLevelLow = 1,
    MemoryLevelCritical = 2,
    MemoryLevelUiHidden = 3,
    MemoryLevelBackgroundModerate = 4,
    MemoryLevelBackgroundLow = 5,
    MemoryLevelBackgroundCritical = 6
}

/// <summary>
/// AbilityConstantWindowMode 枚举
/// </summary>
public enum AbilityConstantWindowMode
{
    WindowModeFullscreen = 1,
    WindowModeSplitPrimary = 100,
    WindowModeSplitSecondary = 101,
    WindowModeSplit = 105
}

/// <summary>
/// OnSaveResult 枚举
/// </summary>
public enum OnSaveResult
{
    AllAgree = 0,
    ContinuationReject = 1,
    ContinuationMismatch = 2,
    RecoveryAgree = 3,
    RecoveryReject = 4,
    AllReject = 5
}

/// <summary>
/// StateType 枚举
/// </summary>
public enum StateType
{
    Continuation = 0,
    AppRecovery = 1
}

/// <summary>
/// ContinueState 枚举
/// </summary>
public enum ContinueState
{
    Active = 0,
    Inactive = 1
}

/// <summary>
/// PrepareTermination 枚举
/// </summary>
public enum PrepareTermination
{
    TerminateImmediately = 0,
    Cancel = 1
}

/// <summary>
/// CollaborateResult 枚举
/// </summary>
public enum CollaborateResult
{
    Accept = 0,
    Reject = 1
}