// HarmonyHandlerFactory：IView → HarmonyOS Handler 的轻量工厂（无 DI）
// M1 范围：switch 类型分派；后续可扩展为注册表模式对接 MAUI 的 Handlers.Map 注册
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace HarmonyOS.Maui.Handlers;

public static class HarmonyHandlerFactory
{
    public static IElementHandler Create(IView view) => Create((Microsoft.Maui.Controls.Element)view);

    public static IElementHandler Create(Microsoft.Maui.Controls.Element element)
    {
        IElementHandler handler = element switch
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
        Microsoft.Maui.Controls.ProgressBar => new HarmonyProgressBarHandler(),
        Microsoft.Maui.Controls.Image => new HarmonyImageHandler(),
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
        // 动画/服务解析口：ViewExtensions（FadeTo 等）经 Handler.MauiContext.Services 取 IAnimationManager
        handler.SetMauiContext(Hosting.HarmonyMauiContext.Shared);
        return handler;
    }
}
