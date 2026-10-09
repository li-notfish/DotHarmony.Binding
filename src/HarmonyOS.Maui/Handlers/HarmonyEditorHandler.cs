using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkTextArea = HarmonyOS.ArkUI.TextArea;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Editor 的 HarmonyOS Handler（ArkUI TextArea 节点）。FontSize 不在核心 IEditor 接口中。</summary>
public class HarmonyEditorHandler : HarmonyViewHandler<Editor, ArkTextArea>, IEditorHandler
{
    public static PropertyMapper<Editor, HarmonyEditorHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(Editor.Text)] = MapText,
        [nameof(Editor.Placeholder)] = MapPlaceholder,
        [nameof(Editor.TextColor)] = MapTextColor,
        [nameof(Editor.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(Editor.FontSize)] = MapFontSize,
        [nameof(Editor.FontFamily)] = MapFontFamily,
        [nameof(Editor.IsReadOnly)] = MapIsReadOnly,
        [nameof(Editor.MaxLength)] = MapMaxLength,
        [nameof(Editor.Keyboard)] = MapKeyboard,
        [nameof(Editor.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(Editor.FontAttributes)] = MapFontAttributes,
        [nameof(Editor.CursorPosition)] = MapCursorPosition,
        [nameof(Editor.SelectionLength)] = MapSelectionLength,
        [nameof(Editor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
        [nameof(Editor.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
        [nameof(Editor.AutoSize)] = MapAutoSize,
        [nameof(Editor.TextTransform)] = MapTextTransform,
        [nameof(Editor.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(Editor.VerticalTextAlignment)] = MapVerticalTextAlignment,
    };

    public HarmonyEditorHandler() : base(Mapper) { }

    IEditor IEditorHandler.VirtualView => VirtualView;

    object IEditorHandler.PlatformView => PlatformView;

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
        h.PlatformView.Text = TransformText(v.Text ?? string.Empty, v);
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
        if (v.FontSize > 0)
            h.PlatformView.FontSize = (float)v.FontSize;
    }

    public static void MapFontFamily(HarmonyEditorHandler h, Editor v)
    {
        if (!string.IsNullOrEmpty(v.FontFamily))
            h.PlatformView.SetFontFamily(v.FontFamily);
    }

    public static void MapIsReadOnly(HarmonyEditorHandler h, Editor v)
    {
        h.PlatformView.ReadOnly = v.IsReadOnly;
    }

    public static void MapMaxLength(HarmonyEditorHandler h, Editor v)
    {
        h.PlatformView.MaxLength = v.MaxLength;
    }

    public static void MapKeyboard(HarmonyEditorHandler h, Editor v)
    {
        h.PlatformView.InputType = v.Keyboard == Microsoft.Maui.Keyboard.Email
            ? ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_EMAIL
            : v.Keyboard == Microsoft.Maui.Keyboard.Numeric
                ? ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_NUMBER
                : v.Keyboard == Microsoft.Maui.Keyboard.Telephone
                    ? ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_PHONE_NUMBER
                    : v.Keyboard == Microsoft.Maui.Keyboard.Password
                        ? ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_PASSWORD
                        : ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_NORMAL;
    }

    public static void MapCharacterSpacing(HarmonyEditorHandler h, Editor v)
        => h.PlatformView.SetLetterSpacing((float)v.CharacterSpacing);

    public static void MapFontAttributes(HarmonyEditorHandler h, Editor v)
    {
        h.PlatformView.SetFontWeight(
            v.FontAttributes.HasFlag(FontAttributes.Bold)
                ? ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W700
                : ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W400);
        h.PlatformView.SetFontStyle(
            v.FontAttributes.HasFlag(FontAttributes.Italic)
                ? ArkUI_FontStyle.ARKUI_FONT_STYLE_ITALIC
                : ArkUI_FontStyle.ARKUI_FONT_STYLE_NORMAL);
    }

    public static void MapCursorPosition(HarmonyEditorHandler h, Editor v)
    {
        h.PlatformView.CaretPosition = v.CursorPosition;
        h.PlatformView.SetSelection(v.CursorPosition, v.SelectionLength);
    }

    public static void MapSelectionLength(HarmonyEditorHandler h, Editor v)
        => MapCursorPosition(h, v);

    public static void MapIsTextPredictionEnabled(HarmonyEditorHandler h, Editor v)
        => h.PlatformView.EnablePreviewText = v.IsTextPredictionEnabled;

    public static void MapIsSpellCheckEnabled(HarmonyEditorHandler h, Editor v)
    {
        // ArkUI TextArea 目前没有独立的 spell-check 属性。
        HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[Editor] IsSpellCheckEnabled is degraded on HarmonyOS.");
    }

    public static void MapAutoSize(HarmonyEditorHandler h, Editor v)
    {
        // ArkUI TextArea 的行高由内容自然撑起；AutoSize 语义保留在 Controls 层，
        // 原生侧不再额外改变高度，避免与父布局的 Auto/Star 轨道打架。
        HarmonyOS.Interop.HiLog.Info("HarmonyHost", "[Editor] AutoSize uses natural TextArea sizing on HarmonyOS.");
    }

    public static void MapTextTransform(HarmonyEditorHandler h, Editor v)
        => h.PlatformView.Text = TransformText(v.Text ?? string.Empty, v);

    public static void MapHorizontalTextAlignment(HarmonyEditorHandler h, Editor v)
    {
        // ArkUI TextArea 没有公开的水平对齐属性。
        HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[Editor] HorizontalTextAlignment is degraded on HarmonyOS.");
    }

    public static void MapVerticalTextAlignment(HarmonyEditorHandler h, Editor v)
    {
        // ArkUI TextArea 没有公开的垂直对齐属性。
        HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[Editor] VerticalTextAlignment is degraded on HarmonyOS.");
    }

    private static string TransformText(string text, Editor v)
    {
        return v.TextTransform switch
        {
            TextTransform.Uppercase => text.ToUpperInvariant(),
            TextTransform.Lowercase => text.ToLowerInvariant(),
            _ => text,
        };
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
