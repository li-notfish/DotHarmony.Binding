using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DataDistributedKVStoreValueType 枚举
/// </summary>
public enum DataDistributedKVStoreValueType
{
    String = 0,
    Integer = 1,
    Float = 2,
    ByteArray = 3,
    Boolean = 4,
    Double = 5
}

/// <summary>
/// DataDistributedKVStoreSyncMode 枚举
/// </summary>
public enum DataDistributedKVStoreSyncMode
{
    PullOnly,
    PushOnly,
    PushPull
}

/// <summary>
/// DataDistributedKVStoreSubscribeType 枚举
/// </summary>
public enum DataDistributedKVStoreSubscribeType
{
    SubscribeTypeLocal,
    SubscribeTypeRemote,
    SubscribeTypeAll
}

/// <summary>
/// DataDistributedKVStoreKVStoreType 枚举
/// </summary>
public enum DataDistributedKVStoreKVStoreType
{
    DeviceCollaboration,
    SingleVersion
}

/// <summary>
/// DataDistributedKVStoreSecurityLevel 枚举
/// </summary>
public enum DataDistributedKVStoreSecurityLevel
{
    S1,
    S2,
    S3,
    S4
}