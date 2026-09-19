using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// RangingTypes 枚举
/// </summary>
public enum RangingTypes
{
    NearlinkHadm = 1
}

/// <summary>
/// RangingState 枚举
/// </summary>
public enum RangingState
{
    RangingStopped = 0,
    RangingStarted = 1
}

/// <summary>
/// RangingStoppedCause 枚举
/// </summary>
public enum RangingStoppedCause
{
    NoError = 0,
    InternalError = 1,
    BusinessConflict = 2,
    BackgroundPaused = 3
}

/// <summary>
/// RangingConfidence 枚举
/// </summary>
public enum RangingConfidence
{
    High = 0,
    Medium = 1,
    Low = 2
}