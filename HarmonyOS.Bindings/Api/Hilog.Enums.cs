using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LogLevel 枚举
/// </summary>
public enum LogLevel
{
    Debug = 3,
    Info = 4,
    Warn = 5,
    Error = 6,
    Fatal = 7
}

/// <summary>
/// PreferStrategy 枚举
/// </summary>
public enum PreferStrategy
{
    UnsetLoglevel = 0,
    PreferCloseLog = 1,
    PreferOpenLog = 2
}

/// <summary>
/// OutputType 枚举
/// </summary>
public enum OutputType
{
    Default = 0,
    ConsoleOnly = 0,
    PrivateSandboxOnly = 1,
    ShareSandboxOnly = 2,
    PrivateSandboxWithConsole = 3,
    ShareSandboxWithConsole = 4
}