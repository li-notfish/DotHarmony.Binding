using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LevelMode 枚举
/// </summary>
public enum LevelMode
{
    Overlay = 0,
    Embedded = 1
}

/// <summary>
/// ImmersiveMode 枚举
/// </summary>
public enum ImmersiveMode
{
    Default = 0,
    Extend = 1
}

/// <summary>
/// ToastShowMode 枚举
/// </summary>
public enum ToastShowMode
{
    Default = 0,
    TopMost = 1
}

/// <summary>
/// CommonState 枚举
/// </summary>
public enum CommonState
{
    Uninitialized = 0,
    Initialized = 1,
    Appearing = 2,
    Appeared = 3,
    Disappearing = 4,
    Disappeared = 5
}