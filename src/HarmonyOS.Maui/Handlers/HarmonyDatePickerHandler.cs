using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using MDatePicker = Microsoft.Maui.Controls.DatePicker;
using HarmonyOS.Bindings.NativeNode;
using ArkDatePicker = HarmonyOS.ArkUI.DatePicker;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// DatePicker → ArkUI DatePicker（内嵌滚轮节点）。
/// M1 限制：MAUI DatePicker 是弹窗交互、ArkUI 是内嵌滚轮，视觉有差异（ROADMAP 1.4）。
/// </summary>
public class HarmonyDatePickerHandler : ViewHandler<MDatePicker, ArkDatePicker>
{
    public static PropertyMapper<MDatePicker, HarmonyDatePickerHandler> Mapper = new(ViewMapper)
    {
        [nameof(IDatePicker.Date)] = MapDate,
        [nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
        [nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
    };

    public HarmonyDatePickerHandler() : base(Mapper) { }

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
        if (handler is HarmonyDatePickerHandler h)
        {
            var d = view.Date ?? DateTime.Today;
            h.PlatformView.SelectedDate = $"{d.Year:d4}-{d.Month:d2}-{d.Day:d2}";
        }
    }

    public static void MapMinimumDate(HarmonyDatePickerHandler handler, MDatePicker view)
    {
        if (handler is HarmonyDatePickerHandler h)
        {
            var mn = view.MinimumDate ?? DateTime.Today;
            h.PlatformView.StartDate = $"{mn.Year:d4}-{mn.Month:d2}-{mn.Day:d2}";
        }
    }

    public static void MapMaximumDate(HarmonyDatePickerHandler handler, MDatePicker view)
    {
        if (handler is HarmonyDatePickerHandler h)
        {
            var mx = view.MaximumDate ?? DateTime.Today;
            h.PlatformView.EndDate = $"{mx.Year:d4}-{mx.Month:d2}-{mx.Day:d2}";
        }
    }

    private void OnDateChange(ArkUINodeEvent e)
    {
        var date = new DateTime(e.GetNumber(0).i32, e.GetNumber(1).i32 + 1, e.GetNumber(2).i32);
        if (VirtualView.Date == date)
            return;
        VirtualView.Date = date;
    }
}
