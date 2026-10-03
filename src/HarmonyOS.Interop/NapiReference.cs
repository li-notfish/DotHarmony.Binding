#if HARMONYOS
using System;
using System.Threading;

namespace HarmonyOS.Interop;

/// <summary>
/// napi_ref 引用管理，防止 JS 对象被 GC 回收
/// </summary>
internal class NapiReference : IDisposable
{
    private const int Active = 0;
    private const int Disposing = 1;
    private const int Disposed = 2;

    private readonly IntPtr _env;
    private IntPtr _ref;
    private int _disposeState;

    /// <summary>
    /// 创建对 napi_value 的引用
    /// </summary>
    public NapiReference(IntPtr napiValue)
    {
        if (napiValue == IntPtr.Zero)
            throw new ArgumentNullException(nameof(napiValue));

        var env = NapiEnv.Current;
        _env = env;
        NativeNodeApi.napi_create_reference(env, napiValue, 1, out var napiRef).ThrowIfFailed();
        _ref = napiRef;
    }

    /// <summary>
    /// 获取引用的 napi_value
    /// </summary>
    public IntPtr Value
    {
        get
        {
            if (Volatile.Read(ref _disposeState) != Active)
                throw new ObjectDisposedException(nameof(NapiReference));

            NativeNodeApi.napi_get_reference_value(_env, _ref, out var value).ThrowIfFailed();
            return value;
        }
    }

    /// <summary>
    /// 获取底层 napi_ref 句柄
    /// </summary>
    public IntPtr Handle => _ref;

    /// <summary>
    /// 释放引用
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 受保护的释放实现
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposeState, Disposing) != Active)
            return;

        try
        {
            var reference = _ref;
            if (reference != IntPtr.Zero)
            {
                // napi_ref is bound to the env that created it. Direct deletion is
                // safe only when the current thread carries that same env.
                if (NapiEnv.IsAvailable && NapiEnv.Current == _env)
                {
                    try
                    {
                        NativeNodeApi.napi_delete_reference(_env, reference).ThrowIfFailed();
                    }
                    catch (Exception ex)
                    {
                        HiLog.Error("NapiReference", $"delete failed, deferred: {ex.GetType().Name}: {ex.Message}");
                        NapiFinalizationQueue.Enqueue(_env, reference);
                    }
                }
                else
                {
                    NapiFinalizationQueue.Enqueue(_env, reference);
                }

                _ref = IntPtr.Zero;
            }
        }
        finally
        {
            Volatile.Write(ref _disposeState, Disposed);
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~NapiReference()
    {
        Dispose(false);
    }
}
#endif
