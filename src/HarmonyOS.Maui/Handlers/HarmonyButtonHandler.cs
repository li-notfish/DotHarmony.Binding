// HarmonyButtonHandler：MAUI Controls.Button（VirtualView）→ HarmonyOS.ArkUI.Button（platform view）
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ArkButton = HarmonyOS.ArkUI.Button;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// MAUI Button 的 HarmonyOS Handler。
/// 属性经 PropertyMapper 流入 ArkUI 原生节点属性；点击经 SendClicked 回流 MAUI 命令体系。
/// </summary>
public class HarmonyButtonHandler : ViewHandler<Microsoft.Maui.Controls.Button, ArkButton>
{
    public static PropertyMapper<Microsoft.Maui.Controls.Button, HarmonyButtonHandler> Mapper =
        new(ViewHandler.ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.Button.Text)] = (h, v) =>
                h.PlatformView.Label = v.Text ?? string.Empty,
            [nameof(Microsoft.Maui.Controls.Button.TextColor)] = (h, v) =>
            {
                if (v.TextColor is { } c)
                    h.PlatformView.SetFontColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
            },
            [nameof(Microsoft.Maui.Controls.Button.BackgroundColor)] = (h, v) =>
            {
                if (v.BackgroundColor is { } c)
                    h.PlatformView.SetBackgroundColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
            },
        };

    public HarmonyButtonHandler() : base(Mapper) { }

    protected override ArkButton CreatePlatformView() => new();

    protected override void ConnectHandler(ArkButton platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Click += _ => VirtualView.SendClicked();
    }

    protected override void DisconnectHandler(ArkButton platformView)
    {
        platformView.Click -= _ => { };
        base.DisconnectHandler(platformView);
    }
}
