// HarmonyProgressBarHandler：MAUI Controls.ProgressBar → HarmonyOS.ArkUI.Progress
using Microsoft.Maui.Handlers;
using ArkProgress = HarmonyOS.ArkUI.Progress;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyProgressBarHandler : ViewHandler<Microsoft.Maui.Controls.ProgressBar, ArkProgress>
{
    public static PropertyMapper<Microsoft.Maui.Controls.ProgressBar, HarmonyProgressBarHandler> Mapper = new(ViewMapper)
    {
        [nameof(Microsoft.Maui.Controls.ProgressBar.Progress)] = MapProgress,
        [nameof(Microsoft.Maui.Controls.ProgressBar.ProgressColor)] = MapProgressColor,
    };

    public HarmonyProgressBarHandler() : base(Mapper) { }

    protected override ArkProgress CreatePlatformView() => new();

    public static void MapProgress(HarmonyProgressBarHandler h, Microsoft.Maui.Controls.ProgressBar v)
        => h.PlatformView.Value = (float)(Math.Clamp(v.Progress, 0, 1) * 100);

    public static void MapProgressColor(HarmonyProgressBarHandler h, Microsoft.Maui.Controls.ProgressBar v)
    {
        if (v.ProgressColor is { } c)
            h.PlatformView.SetColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }
}
