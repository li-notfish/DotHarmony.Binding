using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// AuthenticationResult 枚举
/// </summary>
public enum AuthenticationResult
{
    [Description("-1")]
    NoSupport,
    Success = 0,
    CompareFailure = 1,
    Canceled = 2,
    Timeout = 3,
    CameraFail = 4,
    Busy = 5,
    InvalidParameters = 6,
    Locked = 7,
    NotEnrolled = 8,
    GeneralError = 100
}

/// <summary>
/// UserAuthResultCode 枚举
/// </summary>
public enum UserAuthResultCode
{
    Success = 0,
    Fail = 1,
    GeneralError = 2,
    Canceled = 3,
    Timeout = 4,
    TypeNotSupport = 5,
    TrustLevelNotSupport = 6,
    Busy = 7,
    InvalidParameters = 8,
    Locked = 9,
    NotEnrolled = 10
}

/// <summary>
/// FaceTips 枚举
/// </summary>
public enum FaceTips
{
    FaceAuthTipTooBright = 1,
    FaceAuthTipTooDark = 2,
    FaceAuthTipTooClose = 3,
    FaceAuthTipTooFar = 4,
    FaceAuthTipTooHigh = 5,
    FaceAuthTipTooLow = 6,
    FaceAuthTipTooRight = 7,
    FaceAuthTipTooLeft = 8,
    FaceAuthTipTooMuchMotion = 9,
    FaceAuthTipPoorGaze = 10,
    FaceAuthTipNotDetected = 11
}

/// <summary>
/// FingerprintTips 枚举
/// </summary>
public enum FingerprintTips
{
    FingerprintAuthTipGood = 0,
    FingerprintAuthTipDirty = 1,
    FingerprintAuthTipInsufficient = 2,
    FingerprintAuthTipPartial = 3,
    FingerprintAuthTipTooFast = 4,
    FingerprintAuthTipTooSlow = 5
}

/// <summary>
/// UserAuthType 枚举
/// </summary>
public enum UserAuthType
{
    Pin = 1,
    Face = 2,
    Fingerprint = 4,
    CompanionDevice = 64
}

/// <summary>
/// AuthTrustLevel 枚举
/// </summary>
public enum AuthTrustLevel
{
    Atl1 = 10000,
    Atl2 = 20000,
    Atl3 = 30000,
    Atl4 = 40000
}

/// <summary>
/// ReuseMode 枚举
/// </summary>
public enum ReuseMode
{
    AuthTypeRelevant = 1,
    AuthTypeIrrelevant = 2,
    CallerIrrelevantAuthTypeRelevant = 3,
    CallerIrrelevantAuthTypeIrrelevant = 4
}

/// <summary>
/// UserAuthTipCode 枚举
/// </summary>
public enum UserAuthTipCode
{
    CompareFailure = 1,
    Timeout = 2,
    TemporarilyLocked = 3,
    PermanentlyLocked = 4,
    WidgetLoaded = 5,
    WidgetReleased = 6,
    CompareFailureWithFrozen = 7
}

/// <summary>
/// UserAuthResultCode 枚举
/// </summary>
public enum UserAuthResultCode
{
    Success = 12500000,
    Fail = 12500001,
    GeneralError = 12500002,
    Canceled = 12500003,
    Timeout = 12500004,
    TypeNotSupport = 12500005,
    TrustLevelNotSupport = 12500006,
    Busy = 12500007,
    InvalidParameters = 12500008,
    Locked = 12500009,
    NotEnrolled = 12500010,
    CanceledFromWidget = 12500011,
    PinExpired = 12500013
}