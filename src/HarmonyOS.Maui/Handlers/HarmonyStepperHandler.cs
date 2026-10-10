using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkButton = HarmonyOS.ArkUI.Button;
using ArkRow = HarmonyOS.ArkUI.Row;
using ArkText = HarmonyOS.ArkUI.Text;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyStepperHandler : HarmonyViewHandler<IStepper, ArkRow>, IStepperHandler
{
    private const float ButtonSizeVp = 40f;
    private const float ValueWidthVp = 88f;

    private readonly ArkButton _decrease = new();
    private readonly ArkButton _increase = new();
    private readonly ArkText _value = new();

    public static PropertyMapper<IStepper, HarmonyStepperHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(IStepper.Interval)] = MapIncrement,
        [nameof(IStepper.Maximum)] = MapMaximum,
        [nameof(IStepper.Minimum)] = MapMinimum,
        [nameof(IStepper.Value)] = MapValue,
    };

    public HarmonyStepperHandler() : base(Mapper) { }

    protected override ArkRow CreatePlatformView()
    {
        var row = new ArkRow
        {
            AlignItems = ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER,
            JustifyContent = ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_CENTER,
        };

        foreach (var button in new[] { _decrease, _increase })
        {
            button.Type = ArkUI_ButtonType.ARKUI_BUTTON_TYPE_CIRCLE;
            button.SetWidth(ButtonSizeVp);
            button.SetHeight(ButtonSizeVp);
            button.SetMarginEdges(0, 8, 0, 8);
        }
        _decrease.Label = "-";
        _increase.Label = "+";

        _value.SetWidth(ValueWidthVp);
        _value.TextAlign = ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_CENTER;
        _value.SetMarginEdges(0, 8, 0, 8);

        row.AddChild(_decrease);
        row.AddChild(_value);
        row.AddChild(_increase);
        return row;
    }

    protected override void ConnectHandler(ArkRow platformView)
    {
        base.ConnectHandler(platformView);
        _decrease.Click += OnDecrease;
        _increase.Click += OnIncrease;
        UpdateControls();
    }

    protected override void DisconnectHandler(ArkRow platformView)
    {
        _decrease.Click -= OnDecrease;
        _increase.Click -= OnIncrease;
        base.DisconnectHandler(platformView);
    }

    public override void UpdateValue(string property)
    {
        base.UpdateValue(property);
        if (property == nameof(IView.IsEnabled))
            UpdateControls();
    }

    public static void MapIncrement(HarmonyStepperHandler handler, IStepper view)
        => handler.UpdateControls();

    public static void MapMaximum(HarmonyStepperHandler handler, IStepper view)
        => handler.UpdateControls();

    public static void MapMinimum(HarmonyStepperHandler handler, IStepper view)
        => handler.UpdateControls();

    public static void MapValue(HarmonyStepperHandler handler, IStepper view)
        => handler.UpdateControls();

    private void OnDecrease(ArkUINodeEvent _)
        => VirtualView.Value = Math.Clamp(
            VirtualView.Value - VirtualView.Interval,
            VirtualView.Minimum,
            VirtualView.Maximum);

    private void OnIncrease(ArkUINodeEvent _)
        => VirtualView.Value = Math.Clamp(
            VirtualView.Value + VirtualView.Interval,
            VirtualView.Minimum,
            VirtualView.Maximum);

    private void UpdateControls()
    {
        var view = VirtualView;
        var enabled = view.IsEnabled;
        _decrease.Enabled = enabled && view.Value > view.Minimum;
        _increase.Enabled = enabled && view.Value < view.Maximum;
        _value.Content = view.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    IStepper IStepperHandler.VirtualView => VirtualView;
    object IStepperHandler.PlatformView => PlatformView;
}
