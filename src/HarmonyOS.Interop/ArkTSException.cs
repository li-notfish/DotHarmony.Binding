#if HARMONYOS
using System;

namespace HarmonyOS.Interop;

/// <summary>
/// ArkTS 运行时异常，当 Promise 被 reject 或 AsyncCallback 报错时抛出。
/// 只携带值语义字段（文本 + code）：异常会跨回调帧存活（存进 Task、被 await 方持有），
/// 而 napi_value 只在产生它的 handle scope 内有效，把它放进异常等于带出一个悬空句柄。
/// </summary>
public sealed class ArkTSException : Exception
{
    /// <summary>
    /// ArkTS 错误消息（如 "Error: invalid parameter"）
    /// </summary>
    public string? Reason { get; }

    /// <summary>BusinessError.code；非对象错误或缺失时为 null。</summary>
    public long? ErrorCode { get; }

    /// <summary>
    /// 创建一个包含错误原因的 ArkTS 异常
    /// </summary>
    internal ArkTSException(string message, string? reason)
        : base(message)
    {
        Reason = reason;
        ErrorCode = null;
    }

    /// <summary>
    /// 创建一个包含错误原因与 BusinessError.code 的 ArkTS 异常
    /// </summary>
    internal ArkTSException(string message, string? reason, long? errorCode)
        : base(message)
    {
        Reason = reason;
        ErrorCode = errorCode;
    }
}
#endif
