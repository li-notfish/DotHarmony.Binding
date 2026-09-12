using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SoundInterruptMode 枚举
/// </summary>
public enum SoundInterruptMode
{
    NoInterrupt = 0,
    SameSoundInterrupt = 1
}

/// <summary>
/// StateChangeReason 枚举
/// </summary>
public enum StateChangeReason
{
    User = 1,
    Background = 2
}

/// <summary>
/// HdrType 枚举
/// </summary>
public enum HdrType
{
    AVHdrTypeNone = 0,
    AVHdrTypeVivid = 1
}

/// <summary>
/// AVImageQueryOptions 枚举
/// </summary>
public enum AVImageQueryOptions
{
    AVImageQueryNextSync = 0,
    AVImageQueryPreviousSync,
    AVImageQueryClosestSync,
    AVImageQueryClosest
}

/// <summary>
/// FetchResult 枚举
/// </summary>
public enum FetchResult
{
    FetchFailed = 0,
    FetchSucceeded = 1,
    FetchCanceled = 2
}

/// <summary>
/// AVErrorCode 枚举
/// </summary>
public enum AVErrorCode
{
    AverrOk = 0,
    AverrNoPermission = 201,
    AverrInvalidParameter = 401,
    AverrUnsupportCapability = 801,
    AverrNoMemory = 5400101,
    AverrOperateNotPermit = 5400102,
    AverrIo = 5400103,
    AverrTimeout = 5400104,
    AverrServiceDied = 5400105,
    AverrUnsupportFormat = 5400106,
    AverrAudioInterrupted = 5400107,
    AverrIoHostNotFound = 5411001,
    AverrIoConnectionTimeout = 5411002,
    AverrIoNetworkAbnormal = 5411003,
    AverrIoNetworkUnavailable = 5411004,
    AverrIoNoPermission = 5411005,
    AverrIoRequestDenied = 5411006,
    AverrIoResourceNotFound = 5411007,
    AverrIoSslClientCertNeeded = 5411008,
    AverrIoSslConnectionFailed = 5411009,
    AverrIoSslServerCertUntrusted = 5411010,
    AverrIoUnsupportedRequest = 5411011,
    AverrSeekContinuousUnsupported = 5410002,
    AverrSuperResolutionUnsupported = 5410003,
    AverrSuperResolutionNotEnabled = 5410004,
    AverrIoCleartextNotPermitted = 5411012,
    AverrParameterOutOfRange = 5400108
}

/// <summary>
/// AVMetricsEventType 枚举
/// </summary>
public enum AVMetricsEventType
{
    AVMetricsEventStalling = 1,
    AVMetricsEventLipAsync = 2,
    AVMetricsEventLoadingrateChange = 3,
    AVMetricsEventLoadingError = 4,
    AVMetricsEventContentChanged = 5,
    AVMetricsEventContentDiscontinuity = 6,
    AVMetricsEventAudioAbnormal = 7
}

/// <summary>
/// PlaylistLoopMode 枚举
/// </summary>
public enum PlaylistLoopMode
{
    PlaylistLoopModeAll = 1,
    PlaylistLoopModeOne = 2,
    PlaylistLoopModeShuffle = 3,
    PlaylistLoopModeNone = 4
}

/// <summary>
/// PlaybackMetricsKey 枚举
/// </summary>
public enum PlaybackMetricsKey
{
    [Description("prepare_duration")]
    PrepareDuration,
    [Description("resource_connection_duration")]
    ResourceConnectionDuration,
    [Description("first_frame_decapsulation_duration")]
    FirstFrameDecapsulationDuration,
    [Description("total_playback_time")]
    TotalPlayingTime,
    [Description("loading_requests_count")]
    DownloadRequestsCount,
    [Description("total_loading_time")]
    TotalDownloadTime,
    [Description("total_loading_bytes")]
    TotalDownloadSize,
    [Description("stalling_count")]
    StallingCount,
    [Description("total_stalling_time")]
    TotalStallingTime,
    [Description("lip_async_count")]
    LipAsyncCount,
    [Description("total_lip_async_time")]
    TotalLipAsyncTime
}

/// <summary>
/// PlaybackInfoKey 枚举
/// </summary>
public enum PlaybackInfoKey
{
    [Description("server_ip_address")]
    ServerIPAddress,
    [Description("average_download_rate")]
    AvgDownloadRate,
    [Description("download_rate")]
    DownloadRate,
    [Description("is_downloading")]
    IsDownloading,
    [Description("buffer_duration")]
    BufferDuration
}

/// <summary>
/// MediaErrorCode 枚举
/// </summary>
public enum MediaErrorCode
{
    MserrOk = 0,
    MserrNoMemory = 1,
    MserrOperationNotPermit = 2,
    MserrInvalidVal = 3,
    MserrIo = 4,
    MserrTimeout = 5,
    MserrUnknown = 6,
    MserrServiceDied = 7,
    MserrInvalidState = 8,
    MserrUnsupported = 9
}

/// <summary>
/// BufferingInfoType 枚举
/// </summary>
public enum BufferingInfoType
{
    BufferingStart = 1,
    BufferingEnd = 2,
    BufferingPercent = 3,
    CachedDuration = 4
}

/// <summary>
/// LoadingRequestError 枚举
/// </summary>
public enum LoadingRequestError
{
    LoadingErrorSuccess = 0,
    LoadingErrorNotReady = 1,
    LoadingErrorNoResource = 2,
    LoadingErrorInvaidHandle = 3,
    LoadingErrorAccessDenied = 4,
    LoadingErrorAccessTimeout = 5,
    LoadingErrorAuthorizeFailed = 6
}

/// <summary>
/// AVMimeTypes 枚举
/// </summary>
public enum AVMimeTypes
{
    [Description("application/m3u8")]
    ApplicationM3U8
}

/// <summary>
/// AudioEncoder 枚举
/// </summary>
public enum AudioEncoder
{
    Default = 0,
    AmrNb = 1,
    AmrWb = 2,
    AacLc = 3,
    HeAac = 4
}

/// <summary>
/// AudioOutputFormat 枚举
/// </summary>
public enum AudioOutputFormat
{
    Default = 0,
    Mpeg4 = 2,
    AmrNb = 3,
    AmrWb = 4,
    AacAdts = 6
}

/// <summary>
/// PlaybackSpeed 枚举
/// </summary>
public enum PlaybackSpeed
{
    SpeedForward075X = 0,
    SpeedForward100X = 1,
    SpeedForward125X = 2,
    SpeedForward175X = 3,
    SpeedForward200X = 4,
    SpeedForward050X = 5,
    SpeedForward150X = 6,
    SpeedForward300X = 7,
    SpeedForward025X = 8,
    SpeedForward0125X = 9
}

/// <summary>
/// VideoScaleType 枚举
/// </summary>
public enum VideoScaleType
{
    VideoScaleTypeFit = 0,
    VideoScaleTypeFitCrop = 1,
    VideoScaleTypeScaledAspect = 2
}

/// <summary>
/// ContainerFormatType 枚举
/// </summary>
public enum ContainerFormatType
{
    [Description("mp4")]
    CftMpeg4,
    [Description("m4a")]
    CftMpeg4A,
    [Description("mp3")]
    CftMp3,
    [Description("wav")]
    CftWav,
    [Description("amr")]
    CftAmr,
    [Description("aac")]
    CftAac
}

/// <summary>
/// MediaType 枚举
/// </summary>
public enum MediaType
{
    [Description("-1")]
    MediaTypeUnsupported,
    MediaTypeAud = 0,
    MediaTypeVid = 1,
    MediaTypeSubtitle = 2,
    MediaTypeAttachment = 3,
    MediaTypeData = 4,
    MediaTypeTimedMetadata = 5,
    MediaTypeAuxiliary = 6
}

/// <summary>
/// MediaDescriptionKey 枚举
/// </summary>
public enum MediaDescriptionKey
{
    [Description("track_index")]
    MdKeyTrackIndex,
    [Description("track_type")]
    MdKeyTrackType,
    [Description("codec_mime")]
    MdKeyCodecMime,
    [Description("duration")]
    MdKeyDuration,
    [Description("bitrate")]
    MdKeyBitrate,
    [Description("width")]
    MdKeyWidth,
    [Description("height")]
    MdKeyHeight,
    [Description("frame_rate")]
    MdKeyFrameRate,
    [Description("channel_count")]
    MdKeyAudChannelCount,
    [Description("sample_rate")]
    MdKeyAudSampleRate,
    [Description("sample_depth")]
    MdKeyAudSampleDepth,
    [Description("language")]
    MdKeyLanguage,
    [Description("track_name")]
    MdKeyTrackName,
    [Description("hdr_type")]
    MdKeyHdrType,
    [Description("original_width")]
    MdKeyOriginalWidth,
    [Description("original_height")]
    MdKeyOriginalHeight,
    [Description("mime_type")]
    MdKeyMimeType,
    [Description("ref_track_ids")]
    MdKeyReferenceTrackIds,
    [Description("track_ref_type")]
    MdKeyTrackReferenceType
}

/// <summary>
/// AudioSourceType 枚举
/// </summary>
public enum AudioSourceType
{
    AudioSourceTypeDefault = 0,
    AudioSourceTypeMic = 1,
    AudioSourceTypeVoiceRecognition = 2,
    AudioSourceTypeVoiceCommunication = 7,
    AudioSourceTypeVoiceMessage = 10,
    AudioSourceTypeCamcorder = 13
}

/// <summary>
/// VideoSourceType 枚举
/// </summary>
public enum VideoSourceType
{
    VideoSourceTypeSurfaceYuv = 0,
    VideoSourceTypeSurfaceEs = 1
}

/// <summary>
/// FileGenerationMode 枚举
/// </summary>
public enum FileGenerationMode
{
    AppCreate = 0,
    AutoCreateCameraScene = 1
}

/// <summary>
/// AacProfile 枚举
/// </summary>
public enum AacProfile
{
    AacLc = 0,
    AacHe = 1,
    AacHeV2 = 2
}

/// <summary>
/// SeekMode 枚举
/// </summary>
public enum SeekMode
{
    SeekNextSync = 0,
    SeekPrevSync = 1,
    SeekClosest = 2,
    SeekContinuous = 3
}

/// <summary>
/// SwitchMode 枚举
/// </summary>
public enum SwitchMode
{
    Smooth = 0,
    Segment = 1,
    Closest = 2
}

/// <summary>
/// CodecMimeType 枚举
/// </summary>
public enum CodecMimeType
{
    [Description("video/h263")]
    VideoH263,
    [Description("video/avc")]
    VideoAvc,
    [Description("video/mpeg2")]
    VideoMpeg2,
    [Description("video/mp4v-es")]
    VideoMpeg4,
    [Description("video/x-vnd.on2.vp8")]
    VideoVp8,
    [Description("audio/mp4a-latm")]
    AudioAac,
    [Description("audio/vorbis")]
    AudioVorbis,
    [Description("audio/flac")]
    AudioFlac,
    [Description("video/hevc")]
    VideoHevc,
    [Description("audio/mpeg")]
    AudioMp3,
    [Description("audio/g711mu")]
    AudioG711Mu,
    [Description("audio/3gpp")]
    AudioAmrNb,
    [Description("audio/amr-wb")]
    AudioAmrWb,
    [Description("audio/raw")]
    AudioRaw
}

/// <summary>
/// AVScreenCaptureRecordPreset 枚举
/// </summary>
public enum AVScreenCaptureRecordPreset
{
    ScreenRecordPresetH264AacMp4 = 0,
    ScreenRecordPresetH265AacMp4 = 1
}

/// <summary>
/// AVScreenCaptureFillMode 枚举
/// </summary>
public enum AVScreenCaptureFillMode
{
    PreserveAspectRatio = 0,
    ScaleToFill = 1
}

/// <summary>
/// AVScreenCaptureStateCode 枚举
/// </summary>
public enum AVScreenCaptureStateCode
{
    ScreencaptureStateStarted = 0,
    ScreencaptureStateCanceled = 1,
    ScreencaptureStateStoppedByUser = 2,
    ScreencaptureStateInterruptedByOther = 3,
    ScreencaptureStateStoppedByCall = 4,
    ScreencaptureStateMicUnavailable = 5,
    ScreencaptureStateMicMutedByUser = 6,
    ScreencaptureStateMicUnmutedByUser = 7,
    ScreencaptureStateEnterPrivateScene = 8,
    ScreencaptureStateExitPrivateScene = 9,
    ScreencaptureStateStoppedByUserSwitches = 10,
    ScreencaptureStatePausedByUser = 11,
    ScreencaptureStateResumedByUser = 12,
    ScreencaptureStatePausedByApp = 13,
    ScreencaptureStateResumedByApp = 14
}

/// <summary>
/// PickerMode 枚举
/// </summary>
public enum PickerMode
{
    WindowOnly = 0,
    ScreenOnly = 1,
    ScreenAndWindow = 2,
    AppOnly = 3,
    WindowAndApp = 4,
    ScreenAndApp = 5,
    ScreenWindowAndApp = 6
}