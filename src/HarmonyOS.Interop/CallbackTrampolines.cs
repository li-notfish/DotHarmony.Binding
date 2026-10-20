using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace HarmonyOS.Interop;

/// <summary>
/// JS → C# 回调的单一共享跳板：把 argv 读成 IntPtr[] 后调用 Action&lt;IntPtr[]&gt;。
/// 任意参数个数/类型通用——类型化转换由生成器在适配器闭包中完成，无需按形状生成跳板。
/// </summary>
internal static class CallbackTrampolines
{
    internal static readonly IntPtr ArgsTrampolinePtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&ArgsTrampoline;

    internal static readonly IntPtr ValueTrampolinePtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&ValueTrampoline;

    private const int MaxArgs = 16;

#if HARMONYOS
    /// <summary>napi_add_finalizer 回调：JS 函数被 GC 时释放托管回调的 GCHandle。</summary>
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    internal static void FreeGCHandle(IntPtr env, IntPtr data, IntPtr hint)
    {
        if (data != IntPtr.Zero)
            GCHandle.FromIntPtr(data).Free();
    }
#endif

    internal static IntPtr SafeReturnUndefined(IntPtr env)
    {
        try
        {
            NativeNodeApi.napi_get_and_clear_last_exception(env, out _);
            NativeNodeApi.napi_get_undefined(env, out var undefined).ThrowIfFailed();
            return undefined;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyHost", $"[callback] safe return failed: {ex.GetType().Name}: {ex.Message}");
            return IntPtr.Zero;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr ArgsTrampoline(IntPtr env, IntPtr info)
    {
        Span<IntPtr> argv = stackalloc IntPtr[MaxArgs];
        int count;
        IntPtr[]? wideArgs = null;
        try
        {
            var argc = (IntPtr)MaxArgs;
            NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
                .ThrowIfFailed();
            count = (int)argc;
            if (count > MaxArgs)
            {
                wideArgs = new IntPtr[count];
                var wideArgc = (IntPtr)count;
                NativeNodeApi.napi_get_cb_info(env, info, ref wideArgc, wideArgs, out _, out _)
                    .ThrowIfFailed();
                count = (int)wideArgc;
            }

            var target = GCHandle.FromIntPtr(data).Target;
            if (target is Action<ReadOnlySpan<IntPtr>> spanAdapted)
            {
                spanAdapted(wideArgs is null ? argv.Slice(0, count) : wideArgs.AsSpan(0, count));
            }
            else if (target is Action<IntPtr[]> arrayAdapted)
            {
                if (wideArgs is not null)
                {
                    arrayAdapted(wideArgs);
                }
                else
                {
                    var args = new IntPtr[count];
                    argv.Slice(0, count).CopyTo(args);
                    arrayAdapted(args);
                }
            }
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyHost", $"[callback] handler threw: {ex.GetType().Name}: {ex.Message}");
        }
        return SafeReturnUndefined(env);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr ValueTrampoline(IntPtr env, IntPtr info)
    {
        Span<IntPtr> argv = stackalloc IntPtr[MaxArgs];
        int count;
        IntPtr[]? wideArgs = null;
        try
        {
            var argc = (IntPtr)MaxArgs;
            NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data)
                .ThrowIfFailed();
            count = (int)argc;
            if (count > MaxArgs)
            {
                wideArgs = new IntPtr[count];
                var wideArgc = (IntPtr)count;
                NativeNodeApi.napi_get_cb_info(env, info, ref wideArgc, wideArgs, out _, out _)
                    .ThrowIfFailed();
                count = (int)wideArgc;
            }

            var target = GCHandle.FromIntPtr(data).Target;
            object? result = null;
            if (target is Func<ReadOnlySpan<IntPtr>, object?> spanAdapted)
            {
                result = spanAdapted(wideArgs is null ? argv.Slice(0, count) : wideArgs.AsSpan(0, count));
            }
            else if (target is Func<IntPtr[], object?> arrayAdapted)
            {
                if (wideArgs is not null)
                {
                    result = arrayAdapted(wideArgs);
                }
                else
                {
                    var args = new IntPtr[count];
                    argv.Slice(0, count).CopyTo(args);
                    result = arrayAdapted(args);
                }
            }

            return NativeValue.From(result);
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyHost", $"[callback] handler threw: {ex.GetType().Name}: {ex.Message}");
        }
        return SafeReturnUndefined(env);
    }
}
