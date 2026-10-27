#if HARMONYOS
using System;

namespace HarmonyOS.Interop;

/// <summary>
/// Reads the stable fields of an ArkTS BusinessError. Promise and callback task
/// bridges share this implementation so error semantics cannot drift.
///
/// 读取是**全量 best-effort**：rejection 值可以是 Error 对象、{ code, message } 字面量、
/// { error: { code, message } } 包装、或任意常量（字符串/数字/undefined）。任何一步
/// 失败都只让对应字段为 null，绝不抛——否则「读错误信息失败」会顶掉真正的 rejection 原因。
/// </summary>
internal static class BusinessErrorReader
{
    public static (long? Code, string? Message) Read(IntPtr env, IntPtr error)
    {
        if (error == IntPtr.Zero)
            return (null, null);

        if (NativeNodeApi.napi_typeof(env, error, out var errorType) != napi_status.napi_ok)
            return (null, null);

        if (errorType != NativeNodeApi.napi_valuetype.napi_object)
        {
            // rejection 值不一定是字符串（可以是数字/布尔/undefined），读不出来就是 null。
            return (null, NativeNodeApi.TryReadString(env, error));
        }

        var code = NativeNodeApi.TryReadNamedKey(env, error, "code"u8);
        var message = NativeNodeApi.TryReadNamedString(env, error, "message"u8);

        // ArkTS 侧有把错误再包一层的形态：{ error: { code, message } }
        if (message is null || code is null)
        {
            if (NativeNodeApi.napi_get_named_property(env, error, "error"u8, out var inner) == napi_status.napi_ok &&
                inner != IntPtr.Zero &&
                NativeNodeApi.napi_typeof(env, inner, out var innerType) == napi_status.napi_ok &&
                innerType == NativeNodeApi.napi_valuetype.napi_object)
            {
                message ??= NativeNodeApi.TryReadNamedString(env, inner, "message"u8);
                code ??= NativeNodeApi.TryReadNamedKey(env, inner, "code"u8);
            }
        }

        return (code, message);
    }
}
#endif
