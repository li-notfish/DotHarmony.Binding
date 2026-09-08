using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ArkText = HarmonyOS.ArkUI.Text;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Label 的 HarmonyOS Handler（ArkUI Text 节点）。</summary>
public class HarmonyLabelHandler : ViewHandler<ILabel, ArkText>
{
    public static PropertyMapper<ILabel, HarmonyLabelHandler> Mapper = new(ViewMapper)
    {
        [nameof(ILabel.Text)] = MapText,
        [nameof(ILabel.TextColor)] = MapTextColor,
        [nameof(ILabel.Background)] = MapBackground,
    };

    public HarmonyLabelHandler() : base(Mapper) { }

    protected override ArkText CreatePlatformView() => new();

    public static void MapText(HarmonyLabelHandler h, ILabel v)
    {
        h.PlatformView.Content = v.Text ?? string.Empty;
    }

    public static void MapTextColor(HarmonyLabelHandler h, ILabel v)
    {
        if (v.TextColor is { } c)
            h.PlatformView.SetFontColor(
                (byte)(c.Red * 255), (byte)(c.Green * 255),
                (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }

    public static void MapBackground(HarmonyLabelHandler h, ILabel v)
    {
        BrushHelper.ApplyBackground(h.PlatformView, v.Background);
    }
}
