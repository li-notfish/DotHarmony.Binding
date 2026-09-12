using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CryptoFrameworkResult 枚举
/// </summary>
public enum CryptoFrameworkResult
{
    InvalidParams = 401,
    NotSupport = 801,
    ErrOutOfMemory = 17620001,
    ErrRuntimeError = 17620002,
    ErrParameterCheckFailed = 17620003,
    ErrInvalidCall = 17620004,
    ErrCryptoOperation = 17630001
}

/// <summary>
/// CryptoMode 枚举
/// </summary>
public enum CryptoMode
{
    EncryptMode = 0,
    DecryptMode = 1
}

/// <summary>
/// CipherSpecItem 枚举
/// </summary>
public enum CipherSpecItem
{
    OaepMdNameStr = 100,
    OaepMgfNameStr = 101,
    OaepMgf1MdStr = 102,
    OaepMgf1PsrcUint8Arr = 103,
    Sm2MdNameStr = 104
}

/// <summary>
/// SignSpecItem 枚举
/// </summary>
public enum SignSpecItem
{
    PssMdNameStr = 100,
    PssMgfNameStr = 101,
    PssMgf1MdStr = 102,
    PssSaltLenNum = 103,
    PssTrailerFieldNum = 104,
    Sm2UserIdUint8Arr = 105,
    MlDsaDeterministicBool = 106,
    MlDsaMuBool = 107,
    MlDsaContextUint8Arr = 108
}

/// <summary>
/// AsyKeySpecItem 枚举
/// </summary>
public enum AsyKeySpecItem
{
    DsaPBn = 101,
    DsaQBn = 102,
    DsaGBn = 103,
    DsaSkBn = 104,
    DsaPkBn = 105,
    EccFpPBn = 201,
    EccABn = 202,
    EccBBn = 203,
    EccGXBn = 204,
    EccGYBn = 205,
    EccNBn = 206,
    EccHNum = 207,
    EccSkBn = 208,
    EccPkXBn = 209,
    EccPkYBn = 210,
    EccFieldTypeStr = 211,
    EccFieldSizeNum = 212,
    EccCurveNameStr = 213,
    RsaNBn = 301,
    RsaSkBn = 302,
    RsaPkBn = 303,
    DhPBn = 401,
    DhGBn = 402,
    DhLNum = 403,
    DhSkBn = 404,
    DhPkBn = 405,
    Ed25519SkBn = 501,
    Ed25519PkBn = 502,
    X25519SkBn = 601,
    X25519PkBn = 602
}

/// <summary>
/// AsyKeyDataItem 枚举
/// </summary>
public enum AsyKeyDataItem
{
    MlDsaPrivateSeed = 0,
    MlDsaPrivateRaw = 1,
    MlDsaPublicRaw = 2,
    MlKemPrivateSeed = 3,
    MlKemPrivateRaw = 4,
    MlKemPublicRaw = 5,
    EcPrivateK = 6,
    EcPrivate04XYK = 7,
    EcPublicXY = 8,
    EcPublic04XY = 9,
    EcPublicCompressX = 10
}

/// <summary>
/// AsyKeySpecType 枚举
/// </summary>
public enum AsyKeySpecType
{
    CommonParamsSpec = 0,
    PrivateKeySpec = 1,
    PublicKeySpec = 2,
    KeyPairSpec = 3
}

/// <summary>
/// KemAlgNameId 枚举
/// </summary>
public enum KemAlgNameId
{
    MlKem512 = 0,
    MlKem768 = 1,
    MlKem1024 = 2
}