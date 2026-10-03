// HarmonyHandlerFactory：IView → HarmonyOS Handler 的轻量工厂（无 DI）
// 解析顺序：开放注册表（Register<>，沿类型继承链向上找最近注册）→ 内置 switch 分派。
// 注册表是第三方控件库/应用自定义 Handler 的接入口（鸿蒙宿主不走 UseMauiApp，
// MauiHandlersCollectionExtensions 不可用，此为对应的最小等价物）。
#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace HarmonyOS.Maui.Handlers;

public static class HarmonyHandlerFactory
{
    private static readonly object RegistryLock = new();
    private static readonly Dictionary<Type, Func<IElementHandler>> Registry = new();

    /// <summary>注册自定义 Handler（TView 及其派生类型优先于内置分派）。</summary>
    public static void Register<TView>(Func<IElementHandler> factory)
        where TView : Microsoft.Maui.Controls.Element
    {
        ArgumentNullException.ThrowIfNull(factory);
        lock (RegistryLock)
            Registry[typeof(TView)] = factory;
    }

    /// <summary>注销自定义 Handler（返回 false 表示未注册过）。</summary>
    public static bool Unregister<TView>()
        where TView : Microsoft.Maui.Controls.Element
    {
        lock (RegistryLock)
            return Registry.Remove(typeof(TView));
    }

    /// <summary>注册表解析（纯逻辑，单测覆盖）：从实际类型沿继承链向上找最近注册。</summary>
    internal static Func<IElementHandler>? TryResolveRegistered(Type viewType)
    {
        lock (RegistryLock)
        {
            for (var t = viewType; t is not null && t != typeof(object); t = t.BaseType)
            {
                if (Registry.TryGetValue(t, out var factory))
                    return factory;
            }
        }
        return null;
    }

    public static IElementHandler Create(IView view) => Create((Microsoft.Maui.Controls.Element)view);

    public static IElementHandler Create(Microsoft.Maui.Controls.Element element)
    {
        IElementHandler handler;
        var registered = TryResolveRegistered(element.GetType());
        if (registered is not null)
        {
            handler = registered();
        }
        else
        {
            handler = element switch
            {
        Microsoft.Maui.Controls.NavigationPage => new HarmonyNavigationPageHandler(),
        // Shell 是 Page 直接派生（非 ContentPage），必须在页面 arm 之前分派；
        // 路由/栈语义见 HarmonyShellNavigation
        Microsoft.Maui.Controls.Shell => new HarmonyShellHandler(),
        // TabbedPage : MultiPage<Page> : Page，必须在 ContentPage 之前分派
        Microsoft.Maui.Controls.TabbedPage => new HarmonyTabbedPageHandler(),
        Microsoft.Maui.Controls.ContentPage => new HarmonyContentPageHandler(),
        // 具体派生必须排在基类 arm 之前：RefreshView : ContentView : TemplatedView : Compatibility.Layout
        Microsoft.Maui.Controls.ScrollView => new HarmonyScrollViewHandler(),
        Microsoft.Maui.Controls.RefreshView => new HarmonyRefreshViewHandler(),
        // 模板化容器必须先于 Layout 分派（ContentPresenter/TemplatedView 都派生自 Compatibility.Layout）
        Microsoft.Maui.Controls.ContentPresenter => new HarmonyContentPresenterHandler(),
        Microsoft.Maui.Controls.ContentView => new HarmonyContentViewHandler(),
        Microsoft.Maui.Controls.Button => new HarmonyButtonHandler(),
        Microsoft.Maui.Controls.Label => new HarmonyLabelHandler(),
        Microsoft.Maui.Controls.StackLayout => new HarmonyLayoutHandler(),
        Microsoft.Maui.Controls.Grid => new HarmonyManagedLayoutHandler(),
        Microsoft.Maui.Controls.AbsoluteLayout => new HarmonyManagedLayoutHandler(),
        Microsoft.Maui.Controls.Switch => new HarmonySwitchHandler(),
        Microsoft.Maui.Controls.CheckBox => new HarmonyCheckBoxHandler(),
        Microsoft.Maui.Controls.RadioButton => new HarmonyRadioButtonHandler(),
        Microsoft.Maui.Controls.Entry => new HarmonyEntryHandler(),
        Microsoft.Maui.Controls.Editor => new HarmonyEditorHandler(),
        Microsoft.Maui.Controls.Slider => new HarmonySliderHandler(),
        Microsoft.Maui.Controls.Stepper => new HarmonyStepperHandler(),
        Microsoft.Maui.Controls.ProgressBar => new HarmonyProgressBarHandler(),
        Microsoft.Maui.Controls.ActivityIndicator => new HarmonyActivityIndicatorHandler(),
        Microsoft.Maui.Controls.Image => new HarmonyImageHandler(),
        Microsoft.Maui.Controls.SearchBar => new HarmonySearchBarHandler(),
        Microsoft.Maui.Controls.Picker => new HarmonyPickerHandler(),
        Microsoft.Maui.Controls.DatePicker => new HarmonyDatePickerHandler(),
        Microsoft.Maui.Controls.TimePicker => new HarmonyTimePickerHandler(),
        Microsoft.Maui.Controls.CollectionView => new HarmonyCollectionViewHandler(),
        Microsoft.Maui.Controls.CarouselView => new HarmonyCarouselViewHandler(),
        Microsoft.Maui.Controls.Border => new HarmonyFrameHandler(),
        Microsoft.Maui.Controls.BoxView => new HarmonyBoxViewHandler(),
        Microsoft.Maui.Controls.GraphicsView => new HarmonyGraphicsViewHandler(),
        Microsoft.Maui.Controls.Shapes.Shape => new HarmonyShapeHandler(),
        Microsoft.Maui.Controls.Layout => new HarmonyLayoutHandler(),
        _ => throw new NotSupportedException(
            $"No HarmonyOS handler registered for {element.GetType().Name} (extend HarmonyHandlerFactory)")
        };
        }
        // 动画/服务解析口：ViewExtensions（FadeTo 等）经 Handler.MauiContext.Services 取 IAnimationManager
        handler.SetMauiContext(Hosting.HarmonyMauiContext.Shared);
        // 必须回写 element.Handler：setter 内部完成 SetVirtualView（Connect），
        // 且只有经此建立 element→handler 反向链，后续清理路径的 element.Handler = null
        // 才会真正触发 DisconnectHandler（手势/事件订阅释放）。直接调 handler.SetVirtualView
        // 不回写，element.Handler 恒 null，DisconnectHandler 永不执行 → 原生手势句柄泄漏。
        element.Handler = handler;
        return handler;
    }
}
