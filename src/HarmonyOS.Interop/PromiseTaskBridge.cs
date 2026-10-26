#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace HarmonyOS.Interop;

internal static class PromiseTaskBridge
{
    private static readonly byte[] ThenUtf8 = "then"u8.ToArray();
    private static readonly byte[] FulfilledUtf8 = "fulfilled"u8.ToArray();
    private static readonly byte[] RejectedUtf8 = "rejected"u8.ToArray();

    /// <summary>
    /// 桥状态。两个托管资源必须在三条路径（完成/拒绝/取消）上都释放且只释放一次：
    /// <see cref="BridgeSlotRegistry"/> 的 token（JS 侧函数携带它的 IntPtr）与
    /// CancellationTokenRegistration（挂在调用方 token 上，不释放会随调用次数线性累积）。
    /// 与 <see cref="CallbackTaskBridge"/> 的 State 保持同一套幂等释放协议。
    /// </summary>
    private sealed class State
    {
        public required Func<object?, bool> TrySetResult;
        public required Func<Exception, bool> TrySetException;
        public required Func<bool> TrySetCanceled;
        public Type InnerType = typeof(object);
        public Func<IntPtr, object?>? Convert;
        public readonly object Lock = new();
        /// <summary>桥在 <see cref="BridgeSlotRegistry"/> 里的 token；释放后置 0。</summary>
        public IntPtr Slot;
        public bool Done;
        public CancellationTokenRegistration Registration;

        public bool TryMarkDone()
        {
            lock (Lock)
            {
                if (Done)
                    return false;
                Done = true;
                return true;
            }
        }

        public void DisposeRegistration()
        {
            var registration = Registration;
            Registration = default;
            registration.Dispose();
        }

        /// <summary>三条路径统一出口：幂等，重复调用无副作用。</summary>
        public void Release()
        {
            DisposeRegistration();

            // 注销 token：之后迟到的 settle 解析不到本对象，直接忽略。
            // token 不复用，所以不存在"读到别人的 State"。
            var slot = Slot;
            if (slot != IntPtr.Zero)
            {
                Slot = IntPtr.Zero;
                BridgeSlotRegistry.Unregister(slot);
            }
        }

        public void Cancel()
        {
            if (TryMarkDone())
                TrySetCanceled();
            // 与 CallbackTaskBridge.Cancel 同理：Dispose 由 token 回调线程执行，
            // 运行时会跳过对自身的等待，不会自锁。
            Release();
        }
    }

    private static readonly IntPtr FulfilledPtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&FulfilledTrampoline;
    private static readonly IntPtr RejectedPtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&RejectedTrampoline;

    public static Task<T> ToTask<T>(IntPtr promise, Func<IntPtr, T>? convert = null)
        => ToTask(promise, convert, CancellationToken.None);

    public static Task<T> ToTask<T>(
        IntPtr promise,
        Func<IntPtr, T>? convert,
        CancellationToken cancellationToken)
    {
#if HARMONYOS
        var tcs = new TaskCompletionSource<T>();
        var completion = new AtomicCompletion<T>(tcs);
        var state = new State
        {
            TrySetResult = value => completion.TrySetResult((T)value!),
            TrySetException = completion.TrySetException,
            TrySetCanceled = completion.TrySetCanceled,
            InnerType = typeof(T),
        };

        if (convert != null)
        {
            var conv = convert;
            state.Convert = value => conv(value)!;
        }

        var data = BridgeSlotRegistry.Register(state);
        state.Slot = data;
        var env = NapiEnv.Current;
        var bridgeAttached = false;

        try
        {
            NativeNodeApi.napi_get_named_property(env, promise, ThenUtf8, out var thenFn)
                .ThrowIfFailed();

            NativeNodeApi.napi_create_function(
                env,
                FulfilledUtf8,
                (IntPtr)FulfilledUtf8.Length,
                FulfilledPtr,
                data,
                out var fulfilledFn).ThrowIfFailed();
            NativeNodeApi.napi_create_function(
                env,
                RejectedUtf8,
                (IntPtr)RejectedUtf8.Length,
                RejectedPtr,
                data,
                out var rejectedFn).ThrowIfFailed();

            Span<IntPtr> argv = stackalloc IntPtr[2];
            argv[0] = fulfilledFn;
            argv[1] = rejectedFn;

            var status = NativeNodeApi.napi_call_function(
                env,
                promise,
                thenFn,
                2,
                argv,
                out _);
            if (status != napi_status.napi_ok)
            {
                var clearStatus = NativeNodeApi.napi_get_and_clear_last_exception(
                    env,
                    out var jsError);
                if (clearStatus == napi_status.napi_ok && jsError != IntPtr.Zero)
                {
                    var (code, message) = BusinessErrorReader.Read(env, jsError);
                    throw CreateRichNapiException(status, "promise.then", code, message);
                }

                throw new NapiException(status, "promise.then");
            }

            bridgeAttached = true;
            if (cancellationToken.CanBeCanceled)
            {
                state.Registration = cancellationToken.Register(
                    static stateObject => ((State)stateObject!).Cancel(),
                    state);
                // token 在注册前已取消时 Register 会同步触发回调（此时 Registration 仍是
                // default，Release 只会注销 token）；补一次显式 Cancel 保证注册项也被释放。
                if (cancellationToken.IsCancellationRequested)
                    state.Cancel();
            }

            return tcs.Task;
        }
        catch
        {
            // 桥没接上：trampoline 永不触发，句柄必须在此释放（含 Registration）。
            if (!bridgeAttached)
                state.Release();

            throw;
        }
#else
        throw new PlatformNotSupportedException("PromiseTaskBridge requires HarmonyOS runtime");
#endif
    }

    internal static NapiException CreateRichNapiException(
        napi_status status,
        string operation,
        long? errorCode,
        string? message)
    {
        var detail = errorCode is null
            ? message
            : string.IsNullOrEmpty(message)
                ? $"code {errorCode}"
                : $"code {errorCode}: {message}";

        return new NapiException(
            status,
            operation,
            errorCode,
            detail);
    }

#if HARMONYOS
    /// <summary>
    /// 从回调帧取回 State；句柄已释放（取消后迟到的 settle）时返回 null，
    /// 由调用方按「完成后忽略后续回调」处理，不得抛。
    /// </summary>
    private static State? TakeState(IntPtr env, IntPtr info, out IntPtr firstArg)
    {
        var argc = (IntPtr)4;
        Span<IntPtr> argv = stackalloc IntPtr[4];
        NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
            .ThrowIfFailed();

        firstArg = argc > 0 ? argv[0] : IntPtr.Zero;
        return BridgeSlotRegistry.Resolve(data) as State;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr FulfilledTrampoline(IntPtr env, IntPtr info)
    {
        State? state = null;
        try
        {
            // 本回调不向引擎返回业务值（只返回 undefined），期间创建的临时 napi_value
            // 在 scope 结束时统一回收。返回值在 scope 之外创建，见末尾——
            // 因此这里不能从 scope 内 return，否则返回的句柄会被一起关闭。
            using (NapiHandleScope.Open(env))
            {
                state = TakeState(env, info, out var firstArg);
                if (state is not null && state.TryMarkDone())
                {
                    var value = state.Convert != null
                        ? state.Convert(firstArg)
                        : ValueConverter.ConvertTo(state.InnerType, firstArg);
                    state.TrySetResult(value);
                }
            }
        }
        catch (Exception ex)
        {
            // 异常不得穿透原生帧；能定位到 State 就交给 Task，否则只能记日志
            if (state is null)
                HiLog.Error("PromiseBridge", $"fulfilled handler failed: {ex.GetType().Name}: {ex.Message}");
            else
                state.TrySetException(ex);
        }
        finally
        {
            state?.Release();
        }

        return CallbackTrampolines.SafeReturnUndefined(env);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr RejectedTrampoline(IntPtr env, IntPtr info)
    {
        State? state = null;
        try
        {
            using (NapiHandleScope.Open(env))
            {
                state = TakeState(env, info, out var reasonArg);
                if (state is not null && state.TryMarkDone())
                {
                    var (code, message) = BusinessErrorReader.Read(env, reasonArg);
                    state.TrySetException(new ArkTSException(
                        $"ArkTS promise rejected (code {code?.ToString() ?? "unknown"}): {message ?? "unknown"}",
                        message,
                        code));
                }
            }
        }
        catch (Exception ex)
        {
            if (state is null)
                HiLog.Error("PromiseBridge", $"rejected handler failed: {ex.GetType().Name}: {ex.Message}");
            else
                state.TrySetException(ex);
        }
        finally
        {
            state?.Release();
        }

        return CallbackTrampolines.SafeReturnUndefined(env);
    }
#endif
}
