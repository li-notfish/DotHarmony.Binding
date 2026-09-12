using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkScroll = HarmonyOS.ArkUI.Scroll;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyScrollViewHandler : HarmonyViewHandler<IScrollView, ArkScroll>, IScrollViewHandler
{
    public static PropertyMapper<IScrollView, IScrollViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(IScrollView.Content)] = MapContent,
        [nameof(IScrollView.Orientation)] = MapOrientation,
        [nameof(IScrollView.HorizontalScrollBarVisibility)] = MapHorizontalScrollBarVisibility,
        [nameof(IScrollView.VerticalScrollBarVisibility)] = MapVerticalScrollBarVisibility,
    };

    public static CommandMapper<IScrollView, IScrollViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(IScrollView.RequestScrollTo)] = MapRequestScrollTo,
    };

    public HarmonyScrollViewHandler() : base(Mapper, CommandMapper) { }

    protected override ArkScroll CreatePlatformView() => new();

    protected override void ConnectHandler(ArkScroll platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Scrolled += OnScroll;
        platformView.ScrollStop += OnScrollStop;
    }

    protected override void DisconnectHandler(ArkScroll platformView)
    {
        platformView.Scrolled -= OnScroll;
        platformView.ScrollStop -= OnScrollStop;
        base.DisconnectHandler(platformView);
    }

    private IElementHandler? _contentHandler;

    public static void MapContent(IScrollViewHandler handler, IScrollView view)
    {
        if (handler is not HarmonyScrollViewHandler h) return;
        h.UpdateContent(view.Content);
    }

    private void UpdateContent(object? content)
    {
        if (_contentHandler is not null)
        {
            PlatformView.RemoveAllChildren();
            _contentHandler = null;
        }

        if (content is null) return;
        if (content is not IView view) return;

        var childHandler = HarmonyHandlerFactory.Create(view);
        childHandler.SetVirtualView((IElement)view);
        _contentHandler = childHandler;

        if (childHandler.PlatformView is ArkUINode node)
        {
            node.SetWidthPercent(1.0f);
            PlatformView.AddChild(node);
        }
    }

    public static void MapOrientation(IScrollViewHandler handler, IScrollView view)
    {
        if (handler is not HarmonyScrollViewHandler h) return;
        h.PlatformView.Scrollable = view.Orientation switch
        {
            ScrollOrientation.Vertical => ArkUI_ScrollDirection.ARKUI_SCROLL_DIRECTION_VERTICAL,
            ScrollOrientation.Horizontal => ArkUI_ScrollDirection.ARKUI_SCROLL_DIRECTION_HORIZONTAL,
            _ => ArkUI_ScrollDirection.ARKUI_SCROLL_DIRECTION_VERTICAL,
        };
    }

    public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView view)
    {
    }

    public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView view)
    {
    }

    public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView view, object? args)
    {
        if (handler is not HarmonyScrollViewHandler h) return;
        if (args is not ScrollToRequest request) return;

        h.PlatformView.SetOffset(
            (float)request.HorizontalOffset,
            (float)request.VerticalOffset);

        view.ScrollFinished();
    }

    private void OnScroll(ArkUINodeEvent e)
    {
        var horizontalOffset = e.ComponentData(0).f32;
        var verticalOffset = e.ComponentData(1).f32;
        VirtualView.HorizontalOffset = horizontalOffset;
        VirtualView.VerticalOffset = verticalOffset;
    }

    private void OnScrollStop(ArkUINodeEvent e)
    {
    }
}
