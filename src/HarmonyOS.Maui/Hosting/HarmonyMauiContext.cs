// MAUI 服务容器 + 动画宿主支撑。
// ViewExtensions.FadeToAsync/TranslateToAsync 等走 IAnimationManager（由
// ITicker 驱动帧回调）；鸿蒙宿主不走 UseMauiApp，须自建 MauiContext +
// IServiceProvider 并把 AnimationManager 的周节拍转发到 UI 线程（MainThreadDispatcher）。
#nullable enable
using System;
using System.Threading;
using HarmonyOS.Interop;
using Microsoft.Maui;
using Microsoft.Maui.Animations;

namespace HarmonyOS.Maui.Hosting;

/// <summary>鸿蒙 UI 线程驱动的 ITicker（AnimationManager 的帧源）。</summary>
public sealed class HarmonyTicker : ITicker
{
    public static readonly HarmonyTicker Shared = new();

    public bool IsRunning => _timer is not null;
    public bool SystemEnabled { get; set; } = true;
    public int MaxFps { get; set; } = 60;

    /// <summary>MAUI 10 的 ITicker.Fire 是 Action 属性（非 event）</summary>
    public Action? Fire { get; set; }

    private Timer? _timer;

    public void Start()
    {
        if (_timer is not null) return;
        var interval = TimeSpan.FromMilliseconds(1000.0 / Math.Max(MaxFps, 1));
        _timer = new Timer(_ =>
        {
            if (!SystemEnabled) return;
            var fire = Fire;
            if (fire is not null)
                MainThreadDispatcher.Post(() => fire());
        }, null, interval, interval);
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }
}

/// <summary>鸿蒙宿主的 MauiContext（动画服务等托管服务的解析口）。</summary>
public sealed class HarmonyMauiContext : MauiContext
{
    public static readonly HarmonyMauiContext Shared = new();

    private HarmonyMauiContext() : base(new HarmonyServices()) { }

    private sealed class HarmonyServices : IServiceProvider
    {
        private AnimationManager? _animations;

        public object? GetService(Type serviceType)
        {
            if (serviceType.IsAssignableFrom(typeof(AnimationManager)))
                return _animations ??= new AnimationManager(HarmonyTicker.Shared);
            return null;
        }
    }
}
