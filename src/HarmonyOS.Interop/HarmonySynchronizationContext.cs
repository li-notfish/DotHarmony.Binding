// HarmonySynchronizationContext：ArkUI 主线程调度器。
// TSFN 回调在任意线程进入 C#，但 ArkUI 节点 API 必须在主线程调用。
// 此 Context 在宿主启动时注册到主线程，确保 Post/Send 回到正确的线程。
#nullable enable
using System;
using System.Threading;

namespace HarmonyOS.Interop;

/// <summary>
/// HarmonyOS 主线程 SynchronizationContext。
/// 用法：
/// 1. 宿主启动时在主线程调用 HarmonySynchronizationContext.Install()
/// 2. 后台线程通过 Post/Send 回到主线程
/// </summary>
public sealed class HarmonySynchronizationContext : SynchronizationContext
{
    private static HarmonySynchronizationContext? _instance;

    private HarmonySynchronizationContext() { }

    /// <summary>
    /// 获取当前线程的 HarmonySynchronizationContext（如果已安装）
    /// </summary>
    public new static HarmonySynchronizationContext? Current => _instance;

    /// <summary>
    /// 在主线程安装 SynchronizationContext。
    /// 必须在宿主启动时调用一次。
    /// </summary>
    public static void Install()
    {
        if (_instance != null)
            throw new InvalidOperationException("HarmonySynchronizationContext already installed");

        _instance = new HarmonySynchronizationContext();
        SetSynchronizationContext(_instance);
    }

    /// <summary>
    /// 启动消息循环（在主线程上调用）
    /// </summary>
    public void Run()
    {
        throw new NotSupportedException("HarmonySynchronizationContext.Run is disabled. Use MainThreadDispatcher.");
    }

    /// <summary>
    /// 停止消息循环
    /// </summary>
    public void Stop()
    {
        // Retained for source compatibility; the dispatcher owns scheduling now.
    }

    /// <summary>
    /// 异步投递回调到主线程
    /// </summary>
    public override void Post(SendOrPostCallback d, object? state)
    {
        if (d == null) throw new ArgumentNullException(nameof(d));

        MainThreadDispatcher.TryPost(() => d(state));
    }

    /// <summary>
    /// 同步发送回调到主线程（阻塞直到完成）
    /// </summary>
    public override void Send(SendOrPostCallback d, object? state)
    {
        throw new NotSupportedException("HarmonySynchronizationContext.Send is disabled. Use MainThreadDispatcher.");
    }

    /// <summary>
    /// 在主线程执行 Action
    /// </summary>
    public void Post(Action action)
    {
        Post(_ => action(), null);
    }

    /// <summary>
    /// 在主线程执行 Func&lt;T&gt; 并等待结果
    /// </summary>
    public T Send<T>(Func<T> func)
    {
        T? result = default;
        Send(_ => result = func(), null);
        return result!;
    }
}
