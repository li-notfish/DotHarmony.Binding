#if HARMONYOS
using System;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// ArkTS 运行时异常，当 Promise 被 reject 时抛出
/// </summary>
public sealed class ArkTSException : Exception
{
    /// <summary>
    /// ArkTS 错误消息（如 "Error: invalid parameter"）
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// 原始 napi_value 句柄，用于调试时获取完整错误信息
    /// </summary>
    public IntPtr NapiValue { get; }

    /// <summary>
    /// 创建一个包含错误原因的 ArkTS 异常
    /// </summary>
    internal ArkTSException(string message, string? reason)
        : base(message)
    {
        Reason = reason;
        NapiValue = IntPtr.Zero;
    }

    /// <summary>
    /// 创建一个包含 napi_value 句柄的 ArkTS 异常
    /// </summary>
    internal ArkTSException(string message, IntPtr napiValue)
        : base(message)
    {
        NapiValue = napiValue;
        Reason = null;
    }

    /// <summary>
    /// 创建一个包含错误原因和 napi_value 句柄的 ArkTS 异常
    /// </summary>
    internal ArkTSException(string message, string? reason, IntPtr napiValue)
        : base(message)
    {
        Reason = reason;
        NapiValue = napiValue;
    }
}
#endif
