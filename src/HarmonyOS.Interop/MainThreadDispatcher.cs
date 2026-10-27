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
    private static bool _shutdown;

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
            _shutdown = false;
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

    /// <summary>当前待执行动作数（诊断用；队列满时用于判断是积压还是逻辑挂起）</summary>
    public static int PendingCount
    {
        get
        {
            lock (Sync) return Queue.Count;
        }
    }

    /// <summary>
    /// 排队到 JS/UI 线程执行。未挂接时仅入队，AttachUiThread 之后首个 Post 触发冲刷。
    /// **尽力而为语义**：队列满时该动作被丢弃并返回 false。需要保证送达的同步点
    /// （如"在 UI 线程恢复 await"）必须用 <see cref="TryPost"/> 检查返回值，
    /// 否则等待方可能永久挂起。
    /// </summary>
    /// <returns>是否成功入队；队列满时返回 false（该动作已被丢弃）。</returns>
    public static bool Post(Action action) => TryPost(action);

    /// <summary>在 UI 线程同步清空当前队列；仅供压力测试和宿主收尾使用。</summary>
    internal static void Drain() => DrainQueue();

    /// <summary>尝试排队到 JS/UI 线程；返回是否成功入队。</summary>
    public static bool TryPost(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        ThreadSafeFunction? tsfn;
        lock (Sync)
        {
            // Shutdown 后不得再“成功入队但永不执行”：返回 false 让调用方知道通道已关。
            if (_shutdown)
                return false;

            if (Queue.Count >= MaxQueueSize)
            {
                // 每 1000 次丢弃再报一次：只报首次会让长时间运行后的连续丢弃完全不可见，
                // 每次都报会淹没 hilog。
                var dropped = Interlocked.Increment(ref _queueFullCount);
                if (dropped == 1 || dropped % 1000 == 0)
                    HiLog.Error("MainThread", $"INTEROP_DISPATCH_QUEUE_FULL (capacity {MaxQueueSize}, dropped {dropped})");
                return false;
            }

            if (Interlocked.Exchange(ref _queueFullCount, 0) != 0)
                HiLog.Info("MainThread", "INTEROP_DISPATCH_QUEUE_RECOVERED");

            Queue.Enqueue(action);
            tsfn = _tsfn;
        }

        try
        {
            tsfn?.Call(IntPtr.Zero);
        }
        catch (ObjectDisposedException)
        {
            // Shutdown 恰好插在入队与投递之间：动作已随队列清空，通道已关。
            // 返回 false 而不是抛 ODE——TryPost 的契约是尽力而为的 bool。
            return false;
        }
        return true;
    }

    /// <summary>
    /// 宿主收尾：停止派发并释放 TSFN。队列中未执行的动作在此丢弃并计数（不再执行，
    /// 因为此时 JS 线程可能已不再运行）。可由宿主在退出钩子里调用；不调用也不会崩，
    /// 只是这些动作与 TSFN 句柄留给进程退出清理。
    /// </summary>
    public static void Shutdown()
    {
        ThreadSafeFunction? tsfn;
        int abandoned;
        lock (Sync)
        {
            tsfn = _tsfn;
            _tsfn = null;
            _shutdown = true;
            abandoned = Queue.Count;
            Queue.Clear();
            _queueFullCount = 0;
        }

        if (abandoned > 0)
            HiLog.Info("MainThread", $"INTEROP_DISPATCH_SHUTDOWN: {abandoned} queued action(s) dropped");

        tsfn?.Release();
    }

    private static void DrainQueue()
    {
        // 每个 drain 批次一个 scope：动作里创建的临时 napi_value（属性读写、字符串封送）
        // 在本批结束时统一回收，而不是堆到 TSFN 回调帧的外层 scope 上。
        // 动作若要跨批次持有句柄，须自行转 NapiReference（JsObject 已是这种形态）。
        if (!NapiEnv.IsAvailable)
        {
            // 非 env 线程调用（如宿主收尾路径）：照常执行动作，不碰原生 scope
            RunQueuedActions();
            return;
        }

        using (NapiHandleScope.Open(NapiEnv.Current))
        {
            RunQueuedActions();
        }
    }

    private static void RunQueuedActions()
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
