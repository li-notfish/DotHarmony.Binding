using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ColorSpace 枚举
/// </summary>
public enum ColorSpace
{
    Unknown = 0,
    AdobeRgb1998 = 1,
    DciP3 = 2,
    DisplayP3 = 3,
    Srgb = 4,
    Bt709 = 6,
    Bt601Ebu = 7,
    Bt601SmpteC = 8,
    Bt2020Hlg = 9,
    Bt2020Pq = 10,
    P3Hlg = 11,
    P3Pq = 12,
    AdobeRgb1998Limit = 13,
    DisplayP3Limit = 14,
    SrgbLimit = 15,
    Bt709Limit = 16,
    Bt601EbuLimit = 17,
    Bt601SmpteCLimit = 18,
    Bt2020HlgLimit = 19,
    Bt2020PqLimit = 20,
    P3HlgLimit = 21,
    P3PqLimit = 22,
    LinearP3 = 23,
    LinearSrgb = 24,
    [Description("LINEAR_SRGB")]
    LinearBt709,
    LinearBt2020 = 25,
    [Description("SRGB")]
    DisplaySrgb,
    [Description("DISPLAY_P3")]
    DisplayP3Srgb,
    [Description("P3_HLG")]
    DisplayP3Hlg,
    [Description("P3_PQ")]
    DisplayP3Pq,
    HLog = 26,
    DisplayBt2020Srgb = 27,
    Custom = 5
}