using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PlayingState 枚举
/// </summary>
public enum PlayingState
{
    StateNotPlaying = 0,
    StatePlaying = 1
}

/// <summary>
/// CodecType 枚举
/// </summary>
public enum CodecType
{
    [Description("-1")]
    CodecTypeInvalid,
    CodecTypeSbc = 0,
    CodecTypeAac = 1,
    CodecTypeL2Hc = 2,
    CodecTypeL2Hcst = 3,
    CodecTypeLdac = 4
}

/// <summary>
/// CodecChannelMode 枚举
/// </summary>
public enum CodecChannelMode
{
    CodecChannelModeNone = 0,
    CodecChannelModeMono = 1,
    CodecChannelModeStereo = 2
}

/// <summary>
/// CodecBitsPerSample 枚举
/// </summary>
public enum CodecBitsPerSample
{
    CodecBitsPerSampleNone = 0,
    CodecBitsPerSample16 = 1,
    CodecBitsPerSample24 = 2,
    CodecBitsPerSample32 = 3
}

/// <summary>
/// CodecSampleRate 枚举
/// </summary>
public enum CodecSampleRate
{
    CodecSampleRateNone = 0,
    CodecSampleRate44100 = 1,
    CodecSampleRate48000 = 2,
    CodecSampleRate88200 = 3,
    CodecSampleRate96000 = 4,
    CodecSampleRate176400 = 5,
    CodecSampleRate192000 = 6
}

/// <summary>
/// CodecBitRate 枚举
/// </summary>
public enum CodecBitRate
{
    CodecBitRate96000 = 0,
    CodecBitRate128000 = 1,
    CodecBitRate192000 = 2,
    CodecBitRate256000 = 3,
    CodecBitRate320000 = 4,
    CodecBitRate480000 = 5,
    CodecBitRate640000 = 6,
    CodecBitRate960000 = 7,
    CodecBitRateAbr = 8,
    CodecBitRate1500000 = 9,
    CodecBitRate2300000 = 10
}

/// <summary>
/// CodecFrameLength 枚举
/// </summary>
public enum CodecFrameLength
{
    CodecFrameLength5Ms = 0,
    CodecFrameLength10Ms = 1
}