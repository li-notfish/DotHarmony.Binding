using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// AvsessionProtocolType 枚举
/// </summary>
public enum AvsessionProtocolType
{
    TypeLocal = 0,
    TypeCastPlusStream = 2,
    TypeDlna = 4,
    TypeCastPlusAudio = 8
}

/// <summary>
/// CastDisplayState 枚举
/// </summary>
public enum CastDisplayState
{
    StateOff = 1,
    StateOn = 2
}

/// <summary>
/// ConnectionState 枚举
/// </summary>
public enum ConnectionState
{
    StateConnecting = 0,
    StateConnected = 1,
    StateDisconnected = 6
}

/// <summary>
/// DisplayTag 枚举
/// </summary>
public enum DisplayTag
{
    TagAudioVivid = 1
}

/// <summary>
/// DecoderType 枚举
/// </summary>
public enum DecoderType
{
    [Description("video/avc")]
    OhAvcodecMimeTypeVideoAvc,
    [Description("video/hevc")]
    OhAvcodecMimeTypeVideoHevc,
    [Description("audio/av3a")]
    OhAvcodecMimeTypeAudioVivid
}

/// <summary>
/// ResolutionLevel 枚举
/// </summary>
public enum ResolutionLevel
{
    Resolution480P = 0,
    Resolution720P = 1,
    Resolution1080P = 2,
    Resolution2K = 3,
    Resolution4K = 4
}

/// <summary>
/// ExtraKey 枚举
/// </summary>
public enum ExtraKey
{
    [Description("requireAbilityList")]
    RequireAbilityList,
    [Description("url-cast")]
    SupportUrlCasting,
    [Description("CurrentURIMetadata")]
    DlnaCurrentUriMetadata,
    [Description("DIDL-Lite")]
    DlnaDidlLite
}

/// <summary>
/// CallState 枚举
/// </summary>
public enum CallState
{
    CallStateIdle = 0,
    CallStateIncoming = 1,
    CallStateActive = 2,
    CallStateDialing = 3,
    CallStateWaiting = 4,
    CallStateHolding = 5,
    CallStateDisconnecting = 6
}

/// <summary>
/// AVCastCategory 枚举
/// </summary>
public enum AVCastCategory
{
    CategoryLocal = 0,
    CategoryRemote = 1
}

/// <summary>
/// AvsessionDeviceType 枚举
/// </summary>
public enum AvsessionDeviceType
{
    DeviceTypeLocal = 0,
    DeviceTypeTV = 2,
    DeviceTypeSmartSpeaker = 3,
    DeviceTypeCar = 4,
    DeviceTypePad = 6,
    DeviceTypeDefaultCastPlusStream = 7,
    DeviceType2In1 = 8,
    DeviceTypeBluetooth = 10,
    DeviceTypeHiplay = 15
}

/// <summary>
/// LoopMode 枚举
/// </summary>
public enum LoopMode
{
    LoopModeSequence = 0,
    LoopModeSingle = 1,
    LoopModeList = 2,
    LoopModeShuffle = 3,
    LoopModeCustom = 4
}

/// <summary>
/// SkipIntervals 枚举
/// </summary>
public enum SkipIntervals
{
    Seconds10 = 10,
    Seconds15 = 15,
    Seconds30 = 30
}

/// <summary>
/// BackgroundPlayMode 枚举
/// </summary>
public enum BackgroundPlayMode
{
    EnableBackgroundPlay = 0,
    DisableBackgroundPlay = 1
}

/// <summary>
/// AvsessionPlaybackState 枚举
/// </summary>
public enum AvsessionPlaybackState
{
    PlaybackStateInitial = 0,
    PlaybackStatePrepare = 1,
    PlaybackStatePlay = 2,
    PlaybackStatePause = 3,
    PlaybackStateFastForward = 4,
    PlaybackStateRewind = 5,
    PlaybackStateStop = 6,
    PlaybackStateCompleted = 7,
    PlaybackStateReleased = 8,
    PlaybackStateError = 9,
    PlaybackStateIdle = 10,
    PlaybackStateBuffering = 11
}

/// <summary>
/// CallerType 枚举
/// </summary>
public enum CallerType
{
    [Description("cast")]
    TypeCast,
    [Description("bluetooth")]
    TypeBluetooth,
    [Description("nearlink")]
    TypeNearlink,
    [Description("app")]
    TypeApp
}

/// <summary>
/// AVSessionErrorCode 枚举
/// </summary>
public enum AVSessionErrorCode
{
    ErrCodeServiceException = 6600101,
    ErrCodeSessionNotExist = 6600102,
    ErrCodeControllerNotExist = 6600103,
    ErrCodeRemoteConnectionErr = 6600104,
    ErrCodeCommandInvalid = 6600105,
    ErrCodeSessionInactive = 6600106,
    ErrCodeMessageOverload = 6600107,
    ErrCodeDeviceConnectionFailed = 6600108,
    ErrCodeRemoteConnectionNotExist = 6600109,
    ErrCodeDesktopLyricNotEnabled = 6600110,
    ErrCodeDesktopLyricNotSupported = 6600111,
    ErrCodeCastControlUnspecified = 6611000,
    ErrCodeCastControlRemoteError = 6611001,
    ErrCodeCastControlBehindLiveWindow = 6611002,
    ErrCodeCastControlTimeout = 6611003,
    ErrCodeCastControlRuntimeCheckFailed = 6611004,
    ErrCodeCastControlPlayerNotWorking = 6611100,
    ErrCodeCastControlSeekModeUnsupported = 6611101,
    ErrCodeCastControlIllegalSeekTarget = 6611102,
    ErrCodeCastControlPlayModeUnsupported = 6611103,
    ErrCodeCastControlPlaySpeedUnsupported = 6611104,
    ErrCodeCastControlDeviceMissing = 6611105,
    ErrCodeCastControlInvalidParam = 6611106,
    ErrCodeCastControlNoMemory = 6611107,
    ErrCodeCastControlOperationNotAllowed = 6611108,
    ErrCodeCastControlIoUnspecified = 6612000,
    ErrCodeCastControlIoNetworkConnectionFailed = 6612001,
    ErrCodeCastControlIoNetworkConnectionTimeout = 6612002,
    ErrCodeCastControlIoInvalidHttpContentType = 6612003,
    ErrCodeCastControlIoBadHttpStatus = 6612004,
    ErrCodeCastControlIoFileNotFound = 6612005,
    ErrCodeCastControlIoNoPermission = 6612006,
    ErrCodeCastControlIoCleartextNotPermitted = 6612007,
    ErrCodeCastControlIoReadPositionOutOfRange = 6612008,
    ErrCodeCastControlIoNoContents = 6612100,
    ErrCodeCastControlIoReadError = 6612101,
    ErrCodeCastControlIoContentBusy = 6612102,
    ErrCodeCastControlIoContentExpired = 6612103,
    ErrCodeCastControlIoUseForbidden = 6612104,
    ErrCodeCastControlIoNotVerified = 6612105,
    ErrCodeCastControlIoExhaustedAllowedUses = 6612106,
    ErrCodeCastControlIoNetworkPacketSendingFailed = 6612107,
    ErrCodeCastControlParsingUnspecified = 6613000,
    ErrCodeCastControlParsingContainerMalformed = 6613001,
    ErrCodeCastControlParsingManifestMalformed = 6613002,
    ErrCodeCastControlParsingContainerUnsupported = 6613003,
    ErrCodeCastControlParsingManifestUnsupported = 6613004,
    ErrCodeCastControlDecodingUnspecified = 6614000,
    ErrCodeCastControlDecodingInitFailed = 6614001,
    ErrCodeCastControlDecodingQueryFailed = 6614002,
    ErrCodeCastControlDecodingFailed = 6614003,
    ErrCodeCastControlDecodingFormatExceedsCapabilities = 6614004,
    ErrCodeCastControlDecodingFormatUnsupported = 6614005,
    ErrCodeCastControlAudioRendererUnspecified = 6615000,
    ErrCodeCastControlAudioRendererInitFailed = 6615001,
    ErrCodeCastControlAudioRendererWriteFailed = 6615002,
    ErrCodeCastControlDrmUnspecified = 6616000,
    ErrCodeCastControlDrmSchemeUnsupported = 6616001,
    ErrCodeCastControlDrmProvisioningFailed = 6616002,
    ErrCodeCastControlDrmContentError = 6616003,
    ErrCodeCastControlDrmLicenseAcquisitionFailed = 6616004,
    ErrCodeCastControlDrmDisallowedOperation = 6616005,
    ErrCodeCastControlDrmSystemError = 6616006,
    ErrCodeCastControlDrmDeviceRevoked = 6616007,
    ErrCodeCastControlDrmLicenseExpired = 6616008,
    ErrCodeCastControlDrmProvideKeyResponseError = 6616100
}