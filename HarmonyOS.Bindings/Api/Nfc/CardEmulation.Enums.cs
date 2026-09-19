using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// FeatureType 枚举
/// </summary>
public enum FeatureType
{
    Hce = 0,
    Uicc = 1,
    Ese = 2
}

/// <summary>
/// CardType 枚举
/// </summary>
public enum CardType
{
    [Description("payment")]
    Payment,
    [Description("other")]
    Other
}