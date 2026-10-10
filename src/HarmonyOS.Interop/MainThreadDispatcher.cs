#if HARMONYOS
using System;
using System.Collections.Generic;
using System.Threading;

namespace HarmonyOS.Interop;

/// <summary>
/// 任意线程 → JS/UI 线程的通用派发器（取代未启用的 HarmonySynchronizationContext）。
/// 宿主在 UI 线程初始化时调用 <see cref="AttachUiThread"/> 创建单例 TSFN；
/// 之后任意线程 <see cref="Post"/> 的 Action 在 JS 线程执行（napi env 可用）。
/// 全部异常被捕获并写 hilog——不得穿透原生回调帧。
/// </summary>
public static class MainThreadDispatcher
{
    private const int MaxQueueSize = 10_000;

    private static readonly Queue<Action> Queue = new();
    private static readonly Lock Sync = new();
    private static ThreadSafeFunction? _tsfn;
    private static int _queueFullCount;

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
            if (Queue.Count > 0)
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
        TryPost(action);
    }

    /// <summary>在 UI 线程同步清空当前队列；仅供压力测试和宿主收尾使用。</summary>
    internal static void Drain() => DrainQueue();

    /// <summary>尝试排队到 JS/UI 线程；返回是否成功入队。</summary>
    public static bool TryPost(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        ThreadSafeFunction? tsfn;
        lock (Sync)
        {
            if (Queue.Count >= MaxQueueSize)
            {
                if (Interlocked.Increment(ref _queueFullCount) == 1)
                    HiLog.Error("MainThread", "INTEROP_DISPATCH_QUEUE_FULL");
                return false;
            }
            Queue.Enqueue(action);
            tsfn = _tsfn;
        }

        tsfn?.Call(IntPtr.Zero);
        return true;
    }

    private static void DrainQueue()
    {
        while (true)
        {
            Action action;
            lock (Sync)
            {
                if (Queue.Count == 0)
                    return;
                action = Queue.Dequeue();
            }

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
