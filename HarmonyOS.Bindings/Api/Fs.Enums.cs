using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// MappingMode 枚举
/// </summary>
public enum MappingMode
{
    ReadOnly = 0,
    ReadWrite = 1,
    Private = 2
}

/// <summary>
/// WhenceType 枚举
/// </summary>
public enum WhenceType
{
    SeekSet = 0,
    SeekCur = 1,
    SeekEnd = 2
}

[Flags]
/// <summary>
/// LocationType 枚举
/// </summary>
public enum LocationType
{
    Local = 1,
    Cloud = 2
}

/// <summary>
/// AccessModeType 枚举
/// </summary>
public enum AccessModeType
{
    Exist = 0,
    Write = 2,
    Read = 4,
    ReadWrite = 6
}

/// <summary>
/// AccessFlagType 枚举
/// </summary>
public enum AccessFlagType
{
    Local = 0
}