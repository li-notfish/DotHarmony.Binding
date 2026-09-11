using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// NetworkType 枚举
/// </summary>
public enum NetworkType
{
    NetworkTypeAny = 0,
    NetworkTypeMobile = 1,
    NetworkTypeWifi = 2,
    NetworkTypeBluetooth = 3,
    NetworkTypeWifiP2P = 4,
    NetworkTypeEthernet = 5
}

/// <summary>
/// ChargingType 枚举
/// </summary>
public enum ChargingType
{
    ChargingPluggedAny = 0,
    ChargingPluggedAc = 1,
    ChargingPluggedUsb = 2,
    ChargingPluggedWireless = 3
}

/// <summary>
/// BatteryStatus 枚举
/// </summary>
public enum BatteryStatus
{
    BatteryStatusLow = 0,
    BatteryStatusOkay = 1,
    BatteryStatusLowOrOkay = 2
}

/// <summary>
/// StorageRequest 枚举
/// </summary>
public enum StorageRequest
{
    StorageLevelLow = 0,
    StorageLevelOkay = 1,
    StorageLevelLowOrOkay = 2
}