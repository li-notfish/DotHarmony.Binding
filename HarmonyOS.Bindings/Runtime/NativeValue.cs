#if HARMONYOS
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HarmonyOS.Bindings.Runtime;

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
        NativeNodeApi.napi_create_string_utf8(env, utf8, (IntPtr)utf8.Length, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 C# 双精度浮点数转换为 napi_value
    /// </summary>
    public static IntPtr From(double value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_double(env, value, out var result).ThrowIfFailed();
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
        NativeNodeApi.napi_create_int32(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 C# 无符号 32 位整数转换为 napi_value
    /// </summary>
    public static IntPtr From(uint value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_uint32(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 C# 64 位整数转换为 napi_value
    /// </summary>
    public static IntPtr From(long value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_int64(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 C# 无符号 64 位整数转换为 napi_value
    /// </summary>
    public static IntPtr From(ulong value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_uint64(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 C# 布尔值转换为 napi_value
    /// </summary>
    public static IntPtr From(bool value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_boolean(env, value, out var result).ThrowIfFailed();
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
    /// 将 JsObject 包装实例转换为 napi_value（取强引用当前值）
    /// </summary>
    public static IntPtr From(JsObject value)
    {
        if (value == null) return IntPtr.Zero;
        return value.PinnedValue;
    }

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
        JsObject j => From(j),
        Delegate d => From(d),
        _ => FromRecord(value)
    };

    /// <summary>
    /// 将 C# 委托转换为 napi_function
    /// </summary>
    public static IntPtr From(Delegate del)
    {
        var env = NapiEnv.Current;
        var method = del.Method;
        var parameters = method.GetParameters();
        var gch = GCHandle.Alloc(del);
        var nameBytes = Encoding.UTF8.GetBytes(method.Name);
        var fnPtr = (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&NativeCallbacks.Action_Ptr;
        NativeNodeApi.napi_create_function(
            env, nameBytes, (IntPtr)nameBytes.Length,
            fnPtr, GCHandle.ToIntPtr(gch), out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 record 对象（如 Options）序列化为 JS 对象。
    /// AOT 安全：生成器为每个 record 产出 INapiRecord 显式实现；
    /// IDictionary 走动态路径；其余类型显式失败（而不是被裁剪静默吞掉属性）。
    /// </summary>
    private static IntPtr FromRecord(object value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_object(env, out var obj).ThrowIfFailed();

        if (value is INapiRecord serializable)
        {
            serializable.WriteTo(env, obj);
            return obj;
        }

        if (value is System.Collections.Generic.IDictionary<string, object?> dict)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Value == null) continue;
                var napiValue = From(kvp.Value);
                if (napiValue == IntPtr.Zero) continue;
                var utf8 = Encoding.UTF8.GetBytes(kvp.Key);
                NativeNodeApi.napi_set_named_property(env, obj, utf8, napiValue).ThrowIfFailed();
            }
            return obj;
        }

        throw new NotSupportedException(
            $"Type '{value.GetType().Name}' cannot be marshaled to a JS object. " +
            "Records must implement INapiRecord (generated by the binding generator), " +
            "or pass an IDictionary<string, object?>.");
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 布尔值
    /// </summary>
    public static bool ToBool(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_bool(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 双精度浮点数
    /// </summary>
    public static double ToDouble(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_double(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 32 位整数
    /// </summary>
    public static int ToInt(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_int32(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 无符号 32 位整数
    /// </summary>
    public static uint ToUInt(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_uint32(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 64 位整数
    /// </summary>
    public static long ToLong(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_int64(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 无符号 64 位整数
    /// </summary>
    public static ulong ToUInt64(IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_uint64(env, value, out var result).ThrowIfFailed();
        return result;
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 字节（通过 int32 截断）
    /// </summary>
    public static byte ToByte(IntPtr value)
    {
        return (byte)ToInt(value);
    }

    /// <summary>
    /// 将 napi_value 转换为 C# 字符串
    /// </summary>
    public static string? ToString(IntPtr value)
    {
        if (value == IntPtr.Zero) return null;
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_value_string_utf8(env, value, null, IntPtr.Zero, out var length).ThrowIfFailed();
        if (length == IntPtr.Zero) return string.Empty;
        // napi 第二次调用写入的字符串含 null 终止符空间：缓冲区必须 length+1，否则末字节被裁剪
        var buf = new byte[(int)length + 1];
        NativeNodeApi.napi_get_value_string_utf8(env, value, buf, (IntPtr)buf.Length, out length).ThrowIfFailed();
        return Encoding.UTF8.GetString(buf, 0, (int)length);
    }
}
#endif
