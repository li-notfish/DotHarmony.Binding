// HarmonySliderHandler：MAUI Controls.Slider → HarmonyOS.ArkUI.Slider
using Microsoft.Maui.Handlers;
using ArkSlider = HarmonyOS.ArkUI.Slider;

namespace HarmonyOS.Maui.Handlers;

public class HarmonySliderHandler : ViewHandler<Microsoft.Maui.Controls.Slider, ArkSlider>
{
    public static PropertyMapper<Microsoft.Maui.Controls.Slider, HarmonySliderHandler> Mapper = new(ViewMapper)
    {
        [nameof(Microsoft.Maui.Controls.Slider.Minimum)] = MapRange,
        [nameof(Microsoft.Maui.Controls.Slider.Maximum)] = MapRange,
        [nameof(Microsoft.Maui.Controls.Slider.Value)] = MapValue,
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

    public static void MapRange(HarmonySliderHandler h, Microsoft.Maui.Controls.Slider v)
    {
        // ArkUI Slider 需要单独设置 min/max/value，顺序由 Mapper 决定；
        // Minimum/Maximum 任一变化都重刷范围
        h.PlatformView.MinValue = (float)v.Minimum;
        h.PlatformView.MaxValue = (float)v.Maximum;
        // 重新应用 Value（避免超出新范围）
        var clamped = Math.Clamp(v.Value, v.Minimum, v.Maximum);
        if (clamped != v.Value) v.Value = clamped;
        h.PlatformView.Value = (float)clamped;
    }

    public static void MapValue(HarmonySliderHandler h, Microsoft.Maui.Controls.Slider v)
        => h.PlatformView.Value = (float)Math.Clamp(v.Value, v.Minimum, v.Maximum);

    private void OnValueChange(HarmonyOS.Bindings.NativeNode.ArkUINodeEvent e)
    {
        // data[0].f32 = current value（见 NODE_SLIDER_EVENT_ON_CHANGE 注释）
        var value = (double)e.ComponentData(0).f32;
        if (VirtualView.Value == value) return;
        // 属性 setter 触发 ValueChanged 事件
        VirtualView.Value = value;
    }
}
