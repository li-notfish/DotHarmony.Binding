using System;

namespace HarmonyOS.ArkUI;

[Flags]
/// <summary>
/// HiTraceFlag 枚举
/// </summary>
public enum HiTraceFlag
{
    Default = 0,
    IncludeAsync = 1,
    DonotCreateSpan = 2,
    TpInfo = 4,
    NoBeInfo = 8,
    DisableLog = 16,
    FailureTrigger = 32,
    D2DTpInfo = 64
}

/// <summary>
/// HiTraceTracepointType 枚举
/// </summary>
public enum HiTraceTracepointType
{
    Cs = 0,
    Cr = 1,
    Ss = 2,
    Sr = 3,
    General = 4
}

/// <summary>
/// HiTraceCommunicationMode 枚举
/// </summary>
public enum HiTraceCommunicationMode
{
    Default = 0,
    Thread = 1,
    Process = 2,
    Device = 3
}