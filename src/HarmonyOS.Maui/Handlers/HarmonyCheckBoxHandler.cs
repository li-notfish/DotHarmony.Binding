using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkCheckBox = HarmonyOS.ArkUI.CheckBox;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyCheckBoxHandler : ViewHandler<ICheckBox, ArkCheckBox>, ICheckBoxHandler
{
    public static PropertyMapper<ICheckBox, ICheckBoxHandler> Mapper = new(ViewMapper)
    {
        [nameof(ICheckBox.IsChecked)] = MapIsChecked,
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
        platformView.IsOnChange += OnCheckBoxChanged;
    }

    protected override void DisconnectHandler(ArkCheckBox platformView)
    {
        platformView.IsOnChange -= OnCheckBoxChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapIsChecked(ICheckBoxHandler handler, ICheckBox view)
    {
        if (handler is HarmonyCheckBoxHandler h)
            h.PlatformView.IsSelected = view.IsChecked;
    }

    private void OnCheckBoxChanged(ArkUINodeEvent e)
    {
        var isChecked = e.GetNumber(0).i32 == 1;
        if (VirtualView.IsChecked == isChecked)
            return;
        VirtualView.IsChecked = isChecked;
    }
}
