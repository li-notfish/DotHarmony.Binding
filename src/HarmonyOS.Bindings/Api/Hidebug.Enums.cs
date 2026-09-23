using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// TraceFlag 枚举
/// </summary>
public enum TraceFlag
{
    MainThread = 1,
    AllThreads = 2
}

/// <summary>
/// JsRawHeapTrimLevel 枚举
/// </summary>
public enum JsRawHeapTrimLevel
{
    TrimLevel1 = 0,
    TrimLevel2 = 1
}