using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// InputMethodDirection 枚举
/// </summary>
public enum InputMethodDirection
{
    CursorUp = 1,
    CursorDown,
    CursorLeft,
    CursorRight
}

/// <summary>
/// TextInputType 枚举
/// </summary>
public enum TextInputType
{
    [Description("-1")]
    None,
    Text = 0,
    Multiline,
    Number,
    Phone,
    Datetime,
    EmailAddress,
    Url,
    VisiblePassword,
    NumberPassword,
    ScreenLockPassword,
    UserName,
    NewPassword,
    NumberDecimal,
    OneTimeCode
}

/// <summary>
/// EnterKeyType 枚举
/// </summary>
public enum EnterKeyType
{
    Unspecified = 0,
    None,
    Go,
    Search,
    Send,
    Next,
    Done,
    Previous,
    Newline
}

/// <summary>
/// KeyboardStatus 枚举
/// </summary>
public enum KeyboardStatus
{
    None = 0,
    Hide = 1,
    Show = 2
}

/// <summary>
/// ExtendAction 枚举
/// </summary>
public enum ExtendAction
{
    SelectAll = 0,
    Cut = 3,
    Copy = 4,
    Paste = 5
}

/// <summary>
/// EnabledState 枚举
/// </summary>
public enum EnabledState
{
    Disabled = 0,
    BasicMode,
    FullExperienceMode
}

/// <summary>
/// RequestKeyboardReason 枚举
/// </summary>
public enum RequestKeyboardReason
{
    None = 0,
    Mouse = 1,
    Touch = 2,
    Other = 20
}

/// <summary>
/// CapitalizeMode 枚举
/// </summary>
public enum CapitalizeMode
{
    None = 0,
    Sentences,
    Words,
    Characters
}

/// <summary>
/// AttachFailureReason 枚举
/// </summary>
public enum AttachFailureReason
{
    CallerNotFocused = 0,
    ImeAbnormal,
    ServiceAbnormal
}