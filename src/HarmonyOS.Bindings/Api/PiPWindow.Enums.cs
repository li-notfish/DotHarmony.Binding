using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PiPTemplateType 枚举
/// </summary>
public enum PiPTemplateType
{
    VideoPlay = 0,
    VideoCall = 1,
    VideoMeeting = 2,
    VideoLive = 3
}

/// <summary>
/// PiPState 枚举
/// </summary>
public enum PiPState
{
    AboutToStart = 1,
    Started = 2,
    AboutToStop = 3,
    Stopped = 4,
    AboutToRestore = 5,
    Error = 6
}

/// <summary>
/// VideoPlayControlGroup 枚举
/// </summary>
public enum VideoPlayControlGroup
{
    VideoPreviousNext = 101,
    FastForwardBackward = 102
}

/// <summary>
/// VideoCallControlGroup 枚举
/// </summary>
public enum VideoCallControlGroup
{
    MicrophoneSwitch = 201,
    HangUpButton = 202,
    CameraSwitch = 203,
    MuteSwitch = 204
}

/// <summary>
/// VideoMeetingControlGroup 枚举
/// </summary>
public enum VideoMeetingControlGroup
{
    HangUpButton = 301,
    CameraSwitch = 302,
    MuteSwitch = 303,
    MicrophoneSwitch = 304
}

/// <summary>
/// VideoLiveControlGroup 枚举
/// </summary>
public enum VideoLiveControlGroup
{
    VideoPlayPause = 401,
    MuteSwitch = 402
}

/// <summary>
/// PiPControlStatus 枚举
/// </summary>
public enum PiPControlStatus
{
    Play = 1,
    Pause = 0,
    Open = 1,
    Close = 0
}

/// <summary>
/// PiPControlType 枚举
/// </summary>
public enum PiPControlType
{
    VideoPlayPause = 0,
    VideoPrevious = 1,
    VideoNext = 2,
    FastForward = 3,
    FastBackward = 4,
    HangUpButton = 5,
    MicrophoneSwitch = 6,
    CameraSwitch = 7,
    MuteSwitch = 8
}