#if HARMONYOS
using System;

namespace HarmonyOS.Interop;

/// <summary>
/// 一次原生入口调用的 handle scope（RAII）。
///
/// 背景：napi_value 是所在 handle scope 内的句柄，入口（JS→C# 跳板、TSFN CallJs、
/// 模块加载）自行开 scope 才能保证临时值在本次调用结束后被回收，而不是堆到
/// 引擎为回调帧准备的外层 scope 上。不配对时引擎会返回 napi_handle_scope_mismatch。
///
/// 使用边界（重要）：**只能包住「不需要把 napi_value 带出本段」的代码**。
/// 返回值、要跨帧持有或跨 Post 传递的 napi_value 必须先在 scope 内转成
/// <see cref="NapiReference"/>，不能直接带出——scope 关闭后句柄即失效。
///
/// 用法：
/// <code>
/// using (NapiHandleScope.Open(env))
/// {
///     // 期间创建的临时 napi_value 在块结束时统一失效
/// }
/// </code>
/// </summary>
internal readonly ref struct NapiHandleScope : IDisposable
{
    private readonly IntPtr _env;
    private readonly IntPtr _scope;

    private NapiHandleScope(IntPtr env, IntPtr scope)
    {
        _env = env;
        _scope = scope;
    }

    /// <summary>在调用方的 env 上开启 scope；失败直接抛（此时不应继续执行）。</summary>
    public static NapiHandleScope Open(IntPtr env)
    {
        NativeNodeApi.napi_open_handle_scope(env, out var scope).ThrowIfFailed();
        return new NapiHandleScope(env, scope);
    }

    /// <summary>
    /// 关闭 scope。**不抛**：本方法常在 using 展开期间执行，此时抛出会顶掉调用方
    /// 真正关心的原始异常；scope 不配对的后果会以 napi_handle_scope_mismatch
    /// 在后续调用上暴露，日志足以定位。
    /// </summary>
    public void Dispose()
    {
        if (_scope == IntPtr.Zero)
            return;

        try
        {
            NativeNodeApi.napi_close_handle_scope(_env, _scope).ThrowIfFailed();
        }
        catch (Exception ex)
        {
            HiLog.Error("NapiScope", $"close handle scope failed: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
#endif
