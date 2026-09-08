using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkRadio = HarmonyOS.ArkUI.RadioButton;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyRadioButtonHandler : ViewHandler<IRadioButton, ArkRadio>, IRadioButtonHandler
{
    public static PropertyMapper<IRadioButton, IRadioButtonHandler> Mapper = new(ViewMapper)
    {
        [nameof(IRadioButton.IsChecked)] = MapIsChecked,
    };

    public HarmonyRadioButtonHandler() : base(Mapper) { }

    protected override ArkRadio CreatePlatformView() => new();

    protected override void ConnectHandler(ArkRadio platformView)
    {
        base.ConnectHandler(platformView);
        platformView.IsOnChange += OnRadioChanged;
    }

    protected override void DisconnectHandler(ArkRadio platformView)
    {
        platformView.IsOnChange -= OnRadioChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapIsChecked(IRadioButtonHandler handler, IRadioButton view)
    {
        if (handler is HarmonyRadioButtonHandler h)
            h.PlatformView.IsChecked = view.IsChecked;
    }

    private void OnRadioChanged(ArkUINodeEvent e)
    {
        var isChecked = e.GetNumber(0).i32 == 1;
        if (VirtualView.IsChecked == isChecked)
            return;
        VirtualView.IsChecked = isChecked;
    }
}
