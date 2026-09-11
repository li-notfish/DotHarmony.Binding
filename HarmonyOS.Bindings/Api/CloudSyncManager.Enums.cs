using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DownloadStopReason 枚举
/// </summary>
public enum DownloadStopReason
{
    NoStop = 0,
    NetworkUnavailable = 1,
    LocalStorageFull = 2,
    TemperatureLimit = 3,
    UserStopped = 4,
    AppUnload = 5,
    OtherReason = 6
}

/// <summary>
/// DownloadState 枚举
/// </summary>
public enum DownloadState
{
    Running = 0,
    Completed = 1,
    Stopped = 2
}