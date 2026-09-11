// Promise → Task 桥（ROADMAP 2.1 的最小切片）。
// 前提：被调 Promise 在 JS 线程 resolve/reject，fulfilled/rejected 回调经
// napi_create_function 的原生 trampoline 进入 C#（复用 NativeCallbacks 的
// GCHandle-data 模式）。Task 续体经 TaskCompletionSource
// (RunContinuationsAsynchronously) + ContinueWith 调度到线程池，
// 避免用户续体阻塞 JS 线程。
// 任意线程 → JS 线程的回调式 API（AsyncCallback 风格）仍需完整 TSFN 通道，后续扩展。
#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HarmonyOS.Bindings.Runtime;

internal static class PromiseTaskBridge
{
    private sealed class State
    {
        public readonly TaskCompletionSource<object?> Tcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Type InnerType = typeof(object);
        /// <summary>调用点显式转换委托（数组/JsObject 包装类）；null 时走 ValueConverter 基元路径。</summary>
        public Func<IntPtr, object?>? Convert;
        public bool Done;
    }

    private static readonly IntPtr FulfilledPtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&FulfilledTrampoline;
    private static readonly IntPtr RejectedPtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&RejectedTrampoline;

    /// <summary>
    /// 将 napi promise 值接为 Task&lt;T&gt;。非 promise 值由调用方先行处理。
    /// <paramref name="convert"/> 为复杂结果类型（数组/JsObject 包装类）的显式转换委托；
    /// 基元与枚举结果不需要（走 <see cref="ValueConverter"/>）。
    /// </summary>
    public static Task<T> ToTask<T>(IntPtr promise, Func<IntPtr, T>? convert = null)
    {
#if HARMONYOS
        var state = new State { InnerType = typeof(T) };
        if (convert != null)
        {
            var conv = convert;
            state.Convert = v => conv(v)!;
        }
        var gch = GCHandle.Alloc(state);
        var data = GCHandle.ToIntPtr(gch);

        var env = NapiEnv.Current;
        var nameBytes = "then"u8.ToArray();

        // promise.then(fulfilled, rejected)
        NativeNodeApi.napi_get_named_property(env, promise, nameBytes, out var thenFn).ThrowIfFailed();

        var fulfilledName = "fulfilled"u8.ToArray();
        var rejectedName = "rejected"u8.ToArray();
        NativeNodeApi.napi_create_function(env, fulfilledName, (IntPtr)fulfilledName.Length,
            FulfilledPtr, data, out var fulfilledFn).ThrowIfFailed();
        NativeNodeApi.napi_create_function(env, rejectedName, (IntPtr)rejectedName.Length,
            RejectedPtr, data, out var rejectedFn).ThrowIfFailed();

        var status = NativeNodeApi.napi_call_function(env, promise, thenFn,
            2, new IntPtr[] { fulfilledFn, rejectedFn }, out _);
        // M0 铁律：先清除挂起异常，再检查状态
        NativeNodeApi.napi_get_and_clear_last_exception(env, out _).ThrowIfFailed();
        status.ThrowIfFailed();

        return state.Tcs.Task.ContinueWith(t => (T)t.Result!, TaskScheduler.Default);
#else
        throw new PlatformNotSupportedException("PromiseTaskBridge requires HarmonyOS runtime");
#endif
    }

#if HARMONYOS
    private static State TakeState(IntPtr env, IntPtr info, out IntPtr firstArg, out GCHandle gch)
    {
        var argc = (IntPtr)4;
        var argv = new IntPtr[4];
        NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
            .ThrowIfFailed();
        firstArg = argv[0];
        gch = GCHandle.FromIntPtr(data);
        return (State)gch.Target!;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr FulfilledTrampoline(IntPtr env, IntPtr info)
    {
        GCHandle gch = default;
        try
        {
            var state = TakeState(env, info, out var firstArg, out gch);
            if (state.Done)
            {
                NativeNodeApi.napi_get_undefined(env, out var undefined).ThrowIfFailed();
                return undefined;
            }
            state.Done = true;
            object? value = state.Convert != null
                ? state.Convert(firstArg)
                : ValueConverter.ConvertTo(state.InnerType, firstArg);
            state.Tcs.TrySetResult(value);
        }
        catch (Exception ex)
        {
            if (gch.IsAllocated && gch.Target is State s && !s.Done)
            {
                s.Done = true;
                s.Tcs.TrySetException(ex);
            }
        }
        finally
        {
            if (gch.IsAllocated) gch.Free();
        }
        NativeNodeApi.napi_get_undefined(env, out var undefinedRet).ThrowIfFailed();
        return undefinedRet;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr RejectedTrampoline(IntPtr env, IntPtr info)
    {
        GCHandle gch = default;
        try
        {
            var state = TakeState(env, info, out var reasonArg, out gch);
            if (state.Done)
            {
                NativeNodeApi.napi_get_undefined(env, out var undefined).ThrowIfFailed();
                return undefined;
            }
            state.Done = true;
            string? reason = null;
            try { reason = NativeValue.ToString(reasonArg); }
            catch { /* reason 可能不是字符串，忽略转换失败 */ }
            state.Tcs.TrySetException(new ArkTSException($"ArkTS promise rejected: {reason ?? "unknown"}", reason, reasonArg));
        }
        catch (Exception ex)
        {
            if (gch.IsAllocated && gch.Target is State s && !s.Done)
            {
                s.Done = true;
                s.Tcs.TrySetException(ex);
            }
        }
        finally
        {
            if (gch.IsAllocated) gch.Free();
        }
        NativeNodeApi.napi_get_undefined(env, out var undefinedRet).ThrowIfFailed();
        return undefinedRet;
    }
#endif
}
