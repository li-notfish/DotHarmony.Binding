using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// GeolocationLocationRequestPriority 枚举
/// </summary>
public enum GeolocationLocationRequestPriority
{
    Unset = 512,
    Accuracy,
    LowPower,
    FirstFix
}

/// <summary>
/// GeolocationLocationRequestScenario 枚举
/// </summary>
public enum GeolocationLocationRequestScenario
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