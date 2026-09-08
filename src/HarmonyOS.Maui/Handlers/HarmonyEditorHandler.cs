// HarmonyEditorHandler：MAUI Controls.Editor（多行）→ HarmonyOS.ArkUI.TextArea
using Microsoft.Maui.Handlers;
using ArkTextArea = HarmonyOS.ArkUI.TextArea;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyEditorHandler : ViewHandler<Microsoft.Maui.Controls.Editor, ArkTextArea>
{
    public static PropertyMapper<Microsoft.Maui.Controls.Editor, HarmonyEditorHandler> Mapper = new(ViewMapper)
    {
        [nameof(Microsoft.Maui.Controls.Editor.Text)] = MapText,
        [nameof(Microsoft.Maui.Controls.Editor.Placeholder)] = MapPlaceholder,
        [nameof(Microsoft.Maui.Controls.Editor.TextColor)] = MapTextColor,
        [nameof(Microsoft.Maui.Controls.Editor.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(Microsoft.Maui.Controls.Editor.FontSize)] = MapFontSize,
        [nameof(Microsoft.Maui.Controls.Editor.IsReadOnly)] = MapIsReadOnly,
        [nameof(Microsoft.Maui.Controls.Editor.MaxLength)] = MapMaxLength,
    };

    public HarmonyEditorHandler() : base(Mapper) { }

    protected override ArkTextArea CreatePlatformView() => new();

    protected override void ConnectHandler(ArkTextArea platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TextChange += OnTextChange;
        platformView.Submit += OnSubmit;
    }

    protected override void DisconnectHandler(ArkTextArea platformView)
    {
        platformView.TextChange -= OnTextChange;
        platformView.Submit -= OnSubmit;
        base.DisconnectHandler(platformView);
    }

    public static void MapText(HarmonyEditorHandler h, Microsoft.Maui.Controls.Editor v)
        => h.PlatformView.Text = v.Text ?? string.Empty;

    public static void MapPlaceholder(HarmonyEditorHandler h, Microsoft.Maui.Controls.Editor v)
        => h.PlatformView.Placeholder = v.Placeholder ?? string.Empty;

    public static void MapTextColor(HarmonyEditorHandler h, Microsoft.Maui.Controls.Editor v)
    {
        if (v.TextColor is { } c)
            h.PlatformView.SetFontColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }

    public static void MapPlaceholderColor(HarmonyEditorHandler h, Microsoft.Maui.Controls.Editor v)
    {
        if (v.PlaceholderColor is { } c)
            h.PlatformView.SetPlaceholderColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }

    public static void MapFontSize(HarmonyEditorHandler h, Microsoft.Maui.Controls.Editor v)
    {
        if (v.FontSize >= 0)
            h.PlatformView.FontSize = (float)v.FontSize;
    }

    public static void MapIsReadOnly(HarmonyEditorHandler h, Microsoft.Maui.Controls.Editor v)
    {
        // ArkUI 没有直接的"只读"设置，M1 忽略（TextArea 没有 ReadOnly 属性）
    }

    public static void MapMaxLength(HarmonyEditorHandler h, Microsoft.Maui.Controls.Editor v)
    {
        // M1：TextArea 节点类未暴露 MaxLength，暂忽略
    }

    private void OnTextChange(HarmonyOS.Bindings.NativeNode.ArkUINodeEvent e)
    {
        var text = e.GetString() ?? string.Empty;
        if (VirtualView.Text == text) return;
        VirtualView.Text = text;
    }

    private void OnSubmit(HarmonyOS.Bindings.NativeNode.ArkUINodeEvent e)
    {
        VirtualView.SendCompleted();
    }
}
