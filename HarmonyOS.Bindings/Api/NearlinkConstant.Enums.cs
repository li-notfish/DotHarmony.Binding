using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PairingState 枚举
/// </summary>
public enum PairingState
{
    PairingStateNone = 1,
    PairingStatePairing = 2,
    PairingStatePaired = 3
}

/// <summary>
/// NearlinkConstantConnectionState 枚举
/// </summary>
public enum NearlinkConstantConnectionState
{
    StateConnecting = 0,
    StateConnected = 1,
    StateDisconnecting = 2,
    StateDisconnected = 3
}

/// <summary>
/// DeviceClass 枚举
/// </summary>
public enum DeviceClass
{
    [Description("-1")]
    DeviceInvalidClass,
    DeviceUncategorized = 256,
    DevicePhone = 512,
    DeviceSmartphone = 513,
    DeviceComputer = 768,
    DeviceLaptop = 769,
    DeviceTablet = 770,
    DeviceAllInOneComputer = 771,
    DeviceMiniPc = 772,
    DeviceWatch = 1024,
    DeviceSmartWatch = 1025,
    DeviceHumanInterface = 1280,
    DeviceKeyboard = 1281,
    DeviceMouse = 1282,
    DeviceHandle = 1283,
    DeviceStylus = 1284,
    DeviceTouchpad = 1285,
    DeviceAudioPlayback = 1536,
    DeviceSmartSpeaker = 1537,
    DeviceEchoWall = 1538,
    DeviceAudioCapture = 1792,
    DeviceKaraokeMicrophone = 1793,
    DeviceLapelMicrophone = 1794,
    DeviceWearableAudio = 2048,
    DeviceInEarEarphone = 2049,
    DeviceHeadset = 2050,
    DeviceOverEarHeadphone = 2051,
    DeviceNeckbandEarphone = 2052,
    DevicePersonalCare = 2304,
    DeviceIntelligentToothbrush = 2305,
    DeviceSmartCup = 2306,
    DeviceIntelligentShaver = 2307,
    DeviceHvac = 2560,
    DeviceAirPurifier = 2561,
    DeviceHumidifier = 2562,
    DeviceAirCirculationFan = 2563,
    DeviceElectricRide = 2816,
    DeviceElectricScooter = 2817,
    DeviceElectricBicycle = 2818,
    DeviceLightFitting = 3072,
    DeviceSmartTableLamp = 3073,
    DeviceRemoteControl = 3328,
    DeviceTVRemoteControl = 3329,
    DeviceImaging = 3584,
    DeviceSmartTV = 3585,
    DeviceIPCamera = 3586,
    DeviceScreenCaster = 3587,
    DeviceNetworking = 3840,
    DeviceIotGateway = 3841,
    DeviceAccessControl = 4096,
    DeviceIntelligentLock = 4097,
    DeviceSmartKey = 4098,
    DeviceVehicleKey = 4099,
    DeviceVehicleLock = 4100
}

/// <summary>
/// AcbState 枚举
/// </summary>
public enum AcbState
{
    Disconnected = 0,
    Connected = 1,
    Encrypted = 2
}