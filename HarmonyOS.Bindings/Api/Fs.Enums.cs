using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// MappingMode 枚举
/// </summary>
public enum MappingMode
{
    READ_ONLY = 0,
    READ_WRITE = 1,
    PRIVATE = 2
}

/// <summary>
/// WhenceType 枚举
/// </summary>
public enum WhenceType
{
    SEEK_SET = 0,
    SEEK_CUR = 1,
    SEEK_END = 2
}

[Flags]
/// <summary>
/// LocationType 枚举
/// </summary>
public enum LocationType
{
    LOCAL = 1,
    CLOUD = 2
}

/// <summary>
/// AccessModeType 枚举
/// </summary>
public enum AccessModeType
{
    EXIST = 0,
    WRITE = 2,
    READ = 4,
    READ_WRITE = 6
}

/// <summary>
/// AccessFlagType 枚举
/// </summary>
public enum AccessFlagType
{
    LOCAL = 0
}