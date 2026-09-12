using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Accessibility 枚举
/// </summary>
public enum Accessibility
{
    DevicePoweredOn = 0,
    DeviceFirstUnlocked = 1,
    DeviceUnlocked = 2
}

/// <summary>
/// AuthType 枚举
/// </summary>
public enum AuthType
{
    None = 0,
    Any = 255
}

[Flags]
/// <summary>
/// SyncType 枚举
/// </summary>
public enum SyncType
{
    Never = 0,
    ThisDevice = 1,
    TrustedDevice = 2,
    TrustedAccount = 4
}

/// <summary>
/// WrapType 枚举
/// </summary>
public enum WrapType
{
    Never = 0,
    TrustedAccount = 1
}

/// <summary>
/// AssetConflictResolution 枚举
/// </summary>
public enum AssetConflictResolution
{
    Overwrite = 0,
    ThrowError = 1
}

/// <summary>
/// ReturnType 枚举
/// </summary>
public enum ReturnType
{
    All = 0,
    Attributes = 1
}

/// <summary>
/// AssetOperationType 枚举
/// </summary>
public enum AssetOperationType
{
    NeedSync = 0,
    NeedLogout = 1
}

[Flags]
/// <summary>
/// TagType 枚举
/// </summary>
public enum TagType
{
    Bool = 268435456,
    Number = 536870912,
    Bytes = 805306368
}

[Flags]
/// <summary>
/// Tag 枚举
/// </summary>
public enum Tag
{
    [Description("TagType.BYTES | 0x01")]
    Secret,
    [Description("TagType.BYTES | 0x02")]
    Alias,
    [Description("TagType.NUMBER | 0x03")]
    Accessibility,
    [Description("TagType.BOOL | 0x04")]
    RequirePasswordSet,
    [Description("TagType.NUMBER | 0x05")]
    AuthType,
    [Description("TagType.NUMBER | 0x06")]
    AuthValidityPeriod,
    [Description("TagType.BYTES | 0x07")]
    AuthChallenge,
    [Description("TagType.BYTES | 0x08")]
    AuthToken,
    [Description("TagType.NUMBER | 0x10")]
    SyncType,
    [Description("TagType.BOOL | 0x11")]
    IsPersistent,
    [Description("TagType.BYTES | 0x20")]
    DataLabelCritical1,
    [Description("TagType.BYTES | 0x21")]
    DataLabelCritical2,
    [Description("TagType.BYTES | 0x22")]
    DataLabelCritical3,
    [Description("TagType.BYTES | 0x23")]
    DataLabelCritical4,
    [Description("TagType.BYTES | 0x30")]
    DataLabelNormal1,
    [Description("TagType.BYTES | 0x31")]
    DataLabelNormal2,
    [Description("TagType.BYTES | 0x32")]
    DataLabelNormal3,
    [Description("TagType.BYTES | 0x33")]
    DataLabelNormal4,
    [Description("TagType.BYTES | 0x34")]
    DataLabelNormalLocal1,
    [Description("TagType.BYTES | 0x35")]
    DataLabelNormalLocal2,
    [Description("TagType.BYTES | 0x36")]
    DataLabelNormalLocal3,
    [Description("TagType.BYTES | 0x37")]
    DataLabelNormalLocal4,
    [Description("TagType.NUMBER | 0x40")]
    ReturnType,
    [Description("TagType.NUMBER | 0x41")]
    ReturnLimit,
    [Description("TagType.NUMBER | 0x42")]
    ReturnOffset,
    [Description("TagType.NUMBER | 0x43")]
    ReturnOrderedBy,
    [Description("TagType.NUMBER | 0x44")]
    ConflictResolution,
    [Description("TagType.BYTES | 0x45")]
    UpdateTime,
    [Description("TagType.NUMBER | 0x46")]
    OperationType,
    [Description("TagType.BOOL | 0x47")]
    RequireAttrEncrypted,
    [Description("TagType.BYTES | 0x48")]
    GroupId,
    [Description("TagType.NUMBER | 0x49")]
    WrapType
}

/// <summary>
/// AssetErrorCode 枚举
/// </summary>
public enum AssetErrorCode
{
    PermissionDenied = 201,
    NotSystemApplication = 202,
    InvalidArgument = 401,
    ServiceUnavailable = 24000001,
    NotFound = 24000002,
    Duplicated = 24000003,
    AccessDenied = 24000004,
    StatusMismatch = 24000005,
    OutOfMemory = 24000006,
    DataCorrupted = 24000007,
    DatabaseError = 24000008,
    CryptoError = 24000009,
    IpcError = 24000010,
    BmsError = 24000011,
    AccountError = 24000012,
    AccessTokenError = 24000013,
    FileOperationError = 24000014,
    GetSystemTimeError = 24000015,
    LimitExceeded = 24000016,
    Unsupported = 24000017,
    ParamVerificationFailed = 24000018,
    InconsistentAttribute = 24000019
}