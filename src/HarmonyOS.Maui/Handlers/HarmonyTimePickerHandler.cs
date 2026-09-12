using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using MTimePicker = Microsoft.Maui.Controls.TimePicker;
using HarmonyOS.Bindings.NativeNode;
using ArkTimePicker = HarmonyOS.ArkUI.TimePicker;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// TimePicker → ArkUI TimePicker（内嵌滚轮节点）。
/// M1 限制：MAUI TimePicker 是弹窗交互、ArkUI 是内嵌滚轮，视觉有差异（ROADMAP 1.4）。
/// </summary>
public class HarmonyTimePickerHandler : HarmonyViewHandler<MTimePicker, ArkTimePicker>
{
    public static PropertyMapper<MTimePicker, HarmonyTimePickerHandler> Mapper = new(ViewMapper)
    {
        [nameof(ITimePicker.Time)] = MapTime,
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
        if (handler is HarmonyTimePickerHandler h)
        {
            var t = view.Time ?? TimeSpan.Zero;
            h.PlatformView.SelectedTime = $"{t.Hours:d2}:{t.Minutes:d2}";
        }
    }

    private void OnTimeChange(ArkUINodeEvent e)
    {
        var time = new TimeSpan(e.ComponentData(0).i32, e.ComponentData(1).i32, 0);
        if (VirtualView.Time == time)
            return;
        VirtualView.Time = time;
    }
}
