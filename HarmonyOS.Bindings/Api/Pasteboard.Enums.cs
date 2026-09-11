using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ShareOption 枚举
/// </summary>
public enum ShareOption
{
    INAPP = 0,
    LOCALDEVICE = 1,
    CROSSDEVICE = 2
}

/// <summary>
/// Pattern 枚举
/// </summary>
public enum Pattern
{
    URL = 0,
    NUMBER = 1,
    EMAIL_ADDRESS = 2,
    HTTP_URL = 3,
    FLIGHT_NUMBER = 4
}

/// <summary>
/// FileConflictOptions 枚举
/// </summary>
public enum FileConflictOptions
{
    OVERWRITE = 0,
    SKIP = 1
}

/// <summary>
/// ProgressIndicator 枚举
/// </summary>
public enum ProgressIndicator
{
    NONE = 0,
    DEFAULT = 1
}