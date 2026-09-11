using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// WindowType 枚举
/// </summary>
public enum WindowType
{
    TypeApp = 0,
    TypeSystemAlert = 1,
    TypeFloat = 8,
    TypeDialog = 16,
    TypeMain = 32
}

/// <summary>
/// AvoidAreaType 枚举
/// </summary>
public enum AvoidAreaType
{
    TypeSystem = 0,
    TypeCutout = 1,
    TypeSystemGesture = 2,
    TypeKeyboard = 3,
    TypeNavigationIndicator = 4,
    TypeFloatNavigation = 5
}

/// <summary>
/// SplitRatioPreference 枚举
/// </summary>
public enum SplitRatioPreference
{
    Equal = 0,
    PrimaryDominant = 1,
    SecondaryDominant = 2
}

/// <summary>
/// WindowStatusType 枚举
/// </summary>
public enum WindowStatusType
{
    Undefined = 0,
    FullScreen = 1,
    Maximize = 2,
    Minimize = 3,
    Floating = 4,
    SplitScreen = 5
}

/// <summary>
/// PixelUnit 枚举
/// </summary>
public enum PixelUnit
{
    Px = 0,
    Vp = 1
}

/// <summary>
/// WindowAnimationCurve 枚举
/// </summary>
public enum WindowAnimationCurve
{
    Linear = 0,
    InterpolationSpring = 1,
    CubicBezier = 2
}

/// <summary>
/// WindowTransitionType 枚举
/// </summary>
public enum WindowTransitionType
{
    Destroy = 0
}

/// <summary>
/// AnimationType 枚举
/// </summary>
public enum AnimationType
{
    FadeInOut = 0
}

/// <summary>
/// WindowAnchor 枚举
/// </summary>
public enum WindowAnchor
{
    TopStart = 0,
    Top = 1,
    TopEnd = 2,
    Start = 3,
    Center = 4,
    End = 5,
    BottomStart = 6,
    Bottom = 7,
    BottomEnd = 8
}

/// <summary>
/// ColorSpace 枚举
/// </summary>
public enum ColorSpace
{
    Default = 0,
    WideGamut = 1
}

/// <summary>
/// RectChangeReason 枚举
/// </summary>
public enum RectChangeReason
{
    Undefined = 0,
    Maximize = 1,
    Recover = 2,
    Move = 3,
    Drag = 4,
    DragStart = 5,
    DragEnd = 6
}

[Flags]
/// <summary>
/// GlobalWindowMode 枚举
/// </summary>
public enum GlobalWindowMode
{
    Fullscreen = 1,
    Split = 2,
    Float = 4,
    Pip = 8
}

/// <summary>
/// OcclusionState 枚举
/// </summary>
public enum OcclusionState
{
    NoOcclusion = 0,
    PartialOcclusion = 1,
    FullOcclusion = 2
}

/// <summary>
/// OrientationExecutionResult 枚举
/// </summary>
public enum OrientationExecutionResult
{
    OrientationApplied = 0,
    OrientationIgnored = 1,
    OrientationPending = 2
}

/// <summary>
/// RotationChangeType 枚举
/// </summary>
public enum RotationChangeType
{
    WindowWillRotate = 0,
    WindowDidRotate = 1
}

/// <summary>
/// RectType 枚举
/// </summary>
public enum RectType
{
    RelativeToScreen = 0,
    RelativeToParentWindow = 1
}

/// <summary>
/// ScreenshotEventType 枚举
/// </summary>
public enum ScreenshotEventType
{
    SystemScreenshot = 0,
    SystemScreenshotAbort = 1,
    ScrollShotStart = 2,
    ScrollShotEnd = 3,
    ScrollShotAbort = 4
}

/// <summary>
/// RotationInfoType 枚举
/// </summary>
public enum RotationInfoType
{
    WindowOrientation = 0,
    DisplayOrientation = 1,
    DisplayRotation = 2
}

/// <summary>
/// WindowEventType 枚举
/// </summary>
public enum WindowEventType
{
    WindowShown = 1,
    WindowActive = 2,
    WindowInactive = 3,
    WindowHidden = 4,
    WindowDestroyed = 7
}

/// <summary>
/// MaximizePresentation 枚举
/// </summary>
public enum MaximizePresentation
{
    FollowAppImmersiveSetting = 0,
    ExitImmersive = 1,
    EnterImmersive = 2,
    EnterImmersiveDisableTitleAndDockHover = 3
}

/// <summary>
/// AcrossDisplayPresentation 枚举
/// </summary>
public enum AcrossDisplayPresentation
{
    FollowAcrossDisplaySetting = 0,
    EnterAcrossDisplayMode = 1,
    ExitAcrossDisplayMode = 2
}

/// <summary>
/// WindowStageEventType 枚举
/// </summary>
public enum WindowStageEventType
{
    Shown = 1,
    Active = 2,
    Inactive = 3,
    Hidden = 4,
    Resumed = 5,
    Paused = 6
}

/// <summary>
/// WindowStageLifecycleEventType 枚举
/// </summary>
public enum WindowStageLifecycleEventType
{
    Shown = 1,
    Resumed = 2,
    Paused = 3,
    Hidden = 4
}

/// <summary>
/// ModalityType 枚举
/// </summary>
public enum ModalityType
{
    WindowModality = 0,
    ApplicationModality = 1
}

/// <summary>
/// WindowPostureMode 枚举
/// </summary>
public enum WindowPostureMode
{
    DesktopMode = 0
}