using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// EffectId 枚举
/// </summary>
public enum EffectId
{
    [Description("haptic.clock.timer")]
    EFFECT_CLOCK_TIMER
}

/// <summary>
/// HapticFeedback 枚举
/// </summary>
public enum HapticFeedback
{
    [Description("haptic.effect.soft")]
    EFFECT_SOFT,
    [Description("haptic.effect.hard")]
    EFFECT_HARD,
    [Description("haptic.effect.sharp")]
    EFFECT_SHARP,
    [Description("haptic.notice.success")]
    EFFECT_NOTICE_SUCCESS,
    [Description("haptic.notice.fail")]
    EFFECT_NOTICE_FAILURE,
    [Description("haptic.notice.warning")]
    EFFECT_NOTICE_WARNING
}

/// <summary>
/// VibratorStopMode 枚举
/// </summary>
public enum VibratorStopMode
{
    [Description("time")]
    VIBRATOR_STOP_MODE_TIME,
    [Description("preset")]
    VIBRATOR_STOP_MODE_PRESET
}

/// <summary>
/// VibratorEventType 枚举
/// </summary>
public enum VibratorEventType
{
    CONTINUOUS = 0,
    TRANSIENT = 1
}