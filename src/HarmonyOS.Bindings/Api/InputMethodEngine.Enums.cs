using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ImmersiveMode 枚举
/// </summary>
public enum ImmersiveMode
{
    NoneImmersive = 0,
    Immersive,
    LightImmersive,
    DarkImmersive
}

/// <summary>
/// GradientMode 枚举
/// </summary>
public enum GradientMode
{
    None = 0,
    LinearGradient = 1
}

/// <summary>
/// InputMethodEngineRequestKeyboardReason 枚举
/// </summary>
public enum InputMethodEngineRequestKeyboardReason
{
    None = 0,
    Mouse = 1,
    Touch = 2,
    Other = 20
}

/// <summary>
/// InputMethodEnginePanelFlag 枚举
/// </summary>
public enum InputMethodEnginePanelFlag
{
    FlgFixed = 0,
    FlgFloating,
    FlagCandidate
}

/// <summary>
/// InputMethodEnginePanelType 枚举
/// </summary>
public enum InputMethodEnginePanelType
{
    SoftKeyboard = 0,
    StatusBar
}

/// <summary>
/// InputMethodEngineDirection 枚举
/// </summary>
public enum InputMethodEngineDirection
{
    CursorUp = 1,
    CursorDown,
    CursorLeft,
    CursorRight
}

/// <summary>
/// SecurityMode 枚举
/// </summary>
public enum SecurityMode
{
    Basic = 0,
    Full
}

/// <summary>
/// InputMethodEngineExtendAction 枚举
/// </summary>
public enum InputMethodEngineExtendAction
{
    SelectAll = 0,
    Cut = 3,
    Copy = 4,
    Paste = 5
}

/// <summary>
/// InputMethodEngineCapitalizeMode 枚举
/// </summary>
public enum InputMethodEngineCapitalizeMode
{
    None = 0,
    Sentences,
    Words,
    Characters
}