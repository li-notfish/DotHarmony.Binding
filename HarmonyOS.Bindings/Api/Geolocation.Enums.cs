using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LocationRequestPriority 枚举
/// </summary>
public enum LocationRequestPriority
{
    UNSET = 512,
    ACCURACY,
    LOW_POWER,
    FIRST_FIX
}

/// <summary>
/// LocationRequestScenario 枚举
/// </summary>
public enum LocationRequestScenario
{
    UNSET = 768,
    NAVIGATION,
    TRAJECTORY_TRACKING,
    CAR_HAILING,
    DAILY_LIFE_SERVICE,
    NO_POWER
}

/// <summary>
/// GeoLocationErrorCode 枚举
/// </summary>
public enum GeoLocationErrorCode
{
    INPUT_PARAMS_ERROR,
    REVERSE_GEOCODE_ERROR,
    GEOCODE_ERROR,
    LOCATOR_ERROR,
    LOCATION_SWITCH_ERROR,
    LAST_KNOWN_LOCATION_ERROR,
    LOCATION_REQUEST_TIMEOUT_ERROR
}

/// <summary>
/// LocationPrivacyType 枚举
/// </summary>
public enum LocationPrivacyType
{
    OTHERS = 0,
    STARTUP,
    CORE_LOCATION
}