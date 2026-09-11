using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CameraStatus 枚举
/// </summary>
public enum CameraStatus
{
    CAMERA_STATUS_APPEAR = 0,
    CAMERA_STATUS_DISAPPEAR = 1,
    CAMERA_STATUS_AVAILABLE = 2,
    CAMERA_STATUS_UNAVAILABLE = 3
}

/// <summary>
/// SensorColorFilterArrangement 枚举
/// </summary>
public enum SensorColorFilterArrangement
{
    BGGR = 0,
    GBRG = 1,
    GRBG = 2,
    RGGB = 3
}

/// <summary>
/// CameraErrorCode 枚举
/// </summary>
public enum CameraErrorCode
{
    INVALID_ARGUMENT = 7400101,
    OPERATION_NOT_ALLOWED = 7400102,
    SESSION_NOT_CONFIG = 7400103,
    SESSION_NOT_RUNNING = 7400104,
    SESSION_CONFIG_LOCKED = 7400105,
    DEVICE_SETTING_LOCKED = 7400106,
    CONFLICT_CAMERA = 7400107,
    DEVICE_DISABLED = 7400108,
    DEVICE_PREEMPTED = 7400109,
    UNRESOLVED_CONFLICTS_WITH_CURRENT_CONFIGURATIONS = 7400110,
    SERVICE_FATAL_ERROR = 7400201
}

/// <summary>
/// TorchMode 枚举
/// </summary>
public enum TorchMode
{
    OFF = 0,
    ON = 1,
    AUTO = 2
}

/// <summary>
/// CameraPosition 枚举
/// </summary>
public enum CameraPosition
{
    CAMERA_POSITION_UNSPECIFIED = 0,
    CAMERA_POSITION_BACK = 1,
    CAMERA_POSITION_FRONT = 2,
    CAMERA_POSITION_FOLD_INNER = 3
}

/// <summary>
/// CameraType 枚举
/// </summary>
public enum CameraType
{
    CAMERA_TYPE_DEFAULT = 0,
    CAMERA_TYPE_WIDE_ANGLE = 1,
    CAMERA_TYPE_ULTRA_WIDE = 2,
    CAMERA_TYPE_TELEPHOTO = 3,
    CAMERA_TYPE_TRUE_DEPTH = 4
}

/// <summary>
/// ConnectionType 枚举
/// </summary>
public enum ConnectionType
{
    CAMERA_CONNECTION_BUILT_IN = 0,
    CAMERA_CONNECTION_USB_PLUGIN = 1,
    CAMERA_CONNECTION_REMOTE = 2
}

/// <summary>
/// HostDeviceType 枚举
/// </summary>
public enum HostDeviceType
{
    UNKNOWN_TYPE = 0,
    PHONE = 14,
    TABLET = 17
}

/// <summary>
/// SceneMode 枚举
/// </summary>
public enum SceneMode
{
    NORMAL_PHOTO = 1,
    NORMAL_VIDEO = 2,
    SECURE_PHOTO = 12
}

/// <summary>
/// CameraFormat 枚举
/// </summary>
public enum CameraFormat
{
    CAMERA_FORMAT_RGBA_8888 = 3,
    CAMERA_FORMAT_DNG = 4,
    CAMERA_FORMAT_YUV_420_SP = 1003,
    CAMERA_FORMAT_JPEG = 2000,
    CAMERA_FORMAT_YCBCR_P010,
    CAMERA_FORMAT_YCRCB_P010 = 2002,
    CAMERA_FORMAT_HEIC = 2003,
    CAMERA_FORMAT_DNG_XDRAW = 5
}

/// <summary>
/// FlashMode 枚举
/// </summary>
public enum FlashMode
{
    FLASH_MODE_CLOSE = 0,
    FLASH_MODE_OPEN = 1,
    FLASH_MODE_AUTO = 2,
    FLASH_MODE_ALWAYS_OPEN = 3
}

/// <summary>
/// FlashState 枚举
/// </summary>
public enum FlashState
{
    FLASH_STATE_UNAVAILABLE = 0,
    FLASH_STATE_READY = 1,
    FLASH_STATE_FLASHING = 2
}

/// <summary>
/// ExposureMode 枚举
/// </summary>
public enum ExposureMode
{
    [Description("-1")]
    EXPOSURE_MODE_UNSPECIFIED,
    EXPOSURE_MODE_LOCKED = 0,
    EXPOSURE_MODE_AUTO = 1,
    EXPOSURE_MODE_CONTINUOUS_AUTO = 2,
    EXPOSURE_MODE_MANUAL = 3
}

/// <summary>
/// ExposureState 枚举
/// </summary>
public enum ExposureState
{
    EXPOSURE_STATE_SCAN = 0,
    EXPOSURE_STATE_CONVERGED = 1
}

/// <summary>
/// ExposureMeteringMode 枚举
/// </summary>
public enum ExposureMeteringMode
{
    MATRIX = 0,
    CENTER = 1,
    SPOT = 2
}

/// <summary>
/// FocusMode 枚举
/// </summary>
public enum FocusMode
{
    FOCUS_MODE_MANUAL = 0,
    FOCUS_MODE_CONTINUOUS_AUTO = 1,
    FOCUS_MODE_AUTO = 2,
    FOCUS_MODE_LOCKED = 3
}

/// <summary>
/// FocusState 枚举
/// </summary>
public enum FocusState
{
    FOCUS_STATE_SCAN = 0,
    FOCUS_STATE_FOCUSED = 1,
    FOCUS_STATE_UNFOCUSED = 2
}

/// <summary>
/// WhiteBalanceMode 枚举
/// </summary>
public enum WhiteBalanceMode
{
    AUTO = 0,
    CLOUDY = 1,
    INCANDESCENT = 2,
    FLUORESCENT = 3,
    DAYLIGHT = 4,
    MANUAL = 5,
    LOCKED = 6
}

/// <summary>
/// SmoothZoomMode 枚举
/// </summary>
public enum SmoothZoomMode
{
    NORMAL = 0
}

/// <summary>
/// VideoStabilizationMode 枚举
/// </summary>
public enum VideoStabilizationMode
{
    OFF = 0,
    LOW = 1,
    MIDDLE = 2,
    HIGH = 3,
    AUTO = 4
}

/// <summary>
/// ControlCenterEffectType 枚举
/// </summary>
public enum ControlCenterEffectType
{
    BEAUTY = 0,
    PORTRAIT = 1,
    AUTO_FRAMING = 2,
    COLOR_EFFECT = 3
}

/// <summary>
/// PreconfigType 枚举
/// </summary>
public enum PreconfigType
{
    PRECONFIG_720P = 0,
    PRECONFIG_1080P = 1,
    PRECONFIG_4K = 2,
    PRECONFIG_HIGH_QUALITY = 3,
    PRECONFIG_HIGH_QUALITY_PHOTOSESSION_BT2020 = 4
}

/// <summary>
/// PreconfigRatio 枚举
/// </summary>
public enum PreconfigRatio
{
    PRECONFIG_RATIO_1_1 = 0,
    PRECONFIG_RATIO_4_3 = 1,
    PRECONFIG_RATIO_16_9 = 2
}

/// <summary>
/// PhotoQualityPrioritization 枚举
/// </summary>
public enum PhotoQualityPrioritization
{
    HIGH_QUALITY = 0,
    SPEED = 1
}

/// <summary>
/// QualityPrioritization 枚举
/// </summary>
public enum QualityPrioritization
{
    HIGH_QUALITY = 0,
    POWER_BALANCE = 1
}

/// <summary>
/// SystemPressureLevel 枚举
/// </summary>
public enum SystemPressureLevel
{
    SYSTEM_PRESSURE_NORMAL = 0,
    SYSTEM_PRESSURE_MILD = 1,
    SYSTEM_PRESSURE_SEVERE = 2,
    SYSTEM_PRESSURE_CRITICAL = 3,
    SYSTEM_PRESSURE_SHUTDOWN = 4
}

/// <summary>
/// ImageRotation 枚举
/// </summary>
public enum ImageRotation
{
    ROTATION_0 = 0,
    ROTATION_90 = 90,
    ROTATION_180 = 180,
    ROTATION_270 = 270
}

/// <summary>
/// QualityLevel 枚举
/// </summary>
public enum QualityLevel
{
    QUALITY_LEVEL_HIGH = 0,
    QUALITY_LEVEL_MEDIUM = 1,
    QUALITY_LEVEL_LOW = 2
}

/// <summary>
/// VideoCodecType 枚举
/// </summary>
public enum VideoCodecType
{
    AVC = 0,
    HEVC = 1
}

/// <summary>
/// MetadataObjectType 枚举
/// </summary>
public enum MetadataObjectType
{
    FACE_DETECTION = 0,
    HUMAN_BODY = 1,
    CAT_FACE = 2,
    CAT_BODY = 3,
    DOG_FACE = 4,
    DOG_BODY = 5,
    SALIENT_DETECTION = 6,
    BAR_CODE_DETECTION = 7,
    BASIC_FACE_DETECTION = 8
}

/// <summary>
/// Emotion 枚举
/// </summary>
public enum Emotion
{
    NEUTRAL = 0,
    SADNESS = 1,
    SMILE = 2,
    SURPRISE = 3
}

/// <summary>
/// CameraConcurrentType 枚举
/// </summary>
public enum CameraConcurrentType
{
    CAMERA_FULL_CAPABILITY = 1,
    CAMERA_LIMITED_CAPABILITY = 0
}

/// <summary>
/// OISMode 枚举
/// </summary>
public enum OISMode
{
    OFF = 0,
    AUTO = 1,
    CUSTOM = 2
}

/// <summary>
/// OISAxes 枚举
/// </summary>
public enum OISAxes
{
    PITCH = 0,
    YAW = 1
}

/// <summary>
/// AutomotiveCameraPosition 枚举
/// </summary>
public enum AutomotiveCameraPosition
{
    AUTOMOTIVE_CAMERA_POSITION_EXTERIOR_OTHER = 0,
    AUTOMOTIVE_CAMERA_POSITION_EXTERIOR_FRONT = 1,
    AUTOMOTIVE_CAMERA_POSITION_EXTERIOR_REAR = 2,
    AUTOMOTIVE_CAMERA_POSITION_EXTERIOR_LEFT = 3,
    AUTOMOTIVE_CAMERA_POSITION_EXTERIOR_RIGHT = 4,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_OTHER = 5,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_ROW_1_LEFT = 6,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_ROW_1_CENTER = 7,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_ROW_1_RIGHT = 8,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_ROW_2_LEFT = 9,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_ROW_2_CENTER = 10,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_ROW_2_RIGHT = 11,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_ROW_3_LEFT = 12,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_ROW_3_CENTER = 13,
    AUTOMOTIVE_CAMERA_POSITION_INTERIOR_ROW_3_RIGHT = 14
}