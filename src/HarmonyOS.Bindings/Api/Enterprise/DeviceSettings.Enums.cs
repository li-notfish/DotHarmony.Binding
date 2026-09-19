using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SettingsItem 枚举
/// </summary>
public enum SettingsItem
{
    DeviceName = 0,
    FloatingNavigation = 1
}

/// <summary>
/// SettingsMenu 枚举
/// </summary>
public enum SettingsMenu
{
    AccountId = 0,
    Wifi = 1,
    WifiProxySettings = 2,
    WifiIPSettings = 3,
    Bluetooth = 4,
    Network = 5,
    MobileNetwork = 6,
    SuperDevice = 7,
    MoreConnectivityOptions = 8,
    HomeScreenStyle = 9,
    DisplayBrightness = 10,
    SoundVibration = 11,
    Notifications = 12,
    BiometricsPassword = 13,
    AppsAndServices = 14,
    Battery = 15,
    Storage = 16,
    PrivacyAndSecurity = 17,
    DigitalBalance = 18,
    SmartAssistant = 19,
    Accessibility = 20,
    System = 21,
    AboutDevice = 22,
    SystemNavigation = 23,
    LanguageRegion = 24,
    InputMethods = 25,
    DateTime = 26,
    DataClone = 27,
    BackupSettings = 28,
    Reset = 29,
    Superhub = 30,
    UserExperience = 31,
    ScreenCast = 32,
    PrintersScanners = 33,
    MobileData = 34,
    PersonalHotspot = 35,
    SimManagement = 36,
    AirplaneMode = 37,
    ManageDataUsage = 38,
    VpnSettings = 39,
    TextDisplaySize = 40,
    AppDuplicator = 41,
    Search = 42
}

/// <summary>
/// SwitchKey 枚举
/// </summary>
public enum SwitchKey
{
    Nearlink = 0,
    Bluetooth = 1,
    Wifi = 2,
    Nfc = 3
}

/// <summary>
/// SwitchStatus 枚举
/// </summary>
public enum SwitchStatus
{
    On = 0,
    Off = 1,
    ForceOn = 2
}