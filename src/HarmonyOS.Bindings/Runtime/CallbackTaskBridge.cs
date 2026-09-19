// AsyncCallback 风格 API → Task 桥（2.1 正式通道）。
// 很多 @ohos.* API 只有 callback 形式（getXxx(callback: AsyncCallback<T>)，无 Promise 重载）：
// C# 调用时经 napi_create_function 创建一个 err-first JS 回调作为末参传入；
// JS 在 JS 线程触发该回调时，trampoline 解析 (err, data)：
//   err 为 undefined/null → SetResult(convert(data))
//   否则（BusinessError）  → SetException(ArkTSException，读 err.code/err.message)
// TCS 同步续体（与 PromiseTaskBridge 一致）：await 续体在 JS 线程内联恢复，NapiEnv 可用。
// AsyncCallback 契约只触发一次；若 API 违约多次触发，Done 标记保证只完成一次
// （GCHandle 已释放，与 PromiseTaskBridge 相同的假设）。
#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HarmonyOS.Bindings.Runtime;

internal static class CallbackTaskBridge
{
    private sealed class State
    {
        /// <summary>非泛型承载：CreateCallback&lt;T&gt; 注入强类型 TCS 的完成委托。</summary>
        public required Action<object?> SetResult;
        public required Action<Exception> SetException;
        public Type InnerType = typeof(object);
        /// <summary>data 参数的显式转换委托（数组/JsObject 包装类）；null 时走 ValueConverter 基元路径。</summary>
        public Func<IntPtr, object?>? Convert;
        public bool Done;
    }

    private static readonly IntPtr TrampolinePtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&CallbackTrampoline;

    /// <summary>
    /// 创建 err-first 回调函数及其 Task。回调作为被调方法的最后一个实参传入。
    /// </summary>
    public static (IntPtr jsFunc, Task<T> task) CreateCallback<T>(Func<IntPtr, T>? convert)
    {
#if HARMONYOS
        var tcs = new TaskCompletionSource<T>();
        var state = new State
        {
            SetResult = v => tcs.TrySetResult((T)v!),
            SetException = ex => tcs.TrySetException(ex),
            InnerType = typeof(T),
        };
        if (convert != null)
        {
            var conv = convert;
            state.Convert = v => conv(v)!;
        }
        var gch = GCHandle.Alloc(state);
        var data = GCHandle.ToIntPtr(gch);

        var env = NapiEnv.Current;
        var nameBytes = "asyncCallback"u8.ToArray();
        NativeNodeApi.napi_create_function(env, nameBytes, (IntPtr)nameBytes.Length,
            TrampolinePtr, data, out var jsFunc).ThrowIfFailed();
        return (jsFunc, tcs.Task);
#else
        throw new PlatformNotSupportedException("CallbackTaskBridge requires HarmonyOS runtime");
#endif
    }

#if HARMONYOS
    private static IntPtr GetUndefined(IntPtr env)
    {
        NativeNodeApi.napi_get_undefined(env, out var undefined).ThrowIfFailed();
        return undefined;
    }

    private static long? ReadErrorInt64(IntPtr env, IntPtr err, byte[] key)
    {
        NativeNodeApi.napi_get_named_property(env, err, key, out var value);
        NativeNodeApi.napi_typeof(env, value, out var valueType);
        if (valueType != NativeNodeApi.napi_valuetype.napi_number) return null;
        NativeNodeApi.napi_get_value_int64(env, value, out var result);
        return result;
    }

    private static string? ReadErrorString(IntPtr env, IntPtr err, byte[] key)
    {
        NativeNodeApi.napi_get_named_property(env, err, key, out var value);
        NativeNodeApi.napi_typeof(env, value, out var valueType);
        if (valueType != NativeNodeApi.napi_valuetype.napi_string) return null;
        return NativeValue.ToString(value);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr CallbackTrampoline(IntPtr env, IntPtr info)
    {
        GCHandle gch = default;
        try
        {
            var argc = (IntPtr)2;
            Span<IntPtr> argv = stackalloc IntPtr[2];
            NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
                .ThrowIfFailed();
            gch = GCHandle.FromIntPtr(data);
            var state = (State)gch.Target!;
            if (state.Done)
                return GetUndefined(env);
            state.Done = true;

            var err = argc > 0 ? argv[0] : IntPtr.Zero;
            NativeNodeApi.napi_typeof(env, err, out var errType).ThrowIfFailed();
            if (errType == NativeNodeApi.napi_valuetype.napi_undefined || errType == NativeNodeApi.napi_valuetype.napi_null)
            {
                // 成功路径：AsyncCallback<T> 的 data 在第二位（void AsyncCallback 只有 err）
                var dataArg = argc > 1 ? argv[1] : GetUndefined(env);
                object? value = state.Convert != null
                    ? state.Convert(dataArg)
                    : ValueConverter.ConvertTo(state.InnerType, dataArg);
                state.SetResult(value);
            }
            else
            {
                // BusinessError：{ code: number, message: string }
                long? code = null;
                string? message = null;
                if (errType == NativeNodeApi.napi_valuetype.napi_object)
                {
                    try { code = ReadErrorInt64(env, err, "code"u8.ToArray()); } catch { }
                    try { message = ReadErrorString(env, err, "message"u8.ToArray()); } catch { }
                }
                else
                {
                    try { message = NativeValue.ToString(err); } catch { }
                }
                state.SetException(new ArkTSException(
                    $"ArkTS callback error (code {code?.ToString() ?? "unknown"}): {message ?? "unknown"}",
                    message, err));
            }
        }
        catch (Exception ex)
        {
            if (gch.IsAllocated && gch.Target is State s && !s.Done)
            {
                s.Done = true;
                s.SetException(ex);
            }
        }
        finally
        {
            if (gch.IsAllocated) gch.Free();
        }
        return GetUndefined(env);
    }
#endif
}
