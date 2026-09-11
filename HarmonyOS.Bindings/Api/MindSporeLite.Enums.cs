using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// QuantizationType 枚举
/// </summary>
public enum QuantizationType
{
    NoQuant = 0,
    WeightQuant = 1,
    FullQuant = 2
}

/// <summary>
/// OptimizationLevel 枚举
/// </summary>
public enum OptimizationLevel
{
    O0 = 0,
    O2 = 2,
    O3 = 3,
    Auto = 4
}

/// <summary>
/// PerformanceMode 枚举
/// </summary>
public enum PerformanceMode
{
    PerformanceNone = 0,
    PerformanceLow = 1,
    PerformanceMedium = 2,
    PerformanceHigh = 3,
    PerformanceExtreme = 4
}

/// <summary>
/// Priority 枚举
/// </summary>
public enum Priority
{
    PriorityNone = 0,
    PriorityLow = 1,
    PriorityMedium = 2,
    PriorityHigh = 3
}

/// <summary>
/// NNRTDeviceType 枚举
/// </summary>
public enum NNRTDeviceType
{
    NnrtdeviceOthers = 0,
    NnrtdeviceCpu = 1,
    NnrtdeviceGpu = 2,
    NnrtdeviceAccelerator = 3
}

/// <summary>
/// ThreadAffinityMode 枚举
/// </summary>
public enum ThreadAffinityMode
{
    NoAffinities = 0,
    BigCoresFirst = 1,
    LittleCoresFirst = 2
}

/// <summary>
/// MindSporeLiteDataType 枚举
/// </summary>
public enum MindSporeLiteDataType
{
    TypeUnknown = 0,
    NumberTypeInt8 = 32,
    NumberTypeInt16 = 33,
    NumberTypeInt32 = 34,
    NumberTypeInt64 = 35,
    NumberTypeUint8 = 37,
    NumberTypeUint16 = 38,
    NumberTypeUint32 = 39,
    NumberTypeUint64 = 40,
    NumberTypeFloat16 = 42,
    NumberTypeFloat32 = 43,
    NumberTypeFloat64 = 44
}

/// <summary>
/// Format 枚举
/// </summary>
public enum Format
{
    [Description("-1")]
    DefaultFormat,
    Nchw = 0,
    Nhwc = 1,
    Nhwc4 = 2,
    Hwkc = 3,
    Hwck = 4,
    Kchw = 5
}