#if HARMONYOS
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// napi_env 生命周期管理
/// napi_env 是线程局部的，每次调用都需要获取当前线程的 env
/// </summary>
internal static class NapiEnv
{
    [ThreadStatic]
    private static IntPtr _currentEnv;

    /// <summary>
    /// 获取当前线程的 napi_env
    /// </summary>
    internal static IntPtr Current
    {
        get
        {
            if (_currentEnv == IntPtr.Zero)
                _currentEnv = NativeGetEnv();
            return _currentEnv;
        }
    }

    /// <summary>
    /// 重置当前线程的 env（用于测试或线程结束时）
    /// </summary>
    internal static void Reset()
    {
        _currentEnv = IntPtr.Zero;
    }

    [DllImport("libnapi.so")]
    private static extern IntPtr NativeGetEnv();
}
#endif
