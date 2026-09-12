using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CertificateDialogErrorCode 枚举
/// </summary>
public enum CertificateDialogErrorCode
{
    ErrorGeneric = 29700001,
    ErrorOperationCanceled = 29700002,
    ErrorOperationFailed = 29700003,
    ErrorDeviceNotSupported = 29700004,
    ErrorNotComplySecurityPolicy = 29700005,
    ErrorParameterValidationFailed = 29700006,
    ErrorNoAvailableCertificate = 29700007
}

/// <summary>
/// CertificateDialogPageType 枚举
/// </summary>
public enum CertificateDialogPageType
{
    PageMain = 1,
    PageCaCertificate = 2,
    PageCredential = 3,
    PageInstallCertificate = 4
}

/// <summary>
/// CertificateType 枚举
/// </summary>
public enum CertificateType
{
    CaCert = 1,
    CredentialUser = 2,
    CredentialApp = 3,
    CredentialUkey = 4,
    CredentialSystem = 5
}

/// <summary>
/// CertificateScope 枚举
/// </summary>
public enum CertificateScope
{
    NotSpecified = 0,
    CurrentUser = 1,
    GlobalUser = 2
}