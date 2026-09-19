using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DrmErrorCode 枚举
/// </summary>
public enum DrmErrorCode
{
    ErrorUnknown = 24700101,
    MaxSystemNumReached = 24700103,
    MaxSessionNumReached = 24700104,
    ServiceFatalError = 24700201
}

/// <summary>
/// PreDefinedConfigName 枚举
/// </summary>
public enum PreDefinedConfigName
{
    [Description("vendor")]
    ConfigDeviceVendor,
    [Description("version")]
    ConfigDeviceVersion,
    [Description("description")]
    ConfigDeviceDescription,
    [Description("algorithms")]
    ConfigDeviceAlgorithms,
    [Description("deviceUniqueId")]
    ConfigDeviceUniqueId,
    [Description("maxSessionNum")]
    ConfigSessionMax,
    [Description("currentSessionNum")]
    ConfigSessionCurrent
}

/// <summary>
/// MediaKeyType 枚举
/// </summary>
public enum MediaKeyType
{
    MediaKeyTypeOffline = 0,
    MediaKeyTypeOnline = 1
}

/// <summary>
/// OfflineMediaKeyStatus 枚举
/// </summary>
public enum OfflineMediaKeyStatus
{
    OfflineMediaKeyStatusUnknown = 0,
    OfflineMediaKeyStatusUsable = 1,
    OfflineMediaKeyStatusInactive = 2
}

/// <summary>
/// CertificateStatus 枚举
/// </summary>
public enum CertificateStatus
{
    CertStatusProvisioned = 0,
    CertStatusNotProvisioned = 1,
    CertStatusExpired = 2,
    CertStatusInvalid = 3,
    CertStatusUnavailable = 4
}

/// <summary>
/// MediaKeyRequestType 枚举
/// </summary>
public enum MediaKeyRequestType
{
    MediaKeyRequestTypeUnknown = 0,
    MediaKeyRequestTypeInitial = 1,
    MediaKeyRequestTypeRenewal = 2,
    MediaKeyRequestTypeRelease = 3,
    MediaKeyRequestTypeNone = 4,
    MediaKeyRequestTypeUpdate = 5
}

/// <summary>
/// ContentProtectionLevel 枚举
/// </summary>
public enum ContentProtectionLevel
{
    ContentProtectionLevelUnknown = 0,
    ContentProtectionLevelSwCrypto = 1,
    ContentProtectionLevelHwCrypto = 2,
    ContentProtectionLevelEnhancedHw = 3,
    ContentProtectionLevelMax = 4
}