using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkButton = HarmonyOS.ArkUI.Button;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Button 的 HarmonyOS Handler（ArkUI Button 节点）。Button.Text/TextColor 不在核心 IButton 接口中。</summary>
public class HarmonyButtonHandler : ViewHandler<Button, ArkButton>
{
    public static PropertyMapper<Button, HarmonyButtonHandler> Mapper = new(ViewMapper)
    {
        [nameof(Button.Text)] = MapText,
        [nameof(Button.TextColor)] = MapTextColor,
        [nameof(Button.BackgroundColor)] = MapBackgroundColor,
    };

    public HarmonyButtonHandler() : base(Mapper) { }

    protected override ArkButton CreatePlatformView() => new();

    protected override void ConnectHandler(ArkButton platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Click += OnClick;
    }

    protected override void DisconnectHandler(ArkButton platformView)
    {
        platformView.Click -= OnClick;
        base.DisconnectHandler(platformView);
    }

    public static void MapText(HarmonyButtonHandler h, Button v)
    {
        h.PlatformView.Label = v.Text ?? string.Empty;
    }

    public static void MapTextColor(HarmonyButtonHandler h, Button v)
    {
        if (v.TextColor is { } c)
            h.PlatformView.SetFontColor(
                (byte)(c.Red * 255), (byte)(c.Green * 255),
                (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }

    public static void MapBackgroundColor(HarmonyButtonHandler h, Button v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(
                (byte)(c.Red * 255), (byte)(c.Green * 255),
                (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }

    private void OnClick(HarmonyOS.Bindings.NativeNode.ArkUINodeEvent _)
    {
        VirtualView.SendClicked();
    }
}
