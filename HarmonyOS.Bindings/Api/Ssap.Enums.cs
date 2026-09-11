using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PropertyDescriptorType 枚举
/// </summary>
public enum PropertyDescriptorType
{
    Property = 1,
    ClientPropertyConfig = 2,
    ServerPropertyConfig = 3,
    PropertyFormat = 4,
    TypeVendor = 255
}

/// <summary>
/// SsapOperation 枚举
/// </summary>
public enum SsapOperation
{
    Readable = 1,
    WriteNoResponse = 2,
    WriteWithResponse = 4,
    Notify = 8
}

/// <summary>
/// PropertyWriteType 枚举
/// </summary>
public enum PropertyWriteType
{
    Write = 1,
    WriteNoResponse = 2
}