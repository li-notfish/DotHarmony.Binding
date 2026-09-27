using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkButton = HarmonyOS.ArkUI.Button;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Button 的 HarmonyOS Handler（ArkUI Button 节点）。Button.Text/TextColor 不在核心 IButton 接口中。</summary>
public class HarmonyButtonHandler : HarmonyViewHandler<Button, ArkButton>
{
    /// <summary>点击事件日志开关（插值分配走在调用点，仅在排障时打开）</summary>
    private static readonly bool LogClick = false;

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
        // 不设置内建 Label：设了 label 的 Button 会忽略/不布局子节点（实测子 Text 零尺寸不可见）
        h.PlatformView.SetLabelChild(v.Text ?? string.Empty);
    }

    public static void MapFontSize(HarmonyButtonHandler h, Button v)
    {
        // MAUI FontSize=0(Default) 语义=平台默认；NDK 节点无样式表，须显式补默认字号
        var fs = v.FontSize > 0 ? (float)v.FontSize : 16f;
        h.PlatformView.FontSize = fs;
        h.PlatformView.SetLabelChildFontSize(fs);
    }

    public static void MapFontFamily(HarmonyButtonHandler h, Button v)
    {
        // NODE_FONT_FAMILY 是通用属性；按钮文字实际由子 Text 承载，双写保证样式一致
        h.PlatformView.SetFontFamily(v.FontFamily);
        h.PlatformView.SetLabelChildFontFamily(v.FontFamily);
    }

    public static void MapTextColor(HarmonyButtonHandler h, Button v)
    {
        if (v.TextColor is { } c)
        {
            h.PlatformView.SetFontColor(c);
            h.PlatformView.SetLabelChildColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
        }
    }

    public static void MapBackgroundColor(HarmonyButtonHandler h, Button v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    private void OnClick(HarmonyOS.Bindings.NativeNode.ArkUINodeEvent _)
    {
        if (LogClick) HarmonyOS.Interop.HiLog.Debug("HarmonyHost", $"[Click] {VirtualView?.Text}");
        VirtualView.SendClicked();
    }
}
