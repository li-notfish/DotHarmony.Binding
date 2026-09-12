using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// RequestAction 枚举
/// </summary>
public enum RequestAction
{
    Download,
    Upload
}

/// <summary>
/// Mode 枚举
/// </summary>
public enum Mode
{
    Background,
    Foreground
}

/// <summary>
/// Network 枚举
/// </summary>
public enum Network
{
    Any,
    Wifi,
    Cellular
}

/// <summary>
/// BroadcastEvent 枚举
/// </summary>
public enum BroadcastEvent
{
    [Description("ohos.request.event.COMPLETE")]
    Complete
}

/// <summary>
/// RequestState 枚举
/// </summary>
public enum RequestState
{
    Initialized = 0,
    Waiting = 16,
    Running = 32,
    Retrying = 33,
    Paused = 48,
    Stopped = 49,
    Completed = 64,
    Failed = 65,
    Removed = 80
}

/// <summary>
/// Faults 枚举
/// </summary>
public enum Faults
{
    Others = 255,
    Disconnected = 0,
    Timeout = 16,
    Protocol = 32,
    Param = 48,
    Fsio = 64,
    Dns = 80,
    Tcp = 96,
    Ssl = 112,
    Redirect = 128,
    LowSpeed = 144
}

/// <summary>
/// WaitingReason 枚举
/// </summary>
public enum WaitingReason
{
    TaskQueueFull = 0,
    NetworkNotMatch = 1,
    AppBackground = 2,
    UserInactivated = 3
}