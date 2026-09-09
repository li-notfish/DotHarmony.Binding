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
        public bool Done;
    }

    private static readonly IntPtr FulfilledPtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&FulfilledTrampoline;
    private static readonly IntPtr RejectedPtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&RejectedTrampoline;

    /// <summary>将 napi promise 值接为 Task&lt;T&gt;。非 promise 值由调用方先行处理。</summary>
    public static Task<T> ToTask<T>(IntPtr promise)
    {
#if HARMONYOS
        var state = new State { InnerType = typeof(T) };
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

        NativeNodeApi.napi_call_function(env, promise, thenFn,
            2, new IntPtr[] { fulfilledFn, rejectedFn }, out _).ThrowIfFailed();

        // then 调用本身可能抛 ArkTS 异常（M0 铁律：必须清除挂起异常）
        NativeNodeApi.napi_get_and_clear_last_exception(env, out _).ThrowIfFailed();

        return state.Tcs.Task.ContinueWith(t => (T)t.Result!, TaskScheduler.Default);
#else
        throw new PlatformNotSupportedException("PromiseTaskBridge requires HarmonyOS runtime");
#endif
    }

#if HARMONYOS
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr FulfilledTrampoline(IntPtr env, IntPtr info)
    {
        // napi 回调内抛出的未捕获异常会击穿 VM，必须整体兜底
        try
        {
            var state = TakeState(env, info, out var firstArg);
            var inner = state.InnerType;
            object? value = inner == typeof(string) ? (object?)NativeValue.ToString(firstArg)
                : inner == typeof(double) ? NativeValue.ToDouble(firstArg)
                : inner == typeof(bool) ? NativeValue.ToBool(firstArg)
                : inner == typeof(int) ? (int)NativeValue.ToDouble(firstArg)
                : firstArg; // IntPtr 句柄
            state.Tcs.TrySetResult(value);
        }
        catch (Exception ex)
        {
            FailPendingState(env, info, ex);
        }
        NativeNodeApi.napi_get_undefined(env, out var undefined).ThrowIfFailed();
        return undefined;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr RejectedTrampoline(IntPtr env, IntPtr info)
    {
        try
        {
            _ = TakeState(env, info, out var reasonArg);
            var reason = NativeValue.ToString(reasonArg);
            FailAllPending(env, info, new InvalidOperationException($"ArkTS promise rejected: {reason}"));
        }
        catch (Exception ex)
        {
            FailAllPending(env, info, ex);
        }
        NativeNodeApi.napi_get_undefined(env, out var undefined).ThrowIfFailed();
        return undefined;
    }

    /// <summary>从回调取出共享状态（一次性，Done 防双触发双释放）</summary>
    private static State TakeState(IntPtr env, IntPtr info, out IntPtr firstArg)
    {
        var argc = (IntPtr)4;
        var argv = new IntPtr[4];
        NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
            .ThrowIfFailed();
        firstArg = argv[0];
        var gch = GCHandle.FromIntPtr(data);
        var state = (State)gch.Target!;
        if (!state.Done)
        {
            state.Done = true;
            gch.Free();
        }
        return state;
    }

    private static void FailPendingState(IntPtr env, IntPtr info, Exception ex)
    {
        try { FailAllPending(env, info, ex); } catch { /* 兜底，不再外抛 */ }
    }

    private static void FailAllPending(IntPtr env, IntPtr info, Exception ex)
    {
        try
        {
            var argc = (IntPtr)4;
            var argv = new IntPtr[4];
            NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
                .ThrowIfFailed();
            var gch = GCHandle.FromIntPtr(data);
            if (gch.Target is State state)
            {
                if (!state.Done)
                {
                    state.Done = true;
                    gch.Free();
                }
                state.Tcs.TrySetException(ex);
            }
        }
        catch { /* 状态已释放则忽略 */ }
    }
#endif
}
