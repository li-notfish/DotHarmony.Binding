using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LocationRequestPriority 枚举
/// </summary>
public enum LocationRequestPriority
{
    Unset = 512,
    Accuracy,
    LowPower,
    FirstFix
}

/// <summary>
/// LocationRequestScenario 枚举
/// </summary>
public enum LocationRequestScenario
{
    Unset = 768,
    Navigation,
    TrajectoryTracking,
    CarHailing,
    DailyLifeService,
    NoPower
}

/// <summary>
/// GeoLocationErrorCode 枚举
/// </summary>
public enum GeoLocationErrorCode
{
    InputParamsError,
    ReverseGeocodeError,
    GeocodeError,
    LocatorError,
    LocationSwitchError,
    LastKnownLocationError,
    LocationRequestTimeoutError
}

/// <summary>
/// LocationPrivacyType 枚举
/// </summary>
public enum LocationPrivacyType
{
    Others = 0,
    Startup,
    CoreLocation
}