using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ApplicationConfigurationConstantColorMode 枚举
/// </summary>
public enum ApplicationConfigurationConstantColorMode
{
    [Description("-1")]
    ColorModeNotSet,
    ColorModeDark = 0,
    ColorModeLight = 1
}