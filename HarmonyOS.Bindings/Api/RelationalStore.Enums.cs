using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// AssetStatus 枚举
/// </summary>
public enum AssetStatus
{
    AssetNormal,
    AssetInsert,
    AssetUpdate,
    AssetDelete,
    AssetAbnormal,
    AssetDownloading,
    AssetToDownload
}

/// <summary>
/// EncryptionAlgo 枚举
/// </summary>
public enum EncryptionAlgo
{
    Aes256Gcm = 0,
    Aes256Cbc = 1,
    PlainText = 2
}

/// <summary>
/// HmacAlgo 枚举
/// </summary>
public enum HmacAlgo
{
    Sha1 = 0,
    Sha256 = 1,
    Sha512 = 2
}

/// <summary>
/// KdfAlgo 枚举
/// </summary>
public enum KdfAlgo
{
    KdfSha1 = 0,
    KdfSha256 = 1,
    KdfSha512 = 2
}

/// <summary>
/// Tokenizer 枚举
/// </summary>
public enum Tokenizer
{
    NoneTokenizer = 0,
    IcuTokenizer = 1,
    CustomTokenizer = 2
}

/// <summary>
/// RelationalStoreProgress 枚举
/// </summary>
public enum RelationalStoreProgress
{
    SyncBegin = 0,
    SyncInProgress = 1,
    SyncFinish = 2
}

/// <summary>
/// ProgressCode 枚举
/// </summary>
public enum ProgressCode
{
    Success = 0,
    UnknownError = 1,
    NetworkError = 2,
    CloudDisabled = 3,
    LockedByOthers = 4,
    RecordLimitExceeded = 5,
    NoSpaceForAsset = 6,
    BlockedByNetworkStrategy = 7,
    StopCloudSync = 8
}

/// <summary>
/// SecurityLevel 枚举
/// </summary>
public enum SecurityLevel
{
    S1 = 1,
    S2 = 2,
    S3 = 3,
    S4 = 4
}

/// <summary>
/// SyncMode 枚举
/// </summary>
public enum SyncMode
{
    SyncModePush = 0,
    SyncModePull = 1,
    SyncModeTimeFirst,
    SyncModeNativeFirst,
    SyncModeCloudFirst
}

/// <summary>
/// SubscribeType 枚举
/// </summary>
public enum SubscribeType
{
    SubscribeTypeRemote = 0,
    SubscribeTypeCloud = 1,
    SubscribeTypeCloudDetails = 2,
    SubscribeTypeLocalDetails
}

/// <summary>
/// DistributedTableType 枚举
/// </summary>
public enum DistributedTableType
{
    DeviceCollaboration = 0,
    SingleVersion = 1
}

/// <summary>
/// DistributedType 枚举
/// </summary>
public enum DistributedType
{
    DistributedDevice = 0,
    DistributedCloud = 1
}

/// <summary>
/// AssetConflictPolicy 枚举
/// </summary>
public enum AssetConflictPolicy
{
    ConflictPolicyDefault = 0,
    ConflictPolicyTimeFirst = 1,
    ConflictPolicyTempPath = 2
}

/// <summary>
/// ConflictResolution 枚举
/// </summary>
public enum ConflictResolution
{
    OnConflictNone = 0,
    OnConflictRollback = 1,
    OnConflictAbort = 2,
    OnConflictFail = 3,
    OnConflictIgnore = 4,
    OnConflictReplace = 5
}

/// <summary>
/// Origin 枚举
/// </summary>
public enum Origin
{
    Local = 0,
    Cloud = 1,
    Remote = 2
}

/// <summary>
/// Field 枚举
/// </summary>
public enum Field
{
    [Description("#_cursor")]
    CursorField,
    [Description("#_origin")]
    OriginField,
    [Description("#_deleted_flag")]
    DeletedFlagField,
    [Description("#_data_status")]
    DataStatusField,
    [Description("#_cloud_owner")]
    OwnerField,
    [Description("#_cloud_privilege")]
    PrivilegeField,
    [Description("#_sharing_resource_field")]
    SharingResourceField
}

/// <summary>
/// RebuildType 枚举
/// </summary>
public enum RebuildType
{
    None = 0,
    Rebuilt = 1,
    Repaired = 2
}

/// <summary>
/// TransactionType 枚举
/// </summary>
public enum TransactionType
{
    Deferred = 0,
    Immediate = 1,
    Exclusive = 2
}

/// <summary>
/// ColumnType 枚举
/// </summary>
public enum ColumnType
{
    Null = 0,
    Integer = 1,
    Real = 2,
    Text = 3,
    Blob = 4,
    Asset = 5,
    Assets = 6,
    FloatVector = 7,
    UnlimitedInt = 8
}

/// <summary>
/// SyncResultCode 枚举
/// </summary>
public enum SyncResultCode
{
    Success = 0,
    Fail = 1,
    Offline = 2,
    InvalidArgs = 3,
    DistributedTableNotSet = 4,
    TableFieldMismatch = 5,
    DistributedSchemaMismatch = 6,
    Busy = 7,
    Corrupted = 8,
    Timeout = 9,
    SchemaChanged = 10,
    ConstraintViolation = 11
}