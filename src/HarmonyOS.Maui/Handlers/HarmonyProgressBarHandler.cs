using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ArkProgress = HarmonyOS.ArkUI.Progress;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI ProgressBar 的 HarmonyOS Handler（ArkUI Progress 节点）。</summary>
public class HarmonyProgressBarHandler :
    HarmonyViewHandler<Microsoft.Maui.Controls.ProgressBar, ArkProgress>, IProgressBarHandler
{
    public static PropertyMapper<Microsoft.Maui.Controls.ProgressBar, HarmonyProgressBarHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(Microsoft.Maui.Controls.ProgressBar.Progress)] = MapProgress,
        [nameof(Microsoft.Maui.Controls.ProgressBar.ProgressColor)] = MapProgressColor,
    };

    public HarmonyProgressBarHandler() : base(Mapper) { }

    IProgress IProgressBarHandler.VirtualView => VirtualView;

    object IProgressBarHandler.PlatformView => PlatformView;

    protected override ArkProgress CreatePlatformView() => new();

    public static void MapProgress(HarmonyProgressBarHandler h, Microsoft.Maui.Controls.ProgressBar v)
    {
        h.PlatformView.Value = (float)(Math.Clamp(v.Progress, 0, 1) * 100);
    }

    public static void MapProgressColor(HarmonyProgressBarHandler h, Microsoft.Maui.Controls.ProgressBar v)
    {
        if (v.ProgressColor is { } c)
            h.PlatformView.SetColor(c);
    }
}
