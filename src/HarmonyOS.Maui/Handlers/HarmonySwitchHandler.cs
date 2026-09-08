using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkSwitch = HarmonyOS.ArkUI.Toggle;

namespace HarmonyOS.Maui.Handlers;

public class HarmonySwitchHandler : ViewHandler<ISwitch, ArkSwitch>, ISwitchHandler
{
    public static PropertyMapper<ISwitch, ISwitchHandler> Mapper = new(ViewMapper)
    {
        [nameof(ISwitch.IsOn)] = MapIsOn,
    };

    public HarmonySwitchHandler() : base(Mapper) { }

    protected override ArkSwitch CreatePlatformView() => new();

    protected override void ConnectHandler(ArkSwitch platformView)
    {
        base.ConnectHandler(platformView);
        platformView.IsOnChange += OnToggleChanged;
    }

    protected override void DisconnectHandler(ArkSwitch platformView)
    {
        platformView.IsOnChange -= OnToggleChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapIsOn(ISwitchHandler handler, ISwitch view)
    {
        if (handler is HarmonySwitchHandler h)
            h.PlatformView.IsOn = view.IsOn;
    }

    private void OnToggleChanged(ArkUINodeEvent e)
    {
        var isOn = e.GetNumber(0).i32 == 1;
        if (VirtualView.IsOn == isOn)
            return;
        VirtualView.IsOn = isOn;
    }
}
