#if HARMONYOS
using System;

namespace HarmonyOS.Interop;

/// <summary>
/// Reads the stable fields of an ArkTS BusinessError. Promise and callback task
/// bridges share this implementation so error semantics cannot drift.
/// </summary>
internal static class BusinessErrorReader
{
    public static (long? Code, string? Message) Read(IntPtr env, IntPtr error)
    {
        NativeNodeApi.napi_typeof(env, error, out var errorType).ThrowIfFailed();
        if (errorType != NativeNodeApi.napi_valuetype.napi_object)
        {
            string? fallback = null;
            try { fallback = NativeValue.ToString(error); }
            catch { /* A rejection value does not have to be a string. */ }
            return (null, fallback);
        }

        return (ReadInt64(env, error, "code"u8), ReadString(env, error, "message"u8));
    }

    private static long? ReadInt64(IntPtr env, IntPtr error, ReadOnlySpan<byte> key)
    {
        NativeNodeApi.napi_get_named_property(env, error, key, out var value).ThrowIfFailed();
        NativeNodeApi.napi_typeof(env, value, out var type).ThrowIfFailed();
        if (type != NativeNodeApi.napi_valuetype.napi_number) return null;
        NativeNodeApi.napi_get_value_int64(env, value, out var result).ThrowIfFailed();
        return result;
    }

    private static string? ReadString(IntPtr env, IntPtr error, ReadOnlySpan<byte> key)
    {
        NativeNodeApi.napi_get_named_property(env, error, key, out var value).ThrowIfFailed();
        NativeNodeApi.napi_typeof(env, value, out var type).ThrowIfFailed();
        return type == NativeNodeApi.napi_valuetype.napi_string ? NativeValue.ToString(value) : null;
    }
}
#endif
