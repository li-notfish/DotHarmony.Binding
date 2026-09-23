using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// AppDistributionType 枚举
/// </summary>
public enum AppDistributionType
{
    AppGallery = 1,
    Enterprise = 2,
    EnterpriseNormal = 3,
    EnterpriseMdm = 4,
    Internaltesting = 5,
    Crowdtesting = 6
}

[Flags]
/// <summary>
/// BundleInfoGetFlag 枚举
/// </summary>
public enum BundleInfoGetFlag
{
    Default = 0,
    WithApplicationInfo = 1,
    WithSignatureInfo = 2,
    WithApplicationIconInfo = 4
}