using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// BundleBundleFlag 枚举
/// </summary>
public enum BundleBundleFlag : long
{
    GetBundleDefault = 0,
    GetBundleWithAbilities = 1,
    GetAbilityInfoWithPermission = 2,
    GetAbilityInfoWithApplication = 4,
    GetApplicationInfoWithPermission = 8,
    GetBundleWithRequestedPermission = 16,
    GetAllApplicationInfo = 4294901760,
    GetAbilityInfoWithMetadata = 32,
    GetApplicationInfoWithMetadata = 64,
    GetAbilityInfoSystemappOnly = 128,
    GetAbilityInfoWithDisable = 256,
    GetApplicationInfoWithDisable = 512
}

/// <summary>
/// BundleColorMode 枚举
/// </summary>
public enum BundleColorMode
{
    [Description("-1")]
    AutoMode,
    DarkMode = 0,
    LightMode = 1
}

/// <summary>
/// BundleGrantStatus 枚举
/// </summary>
public enum BundleGrantStatus
{
    [Description("-1")]
    PermissionDenied,
    PermissionGranted = 0
}

/// <summary>
/// BundleAbilityType 枚举
/// </summary>
public enum BundleAbilityType
{
    Unknown = 0,
    Page = 1,
    Service = 2,
    Data = 3
}

/// <summary>
/// AbilitySubType 枚举
/// </summary>
public enum AbilitySubType
{
    Unspecified = 0,
    Ca = 1
}

/// <summary>
/// BundleDisplayOrientation 枚举
/// </summary>
public enum BundleDisplayOrientation
{
    Unspecified = 0,
    Landscape = 1,
    Portrait = 2,
    FollowRecent = 3
}

/// <summary>
/// LaunchMode 枚举
/// </summary>
public enum LaunchMode
{
    Singleton = 0,
    Standard = 1
}

/// <summary>
/// InstallErrorCode 枚举
/// </summary>
public enum InstallErrorCode
{
    Success = 0,
    StatusInstallFailure = 1,
    StatusInstallFailureAborted = 2,
    StatusInstallFailureInvalid = 3,
    StatusInstallFailureConflict = 4,
    StatusInstallFailureStorage = 5,
    StatusInstallFailureIncompatible = 6,
    StatusUninstallFailure = 7,
    StatusUninstallFailureBlocked = 8,
    StatusUninstallFailureAborted = 9,
    StatusUninstallFailureConflict = 10,
    StatusInstallFailureDownloadTimeout = 11,
    StatusInstallFailureDownloadFailed = 12,
    StatusRecoverFailureInvalid = 13,
    StatusAbilityNotFound = 64,
    StatusBmsServiceError = 65,
    StatusFailedNoSpaceLeft = 66,
    StatusGrantRequestPermissionsFailed = 67,
    StatusInstallPermissionDenied = 68,
    StatusUninstallPermissionDenied = 69
}