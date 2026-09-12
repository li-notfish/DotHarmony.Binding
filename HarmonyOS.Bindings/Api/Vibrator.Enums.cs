using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// EffectId 枚举
/// </summary>
public enum EffectId
{
    [Description("haptic.clock.timer")]
    EffectClockTimer
}

/// <summary>
/// HapticFeedback 枚举
/// </summary>
public enum HapticFeedback
{
    [Description("haptic.effect.soft")]
    EffectSoft,
    [Description("haptic.effect.hard")]
    EffectHard,
    [Description("haptic.effect.sharp")]
    EffectSharp,
    [Description("haptic.notice.success")]
    EffectNoticeSuccess,
    [Description("haptic.notice.fail")]
    EffectNoticeFailure,
    [Description("haptic.notice.warning")]
    EffectNoticeWarning
}

/// <summary>
/// VibratorStopMode 枚举
/// </summary>
public enum VibratorStopMode
{
    [Description("time")]
    VibratorStopModeTime,
    [Description("preset")]
    VibratorStopModePreset
}

/// <summary>
/// VibratorEventType 枚举
/// </summary>
public enum VibratorEventType
{
    Continuous = 0,
    Transient = 1
}