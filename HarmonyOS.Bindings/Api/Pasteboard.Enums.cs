using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ShareOption 枚举
/// </summary>
public enum ShareOption
{
    Inapp = 0,
    Localdevice = 1,
    Crossdevice = 2
}

/// <summary>
/// Pattern 枚举
/// </summary>
public enum Pattern
{
    Url = 0,
    Number = 1,
    EmailAddress = 2,
    HttpUrl = 3,
    FlightNumber = 4
}

/// <summary>
/// PasteboardFileConflictOptions 枚举
/// </summary>
public enum PasteboardFileConflictOptions
{
    Overwrite = 0,
    Skip = 1
}

/// <summary>
/// PasteboardProgressIndicator 枚举
/// </summary>
public enum PasteboardProgressIndicator
{
    None = 0,
    Default = 1
}