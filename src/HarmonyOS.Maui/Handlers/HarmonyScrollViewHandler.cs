using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;
using ArkScroll = HarmonyOS.ArkUI.Scroll;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyScrollViewHandler : HarmonyViewHandler<IScrollView, ArkScroll>, IScrollViewHandler
{
    /// <summary>滚动事件日志开关：插值分配走在调用点，开启才付出分配代价（与布局热路径同一纪律）</summary>
    private static readonly bool LogScroll = false;

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
        platformView.EnableScrollInteraction = true;
        platformView.EdgeEffect = (int)ArkUI_EdgeEffect.ARKUI_EDGE_EFFECT_SPRING;
        platformView.SetNestedScroll(
            ArkUI_ScrollNestedMode.ARKUI_SCROLL_NESTED_MODE_SELF_ONLY,
            ArkUI_ScrollNestedMode.ARKUI_SCROLL_NESTED_MODE_SELF_ONLY);
        MapOrientation(this, VirtualView);
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
            HarmonyViewHandler<IScrollView, ArkScroll>.DisposeContent(_contentHandler, PlatformView);
            _contentHandler = null;
        }

        if (content is null) return;
        if (content is not IView view) return;

        var childHandler = HarmonyHandlerFactory.Create(view);
        childHandler.SetVirtualView((IElement)view);
        _contentHandler = childHandler;

        if (childHandler.PlatformView is ArkUINode node)
        {
            ApplyContentSizing(node);
            PlatformView.AddChild(node);
            MapOrientation(this, VirtualView);
        }
    }

    private void ApplyContentSizing(ArkUINode? node = null)
    {
        node ??= _contentHandler?.PlatformView as ArkUINode;
        if (node is null) return;

        var (stretchWidth, stretchHeight) = GetContentSizing(VirtualView.Orientation);
        if (stretchWidth) node.SetWidthPercent(1.0f);
        else node.SetWidthAuto();

        if (stretchHeight) node.SetHeightPercent(1.0f);
        else node.SetHeightAuto();
    }

    /// <summary>
    /// MAUI 官方测量语义：竖向滚动约束宽、横向滚动约束高（交叉轴充满），
    /// Both 两轴均不约束；Neither 禁止滚动 → 两轴均约束到视口（内容不得越过）。
    /// 交叉轴用百分比充满，滚动主轴交还给内容自适应。
    /// </summary>
    internal static (bool StretchWidth, bool StretchHeight) GetContentSizing(ScrollOrientation orientation)
        => orientation switch
        {
            ScrollOrientation.Vertical => (true, false),
            ScrollOrientation.Horizontal => (false, true),
            ScrollOrientation.Both => (false, false),
            ScrollOrientation.Neither => (true, true),
            _ => (true, false),
        };

    public static void MapOrientation(IScrollViewHandler handler, IScrollView view)
    {
        if (handler is not HarmonyScrollViewHandler h) return;

        // Neither 禁止触摸滚动（MAUI 语义：内容两轴约束到视口，见 GetContentSizing）；
        // ArkUI Scroll 无"不可滚"方向，以交互开关表达——ScrollTo 通道不受影响。
        // Both 方向 ArkUI Scroll 单轴承载，沿用 Vertical（横向滚动由 Both 内容自适应补足）。
        h.PlatformView.EnableScrollInteraction =
            view.Orientation is not ScrollOrientation.Neither;
        h.PlatformView.Scrollable = view.Orientation switch
        {
            ScrollOrientation.Horizontal => ArkUI_ScrollDirection.ARKUI_SCROLL_DIRECTION_HORIZONTAL,
            _ => ArkUI_ScrollDirection.ARKUI_SCROLL_DIRECTION_VERTICAL,
        };

        // 方向切换同时刷新内容尺寸（交叉轴约束随方向变化）
        h.ApplyContentSizing();
    }

    public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView view)
    {
        if (handler is HarmonyScrollViewHandler h)
            h.PlatformView.ScrollBarDisplayMode = GetScrollBarDisplayMode(view.HorizontalScrollBarVisibility);
    }

    public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView view)
    {
        if (handler is HarmonyScrollViewHandler h)
            h.PlatformView.ScrollBarDisplayMode = GetScrollBarDisplayMode(view.VerticalScrollBarVisibility);
    }

    internal static ArkUI_ScrollBarDisplayMode GetScrollBarDisplayMode(ScrollBarVisibility visibility)
        => visibility switch
        {
            ScrollBarVisibility.Always => ArkUI_ScrollBarDisplayMode.ARKUI_SCROLL_BAR_DISPLAY_MODE_ON,
            ScrollBarVisibility.Never => ArkUI_ScrollBarDisplayMode.ARKUI_SCROLL_BAR_DISPLAY_MODE_OFF,
            _ => ArkUI_ScrollBarDisplayMode.ARKUI_SCROLL_BAR_DISPLAY_MODE_AUTO,
        };

    public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView view, object? args)
    {
        if (handler is not HarmonyScrollViewHandler h) return;
        if (args is not ScrollToRequest request) return;

        // NODE_SCROLL_OFFSET 绝对定位在横向 Scroll 上实测被钳到边缘（-0.25→0 回弹）；
        // 改走 NODE_SCROLL_BY 相对滚动：目标 - 当前偏移
        h.PlatformView.ScrollBy(
            (float)(request.HorizontalOffset - view.HorizontalOffset),
            (float)(request.VerticalOffset - view.VerticalOffset));

        view.ScrollFinished();
    }

    private void OnScroll(ArkUINodeEvent e)
    {
        var horizontalOffset = e.ComponentData(0).f32;
        var verticalOffset = e.ComponentData(1).f32;
        // 滚动热路径：插值字符串在调用点分配，必须有开关守护
        if (LogScroll) HiLog.Debug("HarmonyHost", $"[Scroll] x={horizontalOffset:F2} y={verticalOffset:F2}");
        VirtualView.HorizontalOffset = horizontalOffset;
        VirtualView.VerticalOffset = verticalOffset;
    }

    private void OnScrollStop(ArkUINodeEvent e)
    {
        if (LogScroll) HiLog.Debug(
            "HarmonyHost",
            $"[ScrollStop] x={VirtualView.HorizontalOffset:F2} y={VirtualView.VerticalOffset:F2}");
    }

}
