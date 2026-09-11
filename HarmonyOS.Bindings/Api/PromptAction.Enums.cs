using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LevelMode 枚举
/// </summary>
public enum LevelMode
{
    OVERLAY = 0,
    EMBEDDED = 1
}

/// <summary>
/// ImmersiveMode 枚举
/// </summary>
public enum ImmersiveMode
{
    DEFAULT = 0,
    EXTEND = 1
}

/// <summary>
/// ToastShowMode 枚举
/// </summary>
public enum ToastShowMode
{
    DEFAULT = 0,
    TOP_MOST = 1
}

/// <summary>
/// CommonState 枚举
/// </summary>
public enum CommonState
{
    UNINITIALIZED = 0,
    INITIALIZED = 1,
    APPEARING = 2,
    APPEARED = 3,
    DISAPPEARING = 4,
    DISAPPEARED = 5
}