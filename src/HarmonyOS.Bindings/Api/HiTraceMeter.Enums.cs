using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// HiTraceOutputLevel 枚举
/// </summary>
public enum HiTraceOutputLevel
{
    Debug = 0,
    Info = 1,
    Critical = 2,
    Commercial = 3,
    [Description("COMMERCIAL")]
    Max
}