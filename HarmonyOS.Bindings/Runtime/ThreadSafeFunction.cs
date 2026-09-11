// Thread-Safe Function (TSFN) 封装（ROADMAP 2.1 完整通道）。
// 允许任意线程安全地调用 JS 线程上的函数，用于 Promise/AsyncCallback 的 C#→JS 桥接。
// 生命周期三路径：完成（release）、取消（abort）、异常（abort + TCS.SetException）。
#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// 线程安全函数封装，用于任意线程→JS 线程的安全调用。
/// </summary>
internal sealed class ThreadSafeFunction : IDisposable
{
    private IntPtr _tsfnHandle;
    private GCHandle _gch;
    private bool _disposed;

    private ThreadSafeFunction() { }

    /// <summary>
    /// 创建线程安全函数，返回 Task&lt;T&gt; 用于等待 JS 回调结果
    /// </summary>
    public static ThreadSafeFunction Create()
    {
        var tsfn = new ThreadSafeFunction();
        tsfn._gch = GCHandle.Alloc(tsfn);

        var env = NapiEnv.Current;
        var resourceName = "ThreadSafeFunction"u8.ToArray();

        NativeNodeApi.napi_create_threadsafe_function(
            env,
            func: default,
            async_resource: default,
            async_resource_name: resourceName,
            max_queue_size: (IntPtr)0,
            initial_thread_count: (IntPtr)1,
            thread_finalize_data: IntPtr.Zero,
            thread_finalize_callback: IntPtr.Zero,
            context: GCHandle.ToIntPtr(tsfn._gch),
            call_js: GetCallJsTrampoline(),
            out var tsfnHandle).ThrowIfFailed();

        tsfn._tsfnHandle = tsfnHandle;
        return tsfn;
    }

    /// <summary>
    /// 从 Promise 创建 TSFN，返回 Task&lt;T&gt;
    /// </summary>
    public static Task<T> FromPromise<T>(IntPtr promise)
    {
        var tcs = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        // 注册 Promise 回调（在 JS 线程）
        RegisterPromiseCallbacks(promise, tcs, typeof(T));

        return tcs.Task.ContinueWith(t => (T)t.Result!, TaskScheduler.Default);
    }

    // JS 线程回调（CallJsTrampoline 在 JS 线程上触发）
    internal Action<IntPtr>? OnCallJs;

    /// <summary>
    /// 从任意线程调用（data 原样穿透到 JS 线程的 OnCallJs）
    /// </summary>
    public void Call(IntPtr data)
    {
        ThrowIfDisposed();
        NativeNodeApi.napi_call_threadsafe_function(_tsfnHandle, data,
            NativeNodeApi.napi_threadsafe_function_call_mode.napi_tsfn_nonblocking).ThrowIfFailed();
    }

    /// <summary>
    /// 正常释放 TSFN（完成路径）
    /// </summary>
    public void Release()
    {
        if (_disposed) return;
        _disposed = true;

        if (_tsfnHandle != IntPtr.Zero)
        {
            NativeNodeApi.napi_release_threadsafe_function(
                _tsfnHandle,
                NativeNodeApi.napi_threadsafe_function_release_mode.napi_tsfn_release);
            _tsfnHandle = IntPtr.Zero;
        }

        if (_gch.IsAllocated)
            _gch.Free();
    }

    /// <summary>
    /// 中止 TSFN（取消路径）
    /// </summary>
    public void Abort()
    {
        if (_disposed) return;
        _disposed = true;

        if (_tsfnHandle != IntPtr.Zero)
        {
            NativeNodeApi.napi_release_threadsafe_function(
                _tsfnHandle,
                NativeNodeApi.napi_threadsafe_function_release_mode.napi_tsfn_abort);
            _tsfnHandle = IntPtr.Zero;
        }

        if (_gch.IsAllocated)
            _gch.Free();
    }

    public void Dispose() => Release();

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ThreadSafeFunction));
    }

    #region Promise Callbacks

    private static unsafe void RegisterPromiseCallbacks(
        IntPtr promise,
        TaskCompletionSource<object?> tcs,
        Type innerType)
    {
        var env = NapiEnv.Current;

        // 用 GCHandle 保存状态
        var state = new PromiseState { Tcs = tcs, InnerType = innerType };
        var gch = GCHandle.Alloc(state);

        var nameBytes = "then"u8.ToArray();
        NativeNodeApi.napi_get_named_property(env, promise, nameBytes, out var thenFn).ThrowIfFailed();

        var fulfilledName = "fulfilled"u8.ToArray();
        var rejectedName = "rejected"u8.ToArray();

        NativeNodeApi.napi_create_function(env, fulfilledName, (IntPtr)fulfilledName.Length,
            GetFulfilledTrampoline(), GCHandle.ToIntPtr(gch), out var fulfilledFn).ThrowIfFailed();
        NativeNodeApi.napi_create_function(env, rejectedName, (IntPtr)rejectedName.Length,
            GetRejectedTrampoline(), GCHandle.ToIntPtr(gch), out var rejectedFn).ThrowIfFailed();

        NativeNodeApi.napi_call_function(env, promise, thenFn,
            2, [fulfilledFn, rejectedFn], out _).ThrowIfFailed();

        // M0 铁律：清除挂起异常
        NativeNodeApi.napi_get_and_clear_last_exception(env, out _).ThrowIfFailed();
    }

    private sealed class PromiseState
    {
        public TaskCompletionSource<object?> Tcs { get; init; } = null!;
        public Type InnerType { get; init; } = null!;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr FulfilledTrampoline(IntPtr env, IntPtr info)
    {
        try
        {
            var state = TakePromiseState(env, info, out var firstArg);
            var inner = state.InnerType;
            object? value = inner == typeof(string) ? (object?)NativeValue.ToString(firstArg)
                : inner == typeof(double) ? NativeValue.ToDouble(firstArg)
                : inner == typeof(bool) ? NativeValue.ToBool(firstArg)
                : inner == typeof(int) ? (int)NativeValue.ToDouble(firstArg)
                : firstArg;
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
            _ = TakePromiseState(env, info, out var reasonArg);
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

    private static PromiseState TakePromiseState(IntPtr env, IntPtr info, out IntPtr firstArg)
    {
        var argc = (IntPtr)4;
        var argv = new IntPtr[4];
        NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
            .ThrowIfFailed();
        firstArg = argv[0];
        var gch = GCHandle.FromIntPtr(data);
        return (PromiseState)gch.Target!;
    }

    private static void FailPendingState(IntPtr env, IntPtr info, Exception ex)
    {
        try { FailAllPending(env, info, ex); } catch { }
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
            if (gch.Target is PromiseState state)
            {
                state.Tcs.TrySetException(ex);
            }
        }
        catch { }
    }

    private static unsafe IntPtr GetFulfilledTrampoline()
    {
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr> ptr = &FulfilledTrampoline;
        return (IntPtr)ptr;
    }

    private static unsafe IntPtr GetRejectedTrampoline()
    {
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr> ptr = &RejectedTrampoline;
        return (IntPtr)ptr;
    }

    private static unsafe IntPtr GetCallJsTrampoline()
    {
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, IntPtr, void> ptr = &CallJsTrampoline;
        return (IntPtr)ptr;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void CallJsTrampoline(IntPtr env, IntPtr js_callback, IntPtr context, IntPtr data)
    {
        // TSFN 回调：libuv 在 JS 线程上触发。context = Create() 里存的 GCHandle<ThreadSafeFunction>。
        if (context == IntPtr.Zero)
            return;
        var handle = GCHandle.FromIntPtr(context);
        if (handle.Target is not ThreadSafeFunction tsfn)
            return;
        tsfn.OnCallJs?.Invoke(data);
    }

    #endregion
}
