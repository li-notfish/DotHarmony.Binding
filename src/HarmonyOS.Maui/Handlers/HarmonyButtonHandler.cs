using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkButton = HarmonyOS.ArkUI.Button;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Button 的 HarmonyOS Handler（ArkUI Button 节点）。Button.Text/TextColor 不在核心 IButton 接口中。</summary>
public class HarmonyButtonHandler : HarmonyViewHandler<Button, ArkButton>
{
    public static PropertyMapper<Button, HarmonyButtonHandler> Mapper = new(ViewMapper)
    {
        [nameof(Button.Text)] = MapText,
        [nameof(Button.TextColor)] = MapTextColor,
        [nameof(Button.FontSize)] = MapFontSize,
        [nameof(Button.FontFamily)] = MapFontFamily,
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

    public static void MapFontSize(HarmonyButtonHandler h, Button v)
    {
        if (v.FontSize > 0)
            h.PlatformView.FontSize = (float)v.FontSize;
    }

    public static void MapFontFamily(HarmonyButtonHandler h, Button v)
    {
        // ArkUI C API 字体族需 NODE_FONT_FAMILY 通用属性，节点类未封装（gap：随 shape 表补齐）
    }

    public static void MapTextColor(HarmonyButtonHandler h, Button v)
    {
        if (v.TextColor is { } c)
            h.PlatformView.SetFontColor(c);
    }

    public static void MapBackgroundColor(HarmonyButtonHandler h, Button v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    private void OnClick(HarmonyOS.Bindings.NativeNode.ArkUINodeEvent _)
    {
        VirtualView.SendClicked();
    }
}
