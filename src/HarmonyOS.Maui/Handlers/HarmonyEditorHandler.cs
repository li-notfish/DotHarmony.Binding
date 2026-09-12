using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkTextArea = HarmonyOS.ArkUI.TextArea;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Editor 的 HarmonyOS Handler（ArkUI TextArea 节点）。FontSize 不在核心 IEditor 接口中。</summary>
public class HarmonyEditorHandler : HarmonyViewHandler<Editor, ArkTextArea>
{
    public static PropertyMapper<Editor, HarmonyEditorHandler> Mapper = new(ViewMapper)
    {
        [nameof(Editor.Text)] = MapText,
        [nameof(Editor.Placeholder)] = MapPlaceholder,
        [nameof(Editor.TextColor)] = MapTextColor,
        [nameof(Editor.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(Editor.FontSize)] = MapFontSize,
        [nameof(Editor.IsReadOnly)] = MapIsReadOnly,
        [nameof(Editor.MaxLength)] = MapMaxLength,
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

    public static void MapText(HarmonyEditorHandler h, Editor v)
    {
        h.PlatformView.Text = v.Text ?? string.Empty;
    }

    public static void MapPlaceholder(HarmonyEditorHandler h, Editor v)
    {
        h.PlatformView.Placeholder = v.Placeholder ?? string.Empty;
    }

    public static void MapTextColor(HarmonyEditorHandler h, Editor v)
    {
        if (v.TextColor is { } c)
            h.PlatformView.SetFontColor(c);
    }

    public static void MapPlaceholderColor(HarmonyEditorHandler h, Editor v)
    {
        if (v.PlaceholderColor is { } c)
            h.PlatformView.SetPlaceholderColor(c);
    }

    public static void MapFontSize(HarmonyEditorHandler h, Editor v)
    {
        if (v.FontSize >= 0)
            h.PlatformView.FontSize = (float)v.FontSize;
    }

    public static void MapIsReadOnly(HarmonyEditorHandler h, Editor v)
    {
        // ArkUI TextArea 没有直接的"只读"设置，M1 忽略
    }

    public static void MapMaxLength(HarmonyEditorHandler h, Editor v)
    {
        // M1：TextArea 节点类未暴露 MaxLength，暂忽略
    }

    private void OnTextChange(ArkUINodeEvent e)
    {
        var text = e.GetString() ?? string.Empty;
        if (VirtualView.Text == text) return;
        VirtualView.Text = text;
    }

    private void OnSubmit(ArkUINodeEvent e)
    {
        VirtualView.SendCompleted();
    }
}
