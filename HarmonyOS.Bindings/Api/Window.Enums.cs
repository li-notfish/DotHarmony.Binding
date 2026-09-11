using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// WindowType 枚举
/// </summary>
public enum WindowType
{
    TYPE_APP = 0,
    TYPE_SYSTEM_ALERT = 1,
    TYPE_FLOAT = 8,
    TYPE_DIALOG = 16,
    TYPE_MAIN = 32
}

/// <summary>
/// AvoidAreaType 枚举
/// </summary>
public enum AvoidAreaType
{
    TYPE_SYSTEM = 0,
    TYPE_CUTOUT = 1,
    TYPE_SYSTEM_GESTURE = 2,
    TYPE_KEYBOARD = 3,
    TYPE_NAVIGATION_INDICATOR = 4,
    TYPE_FLOAT_NAVIGATION = 5
}

/// <summary>
/// SplitRatioPreference 枚举
/// </summary>
public enum SplitRatioPreference
{
    EQUAL = 0,
    PRIMARY_DOMINANT = 1,
    SECONDARY_DOMINANT = 2
}

/// <summary>
/// WindowStatusType 枚举
/// </summary>
public enum WindowStatusType
{
    UNDEFINED = 0,
    FULL_SCREEN = 1,
    MAXIMIZE = 2,
    MINIMIZE = 3,
    FLOATING = 4,
    SPLIT_SCREEN = 5
}

/// <summary>
/// PixelUnit 枚举
/// </summary>
public enum PixelUnit
{
    PX = 0,
    VP = 1
}

/// <summary>
/// WindowAnimationCurve 枚举
/// </summary>
public enum WindowAnimationCurve
{
    LINEAR = 0,
    INTERPOLATION_SPRING = 1,
    CUBIC_BEZIER = 2
}

/// <summary>
/// WindowTransitionType 枚举
/// </summary>
public enum WindowTransitionType
{
    DESTROY = 0
}

/// <summary>
/// AnimationType 枚举
/// </summary>
public enum AnimationType
{
    FADE_IN_OUT = 0
}

/// <summary>
/// WindowAnchor 枚举
/// </summary>
public enum WindowAnchor
{
    TOP_START = 0,
    TOP = 1,
    TOP_END = 2,
    START = 3,
    CENTER = 4,
    END = 5,
    BOTTOM_START = 6,
    BOTTOM = 7,
    BOTTOM_END = 8
}

/// <summary>
/// ColorSpace 枚举
/// </summary>
public enum ColorSpace
{
    DEFAULT = 0,
    WIDE_GAMUT = 1
}

/// <summary>
/// RectChangeReason 枚举
/// </summary>
public enum RectChangeReason
{
    UNDEFINED = 0,
    MAXIMIZE = 1,
    RECOVER = 2,
    MOVE = 3,
    DRAG = 4,
    DRAG_START = 5,
    DRAG_END = 6
}

[Flags]
/// <summary>
/// GlobalWindowMode 枚举
/// </summary>
public enum GlobalWindowMode
{
    FULLSCREEN = 1,
    SPLIT = 2,
    FLOAT = 4,
    PIP = 8
}

/// <summary>
/// OcclusionState 枚举
/// </summary>
public enum OcclusionState
{
    NO_OCCLUSION = 0,
    PARTIAL_OCCLUSION = 1,
    FULL_OCCLUSION = 2
}

/// <summary>
/// OrientationExecutionResult 枚举
/// </summary>
public enum OrientationExecutionResult
{
    ORIENTATION_APPLIED = 0,
    ORIENTATION_IGNORED = 1,
    ORIENTATION_PENDING = 2
}

/// <summary>
/// RotationChangeType 枚举
/// </summary>
public enum RotationChangeType
{
    WINDOW_WILL_ROTATE = 0,
    WINDOW_DID_ROTATE = 1
}

/// <summary>
/// RectType 枚举
/// </summary>
public enum RectType
{
    RELATIVE_TO_SCREEN = 0,
    RELATIVE_TO_PARENT_WINDOW = 1
}

/// <summary>
/// ScreenshotEventType 枚举
/// </summary>
public enum ScreenshotEventType
{
    SYSTEM_SCREENSHOT = 0,
    SYSTEM_SCREENSHOT_ABORT = 1,
    SCROLL_SHOT_START = 2,
    SCROLL_SHOT_END = 3,
    SCROLL_SHOT_ABORT = 4
}

/// <summary>
/// RotationInfoType 枚举
/// </summary>
public enum RotationInfoType
{
    WINDOW_ORIENTATION = 0,
    DISPLAY_ORIENTATION = 1,
    DISPLAY_ROTATION = 2
}

/// <summary>
/// WindowEventType 枚举
/// </summary>
public enum WindowEventType
{
    WINDOW_SHOWN = 1,
    WINDOW_ACTIVE = 2,
    WINDOW_INACTIVE = 3,
    WINDOW_HIDDEN = 4,
    WINDOW_DESTROYED = 7
}

/// <summary>
/// MaximizePresentation 枚举
/// </summary>
public enum MaximizePresentation
{
    FOLLOW_APP_IMMERSIVE_SETTING = 0,
    EXIT_IMMERSIVE = 1,
    ENTER_IMMERSIVE = 2,
    ENTER_IMMERSIVE_DISABLE_TITLE_AND_DOCK_HOVER = 3
}

/// <summary>
/// AcrossDisplayPresentation 枚举
/// </summary>
public enum AcrossDisplayPresentation
{
    FOLLOW_ACROSS_DISPLAY_SETTING = 0,
    ENTER_ACROSS_DISPLAY_MODE = 1,
    EXIT_ACROSS_DISPLAY_MODE = 2
}

/// <summary>
/// WindowStageEventType 枚举
/// </summary>
public enum WindowStageEventType
{
    SHOWN = 1,
    ACTIVE = 2,
    INACTIVE = 3,
    HIDDEN = 4,
    RESUMED = 5,
    PAUSED = 6
}

/// <summary>
/// WindowStageLifecycleEventType 枚举
/// </summary>
public enum WindowStageLifecycleEventType
{
    SHOWN = 1,
    RESUMED = 2,
    PAUSED = 3,
    HIDDEN = 4
}

/// <summary>
/// ModalityType 枚举
/// </summary>
public enum ModalityType
{
    WINDOW_MODALITY = 0,
    APPLICATION_MODALITY = 1
}

/// <summary>
/// WindowPostureMode 枚举
/// </summary>
public enum WindowPostureMode
{
    DESKTOP_MODE = 0
}