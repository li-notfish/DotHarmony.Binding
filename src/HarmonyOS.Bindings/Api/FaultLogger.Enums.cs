using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// FaultType 枚举
/// </summary>
public enum FaultType
{
    NoSpecific = 0,
    CppCrash = 2,
    JsCrash = 3,
    AppFreeze = 4
}