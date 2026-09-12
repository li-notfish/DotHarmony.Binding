using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkStack = HarmonyOS.ArkUI.Stack;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// MAUI BoxView 的 HarmonyOS Handler：纯色矩形 → ArkUI Stack 容器 + 背景色。
/// </summary>
public class HarmonyBoxViewHandler : HarmonyViewHandler<BoxView, ArkStack>
{
    public static PropertyMapper<BoxView, HarmonyBoxViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(BoxView.Color)] = MapColor,
        [nameof(BoxView.BackgroundColor)] = MapBackgroundColor,
    };

    public HarmonyBoxViewHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView() => new();

    public static void MapColor(HarmonyBoxViewHandler h, BoxView v)
    {
        if (v.Color is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    public static void MapBackgroundColor(HarmonyBoxViewHandler h, BoxView v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }
}
