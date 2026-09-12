using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// UserType 枚举
/// </summary>
public enum UserType
{
    SameUserId = 0
}

/// <summary>
/// ValueType 枚举
/// </summary>
public enum ValueType
{
    String = 0,
    Integer = 1,
    Float = 2,
    ByteArray = 3,
    Boolean = 4,
    Double = 5
}

/// <summary>
/// SyncMode 枚举
/// </summary>
public enum SyncMode
{
    PullOnly = 0,
    PushOnly = 1,
    PushPull = 2
}

/// <summary>
/// SubscribeType 枚举
/// </summary>
public enum SubscribeType
{
    SubscribeTypeLocal = 0,
    SubscribeTypeRemote = 1,
    SubscribeTypeAll = 2
}

/// <summary>
/// KVStoreType 枚举
/// </summary>
public enum KVStoreType
{
    DeviceCollaboration = 0,
    SingleVersion = 1,
    MultiVersion = 2
}

/// <summary>
/// SecurityLevel 枚举
/// </summary>
public enum SecurityLevel
{
    NoLevel = 0,
    S0 = 1,
    S1 = 2,
    S2 = 3,
    S3 = 5,
    S4 = 6
}