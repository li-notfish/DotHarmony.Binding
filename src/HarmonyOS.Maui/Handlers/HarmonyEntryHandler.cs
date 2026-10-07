using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkTextInput = HarmonyOS.ArkUI.TextInput;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyEntryHandler : HarmonyViewHandler<IEntry, ArkTextInput>, IEntryHandler
{
    public static PropertyMapper<IEntry, IEntryHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(IEntry.Text)] = MapText,
        [nameof(IEntry.Placeholder)] = MapPlaceholder,
        [nameof(IEntry.TextColor)] = MapTextColor,
        [nameof(Microsoft.Maui.Controls.Entry.FontSize)] = MapFontSize,
        [nameof(Microsoft.Maui.Controls.Entry.FontFamily)] = MapFontFamily,
        [nameof(IEntry.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(IEntry.IsPassword)] = MapIsPassword,
        [nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
        [nameof(IEntry.MaxLength)] = MapMaxLength,
        [nameof(IEntry.ReturnType)] = MapReturnType,
        [nameof(Microsoft.Maui.Controls.Entry.Keyboard)] = MapKeyboard,
        [nameof(Microsoft.Maui.Controls.Entry.ClearButtonVisibility)] = MapClearButtonVisibility,
        [nameof(Microsoft.Maui.Controls.Entry.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(Microsoft.Maui.Controls.Entry.FontAttributes)] = MapFontAttributes,
        [nameof(Microsoft.Maui.Controls.Entry.CursorPosition)] = MapCursorPosition,
        [nameof(Microsoft.Maui.Controls.Entry.SelectionLength)] = MapSelectionLength,
        [nameof(Microsoft.Maui.Controls.Entry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
        [nameof(Microsoft.Maui.Controls.Entry.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
        [nameof(Microsoft.Maui.Controls.Entry.TextTransform)] = MapTextTransform,
        [nameof(Microsoft.Maui.Controls.Entry.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(Microsoft.Maui.Controls.Entry.VerticalTextAlignment)] = MapVerticalTextAlignment,
    };

    public HarmonyEntryHandler() : base(Mapper) { }

    protected override ArkTextInput CreatePlatformView() => new();

    protected override void ConnectHandler(ArkTextInput platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TextChange += OnTextChange;
        platformView.Submit += OnSubmit;
    }

    protected override void DisconnectHandler(ArkTextInput platformView)
    {
        platformView.TextChange -= OnTextChange;
        platformView.Submit -= OnSubmit;
        base.DisconnectHandler(platformView);
    }

    public static void MapFontSize(IEntryHandler handler, IEntry view)
    {
        // FontSize 在 Controls 类型上（核心接口 IEntry 未暴露）
        if (handler is HarmonyEntryHandler h && view is Microsoft.Maui.Controls.Entry entry && entry.FontSize > 0)
            h.PlatformView.FontSize = (float)entry.FontSize;
    }

    public static void MapFontFamily(IEntryHandler handler, IEntry view)
    {
        // FontFamily 在 Controls 类型上（核心接口 IEntry 未暴露）
        if (handler is HarmonyEntryHandler h && view is Microsoft.Maui.Controls.Entry { FontFamily: { } ff })
            h.PlatformView.SetFontFamily(ff);
    }

    public static void MapText(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h)
            h.PlatformView.Text = TransformText(view.Text ?? string.Empty, view);
    }

    public static void MapPlaceholder(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h)
            h.PlatformView.Placeholder = view.Placeholder ?? string.Empty;
    }

    public static void MapTextColor(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h && view.TextColor is { } c)
            h.PlatformView.SetFontColor(c);
    }

    public static void MapPlaceholderColor(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h && view.PlaceholderColor is { } c)
            h.PlatformView.SetPlaceholderColor(c);
    }

    public static void MapIsPassword(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h)
            h.PlatformView.InputType = view.IsPassword
                ? ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_PASSWORD
                : ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_NORMAL;
    }

    public static void MapIsReadOnly(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h)
            h.PlatformView.ReadOnly = view.IsReadOnly;
    }

    public static void MapMaxLength(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h)
            h.PlatformView.MaxLength = view.MaxLength;
    }

    public static void MapReturnType(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h)
            h.PlatformView.EnterKeyType = view.ReturnType switch
            {
                ReturnType.Go => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_GO,
                ReturnType.Search => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_SEARCH,
                ReturnType.Send => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_SEND,
                ReturnType.Next => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_NEXT,
                ReturnType.Done => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_DONE,
                _ => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_DONE,
            };
    }

    public static void MapKeyboard(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h)
            h.PlatformView.InputType = view.Keyboard == Microsoft.Maui.Keyboard.Email
                ? ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_EMAIL
                : view.Keyboard == Microsoft.Maui.Keyboard.Numeric
                    ? ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_NUMBER
                    : view.Keyboard == Microsoft.Maui.Keyboard.Telephone
                        ? ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_PHONE_NUMBER
                        : view.Keyboard == Microsoft.Maui.Keyboard.Password
                            ? ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_PASSWORD
                            : ArkUI_TextInputType.ARKUI_TEXTINPUT_TYPE_NORMAL;
    }

    public static void MapClearButtonVisibility(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h && view is Microsoft.Maui.Controls.Entry entry)
            h.PlatformView.ShowClearButton = entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing;
    }

    public static void MapCharacterSpacing(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h && view is Microsoft.Maui.Controls.Entry entry)
            h.PlatformView.SetLetterSpacing((float)entry.CharacterSpacing);
    }

    public static void MapFontAttributes(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h && view is Microsoft.Maui.Controls.Entry entry)
        {
            h.PlatformView.SetFontWeight(
                entry.FontAttributes.HasFlag(FontAttributes.Bold)
                    ? ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W700
                    : ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W400);
            h.PlatformView.SetFontStyle(
                entry.FontAttributes.HasFlag(FontAttributes.Italic)
                    ? ArkUI_FontStyle.ARKUI_FONT_STYLE_ITALIC
                    : ArkUI_FontStyle.ARKUI_FONT_STYLE_NORMAL);
        }
    }

    public static void MapCursorPosition(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h && view is Microsoft.Maui.Controls.Entry entry)
        {
            h.PlatformView.CaretPosition = entry.CursorPosition;
            h.PlatformView.SetSelection(entry.CursorPosition, entry.SelectionLength);
        }
    }

    public static void MapSelectionLength(IEntryHandler handler, IEntry view)
        => MapCursorPosition(handler, view);

    public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h && view is Microsoft.Maui.Controls.Entry entry)
            h.PlatformView.EnablePreviewText = entry.IsTextPredictionEnabled;
    }

    public static void MapIsSpellCheckEnabled(IEntryHandler handler, IEntry view)
    {
        // ArkUI TextInput 目前没有独立的 spell-check 属性；MAUI 侧状态保持不变。
        HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[Entry] IsSpellCheckEnabled is degraded on HarmonyOS.");
    }

    public static void MapTextTransform(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h)
            h.PlatformView.Text = TransformText(view.Text ?? string.Empty, view);
    }

    public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry view)
    {
        // ArkUI TextInput 没有公开的水平对齐属性。
        HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[Entry] HorizontalTextAlignment is degraded on HarmonyOS.");
    }

    public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry view)
    {
        // ArkUI TextInput 没有公开的垂直对齐属性。
        HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[Entry] VerticalTextAlignment is degraded on HarmonyOS.");
    }

    private static string TransformText(string text, IEntry view)
    {
        if (view is not Microsoft.Maui.Controls.Entry entry)
            return text;
        return entry.TextTransform switch
        {
            TextTransform.Uppercase => text.ToUpperInvariant(),
            TextTransform.Lowercase => text.ToLowerInvariant(),
            _ => text,
        };
    }

    private void OnTextChange(ArkUINodeEvent e)
    {
        var text = e.GetString() ?? string.Empty;
        if (VirtualView.Text == text)
            return;
        VirtualView.Text = text;
    }

    private void OnSubmit(ArkUINodeEvent e)
    {
        VirtualView.Completed();
    }
}
