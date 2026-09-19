using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// HuksErrorCode 枚举
/// </summary>
public enum HuksErrorCode
{
    HuksSuccess = 0,
    [Description("-1")]
    HuksFailure,
    [Description("-2")]
    HuksErrorBadState,
    [Description("-3")]
    HuksErrorInvalidArgument,
    [Description("-4")]
    HuksErrorNotSupported,
    [Description("-5")]
    HuksErrorNoPermission,
    [Description("-6")]
    HuksErrorInsufficientData,
    [Description("-7")]
    HuksErrorBufferTooSmall,
    [Description("-8")]
    HuksErrorInsufficientMemory,
    [Description("-9")]
    HuksErrorCommunicationFailure,
    [Description("-10")]
    HuksErrorStorageFailure,
    [Description("-11")]
    HuksErrorHardwareFailure,
    [Description("-12")]
    HuksErrorAlreadyExists,
    [Description("-13")]
    HuksErrorNotExist,
    [Description("-14")]
    HuksErrorNullPointer,
    [Description("-15")]
    HuksErrorFileSizeFail,
    [Description("-16")]
    HuksErrorReadFileFail,
    [Description("-17")]
    HuksErrorInvalidPublicKey,
    [Description("-18")]
    HuksErrorInvalidPrivateKey,
    [Description("-19")]
    HuksErrorInvalidKeyInfo,
    [Description("-20")]
    HuksErrorHashNotEqual,
    [Description("-21")]
    HuksErrorMallocFail,
    [Description("-22")]
    HuksErrorWriteFileFail,
    [Description("-23")]
    HuksErrorRemoveFileFail,
    [Description("-24")]
    HuksErrorOpenFileFail,
    [Description("-25")]
    HuksErrorCloseFileFail,
    [Description("-26")]
    HuksErrorMakeDirFail,
    [Description("-27")]
    HuksErrorInvalidKeyFile,
    [Description("-28")]
    HuksErrorIpcMsgFail,
    [Description("-29")]
    HuksErrorRequestOverflows,
    [Description("-30")]
    HuksErrorParamNotExist,
    [Description("-31")]
    HuksErrorCryptoEngineError,
    [Description("-32")]
    HuksErrorCommunicationTimeout,
    [Description("-33")]
    HuksErrorIpcInitFail,
    [Description("-34")]
    HuksErrorIpcDlopenFail,
    [Description("-35")]
    HuksErrorEfuseReadFail,
    [Description("-36")]
    HuksErrorNewRootKeyMaterialExist,
    [Description("-37")]
    HuksErrorUpdateRootKeyMaterialFail,
    [Description("-38")]
    HuksErrorVerificationFailed,
    [Description("-100")]
    HuksErrorCheckGetAlgFail,
    [Description("-101")]
    HuksErrorCheckGetKeySizeFail,
    [Description("-102")]
    HuksErrorCheckGetPaddingFail,
    [Description("-103")]
    HuksErrorCheckGetPurposeFail,
    [Description("-104")]
    HuksErrorCheckGetDigestFail,
    [Description("-105")]
    HuksErrorCheckGetModeFail,
    [Description("-106")]
    HuksErrorCheckGetNonceFail,
    [Description("-107")]
    HuksErrorCheckGetAadFail,
    [Description("-108")]
    HuksErrorCheckGetIvFail,
    [Description("-109")]
    HuksErrorCheckGetAeTagFail,
    [Description("-110")]
    HuksErrorCheckGetSaltFail,
    [Description("-111")]
    HuksErrorCheckGetIterationFail,
    [Description("-112")]
    HuksErrorInvalidAlgorithm,
    [Description("-113")]
    HuksErrorInvalidKeySize,
    [Description("-114")]
    HuksErrorInvalidPadding,
    [Description("-115")]
    HuksErrorInvalidPurpose,
    [Description("-116")]
    HuksErrorInvalidMode,
    [Description("-117")]
    HuksErrorInvalidDigest,
    [Description("-118")]
    HuksErrorInvalidSignatureSize,
    [Description("-119")]
    HuksErrorInvalidIv,
    [Description("-120")]
    HuksErrorInvalidAad,
    [Description("-121")]
    HuksErrorInvalidNonce,
    [Description("-122")]
    HuksErrorInvalidAeTag,
    [Description("-123")]
    HuksErrorInvalidSalt,
    [Description("-124")]
    HuksErrorInvalidIteration,
    [Description("-125")]
    HuksErrorInvalidOperation,
    [Description("-999")]
    HuksErrorInternalError,
    [Description("-1000")]
    HuksErrorUnknownError
}

/// <summary>
/// HuksExceptionErrCode 枚举
/// </summary>
public enum HuksExceptionErrCode
{
    HuksErrCodePermissionFail = 201,
    HuksErrCodeNotSystemApp = 202,
    HuksErrCodeIllegalArgument = 401,
    HuksErrCodeNotSupportedApi = 801,
    HuksErrCodeFeatureNotSupported = 12000001,
    HuksErrCodeMissingCryptoAlgArgument = 12000002,
    HuksErrCodeInvalidCryptoAlgArgument = 12000003,
    HuksErrCodeFileOperationFail = 12000004,
    HuksErrCodeCommunicationFail = 12000005,
    HuksErrCodeCryptoFail = 12000006,
    HuksErrCodeKeyAuthPermanentlyInvalidated = 12000007,
    HuksErrCodeKeyAuthVerifyFailed = 12000008,
    HuksErrCodeKeyAuthTimeOut = 12000009,
    HuksErrCodeSessionLimit = 12000010,
    HuksErrCodeItemNotExist = 12000011,
    HuksErrCodeExternalError = 12000012,
    HuksErrCodeCredentialNotExist = 12000013,
    HuksErrCodeInsufficientMemory = 12000014,
    HuksErrCodeCallServiceFailed = 12000015,
    HuksErrCodeDevicePasswordUnset = 12000016,
    HuksErrCodeKeyAlreadyExist = 12000017,
    HuksErrCodeInvalidArgument = 12000018,
    HuksErrCodeItemExists = 12000019,
    HuksErrCodeExternalModule = 12000020,
    HuksErrCodePinLocked = 12000021,
    HuksErrCodePinIncorrect = 12000022,
    HuksErrCodePinNoAuth = 12000023,
    HuksErrCodeBusy = 12000024,
    HuksErrCodeExceedLimit = 12000025,
    HuksErrCodeSeFault = 12000026,
    HuksErrCodeNetworkUnavailable = 12000027
}

/// <summary>
/// HuksKeyPurpose 枚举
/// </summary>
public enum HuksKeyPurpose
{
    HuksKeyPurposeEncrypt = 1,
    HuksKeyPurposeDecrypt = 2,
    HuksKeyPurposeSign = 4,
    HuksKeyPurposeVerify = 8,
    HuksKeyPurposeDerive = 16,
    HuksKeyPurposeWrap = 32,
    HuksKeyPurposeUnwrap = 64,
    HuksKeyPurposeMac = 128,
    HuksKeyPurposeAgree = 256
}

/// <summary>
/// HuksKeyDigest 枚举
/// </summary>
public enum HuksKeyDigest
{
    HuksDigestNone = 0,
    HuksDigestMd5 = 1,
    HuksDigestSm3 = 2,
    HuksDigestSha1 = 10,
    HuksDigestSha224 = 11,
    HuksDigestSha256 = 12,
    HuksDigestSha384 = 13,
    HuksDigestSha512 = 14
}

/// <summary>
/// HuksKeyPadding 枚举
/// </summary>
public enum HuksKeyPadding
{
    HuksPaddingNone = 0,
    HuksPaddingOaep = 1,
    HuksPaddingPss = 2,
    HuksPaddingPkcs1V15 = 3,
    HuksPaddingPkcs5 = 4,
    HuksPaddingPkcs7 = 5,
    HuksPaddingIsoIec97962 = 6,
    HuksPaddingIsoIec97971 = 7
}

/// <summary>
/// HuksCipherMode 枚举
/// </summary>
public enum HuksCipherMode
{
    HuksModeEcb = 1,
    HuksModeCbc = 2,
    HuksModeCtr = 3,
    HuksModeOfb = 4,
    HuksModeCfb = 5,
    HuksModeCcm = 31,
    HuksModeGcm = 32
}

/// <summary>
/// HuksKeySize 枚举
/// </summary>
public enum HuksKeySize
{
    HuksRsaKeySize512 = 512,
    HuksRsaKeySize768 = 768,
    HuksRsaKeySize1024 = 1024,
    HuksRsaKeySize2048 = 2048,
    HuksRsaKeySize3072 = 3072,
    HuksRsaKeySize4096 = 4096,
    HuksEccKeySize224 = 224,
    HuksEccKeySize256 = 256,
    HuksEccKeySize384 = 384,
    HuksEccKeySize521 = 521,
    HuksAesKeySize128 = 128,
    HuksAesKeySize192 = 192,
    HuksAesKeySize256 = 256,
    HuksAesKeySize512 = 512,
    HuksCurve25519KeySize256 = 256,
    HuksDhKeySize2048 = 2048,
    HuksDhKeySize3072 = 3072,
    HuksDhKeySize4096 = 4096,
    HuksSm2KeySize256 = 256,
    HuksSm4KeySize128 = 128,
    HuksDesKeySize64 = 64,
    Huks3DesKeySize128 = 128,
    Huks3DesKeySize192 = 192,
    HuksMlDsaKeyParamSet44 = 44,
    HuksMlDsaKeyParamSet65 = 65,
    HuksMlDsaKeyParamSet87 = 87,
    HuksMlKemKeyParamSet768 = 768,
    HuksMlKemKeyParamSet1024 = 1024
}

/// <summary>
/// HuksKeyAlg 枚举
/// </summary>
public enum HuksKeyAlg
{
    HuksAlgRsa = 1,
    HuksAlgEcc = 2,
    HuksAlgDsa = 3,
    HuksAlgAes = 20,
    HuksAlgHmac = 50,
    HuksAlgHkdf = 51,
    HuksAlgPbkdf2 = 52,
    HuksAlgEcdh = 100,
    HuksAlgX25519 = 101,
    HuksAlgEd25519 = 102,
    HuksAlgDh = 103,
    HuksAlgSm2 = 150,
    HuksAlgSm3 = 151,
    HuksAlgSm4 = 152,
    HuksAlgDes = 160,
    HuksAlg3Des = 161,
    HuksAlgCmac = 162,
    HuksAlgMlKem = 200,
    HuksAlgMlDsa = 201
}

/// <summary>
/// HuksUnwrapSuite 枚举
/// </summary>
public enum HuksUnwrapSuite
{
    HuksUnwrapSuiteX25519Aes256GcmNopadding = 1,
    HuksUnwrapSuiteEcdhAes256GcmNopadding = 2,
    HuksUnwrapSuiteSm2Sm4EcbNopadding = 5
}

/// <summary>
/// HuksKeyGenerateType 枚举
/// </summary>
public enum HuksKeyGenerateType
{
    HuksKeyGenerateTypeDefault = 0,
    HuksKeyGenerateTypeDerive = 1,
    HuksKeyGenerateTypeAgree = 2
}

/// <summary>
/// HuksKeyFlag 枚举
/// </summary>
public enum HuksKeyFlag
{
    HuksKeyFlagImportKey = 1,
    HuksKeyFlagGenerateKey = 2,
    HuksKeyFlagAgreeKey = 3,
    HuksKeyFlagDeriveKey = 4
}

/// <summary>
/// HuksKeyStorageType 枚举
/// </summary>
public enum HuksKeyStorageType
{
    HuksStorageTemp = 0,
    HuksStoragePersistent = 1,
    HuksStorageOnlyUsedInHuks = 2,
    HuksStorageKeyExportAllowed = 3
}

/// <summary>
/// HuksImportKeyType 枚举
/// </summary>
public enum HuksImportKeyType
{
    HuksKeyTypePublicKey = 0,
    HuksKeyTypePrivateKey = 1,
    HuksKeyTypeKeyPair = 2
}

/// <summary>
/// HuksRsaPssSaltLenType 枚举
/// </summary>
public enum HuksRsaPssSaltLenType
{
    HuksRsaPssSaltLenDigest = 0,
    HuksRsaPssSaltLenMax = 1
}

[Flags]
/// <summary>
/// HuksUserAuthType 枚举
/// </summary>
public enum HuksUserAuthType
{
    HuksUserAuthTypeFingerprint = 1,
    HuksUserAuthTypeFace = 2,
    HuksUserAuthTypePin = 4,
    HuksUserAuthTypeTuiPin = 32
}

[Flags]
/// <summary>
/// HuksAuthAccessType 枚举
/// </summary>
public enum HuksAuthAccessType
{
    HuksAuthAccessInvalidClearPassword = 1,
    HuksAuthAccessInvalidNewBioEnroll = 2,
    HuksAuthAccessAlwaysValid = 4
}

/// <summary>
/// HuksUserAuthMode 枚举
/// </summary>
public enum HuksUserAuthMode
{
    HuksUserAuthModeLocal = 0,
    HuksUserAuthModeCoauth = 1
}

/// <summary>
/// HuksAuthStorageLevel 枚举
/// </summary>
public enum HuksAuthStorageLevel
{
    HuksAuthStorageLevelDe = 0,
    HuksAuthStorageLevelCe = 1,
    HuksAuthStorageLevelEce = 2
}

/// <summary>
/// HuksChallengeType 枚举
/// </summary>
public enum HuksChallengeType
{
    HuksChallengeTypeNormal = 0,
    HuksChallengeTypeCustom = 1,
    HuksChallengeTypeNone = 2
}

/// <summary>
/// HuksChallengePosition 枚举
/// </summary>
public enum HuksChallengePosition
{
    HuksChallengePos0 = 0,
    HuksChallengePos1 = 1,
    HuksChallengePos2 = 2,
    HuksChallengePos3 = 3
}

/// <summary>
/// HuksSecureSignType 枚举
/// </summary>
public enum HuksSecureSignType
{
    HuksSecureSignWithAuthinfo = 1
}

/// <summary>
/// HuksSendType 枚举
/// </summary>
public enum HuksSendType
{
    HuksSendTypeAsync = 0,
    HuksSendTypeSync = 1
}

/// <summary>
/// HuksKeyClassType 枚举
/// </summary>
public enum HuksKeyClassType
{
    HuksKeyClassDefault = 0,
    HuksKeyClassExtension = 1
}

/// <summary>
/// HuksKeyWrapType 枚举
/// </summary>
public enum HuksKeyWrapType
{
    HuksKeyWrapTypeHukBased = 2
}

[Flags]
/// <summary>
/// HuksTagType 枚举
/// </summary>
public enum HuksTagType
{
    HuksTagTypeInvalid = 0,
    HuksTagTypeInt = 268435456,
    HuksTagTypeUint = 536870912,
    HuksTagTypeUlong = 805306368,
    HuksTagTypeBool = 1073741824,
    HuksTagTypeBytes = 1342177280
}

/// <summary>
/// HuksKeySecurityLevel 枚举
/// </summary>
public enum HuksKeySecurityLevel
{
    HuksKeySecurityLevelTee = 0,
    HuksKeySecurityLevelSe = 1
}

[Flags]
/// <summary>
/// HuksTag 枚举
/// </summary>
public enum HuksTag
{
    [Description("HuksTagType.HUKS_TAG_TYPE_INVALID | 0")]
    HuksTagInvalid,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 1")]
    HuksTagAlgorithm,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 2")]
    HuksTagPurpose,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 3")]
    HuksTagKeySize,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 4")]
    HuksTagDigest,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 5")]
    HuksTagPadding,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 6")]
    HuksTagBlockMode,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 7")]
    HuksTagKeyType,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 8")]
    HuksTagAssociatedData,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 9")]
    HuksTagNonce,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 10")]
    HuksTagIv,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 11")]
    HuksTagInfo,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 12")]
    HuksTagSalt,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 13")]
    HuksTagPwd,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 14")]
    HuksTagIteration,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 15")]
    HuksTagKeyGenerateType,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 16")]
    HuksTagDeriveMainKey,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 17")]
    HuksTagDeriveFactor,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 18")]
    HuksTagDeriveAlg,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 19")]
    HuksTagAgreeAlg,
    [Description("HuksTagType.HUKS_TAG_TYPE_BOOL | 20")]
    HuksTagAgreePublicKeyIsKeyAlias,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 21")]
    HuksTagAgreePrivateKeyAlias,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 22")]
    HuksTagAgreePublicKey,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 23")]
    HuksTagKeyAlias,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 24")]
    HuksTagDeriveKeySize,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 25")]
    HuksTagImportKeyType,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 26")]
    HuksTagUnwrapAlgorithmSuite,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 29")]
    HuksTagDerivedAgreedKeyStorageFlag,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 30")]
    HuksTagRsaPssSaltLenType,
    [Description("HuksTagType.HUKS_TAG_TYPE_ULONG | 201")]
    HuksTagActiveDatetime,
    [Description("HuksTagType.HUKS_TAG_TYPE_ULONG | 202")]
    HuksTagOriginationExpireDatetime,
    [Description("HuksTagType.HUKS_TAG_TYPE_ULONG | 203")]
    HuksTagUsageExpireDatetime,
    [Description("HuksTagType.HUKS_TAG_TYPE_ULONG | 204")]
    HuksTagCreationDatetime,
    [Description("HuksTagType.HUKS_TAG_TYPE_BOOL | 301")]
    HuksTagAllUsers,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 302")]
    HuksTagUserId,
    [Description("HuksTagType.HUKS_TAG_TYPE_BOOL | 303")]
    HuksTagNoAuthRequired,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 304")]
    HuksTagUserAuthType,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 305")]
    HuksTagAuthTimeout,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 306")]
    HuksTagAuthToken,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 307")]
    HuksTagKeyAuthAccessType,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 308")]
    HuksTagKeySecureSignType,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 309")]
    HuksTagChallengeType,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 310")]
    HuksTagChallengePos,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 311")]
    HuksTagKeyAuthPurpose,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 316")]
    HuksTagAuthStorageLevel,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 319")]
    HuksTagUserAuthMode,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 501")]
    HuksTagAttestationChallenge,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 502")]
    HuksTagAttestationApplicationId,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 503")]
    HuksTagAttestationIdBrand,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 504")]
    HuksTagAttestationIdDevice,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 505")]
    HuksTagAttestationIdProduct,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 506")]
    HuksTagAttestationIdSerial,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 507")]
    HuksTagAttestationIdImei,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 508")]
    HuksTagAttestationIdMeid,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 509")]
    HuksTagAttestationIdManufacturer,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 510")]
    HuksTagAttestationIdModel,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 511")]
    HuksTagAttestationIdAlias,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 512")]
    HuksTagAttestationIdSocid,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 513")]
    HuksTagAttestationIdUdid,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 514")]
    HuksTagAttestationIdSecLevelInfo,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 515")]
    HuksTagAttestationIdVersionInfo,
    [Description("HuksTagType.HUKS_TAG_TYPE_BOOL | 520")]
    HuksTagKeyOverride,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 521")]
    HuksTagAeTagLen,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 522")]
    HuksTagKeyClass,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 523")]
    HuksTagKeyAccessGroup,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 526")]
    HuksTagKeySecurityLevel,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 527")]
    HuksTagAad,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 528")]
    HuksTagContext,
    [Description("HuksTagType.HUKS_TAG_TYPE_BOOL | 1001")]
    HuksTagIsKeyAlias,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 1002")]
    HuksTagKeyStorageFlag,
    [Description("HuksTagType.HUKS_TAG_TYPE_BOOL | 1003")]
    HuksTagIsAllowedWrap,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 1004")]
    HuksTagKeyWrapType,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 1005")]
    HuksTagKeyAuthId,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 1006")]
    HuksTagKeyRole,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 1007")]
    HuksTagKeyFlag,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 1008")]
    HuksTagIsAsynchronized,
    [Description("HuksTagType.HUKS_TAG_TYPE_BOOL | 1009")]
    HuksTagSecureKeyAlias,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 1010")]
    HuksTagSecureKeyUuid,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 1011")]
    HuksTagKeyDomain,
    [Description("HuksTagType.HUKS_TAG_TYPE_BOOL | 1012")]
    HuksTagIsDevicePasswordSet,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 10001")]
    HuksTagProcessName,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 10002")]
    HuksTagPackageName,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 10003")]
    HuksTagAccessTime,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 10004")]
    HuksTagUsesTime,
    [Description("HuksTagType.HUKS_TAG_TYPE_ULONG | 10005")]
    HuksTagCryptoCtx,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 10006")]
    HuksTagKey,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 10007")]
    HuksTagKeyVersion,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 10008")]
    HuksTagPayloadLen,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 10009")]
    HuksTagAeTag,
    [Description("HuksTagType.HUKS_TAG_TYPE_ULONG | 10010")]
    HuksTagIsKeyHandle,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 10101")]
    HuksTagOsVersion,
    [Description("HuksTagType.HUKS_TAG_TYPE_UINT | 10102")]
    HuksTagOsPatchlevel,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 20001")]
    HuksTagSymmetricKeyData,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 20002")]
    HuksTagAsymmetricPublicKeyData,
    [Description("HuksTagType.HUKS_TAG_TYPE_BYTES | 20003")]
    HuksTagAsymmetricPrivateKeyData
}