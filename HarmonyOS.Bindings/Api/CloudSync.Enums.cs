using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SyncState 枚举
/// </summary>
public enum SyncState
{
    Uploading = 0,
    UploadFailed = 1,
    Downloading = 2,
    DownloadFailed = 3,
    Completed = 4,
    Stopped = 5
}

/// <summary>
/// ErrorType 枚举
/// </summary>
public enum ErrorType
{
    NoError = 0,
    NetworkUnavailable = 1,
    WifiUnavailable = 2,
    BatteryLevelLow = 3,
    BatteryLevelWarning = 4,
    CloudStorageFull = 5,
    LocalStorageFull = 6,
    DeviceTemperatureTooHigh = 7,
    RemoteServerAbnormal = 8
}

/// <summary>
/// State 枚举
/// </summary>
public enum State
{
    Running = 0,
    Completed = 1,
    Failed = 2,
    Stopped = 3
}

/// <summary>
/// DownloadErrorType 枚举
/// </summary>
public enum DownloadErrorType
{
    NoError = 0,
    UnknownError = 1,
    NetworkUnavailable = 2,
    LocalStorageFull = 3,
    ContentNotFound = 4,
    FrequentUserRequests = 5
}

/// <summary>
/// DownloadFileType 枚举
/// </summary>
public enum DownloadFileType
{
    Content = 0,
    Thumbnail = 1,
    Lcd = 2
}

/// <summary>
/// FileState 枚举
/// </summary>
public enum FileState
{
    InitialAfterDownload = 0,
    Uploading = 1,
    Stopped = 2,
    ToBeUploaded = 3,
    UploadSuccess = 4,
    UploadFailure = 5
}

/// <summary>
/// NotifyType 枚举
/// </summary>
public enum NotifyType
{
    NotifyAdded = 0,
    NotifyModified = 1,
    NotifyDeleted = 2,
    NotifyRenamed = 3
}