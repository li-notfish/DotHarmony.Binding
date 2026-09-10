using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkRadio = HarmonyOS.ArkUI.RadioButton;
using ArkRow = HarmonyOS.ArkUI.Row;
using ArkText = HarmonyOS.ArkUI.Text;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// IRadioButton → ArkUI RadioButton handler.
/// ArkUI RadioButton（ARKUI_NODE_RADIO）不支持 Content 文本属性（SDK 无 NODE_RADIO_CONTENT），
/// 平台视图为 Row（Radio 圆点 + Text），Content 文本由 Text 节点呈现。
/// </summary>
public class HarmonyRadioButtonHandler : ViewHandler<IRadioButton, ArkRow>, IRadioButtonHandler
{
    public static PropertyMapper<IRadioButton, IRadioButtonHandler> Mapper = new(ViewMapper)
    {
        [nameof(IRadioButton.IsChecked)] = MapIsChecked,
        [nameof(IRadioButton.Content)] = MapContent,
        [nameof(Microsoft.Maui.Controls.RadioButton.GroupName)] = MapGroupName,
    };

    private ArkRadio? _radio;
    private ArkText? _label;

    public HarmonyRadioButtonHandler() : base(Mapper) { }

    protected override ArkRow CreatePlatformView()
    {
        var row = new ArkRow();
        // ArkUI RadioButton 节点需要显式尺寸约束
        _radio = new ArkRadio();
        _radio.SetWidth(24);
        _radio.SetHeight(24);
        _label = new ArkText();
        _label.SetMarginEdges(0, 0, 0, 8);
        row.AddChild(_radio);
        row.AddChild(_label);
        return row;
    }

    protected override void ConnectHandler(ArkRow platformView)
    {
        base.ConnectHandler(platformView);
        if (_radio is not null)
            _radio.CheckedChanged += OnRadioChanged;
    }

    protected override void DisconnectHandler(ArkRow platformView)
    {
        if (_radio is not null)
            _radio.CheckedChanged -= OnRadioChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapIsChecked(IRadioButtonHandler handler, IRadioButton view)
    {
        if (handler is HarmonyRadioButtonHandler h && h._radio is not null)
            h._radio.Checked = view.IsChecked;
    }

    public static void MapContent(IRadioButtonHandler handler, IRadioButton view)
    {
        if (handler is HarmonyRadioButtonHandler h && h._label is not null)
            h._label.Content = view.Content?.ToString() ?? string.Empty;
    }

    public static void MapGroupName(IRadioButtonHandler handler, IRadioButton view)
    {
        if (handler is HarmonyRadioButtonHandler h && h._radio is not null
            && view is Microsoft.Maui.Controls.RadioButton rb)
            h._radio.Group = rb.GroupName ?? string.Empty;
    }

    private void OnRadioChanged(ArkUINodeEvent e)
    {
        var isChecked = e.ComponentData(0).i32 == 1;
        if (VirtualView.IsChecked == isChecked)
            return;
        VirtualView.IsChecked = isChecked;
    }
}
