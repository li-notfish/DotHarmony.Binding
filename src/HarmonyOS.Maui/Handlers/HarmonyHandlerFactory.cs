// HarmonyHandlerFactory：IView → HarmonyOS Handler 的轻量工厂（无 DI）
// M1 范围：switch 类型分派；后续可扩展为注册表模式对接 MAUI 的 Handlers.Map 注册
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace HarmonyOS.Maui.Handlers;

public static class HarmonyHandlerFactory
{
    public static IElementHandler Create(IView view) => view switch
    {
        Microsoft.Maui.Controls.Button => new HarmonyButtonHandler(),
        Microsoft.Maui.Controls.Label => new HarmonyLabelHandler(),
        Microsoft.Maui.Controls.StackLayout => new HarmonyLayoutHandler(),
        Microsoft.Maui.Controls.Layout => new HarmonyLayoutHandler(),
        _ => throw new NotSupportedException(
            $"No HarmonyOS handler registered for {view.GetType().Name} (extend HarmonyHandlerFactory)")
    };
}
