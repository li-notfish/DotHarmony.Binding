#if HARMONYOS
using System;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// napi_value → CLR 的统一转换入口（NodeApi / JsObject / PromiseTaskBridge 共用）。
/// 数组与 JsObject 派生包装类不在此转换——它们需要逐元素取值或构造包装实例的上下文，
/// 由生成代码在调用点显式传入 Func&lt;IntPtr, T&gt; 转换委托（AOT 安全，无反射、无注册表）。
/// </summary>
internal static class ValueConverter
{
    /// <summary>按目标类型转换 napi_value（基元类型 + 枚举 + IntPtr 句柄）。</summary>
    public static object? ConvertTo(Type type, IntPtr value)
    {
        if (type.IsEnum)
        {
            var underlying = Enum.GetUnderlyingType(type);
            return underlying == typeof(uint)
                ? Enum.ToObject(type, NativeValue.ToUInt(value))
                : Enum.ToObject(type, NativeValue.ToInt(value));
        }
        if (type == typeof(bool)) return NativeValue.ToBool(value);
        if (type == typeof(double)) return NativeValue.ToDouble(value);
        if (type == typeof(float)) return (float)NativeValue.ToDouble(value);
        if (type == typeof(int)) return NativeValue.ToInt(value);
        if (type == typeof(uint)) return NativeValue.ToUInt(value);
        if (type == typeof(long)) return NativeValue.ToLong(value);
        if (type == typeof(ulong)) return NativeValue.ToUInt64(value);
        if (type == typeof(byte)) return NativeValue.ToByte(value);
        if (type == typeof(string)) return NativeValue.ToString(value);
        if (type == typeof(IntPtr)) return value;
        throw new NotSupportedException(
            $"Unsupported conversion target: {type.Name}. " +
            "Arrays and JsObject wrappers must use explicit Func<IntPtr, T> converters at generated call sites.");
    }

    public static T Convert<T>(IntPtr value) => (T)ConvertTo(typeof(T), value)!;

    /// <summary>把 napi 数组逐元素转换（生成代码的数组返回值/参数转换基础件）。</summary>
    public static TArr[] ConvertArray<TArr>(IntPtr array, Func<IntPtr, TArr> convertElement)
    {
        var elements = NodeApi.GetArrayElements(array);
        var result = new TArr[elements.Length];
        for (int i = 0; i < elements.Length; i++)
            result[i] = convertElement(elements[i])!;
        return result;
    }
}
#endif
