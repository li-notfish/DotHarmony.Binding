#nullable enable
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkButton = HarmonyOS.ArkUI.Button;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkImage = HarmonyOS.ArkUI.Image;
using ArkRow = HarmonyOS.ArkUI.Row;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkText = HarmonyOS.ArkUI.Text;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// WebView 的降级实现：ArkUI NDK 当前没有可直接挂树的 Web 节点。
/// 这里显示 Source，导航/脚本命令显式降级，不静默失败。
/// </summary>
public class HarmonyWebViewHandler : HarmonyViewHandler<WebView, ArkStack>
{
    public static PropertyMapper<WebView, HarmonyWebViewHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(WebView.Source)] = MapSource,
        [nameof(WebView.UserAgent)] = MapUserAgent,
    };

    public static CommandMapper<WebView, HarmonyWebViewHandler> Commands = new(ViewCommandMapper)
    {
        [nameof(IWebView.GoBack)] = MapGoBack,
        [nameof(IWebView.GoForward)] = MapGoForward,
        [nameof(IWebView.Reload)] = MapReload,
        [nameof(IWebView.Eval)] = MapEval,
        [nameof(IWebView.EvaluateJavaScriptAsync)] = MapEvaluateJavaScriptAsync,
    };

    private ArkText _status = null!;

    public HarmonyWebViewHandler() : base(Mapper, Commands) { }

    protected override ArkStack CreatePlatformView()
    {
        var stack = new ArkStack();
        _status = new ArkText { FontSize = (float)HarmonyControlDefaults.StatusTextFontSizeDefault };
        stack.AddChild(_status);
        return stack;
    }

    public static void MapSource(HarmonyWebViewHandler handler, WebView view)
    {
        handler._status.Content = view.Source switch
        {
            HtmlWebViewSource html => html.Html ?? string.Empty,
            UrlWebViewSource url => url.Url ?? string.Empty,
            WebViewSource source => source.ToString() ?? string.Empty,
            _ => string.Empty,
        };
    }

    public static void MapUserAgent(HarmonyWebViewHandler handler, WebView view)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[WebView] UserAgent is degraded on HarmonyOS.");

    public static void MapGoBack(HarmonyWebViewHandler handler, WebView view, object? args)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[WebView] GoBack is degraded on HarmonyOS.");

    public static void MapGoForward(HarmonyWebViewHandler handler, WebView view, object? args)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[WebView] GoForward is degraded on HarmonyOS.");

    public static void MapReload(HarmonyWebViewHandler handler, WebView view, object? args)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[WebView] Reload is degraded on HarmonyOS.");

    public static void MapEval(HarmonyWebViewHandler handler, WebView view, object? args)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[WebView] Eval is degraded on HarmonyOS.");

    public static void MapEvaluateJavaScriptAsync(HarmonyWebViewHandler handler, WebView view, object? args)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[WebView] EvaluateJavaScriptAsync is degraded on HarmonyOS.");
}

/// <summary>
/// HybridWebView 的降级实现；保留消息/脚本命令入口，但当前宿主没有原生 Web 树。
/// </summary>
public class HarmonyHybridWebViewHandler : HarmonyViewHandler<HybridWebView, ArkStack>
{
    public static PropertyMapper<HybridWebView, HarmonyHybridWebViewHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(HybridWebView.DefaultFile)] = MapDefaultFile,
        [nameof(HybridWebView.HybridRoot)] = MapHybridRoot,
    };

    public static CommandMapper<HybridWebView, HarmonyHybridWebViewHandler> Commands = new(ViewCommandMapper)
    {
        [nameof(IHybridWebView.SendRawMessage)] = MapSendRawMessage,
        [nameof(IHybridWebView.InvokeJavaScriptAsync)] = MapInvokeJavaScriptAsync,
        [nameof(IHybridWebView.EvaluateJavaScriptAsync)] = MapEvaluateJavaScriptAsync,
    };

    private ArkText _status = null!;

    public HarmonyHybridWebViewHandler() : base(Mapper, Commands) { }

    protected override ArkStack CreatePlatformView()
    {
        var stack = new ArkStack();
        _status = new ArkText { FontSize = (float)HarmonyControlDefaults.StatusTextFontSizeDefault };
        stack.AddChild(_status);
        return stack;
    }

    public static void MapDefaultFile(HarmonyHybridWebViewHandler handler, HybridWebView view)
        => handler._status.Content = $"{view.HybridRoot}/{view.DefaultFile}";

    public static void MapHybridRoot(HarmonyHybridWebViewHandler handler, HybridWebView view)
        => MapDefaultFile(handler, view);

    public static void MapSendRawMessage(HarmonyHybridWebViewHandler handler, HybridWebView view, object? args)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[HybridWebView] SendRawMessage is degraded on HarmonyOS.");

    public static void MapInvokeJavaScriptAsync(HarmonyHybridWebViewHandler handler, HybridWebView view, object? args)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[HybridWebView] InvokeJavaScriptAsync is degraded on HarmonyOS.");

    public static void MapEvaluateJavaScriptAsync(HarmonyHybridWebViewHandler handler, HybridWebView view, object? args)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[HybridWebView] EvaluateJavaScriptAsync is degraded on HarmonyOS.");
}

/// <summary>
/// ImageButton → Stack + Image + 点击事件。
/// </summary>
public class HarmonyImageButtonHandler : HarmonyViewHandler<ImageButton, ArkStack>
{
    public static PropertyMapper<ImageButton, HarmonyImageButtonHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(ImageButton.Source)] = MapSource,
        [nameof(ImageButton.Aspect)] = MapAspect,
        [nameof(ImageButton.BackgroundColor)] = MapBackgroundColor,
        [nameof(ImageButton.BorderColor)] = MapBorderColor,
        [nameof(ImageButton.BorderWidth)] = MapBorderWidth,
        [nameof(ImageButton.CornerRadius)] = MapCornerRadius,
        [nameof(ImageButton.Padding)] = MapPadding,
        [nameof(ImageButton.IsOpaque)] = MapIsOpaque,
        [nameof(IImageElement.IsAnimationPlaying)] = MapIsAnimationPlaying,
    };

    private ArkImage _image = null!;

    public HarmonyImageButtonHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView()
    {
        var stack = new ArkStack();
        _image = new ArkImage();
        stack.AddChild(_image);
        return stack;
    }

    protected override void ConnectHandler(ArkStack platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, OnClick);
    }

    protected override void DisconnectHandler(ArkStack platformView)
    {
        platformView.UnsubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, OnClick);
        base.DisconnectHandler(platformView);
    }

    public static void MapSource(HarmonyImageButtonHandler handler, ImageButton view)
    {
        var src = ImageSourceResolver.Resolve(view.Source);
        if (src is not null)
            handler._image.Src = src;
    }

    public static void MapAspect(HarmonyImageButtonHandler handler, ImageButton view)
        => handler._image.ObjectFit = view.Aspect switch
        {
            Aspect.AspectFit => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_CONTAIN,
            Aspect.AspectFill => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_COVER,
            Aspect.Fill => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_FILL,
            _ => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_CONTAIN,
        };

    public static void MapBackgroundColor(HarmonyImageButtonHandler handler, ImageButton view)
    {
        if (view.BackgroundColor is { } c)
            handler.PlatformView.SetBackgroundColor(c.ToUint());
    }

    public static void MapBorderColor(HarmonyImageButtonHandler handler, ImageButton view)
    {
        if (view.BorderColor is { } c)
            handler.PlatformView.SetBorderColor(c.ToUint());
    }

    public static void MapBorderWidth(HarmonyImageButtonHandler handler, ImageButton view)
    {
        if (view.BorderWidth >= 0)
            handler.PlatformView.SetBorderWidth((float)view.BorderWidth);
    }

    public static void MapCornerRadius(HarmonyImageButtonHandler handler, ImageButton view)
    {
        if (view.CornerRadius >= 0)
        {
            var r = view.CornerRadius;
            handler.PlatformView.SetBorderRadius(r, r, r, r);
        }
    }

    public static void MapPadding(HarmonyImageButtonHandler handler, ImageButton view)
    {
        var p = view.Padding;
        if (p.Top > 0 || p.Right > 0 || p.Bottom > 0 || p.Left > 0)
            handler.PlatformView.SetPaddingEdges((float)p.Top, (float)p.Right, (float)p.Bottom, (float)p.Left);
    }

    public static void MapIsOpaque(HarmonyImageButtonHandler handler, ImageButton view)
        => HarmonyOS.Interop.HiLog.Info("HarmonyHost", "[ImageButton] IsOpaque is a rendering hint only.");

    public static void MapIsAnimationPlaying(HarmonyImageButtonHandler handler, ImageButton view)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[ImageButton] IsAnimationPlaying is degraded on HarmonyOS.");

    private void OnClick(ArkUINodeEvent _)
        => VirtualView.SendClicked();
}

/// <summary>
/// IndicatorView → Row + 指示点文本。
/// </summary>
public class HarmonyIndicatorViewHandler : HarmonyViewHandler<IndicatorView, ArkRow>
{
    public static PropertyMapper<IndicatorView, HarmonyIndicatorViewHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(IndicatorView.ItemsSource)] = MapItemsSource,
        [nameof(IndicatorView.Count)] = Rebuild,
        [nameof(IndicatorView.Position)] = Rebuild,
        [nameof(IndicatorView.IndicatorColor)] = Rebuild,
        [nameof(IndicatorView.SelectedIndicatorColor)] = Rebuild,
        [nameof(IndicatorView.IndicatorSize)] = Rebuild,
        [nameof(IndicatorView.MaximumVisible)] = Rebuild,
        [nameof(IndicatorView.HideSingle)] = Rebuild,
        [nameof(IndicatorView.IndicatorsShape)] = Rebuild,
    };

    public HarmonyIndicatorViewHandler() : base(Mapper) { }

    private NativeChildTracker<ArkText> _childTracker = null!;

    protected override ArkRow CreatePlatformView()
    {
        var row = new ArkRow();
        _childTracker = new NativeChildTracker<ArkText>(row.AddChild, row.RemoveAllChildren);
        return row;
    }

    public static void MapItemsSource(HarmonyIndicatorViewHandler handler, IndicatorView view)
    {
        if (view.ItemsSource is ICollection { Count: > 0 } collection)
            view.Count = collection.Count;
        Rebuild(handler, view);
    }

    public static void Rebuild(HarmonyIndicatorViewHandler handler, IndicatorView view)
    {
        if (view.HideSingle && view.Count <= 1)
        {
            handler._childTracker.Clear();
            return;
        }

        var count = Math.Min(view.Count, view.MaximumVisible);
        var children = new List<ArkText>();
        for (var i = 0; i < count; i++)
        {
            var text = new ArkText
            {
                Content = "●",
                FontSize = (float)view.IndicatorSize,
            };
            var color = i == view.Position
                ? view.SelectedIndicatorColor
                : view.IndicatorColor;
            if (color is not null)
                text.SetFontColor(color);
            children.Add(text);
        }
        handler._childTracker.Replace(children);
    }
}

/// <summary>
/// SwipeView 的显式降级实现：内容正常显示，菜单项以按钮行呈现，滑动手势暂不实现。
/// </summary>
public class HarmonySwipeViewHandler : HarmonyViewHandler<SwipeView, ArkStack>
{
    public static PropertyMapper<SwipeView, HarmonySwipeViewHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(SwipeView.Content)] = MapContent,
        [nameof(SwipeView.LeftItems)] = RebuildMenus,
        [nameof(SwipeView.RightItems)] = RebuildMenus,
        [nameof(SwipeView.TopItems)] = RebuildMenus,
        [nameof(SwipeView.BottomItems)] = RebuildMenus,
        [nameof(SwipeView.Threshold)] = MapThreshold,
    };

    private IElementHandler? _contentHandler;
    private readonly List<IElementHandler> _itemHandlers = new();
    private NativeChildTracker<ArkButton> _itemTracker = null!;

    public HarmonySwipeViewHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView()
    {
        var stack = new ArkStack();
        _itemTracker = new NativeChildTracker<ArkButton>(stack.AddChild, stack.RemoveAllChildren);
        return stack;
    }

    protected override void DisconnectHandler(ArkStack platformView)
    {
        if (_contentHandler is not null)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(_contentHandler, platformView);
        foreach (var handler in _itemHandlers)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(handler, platformView);
        _itemHandlers.Clear();
        _itemTracker.Clear();
        base.DisconnectHandler(platformView);
    }

    public static void MapContent(HarmonySwipeViewHandler handler, SwipeView view)
    {
        if (handler._contentHandler is not null)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(handler._contentHandler, handler.PlatformView);
        if (view.Content is IView content)
        {
            var childHandler = HarmonyHandlerFactory.Create(content);
            handler._contentHandler = childHandler;
            if (childHandler.PlatformView is ArkUINode node)
            {
                node.SetWidthPercent(1.0f);
                node.SetHeightPercent(1.0f);
                handler.PlatformView.AddChild(node);
            }
        }
    }

    public static void RebuildMenus(HarmonySwipeViewHandler handler, SwipeView view)
    {
        foreach (var itemHandler in handler._itemHandlers)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(itemHandler, handler.PlatformView);
        handler._itemHandlers.Clear();

        var items = view.LeftItems.Cast<Microsoft.Maui.Controls.ISwipeItem>()
            .Concat(view.RightItems)
            .Concat(view.TopItems)
            .Concat(view.BottomItems)
            .ToList();

        var buttons = new List<ArkButton>();
        foreach (var item in items)
        {
            var button = new ArkButton
            {
                Label = item is MenuItem menuItem ? menuItem.Text ?? string.Empty : string.Empty,
            };
            button.SetWidth(80f);
            button.SetHeight(40f);
            button.Click += _ =>
            {
                if (item is MenuItem menuItem)
                    ((IMenuItemController)menuItem).Activate();
            };
            buttons.Add(button);
        }
        handler._itemTracker.Replace(buttons);

        HarmonyOS.Interop.HiLog.Warn(
            "HarmonyHost",
            "[SwipeView] Swipe gestures are degraded; menu items are rendered as buttons.");
    }

    public static void MapThreshold(HarmonySwipeViewHandler handler, SwipeView view)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[SwipeView] Threshold is degraded on HarmonyOS.");
}

/// <summary>
/// SwipeItem 的兼容实现：原生 Button 承载文本、背景与点击。
/// </summary>
public class HarmonySwipeItemHandler : ElementHandler<SwipeItem, ArkButton>
{
    public static PropertyMapper<SwipeItem, HarmonySwipeItemHandler> Mapper = new(ElementHandler.ElementMapper)
    {
        [nameof(SwipeItem.Text)] = MapText,
        [nameof(SwipeItem.BackgroundColor)] = MapBackgroundColor,
        [nameof(SwipeItem.IsEnabled)] = MapIsEnabled,
        [nameof(SwipeItem.IsVisible)] = MapIsVisible,
    };

    public HarmonySwipeItemHandler() : base(Mapper) { }

    protected override ArkButton CreatePlatformElement() => new();

    protected override void ConnectHandler(ArkButton platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Click += OnClick;
    }

    protected override void DisconnectHandler(ArkButton platformView)
    {
        platformView.Click -= OnClick;
        base.DisconnectHandler(platformView);
    }

    public static void MapText(HarmonySwipeItemHandler handler, SwipeItem view)
        => handler.PlatformView.Label = view.Text ?? string.Empty;

    public static void MapBackgroundColor(HarmonySwipeItemHandler handler, SwipeItem view)
    {
        if (view.BackgroundColor is { } c)
            handler.PlatformView.SetBackgroundColor(c.ToUint());
    }

    public static void MapIsEnabled(HarmonySwipeItemHandler handler, SwipeItem view)
        => handler.PlatformView.Enabled = view.IsEnabled;

    public static void MapIsVisible(HarmonySwipeItemHandler handler, SwipeItem view)
        => handler.PlatformView.SetVisibility(
            view.IsVisible ? ArkUI_Visibility.ARKUI_VISIBILITY_VISIBLE : ArkUI_Visibility.ARKUI_VISIBILITY_NONE);

    private void OnClick(ArkUINodeEvent _)
        => ((Microsoft.Maui.ISwipeItem)VirtualView).OnInvoked();
}

/// <summary>
/// SwipeItemView 的兼容实现：复用 ContentView 通道，并在点击时执行 Command/Invoked。
/// </summary>
public class HarmonySwipeItemViewHandler : HarmonyContentViewHandler
{
    protected override void ConnectHandler(ArkColumn platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, OnClick);
    }

    protected override void DisconnectHandler(ArkColumn platformView)
    {
        platformView.UnsubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, OnClick);
        base.DisconnectHandler(platformView);
    }

    private void OnClick(ArkUINodeEvent _)
    {
        if (VirtualView is SwipeItemView item)
            item.OnInvoked();
    }
}

/// <summary>
/// FlyoutPage → Row + Flyout/Detail 两列。
/// </summary>
public class HarmonyFlyoutPageHandler : HarmonyViewHandler<FlyoutPage, ArkRow>
{
    public static PropertyMapper<FlyoutPage, HarmonyFlyoutPageHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(FlyoutPage.Flyout)] = MapFlyout,
        [nameof(FlyoutPage.Detail)] = MapDetail,
        [nameof(FlyoutPage.IsPresented)] = MapIsPresented,
        [nameof(FlyoutPage.IsGestureEnabled)] = MapIsGestureEnabled,
        [nameof(FlyoutPage.FlyoutLayoutBehavior)] = MapFlyoutLayoutBehavior,
    };

    private IElementHandler? _flyoutHandler;
    private IElementHandler? _detailHandler;
    private ArkColumn _flyoutColumn = null!;
    private ArkColumn _detailColumn = null!;

    public HarmonyFlyoutPageHandler() : base(Mapper) { }

    protected override ArkRow CreatePlatformView()
    {
        var row = new ArkRow();
        _flyoutColumn = new ArkColumn();
        _detailColumn = new ArkColumn();
        row.AddChild(_flyoutColumn);
        row.AddChild(_detailColumn);
        return row;
    }

    protected override void DisconnectHandler(ArkRow platformView)
    {
        if (_flyoutHandler is not null)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(_flyoutHandler, platformView);
        if (_detailHandler is not null)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(_detailHandler, platformView);
        base.DisconnectHandler(platformView);
    }

    public static void MapFlyout(HarmonyFlyoutPageHandler handler, FlyoutPage view)
    {
        if (handler._flyoutHandler is not null)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(handler._flyoutHandler, handler._flyoutColumn);
        if (view.Flyout is IView flyout)
        {
            var childHandler = HarmonyHandlerFactory.Create(flyout);
            handler._flyoutHandler = childHandler;
            if (childHandler.PlatformView is ArkUINode node)
            {
                node.SetWidthPercent(1.0f);
                node.SetHeightPercent(1.0f);
                handler._flyoutColumn.AddChild(node);
            }
        }
    }

    public static void MapDetail(HarmonyFlyoutPageHandler handler, FlyoutPage view)
    {
        if (handler._detailHandler is not null)
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(handler._detailHandler, handler._detailColumn);
        if (view.Detail is IView detail)
        {
            var childHandler = HarmonyHandlerFactory.Create(detail);
            handler._detailHandler = childHandler;
            if (childHandler.PlatformView is ArkUINode node)
            {
                node.SetWidthPercent(1.0f);
                node.SetHeightPercent(1.0f);
                handler._detailColumn.AddChild(node);
            }
        }
    }

    public static void MapIsPresented(HarmonyFlyoutPageHandler handler, FlyoutPage view)
        => handler._flyoutColumn.SetVisibility(
            view.IsPresented
                ? ArkUI_Visibility.ARKUI_VISIBILITY_VISIBLE
                : ArkUI_Visibility.ARKUI_VISIBILITY_NONE);

    public static void MapIsGestureEnabled(HarmonyFlyoutPageHandler handler, FlyoutPage view)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[FlyoutPage] IsGestureEnabled is degraded on HarmonyOS.");

    public static void MapFlyoutLayoutBehavior(HarmonyFlyoutPageHandler handler, FlyoutPage view)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[FlyoutPage] FlyoutLayoutBehavior is degraded on HarmonyOS.");
}

/// <summary>
/// Frame → Stack + 边框/圆角/内容。
/// </summary>
public class HarmonyFrameCompatHandler : HarmonyViewHandler<Frame, ArkStack>
{
    public static PropertyMapper<Frame, HarmonyFrameCompatHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(Frame.Content)] = MapContent,
        [nameof(Frame.BorderColor)] = MapBorderColor,
        [nameof(Frame.CornerRadius)] = MapCornerRadius,
        [nameof(Frame.HasShadow)] = MapHasShadow,
        [nameof(Frame.BackgroundColor)] = MapBackgroundColor,
    };

    public HarmonyFrameCompatHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView()
    {
        var stack = new ArkStack();
        stack.SetPaddingEdges(20, 20, 20, 20);
        return stack;
    }

    public static void MapContent(HarmonyFrameCompatHandler handler, Frame view)
        => ContentHost.Update(handler, view.Content);

    public static void MapBorderColor(HarmonyFrameCompatHandler handler, Frame view)
    {
        if (view.BorderColor is { } c)
            handler.PlatformView.SetBorderColor(c.ToUint());
    }

    public static void MapCornerRadius(HarmonyFrameCompatHandler handler, Frame view)
    {
        if (view.CornerRadius >= 0)
            handler.PlatformView.SetBorderRadius(view.CornerRadius, view.CornerRadius, view.CornerRadius, view.CornerRadius);
    }

    public static void MapHasShadow(HarmonyFrameCompatHandler handler, Frame view)
        => HarmonyOS.Interop.HiLog.Warn("HarmonyHost", "[Frame] HasShadow is degraded on HarmonyOS.");

    public static void MapBackgroundColor(HarmonyFrameCompatHandler handler, Frame view)
    {
        if (view.BackgroundColor is { } c)
            handler.PlatformView.SetBackgroundColor(c.ToUint());
    }
}

/// <summary>
/// ListView 的基础兼容实现：以 Text 行显示 ItemsSource。
/// </summary>
public class HarmonyListViewHandler : HarmonyViewHandler<ListView, ArkColumn>
{
    public static PropertyMapper<ListView, HarmonyListViewHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(ListView.ItemsSource)] = MapItemsSource,
        [nameof(ListView.SelectedItem)] = MapSelectedItem,
        [nameof(ListView.Header)] = MapItemsSource,
        [nameof(ListView.Footer)] = MapItemsSource,
        [nameof(ListView.HasUnevenRows)] = MapItemsSource,
        [nameof(ListView.RowHeight)] = MapItemsSource,
        [nameof(ListView.IsGroupingEnabled)] = MapItemsSource,
        [nameof(ListView.SeparatorVisibility)] = MapItemsSource,
        [nameof(ListView.SeparatorColor)] = MapItemsSource,
    };

    private INotifyCollectionChanged? _observed;
    private NativeChildTracker<ArkText> _rowTracker = null!;

    public HarmonyListViewHandler() : base(Mapper) { }

    protected override ArkColumn CreatePlatformView()
    {
        var column = new ArkColumn();
        _rowTracker = new NativeChildTracker<ArkText>(column.AddChild, column.RemoveAllChildren);
        return column;
    }

    protected override void DisconnectHandler(ArkColumn platformView)
    {
        if (_observed is not null)
            _observed.CollectionChanged -= OnItemsChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapItemsSource(HarmonyListViewHandler handler, ListView view)
    {
        if (handler._observed is not null)
            handler._observed.CollectionChanged -= handler.OnItemsChanged;
        if (view.ItemsSource is INotifyCollectionChanged incc)
        {
            handler._observed = incc;
            incc.CollectionChanged += handler.OnItemsChanged;
        }
        Rebuild(handler, view);
    }

    public static void MapSelectedItem(HarmonyListViewHandler handler, ListView view)
        => Rebuild(handler, view);

    private static void Rebuild(HarmonyListViewHandler handler, ListView view)
    {
        if (view.ItemsSource is null)
        {
            handler._rowTracker.Clear();
            return;
        }

        var rows = new List<ArkText>();
        var index = 0;
        foreach (var item in view.ItemsSource)
        {
            var text = new ArkText
            {
                Content = item?.ToString() ?? string.Empty,
                FontSize = (float)HarmonyControlDefaults.StatusTextFontSizeDefault,
            };
            var capturedIndex = index++;
            text.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, _ =>
            {
                view.NotifyRowTapped(capturedIndex);
            });
            rows.Add(text);
        }
        handler._rowTracker.Replace(rows);

        HarmonyOS.Interop.HiLog.Warn(
            "HarmonyHost",
            "[ListView] Cell templates and grouping are degraded; ItemsSource is rendered as text rows.");
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => MapItemsSource(this, VirtualView);
}

/// <summary>
/// TableView 的基础兼容实现：按 Section/Cell 渲染文本行。
/// </summary>
public class HarmonyTableViewHandler : HarmonyViewHandler<TableView, ArkColumn>
{
    public static PropertyMapper<TableView, HarmonyTableViewHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(TableView.Root)] = MapRoot,
        [nameof(TableView.RowHeight)] = MapRoot,
        [nameof(TableView.HasUnevenRows)] = MapRoot,
        [nameof(TableView.Intent)] = MapRoot,
    };

    public HarmonyTableViewHandler() : base(Mapper) { }

    private NativeChildTracker<ArkText> _rowTracker = null!;

    protected override ArkColumn CreatePlatformView()
    {
        var column = new ArkColumn();
        _rowTracker = new NativeChildTracker<ArkText>(column.AddChild, column.RemoveAllChildren);
        return column;
    }

    public static void MapRoot(HarmonyTableViewHandler handler, TableView view)
    {
        var rows = new List<ArkText>();
        foreach (var section in view.Root)
        {
            var title = new ArkText
            {
                Content = section.Title ?? string.Empty,
                FontSize = (float)HarmonyControlDefaults.StatusTextFontSizeDefault,
            };
            rows.Add(title);

            foreach (var cell in section)
            {
                var row = new ArkText
                {
                    Content = cell switch
                    {
                        ImageCell imageCell => imageCell.Text ?? string.Empty,
                        TextCell textCell => textCell.Text ?? string.Empty,
                        EntryCell entryCell => entryCell.Label ?? string.Empty,
                        ViewCell viewCell => viewCell.View?.GetType().Name ?? string.Empty,
                        SwitchCell switchCell => switchCell.Text ?? string.Empty,
                        _ => string.Empty,
                    },
                    FontSize = (float)HarmonyControlDefaults.StatusTextFontSizeDefault,
                };
                rows.Add(row);
            }
        }
        handler._rowTracker.Replace(rows);
    }
}
