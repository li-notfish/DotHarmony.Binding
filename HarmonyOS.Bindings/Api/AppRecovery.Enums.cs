using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// RestartFlag 枚举
/// </summary>
public enum RestartFlag
{
    AlwaysRestart = 0,
    RestartWhenJsCrash = 1,
    RestartWhenAppFreeze = 2,
    NoRestart = 65535,
    RestartWhenCppCrash = 4
}

/// <summary>
/// SaveOccasionFlag 枚举
/// </summary>
public enum SaveOccasionFlag
{
    SaveWhenError = 1,
    SaveWhenBackground = 2
}

/// <summary>
/// SaveModeFlag 枚举
/// </summary>
public enum SaveModeFlag
{
    SaveWithFile = 1,
    SaveWithSharedMemory = 2
}