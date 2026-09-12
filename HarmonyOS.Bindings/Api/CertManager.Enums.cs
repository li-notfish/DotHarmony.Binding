using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CMErrorCode 枚举
/// </summary>
public enum CMErrorCode
{
    CmErrorNoPermission = 201,
    CmErrorInvalidParams = 401,
    CmErrorGeneric = 17500001,
    CmErrorNoFound = 17500002,
    CmErrorIncorrectFormat = 17500003,
    CmErrorMaxCertCountReached = 17500004,
    CmErrorNoAuthorization = 17500005,
    CmErrorDeviceEnterAdvsecmode = 17500007,
    CmErrorStorePathNotSupported = 17500009,
    CmErrorAccessUkeyServiceFailed = 17500010,
    CmErrorParameterValidationFailed = 17500011
}

/// <summary>
/// CmKeyPurpose 枚举
/// </summary>
public enum CmKeyPurpose
{
    CmKeyPurposeSign = 4,
    CmKeyPurposeVerify = 8
}

/// <summary>
/// CmKeyDigest 枚举
/// </summary>
public enum CmKeyDigest
{
    CmDigestNone = 0,
    CmDigestMd5 = 1,
    CmDigestSha1 = 2,
    CmDigestSha224 = 3,
    CmDigestSha256 = 4,
    CmDigestSha384 = 5,
    CmDigestSha512 = 6,
    CmDigestSm3 = 7
}

/// <summary>
/// CmKeyPadding 枚举
/// </summary>
public enum CmKeyPadding
{
    CmPaddingNone = 0,
    CmPaddingPss = 1,
    CmPaddingPkcs1V15 = 2
}

/// <summary>
/// AuthStorageLevel 枚举
/// </summary>
public enum AuthStorageLevel
{
    El1 = 1,
    El2 = 2,
    El4 = 4
}

/// <summary>
/// CertManagerCertType 枚举
/// </summary>
public enum CertManagerCertType
{
    CaCertSystem = 0,
    CaCertUser = 1
}

/// <summary>
/// CertScope 枚举
/// </summary>
public enum CertScope
{
    CurrentUser = 1,
    GlobalUser = 2
}

/// <summary>
/// CertAlgorithm 枚举
/// </summary>
public enum CertAlgorithm
{
    International = 1,
    Sm = 2
}

/// <summary>
/// CertificatePurpose 枚举
/// </summary>
public enum CertificatePurpose
{
    PurposeDefault = 0,
    PurposeAll = 1,
    PurposeSign = 2,
    PurposeEncrypt = 3
}

/// <summary>
/// CertFileFormat 枚举
/// </summary>
public enum CertFileFormat
{
    PemDer = 0,
    P7B = 1
}