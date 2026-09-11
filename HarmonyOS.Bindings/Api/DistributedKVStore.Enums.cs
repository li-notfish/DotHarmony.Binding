using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DistributedKVStoreValueType 枚举
/// </summary>
public enum DistributedKVStoreValueType
{
    String = 0,
    Integer = 1,
    Float = 2,
    ByteArray = 3,
    Boolean = 4,
    Double = 5
}

/// <summary>
/// DistributedKVStoreSyncMode 枚举
/// </summary>
public enum DistributedKVStoreSyncMode
{
    PullOnly,
    PushOnly,
    PushPull
}

/// <summary>
/// DistributedKVStoreSubscribeType 枚举
/// </summary>
public enum DistributedKVStoreSubscribeType
{
    SubscribeTypeLocal,
    SubscribeTypeRemote,
    SubscribeTypeAll
}

/// <summary>
/// DistributedKVStoreKVStoreType 枚举
/// </summary>
public enum DistributedKVStoreKVStoreType
{
    DeviceCollaboration,
    SingleVersion
}

/// <summary>
/// DistributedKVStoreSecurityLevel 枚举
/// </summary>
public enum DistributedKVStoreSecurityLevel
{
    S1,
    S2,
    S3,
    S4
}