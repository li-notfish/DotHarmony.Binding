using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkRadio = HarmonyOS.ArkUI.RadioButton;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// IRadioButton → ArkUI RadioButton handler.
/// M1 限制：ArkUI RadioButton（ARKUI_NODE_RADIO）不支持 Content 文本属性（SDK 无 NODE_RADIO_CONTENT），
/// 仅显示圆点。Content 文本需等 SDK 补充或改用 Row+Text 包装方案。
/// </summary>
public class HarmonyRadioButtonHandler : ViewHandler<IRadioButton, ArkRadio>, IRadioButtonHandler
{
    public static PropertyMapper<IRadioButton, IRadioButtonHandler> Mapper = new(ViewMapper)
    {
        [nameof(IRadioButton.IsChecked)] = MapIsChecked,
    };

    public HarmonyRadioButtonHandler() : base(Mapper) { }

    protected override ArkRadio CreatePlatformView()
    {
        var radio = new ArkRadio();
        // ArkUI RadioButton 节点需要显式尺寸约束
        radio.SetWidth(24);
        radio.SetHeight(24);
        return radio;
    }

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
