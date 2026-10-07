#pragma warning disable CS0618
using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

// Frame/ListView/TableView/Cell 是官方兼容层控件，测试必须显式覆盖它们。

public class HandlerOfficialSemanticsTests
{
    private static void AssertMapperContains<TVirtual, THandler>(
        PropertyMapper<TVirtual, THandler> mapper,
        params string[] keys)
        where TVirtual : class, Microsoft.Maui.IElement
        where THandler : Microsoft.Maui.IElementHandler
    {
        var actual = mapper.GetKeys().ToArray();
        foreach (var key in keys)
            Assert.Contains(key, actual);
    }

    private static void AssertElementMapperContains<TVirtual, THandler>(
        PropertyMapper<TVirtual, THandler> mapper,
        params string[] keys)
        where TVirtual : class, Microsoft.Maui.IElement
        where THandler : Microsoft.Maui.IElementHandler
    {
        var actual = mapper.GetKeys().ToArray();
        foreach (var key in keys)
            Assert.Contains(key, actual);
    }

    [Fact]
    public void Picker_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyPickerHandler.Mapper,
            nameof(Picker.ItemsSource),
            nameof(Picker.SelectedIndex),
            nameof(Picker.Title),
            nameof(Picker.TitleColor),
            nameof(Picker.FontFamily),
            nameof(Picker.FontSize),
            nameof(Picker.FontAttributes),
            nameof(Picker.CharacterSpacing),
            nameof(Picker.HorizontalTextAlignment),
            nameof(Picker.VerticalTextAlignment),
            nameof(Picker.IsOpen));

    [Fact]
    public void Entry_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyEntryHandler.Mapper,
            nameof(Microsoft.Maui.IEntry.Text),
            nameof(Microsoft.Maui.IEntry.Placeholder),
            nameof(Microsoft.Maui.IEntry.TextColor),
            nameof(Entry.FontSize),
            nameof(Entry.FontFamily),
            nameof(Microsoft.Maui.IEntry.PlaceholderColor),
            nameof(Microsoft.Maui.IEntry.IsPassword),
            nameof(Microsoft.Maui.IEntry.IsReadOnly),
            nameof(Microsoft.Maui.IEntry.MaxLength),
            nameof(Microsoft.Maui.IEntry.ReturnType),
            nameof(Entry.Keyboard),
            nameof(Entry.ClearButtonVisibility),
            nameof(Entry.CharacterSpacing),
            nameof(Entry.FontAttributes),
            nameof(Entry.CursorPosition),
            nameof(Entry.SelectionLength),
            nameof(Entry.IsTextPredictionEnabled),
            nameof(Entry.IsSpellCheckEnabled),
            nameof(Entry.TextTransform),
            nameof(Entry.HorizontalTextAlignment),
            nameof(Entry.VerticalTextAlignment));

    [Fact]
    public void Editor_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyEditorHandler.Mapper,
            nameof(Editor.Text),
            nameof(Editor.Placeholder),
            nameof(Editor.TextColor),
            nameof(Editor.PlaceholderColor),
            nameof(Editor.FontSize),
            nameof(Editor.FontFamily),
            nameof(Editor.IsReadOnly),
            nameof(Editor.MaxLength),
            nameof(Editor.Keyboard),
            nameof(Editor.CharacterSpacing),
            nameof(Editor.FontAttributes),
            nameof(Editor.CursorPosition),
            nameof(Editor.SelectionLength),
            nameof(Editor.IsTextPredictionEnabled),
            nameof(Editor.IsSpellCheckEnabled),
            nameof(Editor.AutoSize),
            nameof(Editor.TextTransform),
            nameof(Editor.HorizontalTextAlignment),
            nameof(Editor.VerticalTextAlignment));

    [Fact]
    public void Button_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyButtonHandler.Mapper,
            nameof(Button.Text),
            nameof(Button.TextColor),
            nameof(Button.FontSize),
            nameof(Button.FontFamily),
            nameof(Button.BackgroundColor),
            nameof(Button.BorderColor),
            nameof(Button.BorderWidth),
            nameof(Button.CornerRadius),
            nameof(Button.Padding),
            nameof(Button.ImageSource),
            nameof(Button.ContentLayout),
            nameof(Button.TextTransform),
            nameof(Button.LineBreakMode),
            nameof(Button.CharacterSpacing),
            nameof(Button.FontAttributes));

    [Fact]
    public void SearchBar_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonySearchBarHandler.Mapper,
            nameof(Microsoft.Maui.ISearchBar.Text),
            nameof(Microsoft.Maui.ISearchBar.Placeholder),
            nameof(Microsoft.Maui.ISearchBar.TextColor),
            nameof(SearchBar.FontSize),
            nameof(SearchBar.FontFamily),
            nameof(Microsoft.Maui.ISearchBar.PlaceholderColor),
            nameof(Microsoft.Maui.ISearchBar.IsReadOnly),
            nameof(Microsoft.Maui.ISearchBar.MaxLength),
            nameof(Microsoft.Maui.ISearchBar.ReturnType),
            nameof(SearchBar.CancelButtonColor),
            nameof(SearchBar.SearchIconColor),
            nameof(SearchBar.CharacterSpacing),
            nameof(SearchBar.FontAttributes),
            nameof(SearchBar.CursorPosition),
            nameof(SearchBar.SelectionLength),
            nameof(SearchBar.IsTextPredictionEnabled),
            nameof(SearchBar.HorizontalTextAlignment),
            nameof(SearchBar.VerticalTextAlignment));

    [Fact]
    public void Image_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyImageHandler.Mapper,
            nameof(Microsoft.Maui.IImage.Source),
            nameof(Microsoft.Maui.IImage.Aspect),
            nameof(Microsoft.Maui.IImage.IsOpaque),
            nameof(Microsoft.Maui.IImage.IsAnimationPlaying));

    [Fact]
    public void RefreshView_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyRefreshViewHandler.Mapper,
            nameof(Microsoft.Maui.IRefreshView.IsRefreshing),
            nameof(Microsoft.Maui.IRefreshView.Content),
            nameof(Microsoft.Maui.IRefreshView.RefreshColor),
            nameof(Microsoft.Maui.IRefreshView.IsRefreshEnabled));

    [Fact]
    public void DatePicker_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyDatePickerHandler.Mapper,
            nameof(Microsoft.Maui.IDatePicker.Date),
            nameof(Microsoft.Maui.IDatePicker.MinimumDate),
            nameof(Microsoft.Maui.IDatePicker.MaximumDate),
            nameof(DatePicker.Format),
            nameof(DatePicker.TextColor),
            nameof(DatePicker.CharacterSpacing),
            nameof(DatePicker.FontFamily),
            nameof(DatePicker.FontSize),
            nameof(DatePicker.FontAttributes),
            nameof(DatePicker.IsOpen));

    [Fact]
    public void TimePicker_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyTimePickerHandler.Mapper,
            nameof(Microsoft.Maui.ITimePicker.Time),
            nameof(TimePicker.Format),
            nameof(TimePicker.TextColor),
            nameof(TimePicker.CharacterSpacing),
            nameof(TimePicker.FontFamily),
            nameof(TimePicker.FontSize),
            nameof(TimePicker.FontAttributes),
            nameof(TimePicker.IsOpen));

    [Fact]
    public void FlexLayout_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyLayoutHandler.Mapper,
            nameof(FlexLayout.Direction),
            nameof(FlexLayout.Wrap),
            nameof(FlexLayout.JustifyContent),
            nameof(FlexLayout.AlignItems),
            nameof(FlexLayout.AlignContent),
            nameof(FlexLayout.Position));

    [Fact]
    public void WebView_MapsOfficialSemantics()
    {
        AssertMapperContains(
            HarmonyWebViewHandler.Mapper,
            nameof(WebView.Source),
            nameof(WebView.UserAgent));

        var commands = HarmonyWebViewHandler.Commands;
        Assert.NotNull(commands.GetCommand(nameof(Microsoft.Maui.IWebView.GoBack)));
        Assert.NotNull(commands.GetCommand(nameof(Microsoft.Maui.IWebView.GoForward)));
        Assert.NotNull(commands.GetCommand(nameof(Microsoft.Maui.IWebView.Reload)));
        Assert.NotNull(commands.GetCommand(nameof(Microsoft.Maui.IWebView.Eval)));
        Assert.NotNull(commands.GetCommand(nameof(Microsoft.Maui.IWebView.EvaluateJavaScriptAsync)));
    }

    [Fact]
    public void ImageButton_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyImageButtonHandler.Mapper,
            nameof(ImageButton.Source),
            nameof(ImageButton.Aspect),
            nameof(ImageButton.BackgroundColor),
            nameof(ImageButton.BorderColor),
            nameof(ImageButton.BorderWidth),
            nameof(ImageButton.CornerRadius),
            nameof(ImageButton.Padding),
            nameof(Microsoft.Maui.Controls.IImageElement.IsOpaque),
            nameof(Microsoft.Maui.Controls.IImageElement.IsAnimationPlaying));

    [Fact]
    public void IndicatorView_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyIndicatorViewHandler.Mapper,
            nameof(IndicatorView.ItemsSource),
            nameof(IndicatorView.Count),
            nameof(IndicatorView.Position),
            nameof(IndicatorView.IndicatorColor),
            nameof(IndicatorView.SelectedIndicatorColor),
            nameof(IndicatorView.IndicatorSize),
            nameof(IndicatorView.MaximumVisible),
            nameof(IndicatorView.HideSingle),
            nameof(IndicatorView.IndicatorsShape));

    [Fact]
    public void SwipeView_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonySwipeViewHandler.Mapper,
            nameof(SwipeView.Content),
            nameof(SwipeView.LeftItems),
            nameof(SwipeView.RightItems),
            nameof(SwipeView.TopItems),
            nameof(SwipeView.BottomItems),
            nameof(SwipeView.Threshold));

    [Fact]
    public void FlyoutPage_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyFlyoutPageHandler.Mapper,
            nameof(FlyoutPage.Flyout),
            nameof(FlyoutPage.Detail),
            nameof(FlyoutPage.IsPresented),
            nameof(FlyoutPage.IsGestureEnabled),
            nameof(FlyoutPage.FlyoutLayoutBehavior));

    [Fact]
    public void Frame_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyFrameCompatHandler.Mapper,
            nameof(Frame.Content),
            nameof(Frame.BorderColor),
            nameof(Frame.CornerRadius),
            nameof(Frame.HasShadow),
            nameof(Frame.BackgroundColor));

    [Fact]
    public void ListView_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyListViewHandler.Mapper,
            nameof(ListView.ItemsSource),
            nameof(ListView.SelectedItem),
            nameof(ListView.Header),
            nameof(ListView.Footer),
            nameof(ListView.HasUnevenRows),
            nameof(ListView.RowHeight),
            nameof(ListView.IsGroupingEnabled),
            nameof(ListView.SeparatorVisibility),
            nameof(ListView.SeparatorColor));

    [Fact]
    public void TableView_MapsOfficialSemantics()
        => AssertMapperContains(
            HarmonyTableViewHandler.Mapper,
            nameof(TableView.Root),
            nameof(TableView.RowHeight),
            nameof(TableView.HasUnevenRows),
            nameof(TableView.Intent));

    [Fact]
    public void MenuBar_MapsOfficialSemantics()
    {
        AssertElementMapperContains(
            HarmonyMenuBarHandler.Mapper,
            nameof(MenuBar.IsEnabled));

        var commands = HarmonyMenuBarHandler.Commands;
        Assert.NotNull(commands.GetCommand(nameof(Microsoft.Maui.Handlers.IMenuBarHandler.Add)));
        Assert.NotNull(commands.GetCommand(nameof(Microsoft.Maui.Handlers.IMenuBarHandler.Remove)));
        Assert.NotNull(commands.GetCommand(nameof(Microsoft.Maui.Handlers.IMenuBarHandler.Clear)));
        Assert.NotNull(commands.GetCommand(nameof(Microsoft.Maui.Handlers.IMenuBarHandler.Insert)));
    }

    [Fact]
    public void Toolbar_MapsOfficialSemantics()
        => AssertElementMapperContains(
            HarmonyToolbarHandler.Mapper,
            nameof(Toolbar.Title),
            nameof(Toolbar.ToolbarItems),
            nameof(Toolbar.IsVisible),
            nameof(Toolbar.BarTextColor),
            nameof(Toolbar.IconColor));

    [Fact]
    public void Cells_MapOfficialSemantics()
    {
        AssertElementMapperContains(
            HarmonyTextCellHandler.Mapper,
            nameof(TextCell.Text),
            nameof(TextCell.Detail),
            nameof(TextCell.TextColor),
            nameof(TextCell.DetailColor),
            nameof(Cell.IsEnabled));

        AssertElementMapperContains(
            HarmonyImageCellHandler.Mapper,
            nameof(ImageCell.ImageSource),
            nameof(TextCell.Text),
            nameof(TextCell.Detail));

        AssertElementMapperContains(
            HarmonyEntryCellHandler.Mapper,
            nameof(EntryCell.Label),
            nameof(EntryCell.Text),
            nameof(EntryCell.Placeholder),
            nameof(EntryCell.LabelColor));

        AssertElementMapperContains(
            HarmonyViewCellHandler.Mapper,
            nameof(ViewCell.View));

        AssertElementMapperContains(
            HarmonySwitchCellHandler.Mapper,
            nameof(SwitchCell.Text),
            nameof(SwitchCell.On),
            nameof(SwitchCell.OnColor));
    }
}
