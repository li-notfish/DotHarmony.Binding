using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// 预编译的 napi 回调 trampoline
/// 用于将 C# 委托包装为 napi_function
/// </summary>
internal static class NativeCallbacks
{
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static IntPtr Action_Ptr(IntPtr env, IntPtr info)
    {
        NativeNodeApi.napi_get_cb_info(env, info, out _, out var argv, out _, out var data);
        if (data == IntPtr.Zero)
        {
            NativeNodeApi.napi_get_undefined(env, out var undefinedZero);
            return undefinedZero;
        }

        var gch = GCHandle.FromIntPtr(data);
        var del = (Delegate)gch.Target!;
        var method = del.Method;
        var parameters = method.GetParameters();

        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var argPtr = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
            args[i] = ConvertArg(argPtr, parameters[i].ParameterType);
        }

        var result = method.Invoke(del.Target, args);

        if (method.ReturnType == typeof(void))
        {
            NativeNodeApi.napi_get_undefined(env, out var undefined);
            return undefined;
        }
        else
        {
            return NativeValue.From(result);
        }
    }

    private static object? ConvertArg(IntPtr napiValue, Type targetType)
    {
        if (napiValue == IntPtr.Zero) return null;

        if (targetType == typeof(bool))
            return NativeValue.ToBool(napiValue);
        if (targetType == typeof(double))
            return NativeValue.ToDouble(napiValue);
        if (targetType == typeof(int))
            return NativeValue.ToInt(napiValue);
        if (targetType == typeof(uint))
            return NativeValue.ToUInt(napiValue);
        if (targetType == typeof(long))
            return NativeValue.ToLong(napiValue);
        if (targetType == typeof(string))
            return NativeValue.ToString(napiValue);
        if (targetType == typeof(IntPtr))
            return napiValue;
        if (targetType.IsEnum)
            return Enum.ToObject(targetType, NativeValue.ToInt(napiValue));

        return napiValue;
    }
}
