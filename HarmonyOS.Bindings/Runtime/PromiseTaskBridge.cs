// Promise → Task 桥（ROADMAP 2.1 的最小切片）。
// 前提：被调 Promise 在 JS 线程 resolve/reject，fulfilled/rejected 回调经
// napi_create_function 的原生 trampoline 进入 C#（复用 NativeCallbacks 的
// GCHandle-data 模式）。
// TCS 不使用 RunContinuationsAsynchronously：Task 续体在 TrySetResult 内联到
// JS 线程执行（与 JS await 微任务语义一致），保证 await 之后的 NAPI 调用
// （wrapper 属性访问等）仍有 env 可用；若调度到线程池，NapiEnv.Current 会
// 因线程亲和性直接抛异常。代价：用户续体在 JS 线程上同步执行，长耗时工作
// 应自行切走（Task.Run）。
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
        /// <summary>非泛型承载：ToTask&lt;T&gt; 注入强类型 TCS 的完成委托与最终 Task。</summary>
        public required Task Task;
        public required Action<object?> SetResult;
        public required Action<Exception> SetException;
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
        // 同步续体：Promise resolve 发生在 JS 线程，await 续体在该线程内联恢复，
        // 使后续 NAPI 调用（wrapper 属性等）拥有 env。
        var tcs = new TaskCompletionSource<T>();
        var state = new State
        {
            Task = tcs.Task,
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

        return tcs.Task;
#else
        throw new PlatformNotSupportedException("PromiseTaskBridge requires HarmonyOS runtime");
#endif
    }

#if HARMONYOS
    private static State TakeState(IntPtr env, IntPtr info, out IntPtr firstArg, out GCHandle gch)
    {
        var argc = (IntPtr)4;
        Span<IntPtr> argv = stackalloc IntPtr[4];
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
            state.SetResult(value);
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
            state.SetException(new ArkTSException($"ArkTS promise rejected: {reason ?? "unknown"}", reason, reasonArg));
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
        NativeNodeApi.napi_get_undefined(env, out var undefinedRet).ThrowIfFailed();
        return undefinedRet;
    }
#endif
}
