using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Action 枚举
/// </summary>
public enum Action
{
    DOWNLOAD,
    UPLOAD
}

/// <summary>
/// Mode 枚举
/// </summary>
public enum Mode
{
    BACKGROUND,
    FOREGROUND
}

/// <summary>
/// Network 枚举
/// </summary>
public enum Network
{
    ANY,
    WIFI,
    CELLULAR
}

/// <summary>
/// BroadcastEvent 枚举
/// </summary>
public enum BroadcastEvent
{
    [Description("ohos.request.event.COMPLETE")]
    COMPLETE
}

/// <summary>
/// State 枚举
/// </summary>
public enum State
{
    INITIALIZED = 0,
    WAITING = 16,
    RUNNING = 32,
    RETRYING = 33,
    PAUSED = 48,
    STOPPED = 49,
    COMPLETED = 64,
    FAILED = 65,
    REMOVED = 80
}

/// <summary>
/// Faults 枚举
/// </summary>
public enum Faults
{
    OTHERS = 255,
    DISCONNECTED = 0,
    TIMEOUT = 16,
    PROTOCOL = 32,
    PARAM = 48,
    FSIO = 64,
    DNS = 80,
    TCP = 96,
    SSL = 112,
    REDIRECT = 128,
    LOW_SPEED = 144
}

/// <summary>
/// WaitingReason 枚举
/// </summary>
public enum WaitingReason
{
    TASK_QUEUE_FULL = 0,
    NETWORK_NOT_MATCH = 1,
    APP_BACKGROUND = 2,
    USER_INACTIVATED = 3
}