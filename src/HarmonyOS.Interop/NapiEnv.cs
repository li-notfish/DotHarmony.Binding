#if HARMONYOS
using System;

namespace HarmonyOS.Interop;

/// <summary>
/// napi_env 生命周期管理。
/// libnapi.so 不提供任何获取 env 的导出：env 只能由宿主在 napi 模块 init 回调（或线程回调）中拿到。
/// 因此入口必须在启动时调用 <see cref="Initialize"/> 注入主线程 env；
/// 其他线程不得直接使用 napi，必须经 ThreadSafeFunction 回到拥有 env 的线程。
/// </summary>
internal static class NapiEnv
{
    [ThreadStatic]
    private static IntPtr _currentEnv;

    /// <summary>
    /// 由宿主入口（napi_module_register init / C shim 引导）调用，绑定当前线程的 env。
    /// </summary>
    internal static void Initialize(IntPtr env)
    {
        if (env == IntPtr.Zero)
            throw new ArgumentException("napi_env must not be zero", nameof(env));

        // 同一线程重复注入是正常入口（HarmonyInit 与 HarmonyBuildUI 各注入一次）。
        // 但 env 变了说明本线程的 napi 上下文被换过：此前缓存的 napi_ref / 裸 napi_value
        // 都绑定在旧 env 上，继续用会得到跨 env 的未定义行为。以前这种替换完全静默，
        // 至少留一条日志，让"为什么句柄突然不好使"可查。
        if (_currentEnv != IntPtr.Zero && _currentEnv != env)
        {
            HiLog.Error(
                "NapiEnv",
                $"env replaced on this thread: {_currentEnv:X} -> {env:X}; references created under the old env are no longer safe");
        }

        _currentEnv = env;
    }

    /// <summary>
    /// 获取当前线程的 napi_env；未注入时显式失败，绝不返回垃圾指针。
    /// </summary>
    internal static IntPtr Current
    {
        get
        {
            if (_currentEnv == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "napi_env has not been initialized on this thread. " +
                    "The host entry must call NapiEnv.Initialize before any NAPI call; " +
                    "background threads must marshal through a ThreadSafeFunction instead.");
            }

            return _currentEnv;
        }
    }

    /// <summary>当前线程是否已绑定 env</summary>
    internal static bool IsAvailable => _currentEnv != IntPtr.Zero;

    /// <summary>
    /// 重置当前线程的 env（测试或线程结束时）
    /// </summary>
    internal static void Reset()
    {
        _currentEnv = IntPtr.Zero;
    }
}
#endif
