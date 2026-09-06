// HarmonyLabelHandler：MAUI Controls.Label → HarmonyOS.ArkUI.Text
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ArkText = HarmonyOS.ArkUI.Text;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Label 的 HarmonyOS Handler（ArkUI Text 节点）。</summary>
public class HarmonyLabelHandler : ViewHandler<Microsoft.Maui.Controls.Label, ArkText>
{
    public static PropertyMapper<Microsoft.Maui.Controls.Label, HarmonyLabelHandler> Mapper =
        new(ViewHandler.ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.Label.Text)] = (h, v) =>
                h.PlatformView.Content = v.Text ?? string.Empty,
            [nameof(Microsoft.Maui.Controls.Label.TextColor)] = (h, v) =>
            {
                if (v.TextColor is { } c)
                    h.PlatformView.SetFontColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
            },
            [nameof(Microsoft.Maui.Controls.Label.BackgroundColor)] = (h, v) =>
            {
                if (v.BackgroundColor is { } c)
                    h.PlatformView.SetBackgroundColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
            },
        };

    public HarmonyLabelHandler() : base(Mapper) { }

    protected override ArkText CreatePlatformView() => new();
}
