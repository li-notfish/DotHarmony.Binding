// HarmonyHandlerFactory：IView → HarmonyOS Handler 的轻量工厂（无 DI）
// M1 范围：switch 类型分派；后续可扩展为注册表模式对接 MAUI 的 Handlers.Map 注册
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace HarmonyOS.Maui.Handlers;

public static class HarmonyHandlerFactory
{
    public static IElementHandler Create(IView view) => Create((Microsoft.Maui.Controls.Element)view);

    public static IElementHandler Create(Microsoft.Maui.Controls.Element element) => element switch
    {
        Microsoft.Maui.Controls.ContentPage => new HarmonyContentPageHandler(),
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
        Microsoft.Maui.Controls.ScrollView => new HarmonyScrollViewHandler(),
        Microsoft.Maui.Controls.Border => new HarmonyFrameHandler(),
        Microsoft.Maui.Controls.Layout => new HarmonyLayoutHandler(),
        _ => throw new NotSupportedException(
            $"No HarmonyOS handler registered for {element.GetType().Name} (extend HarmonyHandlerFactory)")
    };
}
