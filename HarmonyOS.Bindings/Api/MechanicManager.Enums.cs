using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// TrackingEvent 枚举
/// </summary>
public enum TrackingEvent
{
    CameraTrackingUserEnabled = 0,
    CameraTrackingUserDisabled = 1,
    CameraTrackingLayoutChanged = 2
}

/// <summary>
/// MechDeviceType 枚举
/// </summary>
public enum MechDeviceType
{
    GimbalDevice = 0
}

/// <summary>
/// AttachState 枚举
/// </summary>
public enum AttachState
{
    Attached = 0,
    Detached = 1
}

/// <summary>
/// CameraTrackingLayout 枚举
/// </summary>
public enum CameraTrackingLayout
{
    Default = 0,
    Left = 1,
    Middle = 2,
    Right = 3
}