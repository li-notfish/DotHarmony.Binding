#if HARMONYOS
using System;
using System.Collections.Concurrent;

namespace HarmonyOS.Interop;

/// <summary>
/// 任意线程 → JS/UI 线程的通用派发器（HarmonySynchronizationContext 的 TSFN 实质化）。
/// 宿主在 UI 线程初始化时调用 <see cref="AttachUiThread"/> 创建单例 TSFN；
/// 之后任意线程 <see cref="Post"/> 的 Action 在 JS 线程执行（napi env 可用）。
/// 全部异常被捕获并写 hilog——不得穿透原生回调帧。
/// </summary>
public static class MainThreadDispatcher
{
    private static readonly ConcurrentQueue<Action> Queue = new();
    private static readonly object Sync = new();
    private static ThreadSafeFunction? _tsfn;

    /// <summary>在 JS/UI 线程安装派发器（幂等；Host.InitializeCore 负责调用）</summary>
    public static void AttachUiThread()
    {
        lock (Sync)
        {
            if (_tsfn != null)
                return;
            var tsfn = ThreadSafeFunction.Create();
            tsfn.OnCallJs = static _ => DrainQueue();
            _tsfn = tsfn;
            // 挂接完成即冲刷停靠的动作（Attach 前 Post 的积压）
            if (!Queue.IsEmpty)
                tsfn.Call(IntPtr.Zero);
        }
    }

    /// <summary>是否已挂接 UI 线程</summary>
    public static bool IsAttached
    {
        get
        {
            lock (Sync) return _tsfn != null;
        }
    }

    /// <summary>排队到 JS/UI 线程执行。未挂接时仅入队，AttachUiThread 之后首个 Post 触发冲刷</summary>
    public static void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Queue.Enqueue(action);
        ThreadSafeFunction? tsfn;
        lock (Sync) tsfn = _tsfn;
        tsfn?.Call(IntPtr.Zero);
    }

    private static void DrainQueue()
    {
        while (Queue.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                HiLog.Error("MainThread", $"posted action failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
#endif
