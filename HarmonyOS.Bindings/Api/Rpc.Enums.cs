using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// RpcErrorCode 枚举
/// </summary>
public enum RpcErrorCode
{
    CheckParamError = 401,
    OsMmapError = 1900001,
    OsIoctlError = 1900002,
    WriteToAshmemError = 1900003,
    ReadFromAshmemError = 1900004,
    OnlyProxyObjectPermittedError = 1900005,
    OnlyRemoteObjectPermittedError = 1900006,
    CommunicationError = 1900007,
    ProxyOrRemoteObjectInvalidError = 1900008,
    WriteDataToMessageSequenceError = 1900009,
    ReadDataFromMessageSequenceError = 1900010,
    ParcelMemoryAllocError = 1900011,
    CallJsMethodError = 1900012,
    OsDupError = 1900013
}

/// <summary>
/// TypeCode 枚举
/// </summary>
public enum TypeCode
{
    Int8Array = 0,
    Uint8Array = 1,
    Int16Array = 2,
    Uint16Array = 3,
    Int32Array = 4,
    Uint32Array = 5,
    Float32Array = 6,
    Float64Array = 7,
    Bigint64Array = 8,
    Biguint64Array = 9
}