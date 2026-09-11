using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LocationSourceType 枚举
/// </summary>
public enum LocationSourceType
{
    Gnss = 1,
    Network = 2,
    Indoor = 3,
    Rtk = 4
}

/// <summary>
/// CoordinateSystemType 枚举
/// </summary>
public enum CoordinateSystemType
{
    Wgs84 = 1,
    Gcj02 = 2
}

/// <summary>
/// LocationError 枚举
/// </summary>
public enum LocationError
{
    [Description("-1")]
    LocatingFailedDefault,
    [Description("-2")]
    LocatingFailedLocationPermissionDenied,
    [Description("-3")]
    LocatingFailedBackgroundPermissionDenied,
    [Description("-4")]
    LocatingFailedLocationSwitchOff,
    [Description("-5")]
    LocatingFailedInternetAccessFailure
}

/// <summary>
/// GeofenceTransitionEvent 枚举
/// </summary>
public enum GeofenceTransitionEvent
{
    GeofenceTransitionEventEnter = 1,
    GeofenceTransitionEventExit = 2,
    GeofenceTransitionEventDwell = 4
}

/// <summary>
/// SatelliteConstellationCategory 枚举
/// </summary>
public enum SatelliteConstellationCategory
{
    ConstellationCategoryUnknown = 0,
    ConstellationCategoryGps = 1,
    ConstellationCategorySbas = 2,
    ConstellationCategoryGlonass = 3,
    ConstellationCategoryQzss = 4,
    ConstellationCategoryBeidou = 5,
    ConstellationCategoryGalileo = 6,
    ConstellationCategoryIrnss = 7
}

/// <summary>
/// SatelliteAdditionalInfo 枚举
/// </summary>
public enum SatelliteAdditionalInfo
{
    SatellitesAdditionalInfoNull = 0,
    SatellitesAdditionalInfoEphemerisDataExist = 1,
    SatellitesAdditionalInfoAlmanacDataExist = 2,
    SatellitesAdditionalInfoUsedInFix = 4,
    SatellitesAdditionalInfoCarrierFrequencyExist = 8
}

/// <summary>
/// UserActivityScenario 枚举
/// </summary>
public enum UserActivityScenario
{
    Navigation = 1025,
    Sport = 1026,
    Transport = 1027,
    DailyLifeService = 1028
}

/// <summary>
/// LocatingPriority 枚举
/// </summary>
public enum LocatingPriority
{
    PriorityAccuracy = 1281,
    PriorityLocatingSpeed = 1282
}

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
/// PowerConsumptionScenario 枚举
/// </summary>
public enum PowerConsumptionScenario
{
    HighPowerConsumption = 1537,
    LowPowerConsumption = 1538,
    NoPowerConsumption = 1539
}

/// <summary>
/// SportsType 枚举
/// </summary>
public enum SportsType
{
    Running = 1,
    Walking = 2,
    Cycling = 3,
    Skiing = 4
}

/// <summary>
/// CountryCodeType 枚举
/// </summary>
public enum CountryCodeType
{
    CountryCodeFromLocale = 1,
    CountryCodeFromSim = 2,
    CountryCodeFromLocation = 3,
    CountryCodeFromNetwork = 4
}

/// <summary>
/// BeaconFenceInfoType 枚举
/// </summary>
public enum BeaconFenceInfoType
{
    BeaconManufactureData = 1
}