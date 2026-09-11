using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// HuksCryptoExtensionResultCode 枚举
/// </summary>
public enum HuksCryptoExtensionResultCode
{
    HuksCryptoExtensionErrExtensionFail = 34800000,
    HuksCryptoExtensionErrUkeyNotExist = 34800001,
    HuksCryptoExtensionErrUkeyDriverFail = 34800002,
    HuksCryptoExtensionErrPinNoAuth = 34800003,
    HuksCryptoExtensionErrHandleNotExist = 34800004,
    HuksCryptoExtensionErrHandleUnavailable = 34800005,
    HuksCryptoExtensionErrPinIncorrect = 34800006,
    HuksCryptoExtensionErrPinLocked = 34800007
}