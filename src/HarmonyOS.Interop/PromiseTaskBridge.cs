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

    private sealed class State
    {
        public required Func<object?, bool> TrySetResult;
        public required Func<Exception, bool> TrySetException;
        public required Func<bool> TrySetCanceled;
        public Type InnerType = typeof(object);
        public Func<IntPtr, object?>? Convert;
        public CancellationTokenRegistration Registration;
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

        var gch = GCHandle.Alloc(state);
        var data = GCHandle.ToIntPtr(gch);
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
                    static stateObject => CancelState(stateObject),
                    state);
            }

            return tcs.Task;
        }
        catch
        {
            if (!bridgeAttached)
            {
                gch.Free();
            }

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

    private static void CancelState(object? stateObject)
    {
        var state = (State)stateObject!;
        state.TrySetCanceled();
        state.Registration.Dispose();
    }

#if HARMONYOS
    private static State TakeState(IntPtr env, IntPtr info, out IntPtr firstArg, out GCHandle gch)
    {
        var argc = (IntPtr)4;
        Span<IntPtr> argv = stackalloc IntPtr[4];
        NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
            .ThrowIfFailed();

        firstArg = argc > 0 ? argv[0] : IntPtr.Zero;
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
            var value = state.Convert != null
                ? state.Convert(firstArg)
                : ValueConverter.ConvertTo(state.InnerType, firstArg);
            state.TrySetResult(value);
        }
        catch (Exception ex)
        {
            if (gch.IsAllocated && gch.Target is State state)
            {
                state.TrySetException(ex);
            }
        }
        finally
        {
            if (gch.IsAllocated)
            {
                gch.Free();
            }
        }

        return CallbackTrampolines.SafeReturnUndefined(env);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr RejectedTrampoline(IntPtr env, IntPtr info)
    {
        GCHandle gch = default;
        try
        {
            var state = TakeState(env, info, out var reasonArg, out gch);
            var (code, message) = BusinessErrorReader.Read(env, reasonArg);
            state.TrySetException(new ArkTSException(
                $"ArkTS promise rejected (code {code?.ToString() ?? "unknown"}): {message ?? "unknown"}",
                message,
                reasonArg,
                code));
        }
        catch (Exception ex)
        {
            if (gch.IsAllocated && gch.Target is State state)
            {
                state.TrySetException(ex);
            }
        }
        finally
        {
            if (gch.IsAllocated)
            {
                gch.Free();
            }
        }

        return CallbackTrampolines.SafeReturnUndefined(env);
    }
#endif
}
