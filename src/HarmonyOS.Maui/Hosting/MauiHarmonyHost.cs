// MauiHarmonyHost：MAUI 控件树的鸿蒙引导
// 宿主根为一个 100% Stack 容器（挂入 ContentSlot）：Stack 后挂者覆盖先挂者，
// 模态页借此覆盖在页面之上；页面经 HarmonyNavigation 挂入该容器。
using HarmonyOS.Bindings.Hosting;
using ArkStack = HarmonyOS.ArkUI.Stack;
using MApplication = Microsoft.Maui.Controls.Application;
using MPage = Microsoft.Maui.Controls.Page;
using MResourceDictionary = Microsoft.Maui.Controls.ResourceDictionary;
using MWindow = Microsoft.Maui.Controls.Window;

namespace HarmonyOS.Maui.Hosting;

public static class MauiHarmonyHost
{
    /// <summary>
    /// 注册 MAUI 根页面工厂。rootFactory 在 UI 主线程（HarmonyBuildUI 时序）被调用，
    /// 返回的首页经 HarmonyHandlerFactory 装配 handler 并挂载上屏。
    /// 同时注入 Essentials 鸿蒙实现（DeviceInfo/DeviceDisplay/AppInfo/Clipboard，
    /// MAUI 生态的标准静态入口自此可用）。
    /// </summary>
    public static void Run(Func<MPage> rootFactory, MResourceDictionary? globalResources = null)
    {
        Host.RootBuilder = contentHandle =>
        {
            // Essentials 在 UI 线程首次构建时装入（napi env 已可用）——
            // 不能在 ModuleInitializer 阶段（此时 napi 未初始化会闪退）
            Essentials.HarmonyEssentials.Install();

            // IDispatcher 注册：MAUI 控件模板内的 Dispatcher.Dispatch/CreateTimer 依赖
            // DispatcherProvider.Current（鸿蒙宿主不走 UseMauiApp 引导，需手动注册）
            HarmonyDispatcher.EnsureRegistered();

            // 宿主根容器：页面栈 + 模态层的挂载点（Stack 叠加语义）
            var container = new ArkStack();
            container.SetWidthPercent(1.0f);
            container.SetHeightPercent(1.0f);
            Host.AttachRoot(contentHandle, container);

            HarmonyNavigation.Attach(container, globalResources);
            HarmonyNavigation.Push(rootFactory());
        };
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
        Host.RootBuilder = contentHandle =>
        {
            Essentials.HarmonyEssentials.Install();
            HarmonyDispatcher.EnsureRegistered();

            var container = new ArkStack();
            container.SetWidthPercent(1.0f);
            container.SetHeightPercent(1.0f);
            Host.AttachRoot(contentHandle, container);

            var app = applicationFactory();
            HarmonyApplication.EnsureCurrent(globalResources: null, app);

            // MAUI 标准窗口协议：App.MainPage → Window.Page、Windows 集合注册、
            // Parent 链（Page → Window → Application）补齐（Appearing 守卫依赖）
            var window = (MWindow)((Microsoft.Maui.IApplication)app).CreateWindow(null)
                ?? throw new InvalidOperationException("Application.CreateWindow returned no window");
            var rootPage = window.Page
                ?? throw new InvalidOperationException(
                    "Application.CreateWindow produced no root Page (set MainPage in the Application ctor)");

            HarmonyNavigation.Attach(container, app, window);
            HarmonyNavigation.Push(rootPage);
        };
    }

    /// <summary>泛型便捷重载（对齐 UseMauiApp&lt;T&gt; 的习惯写法）。</summary>
    public static void RunApplication<TApplication>() where TApplication : MApplication, new()
        => RunApplication(() => new TApplication());
}
