using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// OperatingHandStatus 枚举
/// </summary>
public enum OperatingHandStatus
{
    UnknownStatus = 0,
    LeftHandOperated = 1,
    RightHandOperated = 2
}

/// <summary>
/// HoldingHandStatus 枚举
/// </summary>
public enum HoldingHandStatus
{
    NotHeld = 0,
    LeftHandHeld = 1,
    RightHandHeld = 2,
    BothHandsHeld = 3,
    UnknownStatus = 16
}