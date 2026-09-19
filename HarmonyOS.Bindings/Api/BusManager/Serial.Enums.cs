using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DataBits 枚举
/// </summary>
public enum DataBits
{
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8
}

/// <summary>
/// StopBits 枚举
/// </summary>
public enum StopBits
{
    One = 1,
    Two = 2
}

/// <summary>
/// Parity 枚举
/// </summary>
public enum Parity
{
    [Description("none")]
    None,
    [Description("even")]
    Even,
    [Description("odd")]
    Odd,
    [Description("mark")]
    Mark,
    [Description("space")]
    Space
}