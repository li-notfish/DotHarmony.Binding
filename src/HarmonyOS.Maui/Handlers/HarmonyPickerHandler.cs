#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkText = HarmonyOS.ArkUI.Text;
using ArkTextPicker = HarmonyOS.ArkUI.TextPicker;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// Picker → 触发文本 + 可显隐 TextPicker。
/// 第 0 个原生选项是空哨兵，用来表达 MAUI SelectedIndex=-1 的“未选择”语义。
/// </summary>
public class HarmonyPickerHandler : HarmonyViewHandler<Picker, ArkColumn>
{
    public static PropertyMapper<Picker, HarmonyPickerHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(Picker.ItemsSource)] = MapItemsSource,
        [nameof(Picker.SelectedIndex)] = MapSelectedIndex,
        [nameof(Picker.Title)] = MapTitle,
        [nameof(Picker.TitleColor)] = MapTitle,
        [nameof(Picker.FontFamily)] = MapText,
        [nameof(Picker.FontSize)] = MapText,
        [nameof(Picker.FontAttributes)] = MapText,
        [nameof(Picker.CharacterSpacing)] = MapText,
        [nameof(Picker.HorizontalTextAlignment)] = MapText,
        [nameof(Picker.VerticalTextAlignment)] = MapText,
        [nameof(Picker.IsOpen)] = MapIsOpen,
    };

    private ArkText _trigger = null!;
    private ArkTextPicker _picker = null!;
    private INotifyCollectionChanged? _observedItemsSource;
    private INotifyCollectionChanged? _observedItems;
    private List<string> _items = new();

    public HarmonyPickerHandler() : base(Mapper) { }

    protected override ArkColumn CreatePlatformView()
    {
        var column = new ArkColumn();
        _trigger = new ArkText
        {
            FontSize = (float)HarmonyControlDefaults.TextFontSizeDefault,
        };
        _trigger.SetHeight(40f);

        _picker = new ArkTextPicker();
        _picker.SetVisibility(ArkUI_Visibility.ARKUI_VISIBILITY_NONE);

        column.AddChild(_trigger);
        column.AddChild(_picker);
        return column;
    }

    protected override void ConnectHandler(ArkColumn platformView)
    {
        base.ConnectHandler(platformView);
        _trigger.Click += OnTriggerClick;
        _picker.OnChange += OnChange;
        MapItemsSource(this, VirtualView);
        MapTitle(this, VirtualView);
        MapIsOpen(this, VirtualView);
    }

    protected override void DisconnectHandler(ArkColumn platformView)
    {
        _trigger.Click -= OnTriggerClick;
        _picker.OnChange -= OnChange;
        UnobserveItems();
        base.DisconnectHandler(platformView);
    }

    public static void MapItemsSource(HarmonyPickerHandler handler, Picker view)
    {
        handler.UnobserveItems();

        if (view.ItemsSource is INotifyCollectionChanged incc)
        {
            handler._observedItemsSource = incc;
            incc.CollectionChanged += handler.OnItemsChanged;
        }
        if (view.Items is INotifyCollectionChanged itemsChanged)
        {
            handler._observedItems = itemsChanged;
            itemsChanged.CollectionChanged += handler.OnItemsChanged;
        }

        handler._items.Clear();
        if (view.ItemsSource is not null)
        {
            foreach (var item in view.ItemsSource)
                handler._items.Add(item?.ToString() ?? string.Empty);
        }
        else
        {
            foreach (var item in view.Items)
                handler._items.Add(item);
        }

        var nativeItems = new List<string> { string.Empty };
        nativeItems.AddRange(handler._items);
        handler._picker.SetRange(nativeItems);
        MapSelectedIndex(handler, view);
    }

    public static void MapSelectedIndex(HarmonyPickerHandler handler, Picker view)
    {
        var nativeIndex = PickerIndexTranslator.ToNative(view.SelectedIndex);
        handler._picker.SelectedIndex = Math.Clamp(nativeIndex, 0, handler._items.Count);
        handler.UpdateTriggerText(view);
    }

    public static void MapTitle(HarmonyPickerHandler handler, Picker view)
    {
        handler.UpdateTriggerText(view);
        if (view.TitleColor is { } c)
            handler._trigger.SetFontColor(c);
    }

    public static void MapText(HarmonyPickerHandler handler, Picker view)
    {
        if (view.FontSize > 0)
            handler._trigger.FontSize = (float)view.FontSize;
        if (!string.IsNullOrEmpty(view.FontFamily))
            handler._trigger.SetFontFamily(view.FontFamily);
        handler._trigger.SetFontWeight(
            view.FontAttributes.HasFlag(FontAttributes.Bold)
                ? ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W700
                : ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W400);
        handler._trigger.SetFontStyle(
            view.FontAttributes.HasFlag(FontAttributes.Italic)
                ? ArkUI_FontStyle.ARKUI_FONT_STYLE_ITALIC
                : ArkUI_FontStyle.ARKUI_FONT_STYLE_NORMAL);
        handler._trigger.SetLetterSpacing((float)view.CharacterSpacing);
        handler._trigger.TextAlign = view.HorizontalTextAlignment switch
        {
            TextAlignment.Start => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_START,
            TextAlignment.Center => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_CENTER,
            TextAlignment.End => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_END,
            _ => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_START,
        };
    }

    public static void MapIsOpen(HarmonyPickerHandler handler, Picker view)
        => handler._picker.SetVisibility(
            view.IsOpen
                ? ArkUI_Visibility.ARKUI_VISIBILITY_VISIBLE
                : ArkUI_Visibility.ARKUI_VISIBILITY_NONE);

    private void UpdateTriggerText(Picker view)
    {
        var text = view.SelectedIndex >= 0 && view.SelectedIndex < _items.Count
            ? _items[view.SelectedIndex]
            : view.Title ?? "Select";
        _trigger.Content = text;
    }

    private void OnTriggerClick(ArkUINodeEvent _)
    {
        if (VirtualView is { } view)
            view.IsOpen = true;
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => MapItemsSource(this, VirtualView);

    private void UnobserveItems()
    {
        if (_observedItemsSource is not null)
            _observedItemsSource.CollectionChanged -= OnItemsChanged;
        if (_observedItems is not null)
            _observedItems.CollectionChanged -= OnItemsChanged;
        _observedItemsSource = null;
        _observedItems = null;
    }

    private void OnChange(ArkUINodeEvent e)
    {
        var nativeIndex = e.ComponentData(0).i32;
        var index = PickerIndexTranslator.FromNative(nativeIndex);
        if (VirtualView.SelectedIndex == index)
            return;

        VirtualView.SelectedIndex = index;
        VirtualView.IsOpen = false;
        UpdateTriggerText(VirtualView);
    }
}
