using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ProfileId 枚举
/// </summary>
public enum ProfileId
{
    ProfileA2DpSource = 1,
    ProfileHandsfreeAudioGateway = 4,
    ProfileHidHost = 6,
    ProfilePanNetwork = 7
}

/// <summary>
/// ProfileUuids 枚举
/// </summary>
public enum ProfileUuids
{
    [Description("0000111F-0000-1000-8000-00805F9B34FB")]
    ProfileUuidHfpAg,
    [Description("0000111E-0000-1000-8000-00805F9B34FB")]
    ProfileUuidHfpHf,
    [Description("00001112-0000-1000-8000-00805F9B34FB")]
    ProfileUuidHspAg,
    [Description("00001108-0000-1000-8000-00805F9B34FB")]
    ProfileUuidHspHs,
    [Description("0000110A-0000-1000-8000-00805F9B34FB")]
    ProfileUuidA2DpSrc,
    [Description("0000110B-0000-1000-8000-00805F9B34FB")]
    ProfileUuidA2DpSink,
    [Description("0000110E-0000-1000-8000-00805F9B34FB")]
    ProfileUuidAvrcpCt,
    [Description("0000110C-0000-1000-8000-00805F9B34FB")]
    ProfileUuidAvrcpTg,
    [Description("00001124-0000-1000-8000-00805F9B34FB")]
    ProfileUuidHid,
    [Description("00001812-0000-1000-8000-00805F9B34FB")]
    ProfileUuidHogp
}

/// <summary>
/// ProfileConnectionState 枚举
/// </summary>
public enum ProfileConnectionState
{
    StateDisconnected = 0,
    StateConnecting = 1,
    StateConnected = 2,
    StateDisconnecting = 3
}

/// <summary>
/// MajorClass 枚举
/// </summary>
public enum MajorClass
{
    MajorMisc = 0,
    MajorComputer = 256,
    MajorPhone = 512,
    MajorNetworking = 768,
    MajorAudioVideo = 1024,
    MajorPeripheral = 1280,
    MajorImaging = 1536,
    MajorWearable = 1792,
    MajorToy = 2048,
    MajorHealth = 2304,
    MajorUncategorized = 7936
}

/// <summary>
/// MajorMinorClass 枚举
/// </summary>
public enum MajorMinorClass
{
    ComputerUncategorized = 256,
    ComputerDesktop = 260,
    ComputerServer = 264,
    ComputerLaptop = 268,
    ComputerHandheldPcPda = 272,
    ComputerPalmSizePcPda = 276,
    ComputerWearable = 280,
    ComputerTablet = 284,
    PhoneUncategorized = 512,
    PhoneCellular = 516,
    PhoneCordless = 520,
    PhoneSmart = 524,
    PhoneModemOrGateway = 528,
    PhoneIsdn = 532,
    NetworkFullyAvailable = 768,
    Network1To17Utilized = 800,
    Network17To33Utilized = 832,
    Network33To50Utilized = 864,
    Network60To67Utilized = 896,
    Network67To83Utilized = 928,
    Network83To99Utilized = 960,
    NetworkNoService = 992,
    AudioVideoUncategorized = 1024,
    AudioVideoWearableHeadset = 1028,
    AudioVideoHandsfree = 1032,
    AudioVideoMicrophone = 1040,
    AudioVideoLoudspeaker = 1044,
    AudioVideoHeadphones = 1048,
    AudioVideoPortableAudio = 1052,
    AudioVideoCarAudio = 1056,
    AudioVideoSetTopBox = 1060,
    AudioVideoHifiAudio = 1064,
    AudioVideoVcr = 1068,
    AudioVideoVideoCamera = 1072,
    AudioVideoCamcorder = 1076,
    AudioVideoVideoMonitor = 1080,
    AudioVideoVideoDisplayAndLoudspeaker = 1084,
    AudioVideoVideoConferencing = 1088,
    AudioVideoVideoGamingToy = 1096,
    PeripheralNonKeyboardNonPointing = 1280,
    PeripheralKeyboard = 1344,
    PeripheralPointingDevice = 1408,
    PeripheralKeyboardPointing = 1472,
    PeripheralUncategorized = 1280,
    PeripheralJoystick = 1284,
    PeripheralGamepad = 1288,
    PeripheralRemoteControl = 1472,
    PeripheralSensingDevice = 1296,
    PeripheralDigitizerTablet = 1300,
    PeripheralCardReader = 1304,
    PeripheralDigitalPen = 1308,
    PeripheralScannerRfid = 1312,
    PeripheralGesturalInput = 1314,
    ImagingUncategorized = 1536,
    ImagingDisplay = 1552,
    ImagingCamera = 1568,
    ImagingScanner = 1600,
    ImagingPrinter = 1664,
    WearableUncategorized = 1792,
    WearableWristWatch = 1796,
    WearablePager = 1800,
    WearableJacket = 1804,
    WearableHelmet = 1808,
    WearableGlasses = 1812,
    ToyUncategorized = 2048,
    ToyRobot = 2052,
    ToyVehicle = 2056,
    ToyDollActionFigure = 2060,
    ToyController = 2064,
    ToyGame = 2068,
    HealthUncategorized = 2304,
    HealthBloodPressure = 2308,
    HealthThermometer = 2312,
    HealthWeighing = 2316,
    HealthGlucose = 2320,
    HealthPulseOximeter = 2324,
    HealthPulseRate = 2328,
    HealthDataDisplay = 2332,
    HealthStepCounter = 2336,
    HealthBodyCompositionAnalyzer = 2340,
    HealthPeakFlowMonitor = 2344,
    HealthMedicationMonitor = 2348,
    HealthKneeProsthesis = 2352,
    HealthAnkleProsthesis = 2356,
    HealthGenericHealthManager = 2360,
    HealthPersonalMobilityDevice = 2364
}