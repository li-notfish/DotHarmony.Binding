using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkText = HarmonyOS.ArkUI.Text;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Label 的 HarmonyOS Handler（ArkUI Text 节点）。</summary>
public class HarmonyLabelHandler : HarmonyViewHandler<Label, ArkText>
{
    public static PropertyMapper<Label, HarmonyLabelHandler> Mapper = new(ViewMapper)
    {
        [nameof(Label.Text)] = MapText,
        [nameof(Label.TextColor)] = MapTextColor,
        [nameof(Label.FontSize)] = MapFontSize,
        [nameof(Label.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(Label.VerticalTextAlignment)] = MapVerticalTextAlignment,
        [nameof(Label.Background)] = MapBackground,
    };

    public HarmonyLabelHandler() : base(Mapper) { }

    protected override ArkText CreatePlatformView() => new();

    public static void MapText(HarmonyLabelHandler h, Label v)
    {
        h.PlatformView.Content = v.Text ?? string.Empty;
    }

    public static void MapTextColor(HarmonyLabelHandler h, Label v)
    {
        if (v.TextColor is { } c)
            h.PlatformView.SetFontColor(c);
    }

    public static void MapFontSize(HarmonyLabelHandler h, Label v)
    {
        if (v.FontSize > 0)
            h.PlatformView.FontSize = (float)v.FontSize;
    }

    public static void MapHorizontalTextAlignment(HarmonyLabelHandler h, Label v)
    {
        h.PlatformView.TextAlign = v.HorizontalTextAlignment switch
        {
            TextAlignment.Start => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_START,
            TextAlignment.Center => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_CENTER,
            TextAlignment.End => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_END,
            _ => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_START,
        };
    }

    public static void MapVerticalTextAlignment(HarmonyLabelHandler h, Label v)
    {
    }

    public static void MapBackground(HarmonyLabelHandler h, Label v)
    {
        BrushHelper.ApplyBackground(h.PlatformView, v.Background);
    }
}
