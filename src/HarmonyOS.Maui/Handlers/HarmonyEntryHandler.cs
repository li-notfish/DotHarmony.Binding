using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkTextInput = HarmonyOS.ArkUI.TextInput;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyEntryHandler : HarmonyViewHandler<IEntry, ArkTextInput>, IEntryHandler
{
    public static PropertyMapper<IEntry, IEntryHandler> Mapper = new(ViewMapper)
    {
        [nameof(IEntry.Text)] = MapText,
        [nameof(IEntry.Placeholder)] = MapPlaceholder,
        [nameof(IEntry.TextColor)] = MapTextColor,
        [nameof(Microsoft.Maui.Controls.Entry.FontSize)] = MapFontSize,
        [nameof(IEntry.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(IEntry.IsPassword)] = MapIsPassword,
        [nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
        [nameof(IEntry.MaxLength)] = MapMaxLength,
        [nameof(IEntry.ReturnType)] = MapReturnType,
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

    public static void MapText(IEntryHandler handler, IEntry view)
    {
        if (handler is HarmonyEntryHandler h)
            h.PlatformView.Text = view.Text ?? string.Empty;
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
