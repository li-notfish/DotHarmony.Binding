using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Interop;
using HarmonyOS.Bindings.NativeNode;
using ArkSwitch = HarmonyOS.ArkUI.Toggle;

namespace HarmonyOS.Maui.Handlers;

public class HarmonySwitchHandler : HarmonyViewHandler<ISwitch, ArkSwitch>, ISwitchHandler
{
    public static PropertyMapper<ISwitch, ISwitchHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(ISwitch.IsOn)] = MapIsOn,
        [nameof(Microsoft.Maui.Controls.Switch.OnColor)] = MapOnColor,
        [nameof(Microsoft.Maui.Controls.Switch.ThumbColor)] = MapThumbColor,
    };

    public HarmonySwitchHandler() : base(Mapper) { }

    protected override ArkSwitch CreatePlatformView()
    {
        var toggle = new ArkSwitch();
        // ArkUI Toggle 节点需要显式尺寸约束，否则在 Row/Column 中会异常放大
        toggle.SetWidth(50);
        toggle.SetHeight(26);
        return toggle;
    }

    protected override void ConnectHandler(ArkSwitch platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Toggled += OnToggleChanged;
        platformView.Click += OnClicked;
    }

    protected override void DisconnectHandler(ArkSwitch platformView)
    {
        platformView.Toggled -= OnToggleChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapIsOn(ISwitchHandler handler, ISwitch view)
    {
        if (handler is HarmonySwitchHandler h)
            h.PlatformView.IsOn = view.IsOn;
    }

    public static void MapOnColor(ISwitchHandler handler, ISwitch view)
    {
        // Toggle.SetSelectedColor = 开态轨道色
        if (handler is HarmonySwitchHandler h && view is Microsoft.Maui.Controls.Switch { OnColor: { } c })
            h.PlatformView.SetSelectedColor(
                (byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }

    public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
    {
        if (handler is HarmonySwitchHandler h && view is Microsoft.Maui.Controls.Switch { ThumbColor: { } c })
            h.PlatformView.SetSwitchPointColor(c.ToUint());
    }

    private void OnToggleChanged(ArkUINodeEvent e)
    {
        var isOn = e.ComponentData(0).i32 == 1;
        if (VirtualView.IsOn == isOn)
            return;

        VirtualView.IsOn = isOn;
    }

    private void OnClicked(ArkUINodeEvent _)
    {
        var platformView = PlatformView;
        MainThreadDispatcher.Post(() =>
        {
            var isOn = platformView.GetIsOn();
            if (VirtualView.IsOn == isOn)
                return;

            VirtualView.IsOn = isOn;
        });
    }
}
