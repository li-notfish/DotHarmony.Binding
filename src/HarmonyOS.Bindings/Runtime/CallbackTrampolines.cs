using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// JS → C# 回调的单一共享跳板：把 argv 读成 IntPtr[] 后调用 Action&lt;IntPtr[]&gt;。
/// 任意参数个数/类型通用——类型化转换由生成器在适配器闭包中完成，无需按形状生成跳板。
/// </summary>
internal static class CallbackTrampolines
{
    internal static readonly IntPtr ArgsTrampolinePtr =
        (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&ArgsTrampoline;

    private const int MaxArgs = 16;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr ArgsTrampoline(IntPtr env, IntPtr info)
    {
        try
        {
            var argc = (IntPtr)MaxArgs;
            Span<IntPtr> argv = stackalloc IntPtr[MaxArgs];
            NativeNodeApi.napi_get_cb_info(env, info, ref argc, argv, out _, out var data);
            var count = (int)argc;
            if (count > MaxArgs) count = MaxArgs;
            var args = new IntPtr[count];
            for (int i = 0; i < count; i++)
                args[i] = argv[i];
            if (GCHandle.FromIntPtr(data).Target is Action<IntPtr[]> adapted)
                adapted(args);
        }
        catch (Exception ex)
        {
            // 事件回调中的用户异常不得泄漏回 JS 线程（会 terminate 应用），记录后吞掉
            HiLog.Error("HarmonyHost", $"[callback] handler threw: {ex.Message}");
        }
        NativeNodeApi.napi_get_undefined(env, out var undefined);
        return undefined;
    }
}
