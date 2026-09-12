using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ZlibErrorCode 枚举
/// </summary>
public enum ZlibErrorCode
{
    ErrorCodeOk = 0,
    [Description("-1")]
    ErrorCodeErrno
}

/// <summary>
/// CompressLevel 枚举
/// </summary>
public enum CompressLevel
{
    CompressLevelNoCompression = 0,
    CompressLevelBestSpeed = 1,
    CompressLevelBestCompression = 9,
    [Description("-1")]
    CompressLevelDefaultCompression
}

/// <summary>
/// CompressStrategy 枚举
/// </summary>
public enum CompressStrategy
{
    CompressStrategyDefaultStrategy = 0,
    CompressStrategyFiltered = 1,
    CompressStrategyHuffmanOnly = 2,
    CompressStrategyRle = 3,
    CompressStrategyFixed = 4
}

/// <summary>
/// ParallelStrategy 枚举
/// </summary>
public enum ParallelStrategy
{
    ParallelStrategySequential = 0,
    ParallelStrategyParallelDecompression = 1
}

/// <summary>
/// PathSeparatorStrategy 枚举
/// </summary>
public enum PathSeparatorStrategy
{
    PathSeparatorStrategyDefault = 0,
    PathSeparatorStrategyReplaceBackslash = 1
}

/// <summary>
/// MemLevel 枚举
/// </summary>
public enum MemLevel
{
    MemLevelMin = 1,
    MemLevelMax = 9,
    MemLevelDefault = 8
}

/// <summary>
/// CompressFlushMode 枚举
/// </summary>
public enum CompressFlushMode
{
    NoFlush = 0,
    PartialFlush = 1,
    SyncFlush = 2,
    FullFlush = 3,
    Finish = 4,
    Block = 5,
    Trees = 6
}

/// <summary>
/// ReturnStatus 枚举
/// </summary>
public enum ReturnStatus
{
    Ok = 0,
    StreamEnd = 1,
    NeedDict = 2,
    [Description("-1")]
    Errno,
    [Description("-2")]
    StreamError,
    [Description("-3")]
    DataError,
    [Description("-4")]
    MemError,
    [Description("-5")]
    BufError
}

/// <summary>
/// CompressMethod 枚举
/// </summary>
public enum CompressMethod
{
    Deflated = 8
}

/// <summary>
/// OffsetReferencePoint 枚举
/// </summary>
public enum OffsetReferencePoint
{
    SeekSet = 0,
    SeekCur = 1
}