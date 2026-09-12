using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CertResult 枚举
/// </summary>
public enum CertResult
{
    InvalidParams = 401,
    NotSupport = 801,
    ErrOutOfMemory = 19020001,
    ErrRuntimeError = 19020002,
    ErrParameterCheckFailed = 19020003,
    ErrCryptoOperation = 19030001,
    ErrCertSignatureFailure = 19030002,
    ErrCertNotYetValid = 19030003,
    ErrCertHasExpired = 19030004,
    ErrUnableToGetIssuerCertLocally = 19030005,
    ErrKeyusageNoCertsign = 19030006,
    ErrKeyusageNoDigitalSignature = 19030007,
    ErrMaybeWrongPassword = 19030008,
    ErrCertUntrusted = 19030009,
    ErrCertHasRevoked = 19030010,
    ErrUnknownCriticalExtension = 19030011,
    ErrCertHostnameMismatch = 19030012,
    ErrCertEmailAddressMismatch = 19030013,
    ErrCertKeyusageMismatch = 19030014,
    ErrCrlNotFound = 19030015,
    ErrCrlNotYetValid = 19030016,
    ErrCrlHasExpired = 19030017,
    ErrCrlSignatureFailure = 19030018,
    ErrCrlIssuerNotFound = 19030019,
    ErrOcspResponseNotFound = 19030020,
    ErrOcspResponseInvalid = 19030021,
    ErrOcspSignatureFailure = 19030022,
    ErrOcspCertStatusUnknown = 19030023,
    ErrNetworkTimeout = 19030024
}

/// <summary>
/// EncodingFormat 枚举
/// </summary>
public enum EncodingFormat
{
    FormatDer = 0,
    FormatPem = 1,
    FormatPkcs7 = 2
}

/// <summary>
/// CertItemType 枚举
/// </summary>
public enum CertItemType
{
    CertItemTypeTbs = 0,
    CertItemTypePublicKey = 1,
    CertItemTypeIssuerUniqueId = 2,
    CertItemTypeSubjectUniqueId = 3,
    CertItemTypeExtensions = 4
}

/// <summary>
/// ExtensionOidType 枚举
/// </summary>
public enum ExtensionOidType
{
    ExtensionOidTypeAll = 0,
    ExtensionOidTypeCritical = 1,
    ExtensionOidTypeUncritical = 2
}

/// <summary>
/// ExtensionEntryType 枚举
/// </summary>
public enum ExtensionEntryType
{
    ExtensionEntryTypeEntry = 0,
    ExtensionEntryTypeEntryCritical = 1,
    ExtensionEntryTypeEntryValue = 2
}

/// <summary>
/// EncodingType 枚举
/// </summary>
public enum EncodingType
{
    EncodingUtf8 = 0
}

/// <summary>
/// CertRevocationFlag 枚举
/// </summary>
public enum CertRevocationFlag
{
    CertRevocationPreferOcsp = 0,
    CertRevocationCrlCheck = 1,
    CertRevocationOcspCheck = 2,
    CertRevocationCheckAllCert = 3
}

/// <summary>
/// OcspDigest 枚举
/// </summary>
public enum OcspDigest
{
    Sha1 = 0,
    Sha224 = 1,
    Sha256 = 2,
    Sha384 = 3,
    Sha512 = 4
}

/// <summary>
/// GeneralNameType 枚举
/// </summary>
public enum GeneralNameType
{
    GeneralNameTypeOtherName = 0,
    GeneralNameTypeRfc822Name = 1,
    GeneralNameTypeDnsName = 2,
    GeneralNameTypeX400Address = 3,
    GeneralNameTypeDirectoryName = 4,
    GeneralNameTypeEdiPartyName = 5,
    GeneralNameTypeUniformResourceId = 6,
    GeneralNameTypeIPAddress = 7,
    GeneralNameTypeRegisteredId = 8
}

/// <summary>
/// EncodingBaseFormat 枚举
/// </summary>
public enum EncodingBaseFormat
{
    Pem = 0,
    Der = 1
}

/// <summary>
/// RevocationCheckOptions 枚举
/// </summary>
public enum RevocationCheckOptions
{
    RevocationCheckOptionPreferOcsp = 0,
    RevocationCheckOptionAccessNetwork = 1,
    RevocationCheckOptionFallbackNoPrefer = 2,
    RevocationCheckOptionFallbackLocal = 3,
    RevocationCheckOptionCheckIntermediateCaOnline = 4,
    RevocationCheckOptionLocalCrlOnlyCheckEndEntityCert = 5,
    RevocationCheckOptionIgnoreNetworkError = 6
}

/// <summary>
/// ValidationPolicyType 枚举
/// </summary>
public enum ValidationPolicyType
{
    ValidationPolicyTypeX509 = 0,
    ValidationPolicyTypeSsl = 1
}

/// <summary>
/// KeyUsageType 枚举
/// </summary>
public enum KeyUsageType
{
    KeyusageDigitalSignature = 0,
    KeyusageNonRepudiation = 1,
    KeyusageKeyEncipherment = 2,
    KeyusageDataEncipherment = 3,
    KeyusageKeyAgreement = 4,
    KeyusageKeyCertSign = 5,
    KeyusageCrlSign = 6,
    KeyusageEncipherOnly = 7,
    KeyusageDecipherOnly = 8
}

/// <summary>
/// CmsContentType 枚举
/// </summary>
public enum CmsContentType
{
    SignedData = 0,
    EnvelopedData = 1
}

/// <summary>
/// CmsContentDataFormat 枚举
/// </summary>
public enum CmsContentDataFormat
{
    Binary = 0,
    Text = 1
}

/// <summary>
/// CmsFormat 枚举
/// </summary>
public enum CmsFormat
{
    Pem = 0,
    Der = 1
}

/// <summary>
/// CmsRsaSignaturePadding 枚举
/// </summary>
public enum CmsRsaSignaturePadding
{
    Pkcs1Padding = 0,
    Pkcs1PssPadding = 1
}

/// <summary>
/// CmsKeyAgreeRecipientDigestAlgorithm 枚举
/// </summary>
public enum CmsKeyAgreeRecipientDigestAlgorithm
{
    Sha256 = 0,
    Sha384 = 1,
    Sha512 = 2
}

/// <summary>
/// CmsRecipientEncryptionAlgorithm 枚举
/// </summary>
public enum CmsRecipientEncryptionAlgorithm
{
    Aes128Cbc = 0,
    Aes192Cbc = 1,
    Aes256Cbc = 2,
    Aes128Gcm = 3,
    Aes192Gcm = 4,
    Aes256Gcm = 5
}

/// <summary>
/// CmsCertType 枚举
/// </summary>
public enum CmsCertType
{
    SignerCerts = 0,
    AllCerts = 1
}

/// <summary>
/// PbesEncryptionAlgorithm 枚举
/// </summary>
public enum PbesEncryptionAlgorithm
{
    Aes128Cbc = 0,
    Aes192Cbc = 1,
    Aes256Cbc = 2
}

/// <summary>
/// Pkcs12MacDigestAlgorithm 枚举
/// </summary>
public enum Pkcs12MacDigestAlgorithm
{
    Sha256 = 0,
    Sha384 = 1,
    Sha512 = 2
}