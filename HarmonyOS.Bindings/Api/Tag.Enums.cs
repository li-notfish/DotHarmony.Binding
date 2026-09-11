using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// TnfType 枚举
/// </summary>
public enum TnfType
{
    TnfEmpty = 0,
    TnfWellKnown = 1,
    TnfMedia = 2,
    TnfAbsoluteUri = 3,
    TnfExtApp = 4,
    TnfUnknown = 5,
    TnfUnchanged = 6
}

/// <summary>
/// NfcForumType 枚举
/// </summary>
public enum NfcForumType
{
    NfcForumType1 = 1,
    NfcForumType2 = 2,
    NfcForumType3 = 3,
    NfcForumType4 = 4,
    MifareClassic = 101
}

/// <summary>
/// MifareClassicType 枚举
/// </summary>
public enum MifareClassicType
{
    TypeUnknown = 0,
    TypeClassic = 1,
    TypePlus = 2,
    TypePro = 3
}

/// <summary>
/// MifareClassicSize 枚举
/// </summary>
public enum MifareClassicSize
{
    McSizeMini = 320,
    McSize1K = 1024,
    McSize2K = 2048,
    McSize4K = 4096
}

/// <summary>
/// MifareUltralightType 枚举
/// </summary>
public enum MifareUltralightType
{
    TypeUnknown = 0,
    TypeUltralight = 1,
    TypeUltralightC = 2
}