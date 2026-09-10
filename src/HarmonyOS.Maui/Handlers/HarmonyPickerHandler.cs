using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkTextPicker = HarmonyOS.ArkUI.TextPicker;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// Picker → ArkUI TextPicker（内嵌滚轮节点）。
/// M1 限制：MAUI Picker 是弹窗交互、TextPicker 是内嵌滚轮，视觉有差异（ROADMAP 1.4）；
/// ItemsSource 仅在映射时读取（列表变更不自动同步）。
/// </summary>
public class HarmonyPickerHandler : ViewHandler<Picker, ArkTextPicker>
{
    public static PropertyMapper<Picker, HarmonyPickerHandler> Mapper = new(ViewMapper)
    {
        [nameof(Picker.ItemsSource)] = MapItemsSource,
        [nameof(Picker.SelectedIndex)] = MapSelectedIndex,
    };

    public HarmonyPickerHandler() : base(Mapper) { }

    protected override ArkTextPicker CreatePlatformView() => new();

    protected override void ConnectHandler(ArkTextPicker platformView)
    {
        base.ConnectHandler(platformView);
        platformView.OnChange += OnChange;
    }

    protected override void DisconnectHandler(ArkTextPicker platformView)
    {
        platformView.OnChange -= OnChange;
        base.DisconnectHandler(platformView);
    }

    public static void MapItemsSource(HarmonyPickerHandler handler, Picker view)
    {
        var items = new List<string>();
        if (view.ItemsSource is not null)
        {
            foreach (var item in view.ItemsSource)
                items.Add(item?.ToString() ?? string.Empty);
        }
        handler.PlatformView.SetRange(items);
        handler.PlatformView.SelectedIndex = Math.Max(view.SelectedIndex, 0);
    }

    public static void MapSelectedIndex(HarmonyPickerHandler handler, Picker view)
    {
        handler.PlatformView.SelectedIndex = Math.Max(view.SelectedIndex, 0);
    }

    private void OnChange(ArkUINodeEvent e)
    {
        var index = e.ComponentData(0).i32;
        if (VirtualView.SelectedIndex == index)
            return;
        VirtualView.SelectedIndex = index;
    }
}
