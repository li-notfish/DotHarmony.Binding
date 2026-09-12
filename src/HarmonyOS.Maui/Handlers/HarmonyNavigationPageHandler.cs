// HarmonyNavigationPageHandler：MAUI NavigationPage 的鸿蒙 Handler（转接层）。
// NavigationPage 的 Push/Pop 经 MauiNavigationImpl → SendHandlerUpdateAsync 打包整包
// NavigationRequest 经 Handler.Invoke("RequestNavigation") 到达本 handler；
// 平台侧把 ArkUI 节点栈同步成请求的栈后，必须回调 IStackNavigation.NavigationFinished，
// 否则 SendHandlerUpdateAsync 内 await 永久挂起（PushAsync 不返回）。
// 子页 NavigationProxy.Inner 由 NavigableElement.OnParentSet 沿父链自动接线，无需干预。
//
// 平台结构：根 Column = 标题栏（ArkRow，可隐藏）+ 内容区（ArkStack，FlexGrow 占满）。
// 出栈过渡：旧栈顶淡出（AnimateAsync 完成回调为汇合点）→ 摘除释放 → NavigationFinished
// （SendHandlerUpdateAsync 因此延后一个动画时长返回，属预期）。
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Maui.Hosting;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkRow = HarmonyOS.ArkUI.Row;
using ArkText = HarmonyOS.ArkUI.Text;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI NavigationPage 的 HarmonyOS Handler（标题栏 + ArkUI Stack 承载页面栈）。</summary>
public class HarmonyNavigationPageHandler : ViewHandler<NavigationPage, ArkColumn>
{
    public static readonly PropertyMapper<NavigationPage, HarmonyNavigationPageHandler> Mapper =
        new(ViewMapper)
        {
            [nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
            [nameof(NavigationPage.BarBackground)] = MapBarBackground,
        };

    public static readonly CommandMapper<NavigationPage, HarmonyNavigationPageHandler> Commands =
        new(ViewCommandMapper)
        {
            [nameof(IStackNavigation.RequestNavigation)] = MapRequestNavigation,
        };

    public HarmonyNavigationPageHandler() : base(Mapper, Commands) { }

    // ───────────────────────── 平台结构 ─────────────────────────

    private ArkRow? _toolbar;
    private ArkText? _backButton;
    private ArkText? _titleLabel;
    private ArkStack? _content;

    protected override ArkColumn CreatePlatformView()
    {
        var root = new ArkColumn();
        root.SetWidthPercent(1.0f);
        root.SetHeightPercent(1.0f);

        // 标题栏：返回键 + 标题（高 56vp，HasNavigationBar=false 或栈深 1 时隐藏返回键/整栏）
        _toolbar = new ArkRow();
        _toolbar.SetWidthPercent(1.0f);
        _toolbar.SetHeight(56f);
        _backButton = new ArkText();
        _backButton.Content = "←";
        _backButton.SetMarginEdges(0, 0, 0, 12f);
        _backButton.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
        _backButton.Click += OnBackClicked;
        _titleLabel = new ArkText();
        _titleLabel.Content = string.Empty;
        _titleLabel.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
        _toolbar.AddChild(_backButton);
        _toolbar.AddChild(_titleLabel);
        root.AddChild(_toolbar);

        // 内容区：占满标题栏以下剩余空间
        _content = new ArkStack();
        _content.SetWidthPercent(1.0f);
        _content.SetFlexGrow(1f);
        root.AddChild(_content);

        return root;
    }

    protected override void DisconnectHandler(ArkColumn platformView)
    {
        if (_backButton is not null)
            _backButton.Click -= OnBackClicked;
        UnsubscribeTitleTracking();
        base.DisconnectHandler(platformView);
    }

    private void OnBackClicked(HarmonyOS.Bindings.NativeNode.ArkUINodeEvent _)
    {
        // 返回键 → MAUI 标准导航协议（经 MauiNavigationImpl 走 RequestNavigation 回到本 handler）
        VirtualView.PopAsync().FireAndForget();
    }

    // ───────────────────────── 栈同步 ─────────────────────────

    // 平台侧当前栈：顺序 = MAUI 导航栈；节点摘除保留句柄（返回时重挂，状态不丢）
    readonly List<Page> _mounted = new();
    readonly Dictionary<Page, ArkUINode> _nodes = new();
    readonly Dictionary<Page, System.ComponentModel.PropertyChangedEventHandler> _titleWatchers = new();

    static void MapRequestNavigation(HarmonyNavigationPageHandler handler, NavigationPage page, object? args)
    {
        if (args is not NavigationRequest request)
            return;

        var target = request.NavigationStack.Cast<Page>().ToList();
        var popped = handler._mounted.Where(p => !target.Contains(p)).ToList();

        // 出栈且旧栈顶就是要摘除的页：先淡出（Animate 的完成回调在 UI 线程内联执行），
        // 完成后同步栈并回调 NavigationFinished——SendHandlerUpdateAsync 因此延后一个
        // 动画时长返回，属预期；NavigationFinished 必须恰好一次，否则 PushAsync 挂起
        if (popped.Count > 0
            && handler._mounted.Count > 0
            && handler._mounted[^1] is var oldTop
            && popped.Contains(oldTop)
            && handler._nodes.GetValueOrDefault(oldTop) is { } oldNode)
        {
            oldNode.Animate(
                () => oldNode.SetOpacity(0f),
                () =>
                {
                    handler.SyncToStack(request);
                    ((IStackNavigation)handler.VirtualView).NavigationFinished(request.NavigationStack);
                });
            return;
        }

        handler.SyncToStack(request);
        // handler 完成后必须回调，NavigationPage 才会置 CurrentPage/RootPage 并完成 Pending Task
        ((IStackNavigation)handler.VirtualView).NavigationFinished(request.NavigationStack);
    }

    private void SyncToStack(NavigationRequest request)
    {
        var target = request.NavigationStack.Cast<Page>().ToList();

        // 出栈页（Pop/PopToRoot/Remove）：平台节点摘除并释放（同一实例再入栈时重新装配）
        foreach (var page in _mounted.Where(p => !target.Contains(p)).ToList())
        {
            if (_nodes.Remove(page, out var node))
            {
                UnsubscribeTitleTracking(page);
                _content!.RemoveChild(node);
                node.Dispose();
            }
            _mounted.Remove(page);
        }

        // 新入栈页：装配 handler + 平台节点
        foreach (var page in target)
        {
            if (_nodes.ContainsKey(page))
                continue;

            var pageHandler = HarmonyHandlerFactory.Create((Microsoft.Maui.Controls.Element)page);
            pageHandler.SetVirtualView(page);
            var node = pageHandler.PlatformView as ArkUINode
                ?? throw new InvalidOperationException(
                    $"page handler PlatformView is not an ArkUI node: {pageHandler.PlatformView?.GetType().Name}");
            _nodes[page] = node;
            _mounted.Add(page);
        }

        // 仅栈顶挂入内容区；低层页节点摘除保留（与 HarmonyNavigation 同一策略）
        _content!.RemoveAllChildren();
        if (target.Count > 0)
        {
            var topPage = target[^1];
            _content.AddChild(_nodes[topPage]);
            SubscribeTitleTracking(topPage);
        }

        UpdateToolbar(target);
    }

    // ───────────────────────── 标题栏 ─────────────────────────

    private void UpdateToolbar(List<Page> target)
    {
        if (_toolbar is null || _content is null)
            return;
        var top = target.Count > 0 ? target[^1] : null;

        bool hasBar = top is null || NavigationPage.GetHasNavigationBar(top);
        _toolbar.Visible = hasBar;
        _backButton!.Visible = target.Count > 1;
        _titleLabel!.Content = top?.Title ?? string.Empty;
        _toolbar.SetZIndex(int.MaxValue); // 覆盖页面内容之上（模态在宿主层，不受影响）
    }

    private void SubscribeTitleTracking(Page page)
    {
        if (_titleWatchers.ContainsKey(page))
            return;
        System.ComponentModel.PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == nameof(Page.Title) && s is Page p)
                _titleLabel!.Content = p.Title ?? string.Empty;
        };
        page.PropertyChanged += handler;
        _titleWatchers[page] = handler;
    }

    private void UnsubscribeTitleTracking(Page? page = null)
    {
        if (page is not null)
        {
            if (_titleWatchers.Remove(page, out var handler))
                page.PropertyChanged -= handler;
            return;
        }
        foreach (var (watched, handler) in _titleWatchers)
            watched.PropertyChanged -= handler;
        _titleWatchers.Clear();
    }

    // ───────────────────────── 颜色 ─────────────────────────

    static void MapBackgroundColor(HarmonyNavigationPageHandler h, NavigationPage v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    static void MapBarBackground(HarmonyNavigationPageHandler h, NavigationPage v)
    {
        if (_toolbarField(h) is { } toolbar)
            BrushHelper.ApplyBackground(toolbar, v.BarBackground);
    }

    private static ArkRow? _toolbarField(HarmonyNavigationPageHandler h) => h._toolbar;
}
