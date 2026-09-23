using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// BundleFlag 枚举
/// </summary>
public enum BundleFlag
{
    GetBundleInfoDefault = 0,
    GetBundleInfoWithApplication = 1,
    GetBundleInfoWithHapModule = 2,
    GetBundleInfoWithAbility = 4,
    GetBundleInfoWithExtensionAbility = 8,
    GetBundleInfoWithRequestedPermission = 16,
    GetBundleInfoWithMetadata = 32,
    GetBundleInfoWithDisable = 64,
    GetBundleInfoWithSignatureInfo = 128,
    GetBundleInfoWithMenu = 256,
    GetBundleInfoWithRouterMap = 512,
    GetBundleInfoWithSkill = 2048,
    GetBundleInfoWithEntryModule = 65536
}

/// <summary>
/// AbilityFlag 枚举
/// </summary>
public enum AbilityFlag
{
    GetAbilityInfoDefault = 0,
    GetAbilityInfoWithPermission = 1,
    GetAbilityInfoWithApplication = 2,
    GetAbilityInfoWithMetadata = 4,
    GetAbilityInfoWithDisable = 8,
    GetAbilityInfoOnlySystemApp = 16,
    GetAbilityInfoWithAppLinking = 64,
    GetAbilityInfoWithSkill = 128
}

/// <summary>
/// ExtensionAbilityType 枚举
/// </summary>
public enum ExtensionAbilityType
{
    Form = 0,
    WorkScheduler = 1,
    InputMethod = 2,
    Service = 3,
    Accessibility = 4,
    DataShare = 5,
    FileShare = 6,
    StaticSubscriber = 7,
    Wallpaper = 8,
    Backup = 9,
    Window = 10,
    EnterpriseAdmin = 11,
    Thumbnail = 13,
    Preview = 14,
    Print = 15,
    Share = 16,
    Push = 17,
    Driver = 18,
    Action = 19,
    AdsService = 20,
    EmbeddedUi = 21,
    InsightIntentUi = 22,
    Fence = 24,
    CallerInfoQuery = 25,
    AssetAcceleration = 26,
    FormEdit = 27,
    Distributed = 28,
    AppService = 29,
    LiveForm = 30,
    Selection = 31,
    WebNativeMessaging = 32,
    FaultLog = 33,
    NotificationSubscriber = 34,
    Crypto = 35,
    PartnerAgent = 36,
    Agent = 37,
    AgentUi = 38,
    ModularObject = 39,
    Unspecified = 255
}

/// <summary>
/// PermissionGrantState 枚举
/// </summary>
public enum PermissionGrantState
{
    [Description("-1")]
    PermissionDenied,
    PermissionGranted = 0
}

/// <summary>
/// SupportWindowMode 枚举
/// </summary>
public enum SupportWindowMode
{
    FullScreen = 0,
    Split = 1,
    Floating = 2
}

/// <summary>
/// LaunchType 枚举
/// </summary>
public enum LaunchType
{
    Singleton = 0,
    Multiton = 1,
    Specified = 2
}

/// <summary>
/// AbilityType 枚举
/// </summary>
public enum AbilityType
{
    Page = 1,
    Service = 2,
    Data = 3
}

/// <summary>
/// DisplayOrientation 枚举
/// </summary>
public enum DisplayOrientation
{
    Unspecified = 0,
    Landscape = 1,
    Portrait = 2,
    FollowRecent = 3,
    LandscapeInverted = 4,
    PortraitInverted = 5,
    AutoRotation = 6,
    AutoRotationLandscape = 7,
    AutoRotationPortrait = 8,
    AutoRotationRestricted = 9,
    AutoRotationLandscapeRestricted = 10,
    AutoRotationPortraitRestricted = 11,
    Locked = 12,
    AutoRotationUnspecified = 13,
    FollowDesktop = 14
}

/// <summary>
/// ModuleType 枚举
/// </summary>
public enum ModuleType
{
    Entry = 1,
    Feature = 2,
    Shared = 3
}

/// <summary>
/// BundleType 枚举
/// </summary>
public enum BundleType
{
    App = 0,
    AtomicService = 1
}

/// <summary>
/// CompatiblePolicy 枚举
/// </summary>
public enum CompatiblePolicy
{
    BackwardCompatibility = 1
}

/// <summary>
/// MultiAppModeType 枚举
/// </summary>
public enum MultiAppModeType
{
    Unspecified = 0,
    MultiInstance = 1,
    AppClone = 2
}