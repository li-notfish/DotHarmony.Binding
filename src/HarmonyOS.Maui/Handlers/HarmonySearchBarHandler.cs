using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkTextInput = HarmonyOS.ArkUI.TextInput;

namespace HarmonyOS.Maui.Handlers;

public class HarmonySearchBarHandler : HarmonyViewHandler<ISearchBar, ArkTextInput>, ISearchBarHandler
{
    public static PropertyMapper<ISearchBar, HarmonySearchBarHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(ISearchBar.Text)] = MapText,
        [nameof(ISearchBar.Placeholder)] = MapPlaceholder,
        [nameof(ISearchBar.TextColor)] = MapTextColor,
        [nameof(Microsoft.Maui.Controls.SearchBar.FontSize)] = MapFontSize,
        [nameof(Microsoft.Maui.Controls.SearchBar.FontFamily)] = MapFontFamily,
        [nameof(ISearchBar.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(ISearchBar.IsReadOnly)] = MapIsReadOnly,
        [nameof(ISearchBar.MaxLength)] = MapMaxLength,
        [nameof(ISearchBar.ReturnType)] = MapReturnType,
    };

    public HarmonySearchBarHandler() : base(Mapper) { }

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

    public static void MapFontSize(HarmonySearchBarHandler handler, ISearchBar view)
    {
        if (view is Microsoft.Maui.Controls.SearchBar { FontSize: > 0 } searchBar)
            handler.PlatformView.FontSize = (float)searchBar.FontSize;
    }

    public static void MapFontFamily(HarmonySearchBarHandler handler, ISearchBar view)
    {
        if (view is Microsoft.Maui.Controls.SearchBar { FontFamily: { } fontFamily })
            handler.PlatformView.SetFontFamily(fontFamily);
    }

    public static void MapText(HarmonySearchBarHandler handler, ISearchBar view)
        => handler.PlatformView.Text = view.Text ?? string.Empty;

    public static void MapPlaceholder(HarmonySearchBarHandler handler, ISearchBar view)
        => handler.PlatformView.Placeholder = view.Placeholder ?? string.Empty;

    public static void MapTextColor(HarmonySearchBarHandler handler, ISearchBar view)
    {
        if (view.TextColor is { } color)
            handler.PlatformView.SetFontColor(color);
    }

    public static void MapPlaceholderColor(HarmonySearchBarHandler handler, ISearchBar view)
    {
        if (view.PlaceholderColor is { } color)
            handler.PlatformView.SetPlaceholderColor(color);
    }

    public static void MapIsReadOnly(HarmonySearchBarHandler handler, ISearchBar view)
        => handler.PlatformView.ReadOnly = view.IsReadOnly;

    public static void MapMaxLength(HarmonySearchBarHandler handler, ISearchBar view)
        => handler.PlatformView.MaxLength = view.MaxLength;

    public static void MapReturnType(HarmonySearchBarHandler handler, ISearchBar view)
    {
        handler.PlatformView.EnterKeyType = view.ReturnType switch
        {
            ReturnType.Go => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_GO,
            ReturnType.Search => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_SEARCH,
            ReturnType.Send => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_SEND,
            ReturnType.Next => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_NEXT,
            _ => ArkUI_EnterKeyType.ARKUI_ENTER_KEY_TYPE_DONE,
        };
    }

    private void OnTextChange(ArkUINodeEvent nodeEvent)
    {
        var text = nodeEvent.GetString() ?? string.Empty;
        if (VirtualView.Text == text)
            return;
        VirtualView.Text = text;
    }

    private void OnSubmit(ArkUINodeEvent _)
        => VirtualView.SearchButtonPressed();

    ISearchBar ISearchBarHandler.VirtualView => VirtualView;
    object ISearchBarHandler.PlatformView => PlatformView;
    object? ISearchBarHandler.QueryEditor => PlatformView;
}
