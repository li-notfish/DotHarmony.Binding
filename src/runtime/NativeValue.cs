#if HARMONYOS
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HarmonyOS.ArkUI;

/// <summary>
/// C# 类型与 napi_value 之间的转换工具
/// </summary>
internal static class NativeValue
{
    /// <summary>
    /// 将 C# 字符串转换为 napi_value
    /// </summary>
    public static IntPtr From(string? value)
    {
        if (value == null) return IntPtr.Zero;
        var env = NapiEnv.Current;
        var utf8 = Encoding.UTF8.GetBytes(value);
        NativeNodeApi.napi_create_string_utf8(env, utf8, (IntPtr)utf8.Length, out var result);
        return result;
    }

    /// <summary>
    /// 将 C# 双精度浮点数转换为 napi_value
    /// </summary>
    public static IntPtr From(double value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_double(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 C# 单精度浮点数转换为 napi_value
    /// </summary>
    public static IntPtr From(float value) => From((double)value);

    /// <summary>
    /// 将 C# 32 位整数转换为 napi_value
    /// </summary>
    public static IntPtr From(int value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_int32(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 C# 无符号 32 位整数转换为 napi_value
    /// </summary>
    public static IntPtr From(uint value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_uint32(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 C# 64 位整数转换为 napi_value
    /// </summary>
    public static IntPtr From(long value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_int64(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 C# 无符号 64 位整数转换为 napi_value
    /// </summary>
    public static IntPtr From(ulong value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_uint64(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 C# 布尔值转换为 napi_value
    /// </summary>
    public static IntPtr From(bool value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_boolean(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 napi_value 句柄直接返回（已经是 napi_value）
    /// </summary>
    public static IntPtr From(IntPtr value) => value;

    /// <summary>
    /// 将 C# 枚举转换为 napi_value（通过 int）
    /// </summary>
    public static IntPtr From(Enum value) => From(Convert.ToInt32(value));

    /// <summary>
    /// 自动将任意 C# 对象转换为 napi_value
    /// </summary>
    public static IntPtr From(object? value) => value switch
    {
        null => IntPtr.Zero,
        string s => From(s),
        double d => From(d),
        float f => From(f),
        int i => From(i),
        uint ui => From(ui),
        long l => From(l),
        ulong ul => From(ul),
        bool b => From(b),
        IntPtr p => From(p),
        Enum e => From(e),
        _ => throw new NotSupportedException($"Unsupported type for napi conversion: {value.GetType().Name}")
    };

    /// <summary>
    /// 将 napi_value 转换为 C# 布尔值
    /// </summary>
    public static bool ToBool(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_bool(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 双精度浮点数
    /// </summary>
    public static double ToDouble(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_double(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 32 位整数
    /// </summary>
    public static int ToInt(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_int32(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 无符号 32 位整数
    /// </summary>
    public static uint ToUInt(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_uint32(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 64 位整数
    /// </summary>
    public static long ToLong(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_int64(env, value, out var result);
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 字符串
    /// </summary>
    public static string? ToString(IntPtr value)
    {
        if (value == IntPtr.Zero) return null;
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_string_utf8(env, value, null, IntPtr.Zero, out var length);
        if (length == IntPtr.Zero) return string.Empty;
        var buf = new byte[(int)length];
        NativeNodeApi.napi_get_value_string_utf8(env, value, buf, (IntPtr)buf.Length, out length);
        return Encoding.UTF8.GetString(buf);
    }
}
#endif
