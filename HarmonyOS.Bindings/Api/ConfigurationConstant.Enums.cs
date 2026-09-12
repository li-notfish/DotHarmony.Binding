using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ColorMode 枚举
/// </summary>
public enum ColorMode
{
    [Description("-1")]
    ColorModeNotSet,
    ColorModeDark = 0,
    ColorModeLight = 1
}

/// <summary>
/// Direction 枚举
/// </summary>
public enum Direction
{
    [Description("-1")]
    DirectionNotSet,
    DirectionVertical = 0,
    DirectionHorizontal = 1
}

/// <summary>
/// ScreenDensity 枚举
/// </summary>
public enum ScreenDensity
{
    ScreenDensityNotSet = 0,
    ScreenDensitySdpi = 120,
    ScreenDensityMdpi = 160,
    ScreenDensityLdpi = 240,
    ScreenDensityXldpi = 320,
    ScreenDensityXxldpi = 480,
    ScreenDensityXxxldpi = 640
}