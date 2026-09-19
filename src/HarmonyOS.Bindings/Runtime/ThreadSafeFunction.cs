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
    private readonly object _sync = new();

    private ThreadSafeFunction() { }

    /// <summary>
    /// 创建线程安全函数，返回 Task&lt;T&gt; 用于等待 JS 回调结果
    /// </summary>
    public static ThreadSafeFunction Create()
    {
        var tsfn = new ThreadSafeFunction();
        tsfn._gch = GCHandle.Alloc(tsfn);

        var env = NapiEnv.Current;
        ReadOnlySpan<byte> resourceNameBytes = "ThreadSafeFunction"u8;
        NativeNodeApi.napi_create_string_utf8(env, resourceNameBytes, (IntPtr)resourceNameBytes.Length, out var resourceName).ThrowIfFailed();

        // GCHandle 在 finalize 回调（TSFN 真正销毁、队列清空之后）释放，
        // 而不是 Release/Abort 时——abort 后已入队的消息仍会派发到 CallJsTrampoline，
        // 过早释放 GCHandle 会 UAF。
        NativeNodeApi.napi_create_threadsafe_function(
            env,
            func: default,
            async_resource: default,
            async_resource_name: resourceName,
            max_queue_size: (IntPtr)0,
            initial_thread_count: (IntPtr)1,
            thread_finalize_data: GCHandle.ToIntPtr(tsfn._gch),
            thread_finalize_callback: FinalizeTrampolinePtr,
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
        => PromiseTaskBridge.ToTask<T>(promise);

    // JS 线程回调（CallJsTrampoline 在 JS 线程上触发）
    internal Action<IntPtr>? OnCallJs;

    /// <summary>
    /// 从任意线程调用（data 原样穿透到 JS 线程的 OnCallJs）
    /// </summary>
    public void Call(IntPtr data)
    {
        // 与 Release/Abort 互斥：句柄在本机调用期间保持有效
        lock (_sync)
        {
            ThrowIfDisposed();
            NativeNodeApi.napi_call_threadsafe_function(_tsfnHandle, data,
                NativeNodeApi.napi_threadsafe_function_call_mode.napi_tsfn_nonblocking).ThrowIfFailed();
        }
    }

    /// <summary>
    /// 正常释放 TSFN（完成路径）
    /// </summary>
    public void Release()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            ReleaseHandle(NativeNodeApi.napi_threadsafe_function_release_mode.napi_tsfn_release);
        }
        // GCHandle 由 FinalizeTrampoline 释放（勿在此 Free——见 Create 注释）
    }

    /// <summary>
    /// 中止 TSFN（取消路径）
    /// </summary>
    public void Abort()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            ReleaseHandle(NativeNodeApi.napi_threadsafe_function_release_mode.napi_tsfn_abort);
        }
    }

    private void ReleaseHandle(NativeNodeApi.napi_threadsafe_function_release_mode mode)
    {
        if (_tsfnHandle != IntPtr.Zero)
        {
            NativeNodeApi.napi_release_threadsafe_function(_tsfnHandle, mode);
            _tsfnHandle = IntPtr.Zero;
        }
    }

    public void Dispose() => Release();

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ThreadSafeFunction));
    }

    // Promise → Task 的完整实现见 PromiseTaskBridge（唯一实现，勿在此复制）。

    private static unsafe IntPtr GetCallJsTrampoline()
    {
        delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, IntPtr, void> ptr = &CallJsTrampoline;
        return (IntPtr)ptr;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void CallJsTrampoline(IntPtr env, IntPtr js_callback, IntPtr context, IntPtr data)
    {
        // TSFN 回调：libuv 在 JS 线程上触发。context = Create() 里存的 GCHandle<ThreadSafeFunction>。
        // finalize（释放 GCHandle）必然晚于最后一次本回调，FromIntPtr 恒有效。
        try
        {
            if (context == IntPtr.Zero)
                return;
            var handle = GCHandle.FromIntPtr(context);
            if (handle.Target is not ThreadSafeFunction tsfn)
                return;
            tsfn.OnCallJs?.Invoke(data);
        }
        catch (Exception ex)
        {
            // 异常不得穿透回原生帧（会终止进程）
            HiLog.Error("TSFN", $"CallJs callback failed: {ex}");
        }
    }

    private static readonly IntPtr FinalizeTrampolinePtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void>)&FinalizeTrampoline;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void FinalizeTrampoline(IntPtr env, IntPtr finalizeData, IntPtr finalizeHint)
    {
        if (finalizeData != IntPtr.Zero)
            GCHandle.FromIntPtr(finalizeData).Free();
    }
}
