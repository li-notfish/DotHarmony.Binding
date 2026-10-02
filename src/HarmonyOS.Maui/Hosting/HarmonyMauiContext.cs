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
        private static readonly HarmonyServices ServiceStore = new();

        public static readonly HarmonyMauiContext Shared = new();

        private HarmonyMauiContext() : base(ServiceStore) { }

    /// <summary>
    /// 注册托管服务（第三方库/应用扩展点：鸿蒙宿主无 UseMauiApp 的 IServiceCollection，
    /// 此为最小等价物）。同类型后注册覆盖先注册；AnimationManager 为内置服务不可覆盖。
    /// </summary>
        public static void RegisterService<T>(T instance) where T : notnull
        {
            ArgumentNullException.ThrowIfNull(instance);
            ServiceStore.Register(typeof(T), instance);
        }

    /// <summary>
    /// 接入应用级 DI 容器（HarmonyMauiAppBuilder.Build 的产物）：解析顺序
    /// 内置 AnimationManager → RegisterService 注册表 → 本容器。可多次调用，后者覆盖前者。
    /// </summary>
        public static void RegisterServiceProvider(IServiceProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);
            ServiceStore.ChainProvider(provider);
        }

    private sealed class HarmonyServices : IServiceProvider
    {
        private AnimationManager? _animations;
        private readonly System.Collections.Generic.Dictionary<Type, object> _services = new();
        private readonly object _lock = new();
        private volatile IServiceProvider? _chained;

        public void Register(Type serviceType, object instance)
        {
            lock (_lock)
                _services[serviceType] = instance;
        }

        public void ChainProvider(IServiceProvider provider) => _chained = provider;

        public object? GetService(Type serviceType)
        {
            if (serviceType.IsAssignableFrom(typeof(AnimationManager)))
                return _animations ??= new AnimationManager(HarmonyTicker.Shared);
            lock (_lock)
            {
                if (_services.TryGetValue(serviceType, out var service))
                    return service;
            }
            return _chained?.GetService(serviceType);
        }
    }
}
