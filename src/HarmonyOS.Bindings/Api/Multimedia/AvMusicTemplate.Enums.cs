using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DownloadStatus 枚举
/// </summary>
public enum DownloadStatus
{
    DownloadSuccess = 0,
    Downloading = 1,
    DownloadFail = 2
}

/// <summary>
/// SettingType 枚举
/// </summary>
public enum SettingType
{
    Switch = 0,
    List = 1,
    Jump = 2
}

/// <summary>
/// DialogType 枚举
/// </summary>
public enum DialogType
{
    Normal = 0,
    Internet = 1,
    Flow = 2,
    Paid = 3,
    Vip = 4,
    Login = 5,
    Error = 6,
    Unknown = 7
}

/// <summary>
/// ButtonType 枚举
/// </summary>
public enum ButtonType
{
    Normal = 0,
    Emphasize = 1
}

/// <summary>
/// EntityType 枚举
/// </summary>
public enum EntityType
{
    Unknown = 0,
    Single = 1,
    Singer = 2,
    Album = 3,
    Ranking = 4,
    Banner = 5,
    RadioStation = 6
}

/// <summary>
/// PlaybackState 枚举
/// </summary>
public enum PlaybackState
{
    PlaybackStatePrepare = 0,
    PlaybackStatePlay = 1,
    PlaybackStatePause = 2,
    PlaybackStateStop = 3,
    PlaybackStateCompleted = 4,
    PlaybackStateError = 5,
    PlaybackStateBuffering = 6
}

/// <summary>
/// MemberPurchaseType 枚举
/// </summary>
public enum MemberPurchaseType
{
    [Description("normal")]
    Normal,
    [Description("banner")]
    Banner
}

/// <summary>
/// AVMusicTemplateType 枚举
/// </summary>
public enum AVMusicTemplateType
{
    [Description("smartCar")]
    Default
}

/// <summary>
/// SearchPlayInfoType 枚举
/// </summary>
public enum SearchPlayInfoType
{
    [Description("playMusic")]
    PlayMusic,
    [Description("playVideo")]
    PlayVideo
}

/// <summary>
/// Sort 枚举
/// </summary>
public enum Sort
{
    None = 0,
    Order = 1,
    ReverseOrder = 2
}

/// <summary>
/// AVMusicTemplateErrorCode 枚举
/// </summary>
public enum AVMusicTemplateErrorCode
{
    ErrCodeCreateAVMusicTemplateFailed = 35000001,
    ErrCodeCreateAVMusicTemplateControllerFailed = 35000002,
    ErrCodeTemplateListenerNoExit = 35000003,
    ErrCodeControllerCallbackNoExit = 35000004,
    ErrCodeAVMusicTemplateNotExist = 35000005,
    ErrCodeControllerNotExist = 35000006,
    ErrCodeControllerIsExist = 35000007,
    ErrCodeServiceNotExist = 35000008,
    ErrCodeServiceException = 35000009,
    ErrCodeExceedMaxDataSize = 35000010,
    ErrCodeWriteResultException = 35000011,
    ErrCodeAVMusicTemplateError = 35000012
}