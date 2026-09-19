using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

[Flags]
/// <summary>
/// HuksExternalCryptoTagType 枚举
/// </summary>
public enum HuksExternalCryptoTagType
{
    HuksExtCryptoTagTypeInt = 268435456,
    HuksExtCryptoTagTypeBytes = 1342177280
}

[Flags]
/// <summary>
/// HuksExternalCryptoTag 枚举
/// </summary>
public enum HuksExternalCryptoTag
{
    [Description("HuksExternalCryptoTagType.HUKS_EXT_CRYPTO_TAG_TYPE_BYTES | 200001")]
    HuksExtCryptoTagUkeyPin,
    [Description("HuksExternalCryptoTagType.HUKS_EXT_CRYPTO_TAG_TYPE_BYTES | 200002")]
    HuksExtCryptoTagAbilityName,
    [Description("HuksExternalCryptoTagType.HUKS_EXT_CRYPTO_TAG_TYPE_BYTES | 200003")]
    HuksExtCryptoTagExtraData,
    [Description("HuksExternalCryptoTagType.HUKS_EXT_CRYPTO_TAG_TYPE_INT | 200004")]
    HuksExtCryptoTagUid,
    [Description("HuksExternalCryptoTagType.HUKS_EXT_CRYPTO_TAG_TYPE_INT | 200005")]
    HuksExtCryptoTagPurpose,
    [Description("HuksExternalCryptoTagType.HUKS_EXT_CRYPTO_TAG_TYPE_BYTES | 200007")]
    HuksExtCryptoTagResourceInfo,
    [Description("HuksExternalCryptoTagType.HUKS_EXT_CRYPTO_TAG_TYPE_BYTES | 200008")]
    HuksExtCryptoTagAbilityInfo,
    [Description("HuksExternalCryptoTagType.HUKS_EXT_CRYPTO_TAG_TYPE_BYTES | 200009")]
    HuksExtCryptoTagBundleName
}

/// <summary>
/// HuksExternalPinAuthState 枚举
/// </summary>
public enum HuksExternalPinAuthState
{
    HuksExtCryptoPinNoAuth = 0,
    HuksExtCryptoPinAuthSucceeded = 1,
    HuksExtCryptoPinLocked = 2
}