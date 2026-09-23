#if HARMONYOS
using System;

namespace HarmonyOS.Interop;

/// <summary>
/// napi_ref 引用管理，防止 JS 对象被 GC 回收
/// </summary>
internal class NapiReference : IDisposable
{
    private IntPtr _ref;
    private bool _disposed;

    /// <summary>
    /// 创建对 napi_value 的引用
    /// </summary>
    public NapiReference(IntPtr napiValue)
    {
        if (napiValue == IntPtr.Zero)
            throw new ArgumentNullException(nameof(napiValue));

        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_reference(env, napiValue, 1, out var napiRef);
        _ref = napiRef;
    }

    /// <summary>
    /// 获取引用的 napi_value
    /// </summary>
    public IntPtr Value
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NapiReference));

            var env = NapiEnv.Current;
            NativeNodeApi.napi_get_reference_value(env, _ref, out var value);
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
        if (!_disposed)
        {
            if (disposing)
            {
                // 释放托管资源
            }

            if (_ref != IntPtr.Zero)
            {
                // napi_delete_reference 只允许在持有 napi_env 的 JS 线程调用
                // （env 为 ThreadStatic；napi_ref 绑定其创建时的 env 实例）。
                // 终结器线程上没有 env——直接落原生既会抛异常终止进程、也泄漏引用，
                // 改由队列暂存，待 JS 线程事件分发时批量回收（见 NapiFinalizationQueue）。
                if (NapiEnv.IsAvailable)
                {
                    NativeNodeApi.napi_delete_reference(NapiEnv.Current, _ref);
                }
                else
                {
                    NapiFinalizationQueue.Enqueue(_ref);
                }
                _ref = IntPtr.Zero;
            }

            _disposed = true;
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
