#nullable enable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkTimePicker = HarmonyOS.ArkUI.TimePicker;
using MTimePicker = Microsoft.Maui.Controls.TimePicker;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyTimePickerHandler : HarmonyViewHandler<MTimePicker, ArkTimePicker>
{
    public static PropertyMapper<MTimePicker, HarmonyTimePickerHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(ITimePicker.Time)] = MapTime,
        [nameof(MTimePicker.Format)] = MapText,
        [nameof(MTimePicker.TextColor)] = MapText,
        [nameof(MTimePicker.CharacterSpacing)] = MapText,
        [nameof(MTimePicker.FontFamily)] = MapText,
        [nameof(MTimePicker.FontSize)] = MapText,
        [nameof(MTimePicker.FontAttributes)] = MapText,
        [nameof(MTimePicker.FontAutoScalingEnabled)] = MapText,
        [nameof(MTimePicker.IsOpen)] = MapIsOpen,
    };

    public HarmonyTimePickerHandler() : base(Mapper) { }

    protected override ArkTimePicker CreatePlatformView() => new();

    protected override void ConnectHandler(ArkTimePicker platformView)
    {
        base.ConnectHandler(platformView);
        platformView.OnTimeChange += OnTimeChange;
    }

    protected override void DisconnectHandler(ArkTimePicker platformView)
    {
        platformView.OnTimeChange -= OnTimeChange;
        base.DisconnectHandler(platformView);
    }

    public static void MapTime(HarmonyTimePickerHandler handler, MTimePicker view)
    {
        var t = view.Time ?? TimeSpan.Zero;
        handler.PlatformView.SelectedTime = $"{t.Hours:d2}:{t.Minutes:d2}";
    }

    public static void MapText(HarmonyTimePickerHandler handler, MTimePicker view)
    {
        // ArkUI TimePicker 节点不支持 FontFamily/FontAttributes/FontSize/TextColor/CharacterSpacing。
        // 这里必须保持 no-op；SetAttribute(401) 会让整棵页面构建失败。
        HarmonyOS.Interop.HiLog.Warn(
            "HarmonyHost",
            "[TimePicker] FontFamily/FontAttributes/FontSize/TextColor/CharacterSpacing are degraded on HarmonyOS.");
    }

    public static void MapIsOpen(HarmonyTimePickerHandler handler, MTimePicker view)
    {
        HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[TimePicker] IsOpen is degraded on HarmonyOS.");
    }

    private void OnTimeChange(ArkUINodeEvent e)
    {
        var time = new TimeSpan(e.ComponentData(0).i32, e.ComponentData(1).i32, 0);
        if (VirtualView.Time == time)
            return;
        VirtualView.Time = time;
    }
}
