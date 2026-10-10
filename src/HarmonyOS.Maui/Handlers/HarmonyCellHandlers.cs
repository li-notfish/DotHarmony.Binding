#nullable enable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkImage = HarmonyOS.ArkUI.Image;
using ArkRow = HarmonyOS.ArkUI.Row;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkText = HarmonyOS.ArkUI.Text;
using ArkTextInput = HarmonyOS.ArkUI.TextInput;
using ArkToggle = HarmonyOS.ArkUI.Toggle;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyTextCellHandler : ElementHandler<TextCell, ArkRow>
{
    public static PropertyMapper<TextCell, HarmonyTextCellHandler> Mapper =
        new(ElementHandler.ElementMapper)
        {
            [nameof(TextCell.Text)] = Rebuild,
            [nameof(TextCell.Detail)] = Rebuild,
            [nameof(TextCell.TextColor)] = Rebuild,
            [nameof(TextCell.DetailColor)] = Rebuild,
            [nameof(Cell.IsEnabled)] = Rebuild,
        };

    public HarmonyTextCellHandler() : base(Mapper) { }

    private NativeChildTracker<ArkText> _childTracker = null!;

    protected override ArkRow CreatePlatformElement()
    {
        var row = new ArkRow();
        _childTracker = new NativeChildTracker<ArkText>(row.AddChild, row.RemoveAllChildren);
        return row;
    }

    public static void Rebuild(HarmonyTextCellHandler handler, TextCell view)
    {
        var children = new List<ArkText>();

        var text = new ArkText
        {
            Content = view.Text ?? string.Empty,
            FontSize = (float)HarmonyControlDefaults.TextFontSizeDefault,
        };
        if (view.TextColor is { } c)
            text.SetFontColor(c);
        children.Add(text);

        if (string.IsNullOrWhiteSpace(view.Detail))
            return;

        var detail = new ArkText
        {
            Content = view.Detail,
            FontSize = (float)HarmonyControlDefaults.DetailTextFontSizeDefault,
        };
        if (view.DetailColor is { } dc)
            detail.SetFontColor(dc);
        children.Add(detail);
        handler._childTracker.Replace(children);
    }
}

public class HarmonyImageCellHandler : ElementHandler<ImageCell, ArkRow>
{
    public static PropertyMapper<ImageCell, HarmonyImageCellHandler> Mapper =
        new(ElementHandler.ElementMapper)
        {
            [nameof(ImageCell.ImageSource)] = MapImageSource,
            [nameof(TextCell.Text)] = MapText,
            [nameof(TextCell.Detail)] = MapDetail,
        };

    private ArkImage _image = null!;
    private ArkText _text = null!;
    private ArkText _detail = null!;

    public HarmonyImageCellHandler() : base(Mapper) { }

    protected override ArkRow CreatePlatformElement()
    {
        var row = new ArkRow();
        _image = new ArkImage();
        _text = new ArkText { FontSize = (float)HarmonyControlDefaults.TextFontSizeDefault };
        _detail = new ArkText { FontSize = (float)HarmonyControlDefaults.DetailTextFontSizeDefault };
        row.AddChild(_image);
        row.AddChild(_text);
        row.AddChild(_detail);
        return row;
    }

    public static void MapImageSource(HarmonyImageCellHandler handler, ImageCell view)
    {
        var src = ImageSourceResolver.Resolve(view.ImageSource);
        if (src is not null)
            handler._image.Src = src;
    }

    public static void MapText(HarmonyImageCellHandler handler, ImageCell view)
        => handler._text.Content = view.Text ?? string.Empty;

    public static void MapDetail(HarmonyImageCellHandler handler, ImageCell view)
        => handler._detail.Content = view.Detail ?? string.Empty;
}

public class HarmonyEntryCellHandler : ElementHandler<EntryCell, ArkRow>
{
    public static PropertyMapper<EntryCell, HarmonyEntryCellHandler> Mapper =
        new(ElementHandler.ElementMapper)
        {
            [nameof(EntryCell.Label)] = MapLabel,
            [nameof(EntryCell.Text)] = MapText,
            [nameof(EntryCell.Placeholder)] = MapPlaceholder,
            [nameof(EntryCell.LabelColor)] = MapLabelColor,
        };

    private ArkText _label = null!;
    private ArkTextInput _input = null!;

    public HarmonyEntryCellHandler() : base(Mapper) { }

    protected override ArkRow CreatePlatformElement()
    {
        var row = new ArkRow();
        _label = new ArkText { FontSize = (float)HarmonyControlDefaults.TextFontSizeDefault };
        _input = new ArkTextInput();
        row.AddChild(_label);
        row.AddChild(_input);
        return row;
    }

    public static void MapLabel(HarmonyEntryCellHandler handler, EntryCell view)
        => handler._label.Content = view.Label ?? string.Empty;

    public static void MapText(HarmonyEntryCellHandler handler, EntryCell view)
        => handler._input.Text = view.Text ?? string.Empty;

    public static void MapPlaceholder(HarmonyEntryCellHandler handler, EntryCell view)
        => handler._input.Placeholder = view.Placeholder ?? string.Empty;

    public static void MapLabelColor(HarmonyEntryCellHandler handler, EntryCell view)
    {
        if (view.LabelColor is { } c)
            handler._label.SetFontColor(c);
    }
}

public class HarmonyViewCellHandler : ElementHandler<ViewCell, ArkStack>
{
    public static PropertyMapper<ViewCell, HarmonyViewCellHandler> Mapper =
        new(ElementHandler.ElementMapper)
        {
            [nameof(ViewCell.View)] = MapView,
        };

    private IElementHandler? _childHandler;

    public HarmonyViewCellHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformElement() => new();

    protected override void DisconnectHandler(ArkStack platformView)
    {
        if (_childHandler is not null)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(_childHandler, platformView);
        base.DisconnectHandler(platformView);
    }

    public static void MapView(HarmonyViewCellHandler handler, ViewCell view)
    {
        if (handler._childHandler is not null)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(handler._childHandler, handler.PlatformView);
        if (view.View is IView content)
        {
            var childHandler = HarmonyHandlerFactory.Create(content);
            handler._childHandler = childHandler;
            if (childHandler.PlatformView is ArkUINode node)
            {
                node.SetWidthPercent(1.0f);
                node.SetHeightPercent(1.0f);
                handler.PlatformView.AddChild(node);
            }
        }
    }
}

public class HarmonySwitchCellHandler : ElementHandler<SwitchCell, ArkRow>
{
    public static PropertyMapper<SwitchCell, HarmonySwitchCellHandler> Mapper =
        new(ElementHandler.ElementMapper)
        {
            [nameof(SwitchCell.Text)] = MapText,
            [nameof(SwitchCell.On)] = MapOn,
            [nameof(SwitchCell.OnColor)] = MapOnColor,
        };

    private ArkText _text = null!;
    private ArkToggle _toggle = null!;

    public HarmonySwitchCellHandler() : base(Mapper) { }

    protected override ArkRow CreatePlatformElement()
    {
        var row = new ArkRow();
        _text = new ArkText { FontSize = (float)HarmonyControlDefaults.TextFontSizeDefault };
        _toggle = new ArkToggle();
        _toggle.SetWidth(50f);
        _toggle.SetHeight(26f);
        row.AddChild(_text);
        row.AddChild(_toggle);
        return row;
    }

    public static void MapText(HarmonySwitchCellHandler handler, SwitchCell view)
        => handler._text.Content = view.Text ?? string.Empty;

    public static void MapOn(HarmonySwitchCellHandler handler, SwitchCell view)
        => handler._toggle.IsOn = view.On;

    public static void MapOnColor(HarmonySwitchCellHandler handler, SwitchCell view)
    {
        if (view.OnColor is { } c)
            handler._toggle.SetSelectedColor(
                (byte)(c.Red * 255),
                (byte)(c.Green * 255),
                (byte)(c.Blue * 255),
                (byte)(c.Alpha * 255));
    }
}

/// <summary>
/// 自定义 Cell 的轻量 fallback；具体 Cell 类型仍优先使用上方专用 Handler。
/// </summary>
public class HarmonyCellHandler : ElementHandler<Cell, ArkStack>
{
    public HarmonyCellHandler() : base(ElementHandler.ElementMapper) { }

    protected override ArkStack CreatePlatformElement() => new();
}
