#if HARMONYOS
using System;

namespace HarmonyOS.Interop;

/// <summary>
/// Node-API 调用失败异常，携带 napi_status。
/// public 顶层类（原为 NativeNodeApi 内嵌 internal 类）：包装层调用方（如 Essentials）
/// 需捕获 napi 侧失败（JS 抛错 → napi_pending_exception），漏斗纪律禁止其在
/// Handler/Hosting 层引用 NativeNodeApi 符号本身，故提升至命名空间级。
/// Status 保持 internal（napi_status 为 internal 枚举），Message 已携带等价信息。
/// </summary>
public sealed class NapiException : Exception
{
    internal NapiException(napi_status status, string op)
        : base($"NAPI call '{op}' failed with status {status} ({(int)status})")
    {
        Status = status;
        Operation = op;
    }

    internal napi_status Status { get; }
    public string Operation { get; }
}
#endif
