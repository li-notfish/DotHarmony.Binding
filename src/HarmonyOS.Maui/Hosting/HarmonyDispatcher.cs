// IDispatcher 桥：托管 MAUI 控件库常用 Dispatcher.Dispatch/CreateTimer（如模板
// 应用模板回调触发布局刷新、ApplicationBar 菜单定时器）。鸿蒙宿主不经 UseMauiApp
// 引导，DispatcherProvider 未注册会抛 "BindableObject was not instantiated on a
// thread"——在 UI 引导时把宿主 MainThreadDispatcher（TSFN 通道）包装注册进去。
// UI 线程即 napi/JS 线程（BuildUICore 时序）：同线程内联执行，跨线程经 Post。
using System;
using System.Threading;
using HarmonyOS.Interop;
using Microsoft.Maui.Dispatching;

namespace HarmonyOS.Maui.Hosting;

/// <summary>鸿蒙宿主线程模型下的 <see cref="IDispatcher"/> 最小实现。</summary>
public sealed class HarmonyDispatcher : IDispatcher
{
    public static readonly HarmonyDispatcher Instance = new();

    private static int _uiThreadId = -1;

    /// <summary>在 UI 主线程调用一次（MauiHarmonyHost.Run 的引导点内）。</summary>
    public static void EnsureRegistered()
    {
        _uiThreadId = Environment.CurrentManagedThreadId;
        DispatcherProvider.SetCurrent(new Provider());
    }

    private sealed class Provider : IDispatcherProvider
    {
        public IDispatcher? GetForCurrentThread() => Instance;
    }

    public bool IsDispatchRequired
        => Environment.CurrentManagedThreadId != _uiThreadId;

    public bool Dispatch(Action action)
    {
        if (IsDispatchRequired)
        {
            MainThreadDispatcher.Post(action);
            return true;
        }
        action();
        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action)
    {
        Timer? timer = null;
        timer = new Timer(_ =>
        {
            Dispatch(action);
            timer?.Dispose();
        }, null, delay, Timeout.InfiniteTimeSpan);
        return true;
    }

    public IDispatcherTimer CreateTimer() => new HarmonyDispatcherTimer(this);

    private sealed class HarmonyDispatcherTimer : IDispatcherTimer
    {
        private readonly IDispatcher _dispatcher;
        private Timer? _timer;

        public HarmonyDispatcherTimer(IDispatcher dispatcher) => _dispatcher = dispatcher;

        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);
        public bool IsRepeating { get; set; } = true;
        public bool IsRunning => _timer != null;
#pragma warning disable CS0067
        public event EventHandler? Tick;
#pragma warning restore CS0067

        public void Start()
        {
            Stop();
            _timer = new Timer(_ => _dispatcher.Dispatch(() => Tick?.Invoke(this, EventArgs.Empty)),
                null, Interval, IsRepeating ? Interval : Timeout.InfiniteTimeSpan);
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}
