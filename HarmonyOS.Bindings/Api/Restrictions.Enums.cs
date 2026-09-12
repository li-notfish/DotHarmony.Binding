using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// FeatureForDevice 枚举
/// </summary>
public enum FeatureForDevice
{
    WifiP2P = 0,
    LocalInput = 2,
    TrafficRedirection = 5,
    CoreDump = 6,
    Rs232 = 7,
    DiskErasure = 8,
    Bluetooth = 9,
    ModifyDateTime = 10,
    Printer = 11,
    Hdc = 12,
    Microphone = 13,
    Fingerprint = 14,
    Usb = 15,
    Wifi = 16,
    Tethering = 17,
    InactiveUserFreeze = 18,
    Camera = 19,
    MtpClient = 20,
    MtpServer = 21,
    SambaClient = 22,
    SambaServer = 23,
    BackupAndRestore = 24,
    MaintenanceMode = 25,
    Mms = 26,
    Sms = 27,
    MobileData = 28,
    AirplaneMode = 29,
    Vpn = 30,
    Notification = 31,
    Nfc = 32,
    PrivateSpace = 33,
    TelephoneCall = 34,
    AppClone = 35,
    ExternalStorageCard = 36,
    RandomMac = 37,
    UnmuteDevice = 38,
    HdcRemote = 39,
    VirtualService = 40,
    UsbSerial = 41,
    ScreenShot = 42,
    ScreenRecord = 43,
    DiskRecoveryKey = 44,
    NearLink = 45,
    DeveloperMode = 46,
    ResetFactory = 47,
    RemoteDesk = 48,
    RemoteDiagnosis = 49,
    OtaUpdate = 50
}

/// <summary>
/// FeatureForAccount 枚举
/// </summary>
public enum FeatureForAccount
{
    MultiWindow = 0,
    DistributedTransmission = 1,
    SuperHub = 2,
    Fingerprint = 3,
    Print = 4,
    MtpClient = 5,
    UsbStorageDeviceWrite = 6,
    DiskRecoveryKey = 7,
    Sudo = 8,
    DistributedTransmissionOutgoing = 9,
    OpenFileBoost = 10
}

/// <summary>
/// SettingsForDevice 枚举
/// </summary>
public enum SettingsForDevice
{
    SetApn = 0,
    PowerLongPress = 1,
    SetEthernetIP = 2,
    SetDeviceName = 3,
    SetBiometricsAndScreenLock = 4
}

/// <summary>
/// SettingsForAccount 枚举
/// </summary>
public enum SettingsForAccount
{
    ModifyWallpaper = 0
}