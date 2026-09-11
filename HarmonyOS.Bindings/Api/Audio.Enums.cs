using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// AudioErrors 枚举
/// </summary>
public enum AudioErrors
{
    ErrorInvalidParam = 6800101,
    ErrorNoMemory = 6800102,
    ErrorIllegalState = 6800103,
    ErrorUnsupported = 6800104,
    ErrorTimeout = 6800105,
    ErrorStreamLimit = 6800201,
    ErrorSystem = 6800301
}

/// <summary>
/// AudioState 枚举
/// </summary>
public enum AudioState
{
    [Description("-1")]
    StateInvalid,
    StateNew = 0,
    StatePrepared = 1,
    StateRunning = 2,
    StateStopped = 3,
    StateReleased = 4,
    StatePaused = 5
}

/// <summary>
/// AudioLoopbackMode 枚举
/// </summary>
public enum AudioLoopbackMode
{
    Hardware = 0
}

/// <summary>
/// AudioLoopbackStatus 枚举
/// </summary>
public enum AudioLoopbackStatus
{
    [Description("-2")]
    UnavailableDevice,
    [Description("-1")]
    UnavailableScene,
    AvailableIdle = 0,
    AvailableRunning = 1
}

/// <summary>
/// AudioLoopbackReverbPreset 枚举
/// </summary>
public enum AudioLoopbackReverbPreset
{
    Original = 1,
    Ktv = 2,
    Theater = 3,
    Concert = 4
}

/// <summary>
/// AudioLoopbackEqualizerPreset 枚举
/// </summary>
public enum AudioLoopbackEqualizerPreset
{
    Flat = 1,
    Full = 2,
    Bright = 3
}

/// <summary>
/// AudioVolumeType 枚举
/// </summary>
public enum AudioVolumeType
{
    VoiceCall = 0,
    Ringtone = 2,
    Media = 3,
    Alarm = 4,
    Accessibility = 5,
    VoiceAssistant = 9
}

/// <summary>
/// DeviceFlag 枚举
/// </summary>
public enum DeviceFlag
{
    OutputDevicesFlag = 1,
    InputDevicesFlag = 2,
    AllDevicesFlag = 3
}

/// <summary>
/// DeviceUsage 枚举
/// </summary>
public enum DeviceUsage
{
    MediaOutputDevices = 1,
    MediaInputDevices = 2,
    AllMediaDevices = 3,
    CallOutputDevices = 4,
    CallInputDevices = 8,
    AllCallDevices = 12
}

/// <summary>
/// DeviceRole 枚举
/// </summary>
public enum DeviceRole
{
    InputDevice = 1,
    OutputDevice = 2
}

/// <summary>
/// DeviceType 枚举
/// </summary>
public enum DeviceType
{
    Invalid = 0,
    Earpiece = 1,
    Speaker = 2,
    WiredHeadset = 3,
    WiredHeadphones = 4,
    BluetoothSco = 7,
    BluetoothA2Dp = 8,
    Mic = 15,
    UsbHeadset = 22,
    DisplayPort = 23,
    RemoteCast = 24,
    UsbDevice = 25,
    Hdmi = 27,
    LineDigital = 28,
    RemoteDaudio = 29,
    HearingAid = 30,
    Nearlink = 31,
    SystemPrivate = 200,
    Default = 1000
}

/// <summary>
/// ActiveDeviceType 枚举
/// </summary>
public enum ActiveDeviceType
{
    Speaker = 2,
    BluetoothSco = 7
}

/// <summary>
/// CommunicationDeviceType 枚举
/// </summary>
public enum CommunicationDeviceType
{
    Speaker = 2
}

/// <summary>
/// AudioRingMode 枚举
/// </summary>
public enum AudioRingMode
{
    RingerModeSilent = 0,
    RingerModeVibrate = 1,
    RingerModeNormal = 2
}

/// <summary>
/// AudioSampleFormat 枚举
/// </summary>
public enum AudioSampleFormat
{
    [Description("-1")]
    SampleFormatInvalid,
    SampleFormatU8 = 0,
    SampleFormatS16Le = 1,
    SampleFormatS24Le = 2,
    SampleFormatS32Le = 3,
    SampleFormatF32Le = 4
}

/// <summary>
/// AudioChannel 枚举
/// </summary>
public enum AudioChannel
{
    Channel1 = 1,
    Channel2 = 2,
    Channel3 = 3,
    Channel4 = 4,
    Channel5 = 5,
    Channel6 = 6,
    Channel7 = 7,
    Channel8 = 8,
    Channel9 = 9,
    Channel10 = 10,
    Channel12 = 12,
    Channel14 = 14,
    Channel16 = 16
}

/// <summary>
/// AudioSamplingRate 枚举
/// </summary>
public enum AudioSamplingRate
{
    SampleRate8000 = 8000,
    SampleRate11025 = 11025,
    SampleRate12000 = 12000,
    SampleRate16000 = 16000,
    SampleRate22050 = 22050,
    SampleRate24000 = 24000,
    SampleRate32000 = 32000,
    SampleRate44100 = 44100,
    SampleRate48000 = 48000,
    SampleRate64000 = 64000,
    SampleRate88200 = 88200,
    SampleRate96000 = 96000,
    SampleRate176400 = 176400,
    SampleRate192000 = 192000,
    SampleRate384000 = 384000
}

/// <summary>
/// AudioEncodingType 枚举
/// </summary>
public enum AudioEncodingType
{
    [Description("-1")]
    EncodingTypeInvalid,
    EncodingTypeRaw = 0
}

/// <summary>
/// ContentType 枚举
/// </summary>
public enum ContentType
{
    ContentTypeUnknown = 0,
    ContentTypeSpeech = 1,
    ContentTypeMusic = 2,
    ContentTypeMovie = 3,
    ContentTypeSonification = 4,
    ContentTypeRingtone = 5
}

/// <summary>
/// StreamUsage 枚举
/// </summary>
public enum StreamUsage
{
    StreamUsageUnknown = 0,
    StreamUsageMedia = 1,
    StreamUsageMusic = 1,
    StreamUsageVoiceCommunication = 2,
    StreamUsageVoiceAssistant = 3,
    StreamUsageAlarm = 4,
    StreamUsageVoiceMessage = 5,
    StreamUsageNotificationRingtone = 6,
    StreamUsageRingtone = 6,
    StreamUsageNotification = 7,
    StreamUsageAccessibility = 8,
    StreamUsageMovie = 10,
    StreamUsageGame = 11,
    StreamUsageAudiobook = 12,
    StreamUsageNavigation = 13,
    StreamUsageVideoCommunication = 17
}

/// <summary>
/// AudioPrivacyType 枚举
/// </summary>
public enum AudioPrivacyType
{
    PrivacyTypePublic = 0,
    PrivacyTypePrivate = 1,
    PrivacyTypeShared = 2
}

/// <summary>
/// InterruptMode 枚举
/// </summary>
public enum InterruptMode
{
    ShareMode = 0,
    IndependentMode = 1
}

/// <summary>
/// AudioRendererRate 枚举
/// </summary>
public enum AudioRendererRate
{
    RenderRateNormal = 0,
    RenderRateDouble = 1,
    RenderRateHalf = 2
}

/// <summary>
/// InterruptType 枚举
/// </summary>
public enum InterruptType
{
    InterruptTypeBegin = 1,
    InterruptTypeEnd = 2
}

/// <summary>
/// InterruptHint 枚举
/// </summary>
public enum InterruptHint
{
    InterruptHintNone = 0,
    InterruptHintResume = 1,
    InterruptHintPause = 2,
    InterruptHintStop = 3,
    InterruptHintDuck = 4,
    InterruptHintUnduck = 5,
    InterruptHintMute = 6,
    InterruptHintUnmute = 7
}

/// <summary>
/// InterruptForceType 枚举
/// </summary>
public enum InterruptForceType
{
    InterruptForce = 0,
    InterruptShare = 1
}

/// <summary>
/// InterruptActionType 枚举
/// </summary>
public enum InterruptActionType
{
    TypeActivated = 0,
    TypeInterrupt = 1
}

/// <summary>
/// DeviceChangeType 枚举
/// </summary>
public enum DeviceChangeType
{
    Connect = 0,
    Disconnect = 1
}

/// <summary>
/// AudioScene 枚举
/// </summary>
public enum AudioScene
{
    AudioSceneDefault = 0,
    AudioSceneRinging = 1,
    AudioScenePhoneCall = 2,
    AudioSceneVoiceChat = 3
}

/// <summary>
/// DeviceBlockStatus 枚举
/// </summary>
public enum DeviceBlockStatus
{
    Unblocked = 0,
    Blocked = 1
}

/// <summary>
/// AudioConcurrencyMode 枚举
/// </summary>
public enum AudioConcurrencyMode
{
    ConcurrencyDefault = 0,
    ConcurrencyMixWithOthers = 1,
    ConcurrencyDuckOthers = 2,
    ConcurrencyPauseOthers = 3
}

/// <summary>
/// AudioSessionDeactivatedReason 枚举
/// </summary>
public enum AudioSessionDeactivatedReason
{
    DeactivatedLowerPriority = 0,
    DeactivatedTimeout = 1
}

/// <summary>
/// AudioSessionScene 枚举
/// </summary>
public enum AudioSessionScene
{
    AudioSessionSceneMedia = 0,
    AudioSessionSceneGame = 1,
    AudioSessionSceneVoiceCommunication = 2
}

/// <summary>
/// AudioSessionStateChangeHint 枚举
/// </summary>
public enum AudioSessionStateChangeHint
{
    AudioSessionStateChangeHintResume = 0,
    AudioSessionStateChangeHintPause = 1,
    AudioSessionStateChangeHintStop = 2,
    AudioSessionStateChangeHintTimeOutStop = 3,
    AudioSessionStateChangeHintDuck = 4,
    AudioSessionStateChangeHintUnduck = 5,
    AudioSessionStateChangeHintMuteSuggestion = 6,
    AudioSessionStateChangeHintUnmuteSuggestion = 7,
    AudioSessionStateChangeHintMute = 8,
    AudioSessionStateChangeHintUnmute = 9
}

/// <summary>
/// OutputDeviceChangeRecommendedAction 枚举
/// </summary>
public enum OutputDeviceChangeRecommendedAction
{
    DeviceChangeRecommendToContinue = 0,
    DeviceChangeRecommendToStop = 1
}

/// <summary>
/// AudioSessionBehaviorFlags 枚举
/// </summary>
public enum AudioSessionBehaviorFlags
{
    DefaultBehavior = 0,
    MuteWhenInterrupted = 2,
    PauseWhenInterrupted = 4
}

/// <summary>
/// BluetoothAndNearlinkPreferredRecordCategory 枚举
/// </summary>
public enum BluetoothAndNearlinkPreferredRecordCategory
{
    PreferredNone = 0,
    PreferredDefault = 1,
    PreferredLowLatency = 2,
    PreferredHighQuality = 3
}

/// <summary>
/// NoiseReductionMode 枚举
/// </summary>
public enum NoiseReductionMode
{
    Fidelity = 0,
    PureVocals = 1,
    Standard = 2
}

/// <summary>
/// AudioVolumeMode 枚举
/// </summary>
public enum AudioVolumeMode
{
    SystemGlobal = 0,
    AppIndividual = 1
}

/// <summary>
/// ChannelBlendMode 枚举
/// </summary>
public enum ChannelBlendMode
{
    ModeDefault = 0,
    ModeBlendLr = 1,
    ModeAllLeft = 2,
    ModeAllRight = 3
}

/// <summary>
/// AudioStreamDeviceChangeReason 枚举
/// </summary>
public enum AudioStreamDeviceChangeReason
{
    ReasonUnknown = 0,
    ReasonNewDeviceAvailable = 1,
    ReasonOldDeviceUnavailable = 2,
    ReasonOverrode = 3,
    ReasonSessionActivated = 4,
    ReasonStreamPriorityChanged = 5
}

/// <summary>
/// AudioDataCallbackResult 枚举
/// </summary>
public enum AudioDataCallbackResult
{
    [Description("-1")]
    Invalid,
    Valid = 0
}

/// <summary>
/// AudioLatencyType 枚举
/// </summary>
public enum AudioLatencyType
{
    LatencyTypeAll = 0,
    LatencyTypeSoftware = 1,
    LatencyTypeHardware = 2
}

/// <summary>
/// SourceType 枚举
/// </summary>
public enum SourceType
{
    [Description("-1")]
    SourceTypeInvalid,
    SourceTypeMic = 0,
    SourceTypeVoiceRecognition = 1,
    SourceTypePlaybackCapture = 2,
    SourceTypeVoiceCommunication = 7,
    SourceTypeVoiceMessage = 10,
    SourceTypeCamcorder = 13,
    SourceTypeUnprocessed = 14,
    SourceTypeLive = 17
}

/// <summary>
/// AudioPlaybackCaptureMode 枚举
/// </summary>
public enum AudioPlaybackCaptureMode
{
    ModeDefault = 0,
    ModeMedia = 1,
    ModeExcludingSelf = 32768
}

/// <summary>
/// PlaybackCaptureStartState 枚举
/// </summary>
public enum PlaybackCaptureStartState
{
    StateSuccess = 0,
    StateFailed = 1,
    StateNotAuthorized = 2
}

/// <summary>
/// AudioEffectMode 枚举
/// </summary>
public enum AudioEffectMode
{
    EffectNone = 0,
    EffectDefault = 1
}

/// <summary>
/// AudioChannelLayout 枚举
/// </summary>
public enum AudioChannelLayout : long
{
    ChLayoutUnknown = 0,
    ChLayoutMono = 4,
    ChLayoutStereo = 3,
    ChLayoutStereoDownmix = 1610612736,
    ChLayout2Point1 = 11,
    ChLayout3Point0 = 259,
    ChLayoutSurround = 7,
    ChLayout3Point1 = 15,
    ChLayout4Point0 = 263,
    ChLayoutQuad = 51,
    ChLayoutQuadSide = 1539,
    ChLayout2Point0Point2 = 206158430211,
    ChLayoutAmbOrder1AcnN3D = 17592186044417,
    ChLayoutAmbOrder1AcnSn3D = 17592186048513,
    ChLayoutAmbOrder1Fuma = 17592186044673,
    ChLayout4Point1 = 271,
    ChLayout5Point0 = 1543,
    ChLayout5Point0Back = 55,
    ChLayout2Point1Point2 = 206158430219,
    ChLayout3Point0Point2 = 206158430215,
    ChLayout5Point1 = 1551,
    ChLayout5Point1Back = 63,
    ChLayout6Point0 = 1799,
    ChLayoutHexagonal = 311,
    ChLayout3Point1Point2 = 20495,
    ChLayout6Point0Front = 1731,
    ChLayout6Point1 = 1807,
    ChLayout6Point1Back = 319,
    ChLayout6Point1Front = 1739,
    ChLayout7Point0 = 1591,
    ChLayout7Point0Front = 1735,
    ChLayout7Point1 = 1599,
    ChLayoutOctagonal = 1847,
    ChLayout5Point1Point2 = 206158431759,
    ChLayout7Point1Wide = 1743,
    ChLayout7Point1WideBack = 255,
    ChLayoutAmbOrder2AcnN3D = 17592186044418,
    ChLayoutAmbOrder2AcnSn3D = 17592186048514,
    ChLayoutAmbOrder2Fuma = 17592186044674,
    ChLayout5Point1Point4 = 185871,
    ChLayout7Point1Point2 = 206158431807,
    ChLayout7Point1Point4 = 185919,
    ChLayout10Point2 = 6442473271,
    ChLayout9Point1Point4 = 6442636863,
    ChLayout9Point1Point6 = 212601067071,
    ChLayoutHexadecagonal = 6442710839,
    ChLayoutAmbOrder3AcnN3D = 17592186044419,
    ChLayoutAmbOrder3AcnSn3D = 17592186048515,
    ChLayoutAmbOrder3Fuma = 17592186044675
}