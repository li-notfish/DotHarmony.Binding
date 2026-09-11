using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// 预编译的 napi 回调 trampoline
/// 用于将 C# 委托包装为 napi_function。
/// 通用反射后备路径：生成的类型化跳板（每个事件一个）是主路径，此实现兜底。
/// </summary>
internal static class NativeCallbacks
{
    private const int MaxArgs = 16;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    internal static IntPtr Action_Ptr(IntPtr env, IntPtr info)
    {
        // napi 回调内抛出的未捕获异常会击穿 VM，必须整体兜底
        try
        {
            var argc = (IntPtr)MaxArgs;
            Span<IntPtr> argv = stackalloc IntPtr[MaxArgs];
            NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
                .ThrowIfFailed();

            if (data == IntPtr.Zero)
            {
                NativeNodeApi.napi_get_undefined(env, out var undefinedZero).ThrowIfFailed();
                return undefinedZero;
            }

            var gch = GCHandle.FromIntPtr(data);
            var del = (Delegate)gch.Target!;
            var method = del.Method;
            var parameters = method.GetParameters();

            var args = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length && i < (int)argc; i++)
            {
                args[i] = ConvertArg(argv[i], parameters[i].ParameterType);
            }

            var result = method.Invoke(del.Target, args);

            if (method.ReturnType == typeof(void))
            {
                NativeNodeApi.napi_get_undefined(env, out var undefined).ThrowIfFailed();
                return undefined;
            }
            else
            {
                return NativeValue.From(result);
            }
        }
        catch
        {
            if (NativeNodeApi.napi_get_undefined(env, out var undefined) == NativeNodeApi.napi_status.napi_ok)
                return undefined;
            return IntPtr.Zero;
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
