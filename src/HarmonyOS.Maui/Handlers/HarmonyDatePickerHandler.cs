#nullable enable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkDatePicker = HarmonyOS.ArkUI.DatePicker;
using MDatePicker = Microsoft.Maui.Controls.DatePicker;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyDatePickerHandler :
    HarmonyViewHandler<MDatePicker, ArkDatePicker>, IDatePickerHandler
{
    public static PropertyMapper<MDatePicker, HarmonyDatePickerHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(IDatePicker.Date)] = MapDate,
        [nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
        [nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
        [nameof(MDatePicker.Format)] = MapText,
        [nameof(MDatePicker.TextColor)] = MapText,
        [nameof(MDatePicker.CharacterSpacing)] = MapText,
        [nameof(MDatePicker.FontFamily)] = MapText,
        [nameof(MDatePicker.FontSize)] = MapText,
        [nameof(MDatePicker.FontAttributes)] = MapText,
        [nameof(MDatePicker.FontAutoScalingEnabled)] = MapText,
        [nameof(MDatePicker.IsOpen)] = MapIsOpen,
    };

    public HarmonyDatePickerHandler() : base(Mapper) { }

    IDatePicker IDatePickerHandler.VirtualView => VirtualView;

    object IDatePickerHandler.PlatformView => PlatformView;

    protected override ArkDatePicker CreatePlatformView() => new();

    protected override void ConnectHandler(ArkDatePicker platformView)
    {
        base.ConnectHandler(platformView);
        platformView.OnDateChange += OnDateChange;
    }

    protected override void DisconnectHandler(ArkDatePicker platformView)
    {
        platformView.OnDateChange -= OnDateChange;
        base.DisconnectHandler(platformView);
    }

    public static void MapDate(HarmonyDatePickerHandler handler, MDatePicker view)
    {
        var d = view.Date ?? DateTime.Today;
        handler.PlatformView.SelectedDate = $"{d.Year:d4}-{d.Month:d2}-{d.Day:d2}";
    }

    public static void MapMinimumDate(HarmonyDatePickerHandler handler, MDatePicker view)
    {
        var mn = view.MinimumDate ?? DateTime.Today;
        handler.PlatformView.StartDate = $"{mn.Year:d4}-{mn.Month:d2}-{mn.Day:d2}";
    }

    public static void MapMaximumDate(HarmonyDatePickerHandler handler, MDatePicker view)
    {
        var mx = view.MaximumDate ?? DateTime.Today;
        handler.PlatformView.EndDate = $"{mx.Year:d4}-{mx.Month:d2}-{mx.Day:d2}";
    }

    public static void MapText(HarmonyDatePickerHandler handler, MDatePicker view)
    {
        // ArkUI DatePicker 节点不支持 FontFamily/FontAttributes/FontSize/TextColor/CharacterSpacing。
        // 这里必须保持 no-op；SetAttribute(401) 会让整棵页面构建失败。
        HarmonyOS.Interop.HiLog.Warn(
            "HarmonyHost",
            "[DatePicker] FontFamily/FontAttributes/FontSize/TextColor/CharacterSpacing are degraded on HarmonyOS.");
    }

    public static void MapIsOpen(HarmonyDatePickerHandler handler, MDatePicker view)
    {
        HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[DatePicker] IsOpen is degraded on HarmonyOS.");
    }

    private void OnDateChange(ArkUINodeEvent e)
    {
        var date = new DateTime(e.ComponentData(0).i32, e.ComponentData(1).i32 + 1, e.ComponentData(2).i32);
        if (VirtualView.Date == date)
            return;
        VirtualView.Date = date;
    }
}
