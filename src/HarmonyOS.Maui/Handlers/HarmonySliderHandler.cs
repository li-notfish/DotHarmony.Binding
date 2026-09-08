using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkSlider = HarmonyOS.ArkUI.Slider;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Slider 的 HarmonyOS Handler（ArkUI Slider 节点）。</summary>
public class HarmonySliderHandler : ViewHandler<ISlider, ArkSlider>
{
    public static PropertyMapper<ISlider, HarmonySliderHandler> Mapper = new(ViewMapper)
    {
        [nameof(ISlider.Minimum)] = MapMinimum,
        [nameof(ISlider.Maximum)] = MapMaximum,
        [nameof(ISlider.Value)] = MapValue,
    };

    public HarmonySliderHandler() : base(Mapper) { }

    protected override ArkSlider CreatePlatformView() => new();

    protected override void ConnectHandler(ArkSlider platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ValueChange += OnValueChange;
    }

    protected override void DisconnectHandler(ArkSlider platformView)
    {
        platformView.ValueChange -= OnValueChange;
        base.DisconnectHandler(platformView);
    }

    public static void MapMinimum(HarmonySliderHandler h, ISlider v)
    {
        h.PlatformView.MinValue = (float)v.Minimum;
    }

    public static void MapMaximum(HarmonySliderHandler h, ISlider v)
    {
        h.PlatformView.MaxValue = (float)v.Maximum;
    }

    public static void MapValue(HarmonySliderHandler h, ISlider v)
    {
        var clamped = Math.Clamp(v.Value, v.Minimum, v.Maximum);
        h.PlatformView.Value = (float)clamped;
    }

    private void OnValueChange(ArkUINodeEvent e)
    {
        var value = (double)e.ComponentData(0).f32;
        if (VirtualView.Value == value) return;
        VirtualView.Value = value;
    }
}
