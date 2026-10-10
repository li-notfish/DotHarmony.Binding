using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Interop;
using HarmonyOS.Bindings.NativeNode;
using ArkCheckBox = HarmonyOS.ArkUI.CheckBox;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyCheckBoxHandler : HarmonyViewHandler<ICheckBox, ArkCheckBox>, ICheckBoxHandler
{
    public static PropertyMapper<ICheckBox, ICheckBoxHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(ICheckBox.IsChecked)] = MapIsChecked,
        [nameof(Microsoft.Maui.Controls.CheckBox.Color)] = MapColor,
    };

    public HarmonyCheckBoxHandler() : base(Mapper) { }

    protected override ArkCheckBox CreatePlatformView()
    {
        var cb = new ArkCheckBox();
        // ArkUI CheckBox 节点需要显式尺寸约束，否则在 Row/Column 中不可见
        cb.SetWidth(24);
        cb.SetHeight(24);
        return cb;
    }

    protected override void ConnectHandler(ArkCheckBox platformView)
    {
        base.ConnectHandler(platformView);
        platformView.CheckedChanged += OnCheckBoxChanged;
        platformView.Click += OnClicked;
    }

    protected override void DisconnectHandler(ArkCheckBox platformView)
    {
        platformView.CheckedChanged -= OnCheckBoxChanged;
        platformView.Click -= OnClicked;
        base.DisconnectHandler(platformView);
    }

    public static void MapIsChecked(ICheckBoxHandler handler, ICheckBox view)
    {
        if (handler is HarmonyCheckBoxHandler h)
            h.PlatformView.Select = view.IsChecked;
    }

    public static void MapColor(ICheckBoxHandler handler, ICheckBox view)
    {
        // MAUI CheckBox.Color = 选中态标记色 → NODE_CHECKBOX_SELECT_COLOR
        if (handler is HarmonyCheckBoxHandler h && view is Microsoft.Maui.Controls.CheckBox { Color: { } c })
            h.PlatformView.SetSelectColor(c.ToUint());
    }

    private void OnCheckBoxChanged(ArkUINodeEvent e)
    {
        var isChecked = e.ComponentData(0).i32 == 1;
        if (VirtualView.IsChecked == isChecked)
            return;

        VirtualView.IsChecked = isChecked;
    }

    private void OnClicked(ArkUINodeEvent _)
    {
        var platformView = PlatformView;
        MainThreadDispatcher.Post(() =>
        {
            var isChecked = platformView.GetIsChecked();
            if (VirtualView.IsChecked == isChecked)
                return;

            VirtualView.IsChecked = isChecked;
        });
    }
}
