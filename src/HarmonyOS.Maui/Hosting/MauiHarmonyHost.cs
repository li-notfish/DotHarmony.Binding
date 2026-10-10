// MauiHarmonyHost：MAUI 控件树的鸿蒙引导
// 宿主根为一个 100% Stack 容器（挂入 ContentSlot）：Stack 后挂者覆盖先挂者，
// 模态页借此覆盖在页面之上；页面经 HarmonyNavigation 挂入该容器。
using HarmonyOS.Bindings.Hosting;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using ArkStack = HarmonyOS.ArkUI.Stack;
using MApplication = Microsoft.Maui.Controls.Application;
using MPage = Microsoft.Maui.Controls.Page;
using MResourceDictionary = Microsoft.Maui.Controls.ResourceDictionary;
using MWindow = Microsoft.Maui.Controls.Window;

namespace HarmonyOS.Maui.Hosting;

public static class MauiHarmonyHost
{
    /// <summary>宿主回调注册 seam；单测注入 fake，产品态走 libentry。</summary>
    internal static IHarmonyHostCallbackRegistrar CallbackRegistrar { get; set; } = new LibEntryCallbackRegistrar();

    /// <summary>
    /// 系统 colorMode 变化入口。正常路径：宿主初始化时经 RegisterThemeCallback
    /// 把 <see cref="OnThemeChangedNative"/> 的函数指针注册进 libentry 回调表；
    /// 旧版宿主（无 HarmonyHostSetThemeChangedCallback 导出）由应用层
    /// HarmonyThemeChanged 导出经 dlsym 回退转发到这里。
    /// </summary>
    public static int ThemeChangedCore(nint env, int colorMode)
        => HarmonyApplication.ThemeChangedCore(env, colorMode);

    // 函数指针注册用入口：UnmanagedCallersOnly 取地址不受 ILC 入口程序集导出限制
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static int OnThemeChangedNative(nint env, int colorMode) => ThemeChangedCore(env, colorMode);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static int OnLifecycleChangedNative(nint env, int lifecycleEvent)
    {
        HarmonyLifecycleController.Notify(lifecycleEvent);
        return 0;
    }

    /// <summary>向 libentry.so 注册主题回调函数指针（旧宿主无此导出时静默跳过，走 dlsym 回退）。</summary>
    private static unsafe void RegisterThemeCallback()
    {
        try
        {
            if (NativeLibrary.TryLoad("libentry.so", out var lib) &&
                NativeLibrary.TryGetExport(lib, "HarmonyHostSetThemeChangedCallback", out var registrar))
            {
                ((delegate* unmanaged[Cdecl]<nint, void>)registrar)(
                    (nint)(delegate* unmanaged[Cdecl]<nint, int, int>)&OnThemeChangedNative);
                Interop.HiLog.Info("HarmonyHost", "theme callback registered via libentry");
            }
            else
            {
                Interop.HiLog.Warn("HarmonyHost",
                    "HarmonyHostSetThemeChangedCallback not found; theme sync relies on dlsym fallback");
            }
        }
        catch
        {
            // 注册失败不致命：仅影响系统深浅色跟随
            Interop.HiLog.Warn("HarmonyHost", "theme callback registration failed; dlsym fallback only");
        }
    }

    /// <summary>向 libentry.so 注册生命周期回调函数指针。</summary>
    private static unsafe void RegisterLifecycleCallback()
    {
        try
        {
            if (NativeLibrary.TryLoad("libentry.so", out var lib) &&
                NativeLibrary.TryGetExport(lib, "HarmonyHostSetLifecycleChangedCallback", out var registrar))
            {
                ((delegate* unmanaged[Cdecl]<nint, void>)registrar)(
                    (nint)(delegate* unmanaged[Cdecl]<nint, int, int>)&OnLifecycleChangedNative);
                Interop.HiLog.Info("HarmonyHost", "lifecycle callback registered via libentry");
            }
            else
            {
                Interop.HiLog.Warn("HarmonyHost",
                    "HarmonyHostSetLifecycleChangedCallback not found; MAUI app/window lifecycle events will not fire");
            }
        }
        catch
        {
            Interop.HiLog.Warn("HarmonyHost", "lifecycle callback registration failed");
        }
    }

    /// <summary>
    /// 注册 MAUI 根页面工厂。rootFactory 在 UI 主线程（HarmonyBuildUI 时序）被调用，
    /// 返回的首页经 HarmonyHandlerFactory 装配 handler 并挂载上屏。
    /// 同时注入 Essentials 鸿蒙实现（DeviceInfo/DeviceDisplay/AppInfo/Clipboard，
    /// MAUI 生态的标准静态入口自此可用）。
    /// </summary>
    public static void Run(Func<MPage> rootFactory, MResourceDictionary? globalResources = null)
    {
        StartApplication(() => new RootPageApplication(rootFactory), globalResources);
    }

    /// <summary>
    /// 真实 Application 宿主：接收应用的 Application 子类（其构造函数里设置 MainPage、
    /// 装载 App.xaml 资源/UserAppTheme），经 IApplication.CreateWindow(null) 走 MAUI
    /// 标准窗口创建拿根 Window/Page —— Application.Resources、主题切换、Windows 集合、
    /// 应用服务上下文均为应用自带，页面按标准继承链解析 StaticResource/DynamicResource/AppThemeBinding。
    /// 与 Run(...) 的差别：Run 装配"最小 Application + 手传资源字典"，本入口不接资源字典
    /// （资源属于应用），宿主只负责把 Window.Page 挂上屏。
    /// </summary>
    public static void RunApplication(Func<MApplication> applicationFactory)
    {
        StartApplication(applicationFactory, globalResources: null);
    }

    private static void StartApplication(Func<MApplication> applicationFactory, MResourceDictionary? globalResources)
    {
        RegisterHostCallbacks();
        Host.RootBuilder = contentHandle =>
        {
            Essentials.HarmonyEssentials.Install();
            HarmonyDispatcher.EnsureRegistered();

            var container = new ArkStack();
            container.SetWidthPercent(1.0f);
            container.SetHeightPercent(1.0f);
            Host.AttachRoot(contentHandle, container);

            var app = applicationFactory();
            HarmonyApplication.EnsureCurrent(globalResources, app);

            // MAUI 标准窗口协议：App.MainPage → Window.Page、Windows 集合注册、
            // Parent 链（Page → Window → Application）补齐（Appearing 守卫依赖）
            var window = (MWindow)((Microsoft.Maui.IApplication)app).CreateWindow(null)
                ?? throw new InvalidOperationException("Application.CreateWindow returned no window");
            var rootPage = window.Page
                ?? throw new InvalidOperationException(
                    "Application.CreateWindow produced no root Page (set MainPage in the Application ctor)");

            HarmonyNavigation.Attach(container, app, window);
            HarmonyLifecycleController.Attach(window);
            HarmonyLifecycleController.Current?.Created();
            HarmonyNavigation.Push(rootPage);
        };
    }

    private static void RegisterHostCallbacks()
    {
        CallbackRegistrar.RegisterThemeCallback();
        CallbackRegistrar.RegisterLifecycleCallback();
    }

    private sealed class RootPageApplication : MApplication
    {
        private readonly Func<MPage> _rootFactory;

        public RootPageApplication(Func<MPage> rootFactory) => _rootFactory = rootFactory;

        protected override Window CreateWindow(IActivationState? activationState)
            => new(_rootFactory());
    }

    private sealed class LibEntryCallbackRegistrar : IHarmonyHostCallbackRegistrar
    {
        public void RegisterThemeCallback() => MauiHarmonyHost.RegisterThemeCallback();

        public void RegisterLifecycleCallback() => MauiHarmonyHost.RegisterLifecycleCallback();
    }

    /// <summary>泛型便捷重载（对齐 UseMauiApp&lt;T&gt; 的习惯写法）。</summary>
    public static void RunApplication<TApplication>() where TApplication : MApplication, new()
        => RunApplication(() => new TApplication());
}

internal interface IHarmonyHostCallbackRegistrar
{
    void RegisterThemeCallback();

    void RegisterLifecycleCallback();
}
