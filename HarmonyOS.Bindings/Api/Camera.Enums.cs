using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CameraStatus 枚举
/// </summary>
public enum CameraStatus
{
    CameraStatusAppear = 0,
    CameraStatusDisappear = 1,
    CameraStatusAvailable = 2,
    CameraStatusUnavailable = 3
}

/// <summary>
/// CameraFoldStatus 枚举
/// </summary>
public enum CameraFoldStatus
{
    NonFoldable = 0,
    Expanded = 1,
    Folded = 2
}

/// <summary>
/// SensorColorFilterArrangement 枚举
/// </summary>
public enum SensorColorFilterArrangement
{
    Bggr = 0,
    Gbrg = 1,
    Grbg = 2,
    Rggb = 3
}

/// <summary>
/// CameraErrorCode 枚举
/// </summary>
public enum CameraErrorCode
{
    InvalidArgument = 7400101,
    OperationNotAllowed = 7400102,
    SessionNotConfig = 7400103,
    SessionNotRunning = 7400104,
    SessionConfigLocked = 7400105,
    DeviceSettingLocked = 7400106,
    ConflictCamera = 7400107,
    DeviceDisabled = 7400108,
    DevicePreempted = 7400109,
    UnresolvedConflictsWithCurrentConfigurations = 7400110,
    ServiceFatalError = 7400201
}

/// <summary>
/// TorchMode 枚举
/// </summary>
public enum TorchMode
{
    Off = 0,
    On = 1,
    Auto = 2
}

/// <summary>
/// CameraPosition 枚举
/// </summary>
public enum CameraPosition
{
    CameraPositionUnspecified = 0,
    CameraPositionBack = 1,
    CameraPositionFront = 2,
    CameraPositionFoldInner = 3
}

/// <summary>
/// CameraType 枚举
/// </summary>
public enum CameraType
{
    CameraTypeDefault = 0,
    CameraTypeWideAngle = 1,
    CameraTypeUltraWide = 2,
    CameraTypeTelephoto = 3,
    CameraTypeTrueDepth = 4
}

/// <summary>
/// ConnectionType 枚举
/// </summary>
public enum ConnectionType
{
    CameraConnectionBuiltIn = 0,
    CameraConnectionUsbPlugin = 1,
    CameraConnectionRemote = 2
}

/// <summary>
/// HostDeviceType 枚举
/// </summary>
public enum HostDeviceType
{
    UnknownType = 0,
    Phone = 14,
    Tablet = 17
}

/// <summary>
/// SceneMode 枚举
/// </summary>
public enum SceneMode
{
    NormalPhoto = 1,
    NormalVideo = 2,
    SecurePhoto = 12
}

/// <summary>
/// CameraFormat 枚举
/// </summary>
public enum CameraFormat
{
    CameraFormatRgba8888 = 3,
    CameraFormatDng = 4,
    CameraFormatYuv420Sp = 1003,
    CameraFormatJpeg = 2000,
    CameraFormatYcbcrP010,
    CameraFormatYcrcbP010 = 2002,
    CameraFormatHeic = 2003,
    CameraFormatDngXdraw = 5
}

/// <summary>
/// FlashMode 枚举
/// </summary>
public enum FlashMode
{
    FlashModeClose = 0,
    FlashModeOpen = 1,
    FlashModeAuto = 2,
    FlashModeAlwaysOpen = 3
}

/// <summary>
/// FlashState 枚举
/// </summary>
public enum FlashState
{
    FlashStateUnavailable = 0,
    FlashStateReady = 1,
    FlashStateFlashing = 2
}

/// <summary>
/// ExposureMode 枚举
/// </summary>
public enum ExposureMode
{
    [Description("-1")]
    ExposureModeUnspecified,
    ExposureModeLocked = 0,
    ExposureModeAuto = 1,
    ExposureModeContinuousAuto = 2,
    ExposureModeManual = 3
}

/// <summary>
/// ExposureState 枚举
/// </summary>
public enum ExposureState
{
    ExposureStateScan = 0,
    ExposureStateConverged = 1
}

/// <summary>
/// ExposureMeteringMode 枚举
/// </summary>
public enum ExposureMeteringMode
{
    Matrix = 0,
    Center = 1,
    Spot = 2
}

/// <summary>
/// FocusMode 枚举
/// </summary>
public enum FocusMode
{
    FocusModeManual = 0,
    FocusModeContinuousAuto = 1,
    FocusModeAuto = 2,
    FocusModeLocked = 3
}

/// <summary>
/// FocusState 枚举
/// </summary>
public enum FocusState
{
    FocusStateScan = 0,
    FocusStateFocused = 1,
    FocusStateUnfocused = 2
}

/// <summary>
/// WhiteBalanceMode 枚举
/// </summary>
public enum WhiteBalanceMode
{
    Auto = 0,
    Cloudy = 1,
    Incandescent = 2,
    Fluorescent = 3,
    Daylight = 4,
    Manual = 5,
    Locked = 6
}

/// <summary>
/// SmoothZoomMode 枚举
/// </summary>
public enum SmoothZoomMode
{
    Normal = 0
}

/// <summary>
/// VideoStabilizationMode 枚举
/// </summary>
public enum VideoStabilizationMode
{
    Off = 0,
    Low = 1,
    Middle = 2,
    High = 3,
    Auto = 4
}

/// <summary>
/// ControlCenterEffectType 枚举
/// </summary>
public enum ControlCenterEffectType
{
    Beauty = 0,
    Portrait = 1,
    AutoFraming = 2,
    ColorEffect = 3
}

/// <summary>
/// PreconfigType 枚举
/// </summary>
public enum PreconfigType
{
    Preconfig720P = 0,
    Preconfig1080P = 1,
    Preconfig4K = 2,
    PreconfigHighQuality = 3,
    PreconfigHighQualityPhotosessionBt2020 = 4
}

/// <summary>
/// PreconfigRatio 枚举
/// </summary>
public enum PreconfigRatio
{
    PreconfigRatio11 = 0,
    PreconfigRatio43 = 1,
    PreconfigRatio169 = 2
}

/// <summary>
/// PhotoQualityPrioritization 枚举
/// </summary>
public enum PhotoQualityPrioritization
{
    HighQuality = 0,
    Speed = 1
}

/// <summary>
/// QualityPrioritization 枚举
/// </summary>
public enum QualityPrioritization
{
    HighQuality = 0,
    PowerBalance = 1
}

/// <summary>
/// SystemPressureLevel 枚举
/// </summary>
public enum SystemPressureLevel
{
    SystemPressureNormal = 0,
    SystemPressureMild = 1,
    SystemPressureSevere = 2,
    SystemPressureCritical = 3,
    SystemPressureShutdown = 4
}

/// <summary>
/// ImageRotation 枚举
/// </summary>
public enum ImageRotation
{
    Rotation0 = 0,
    Rotation90 = 90,
    Rotation180 = 180,
    Rotation270 = 270
}

/// <summary>
/// QualityLevel 枚举
/// </summary>
public enum QualityLevel
{
    QualityLevelHigh = 0,
    QualityLevelMedium = 1,
    QualityLevelLow = 2
}

/// <summary>
/// VideoCodecType 枚举
/// </summary>
public enum VideoCodecType
{
    Avc = 0,
    Hevc = 1
}

/// <summary>
/// MetadataObjectType 枚举
/// </summary>
public enum MetadataObjectType
{
    FaceDetection = 0,
    HumanBody = 1,
    CatFace = 2,
    CatBody = 3,
    DogFace = 4,
    DogBody = 5,
    SalientDetection = 6,
    BarCodeDetection = 7,
    BasicFaceDetection = 8
}

/// <summary>
/// Emotion 枚举
/// </summary>
public enum Emotion
{
    Neutral = 0,
    Sadness = 1,
    Smile = 2,
    Surprise = 3
}

/// <summary>
/// CameraConcurrentType 枚举
/// </summary>
public enum CameraConcurrentType
{
    CameraFullCapability = 1,
    CameraLimitedCapability = 0
}

/// <summary>
/// OISMode 枚举
/// </summary>
public enum OISMode
{
    Off = 0,
    Auto = 1,
    Custom = 2
}

/// <summary>
/// OISAxes 枚举
/// </summary>
public enum OISAxes
{
    Pitch = 0,
    Yaw = 1
}

/// <summary>
/// AutomotiveCameraPosition 枚举
/// </summary>
public enum AutomotiveCameraPosition
{
    AutomotiveCameraPositionExteriorOther = 0,
    AutomotiveCameraPositionExteriorFront = 1,
    AutomotiveCameraPositionExteriorRear = 2,
    AutomotiveCameraPositionExteriorLeft = 3,
    AutomotiveCameraPositionExteriorRight = 4,
    AutomotiveCameraPositionInteriorOther = 5,
    AutomotiveCameraPositionInteriorRow1Left = 6,
    AutomotiveCameraPositionInteriorRow1Center = 7,
    AutomotiveCameraPositionInteriorRow1Right = 8,
    AutomotiveCameraPositionInteriorRow2Left = 9,
    AutomotiveCameraPositionInteriorRow2Center = 10,
    AutomotiveCameraPositionInteriorRow2Right = 11,
    AutomotiveCameraPositionInteriorRow3Left = 12,
    AutomotiveCameraPositionInteriorRow3Center = 13,
    AutomotiveCameraPositionInteriorRow3Right = 14
}