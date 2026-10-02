// HarmonyTabbedPageHandler：MAUI TabbedPage → ArkUI Column + 底部 TabBar。
// 复用 Shell Handler 的 TabBar 模式：底部 ArkRow + 图标/标题 + 点击切换。
// TabbedPage.Children 为页面集合；SelectedItem 变更经 mapper 反向同步。
using System.Collections.Specialized;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Interop;
using HarmonyOS.Bindings.NativeNode;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkRow = HarmonyOS.ArkUI.Row;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkText = HarmonyOS.ArkUI.Text;
using ArkImage = HarmonyOS.ArkUI.Image;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;
using MGraphicsColor = Microsoft.Maui.Graphics.Color;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyTabbedPageHandler : ViewHandler<TabbedPage, ArkColumn>
{
    public static PropertyMapper<TabbedPage, HarmonyTabbedPageHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(TabbedPage.SelectedItem)] = MapSelectedItem,
        [nameof(TabbedPage.BarBackgroundColor)] = (h, v) => ((HarmonyTabbedPageHandler)h).ApplyTabBarColors(),
        [nameof(TabbedPage.BarBackground)] = (h, v) => ((HarmonyTabbedPageHandler)h).ApplyTabBarColors(),
        [nameof(TabbedPage.BarTextColor)] = (h, v) => ((HarmonyTabbedPageHandler)h).ApplyTabBarColors(),
        [nameof(TabbedPage.SelectedTabColor)] = (h, v) => ((HarmonyTabbedPageHandler)h).ApplyTabBarColors(),
        [nameof(TabbedPage.UnselectedTabColor)] = (h, v) => ((HarmonyTabbedPageHandler)h).ApplyTabBarColors(),
    };

    public HarmonyTabbedPageHandler() : base(Mapper) { }

    private ArkStack? _contentHost;
    private ArkRow? _tabBar;
    private readonly Dictionary<Page, ArkUINode> _nodes = new();
    private readonly List<(ArkUINode Node, Page Page, ArkText Title)> _tabEntries = new();
    private Page? _visiblePage;
    private ArkUINode? _visibleNode;

    protected override ArkColumn CreatePlatformView()
    {
        var root = new ArkColumn();
        root.SetWidthPercent(1.0f);
        root.SetHeightPercent(1.0f);
        return root;
    }

    protected override void ConnectHandler(ArkColumn platformView)
    {
        base.ConnectHandler(platformView);

        _contentHost = new ArkStack();
        _contentHost.SetWidthPercent(1.0f);
        _contentHost.SetFlexGrow(1f);
        platformView.AddChild(_contentHost);

        _tabBar = new ArkRow();
        _tabBar.SetWidthPercent(1.0f);
        _tabBar.SetHeight(HarmonyShellTheme.TabBarHeightVp);
        platformView.AddChild(_tabBar);

        BuildTabBar();
        ShowInitial();
        // 运行时增删页面：重建 TabBar（页面节点缓存 _nodes 保留，已建页不丢状态）
        if (VirtualView?.Children is INotifyCollectionChanged notifying)
            notifying.CollectionChanged += OnChildrenChanged;
    }

    protected override void DisconnectHandler(ArkColumn platformView)
    {
        if (VirtualView?.Children is INotifyCollectionChanged notifying)
            notifying.CollectionChanged -= OnChildrenChanged;
        foreach (var (node, _, _) in _tabEntries)
            node.Dispose();
        _tabEntries.Clear();
        foreach (var page in _nodes.Keys.ToArray())
        {
            if (_nodes.Remove(page, out var node))
            {
                if (ReferenceEquals(_visibleNode, node))
                    _visibleNode = null;
                if (ReferenceEquals(_visiblePage, page))
                    _visiblePage = null;
                page.Handler = null;
                node.Dispose();
            }
        }
        _visibleNode = null;
        _visiblePage = null;
        base.DisconnectHandler(platformView);
    }

    private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_tabBar is null)
            return;
        foreach (var (node, _, _) in _tabEntries)
            node.Dispose();
        _tabEntries.Clear();
        BuildTabBar();
        // 当前可见页被移除时回退到剩余首页
        if (VirtualView is { } tp && _visiblePage is not null && !tp.Children.Contains(_visiblePage))
        {
            _visiblePage = null;
            _visibleNode = null;
            ShowInitial();
        }
    }

    private static void MapSelectedItem(HarmonyTabbedPageHandler handler, TabbedPage tabbedPage)
    {
        if (tabbedPage.SelectedItem is Page selected)
            handler.ShowPage(selected);
    }

    private void BuildTabBar()
    {
        var tabbedPage = VirtualView;
        if (_tabBar is null || tabbedPage is null)
            return;
        var pages = tabbedPage.Children.OfType<Page>().ToList();
        _tabBar.Visible = pages.Count > 1;
        foreach (var page in pages)
        {
            var entry = new ArkColumn();
            entry.SetFlexGrow(1f);
            entry.SetHeightPercent(1.0f);
            entry.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);

            var title = new ArkText();
            title.Content = string.IsNullOrEmpty(page.Title) ? page.GetType().Name : page.Title;
            title.TextAlign = ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_CENTER;
            title.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
            entry.AddChild(title);

            var captured = page;
            entry.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, _ => ShowPage(captured));
            _tabBar.AddChild(entry);
            _tabEntries.Add((entry, page, title));
        }
        ApplyTabBarColors();
    }

    private void ApplyTabBarColors()
    {
        var tp = VirtualView;
        if (tp is null || _tabBar is null)
            return;

        // TabBar 背景色（BarBackgroundColor 优先于 BarBackground）
        if (tp.BarBackgroundColor is { } barBg)
            _tabBar.SetBackgroundColor(barBg.ToUint());
        else if (tp.BarBackground is { } barBgBrush)
            BrushHelper.ApplyBackground(_tabBar, barBgBrush);

        var selected = tp.SelectedItem as Page;
        var fgColor = tp.BarTextColor ?? HarmonyShellTheme.TabSelected;
        var unselected = tp.UnselectedTabColor ?? HarmonyShellTheme.TabUnselected;
        var selectedColor = tp.SelectedTabColor ?? fgColor;

        foreach (var (entry, page, title) in _tabEntries)
        {
            var c = ReferenceEquals(page, selected) ? selectedColor : unselected;
            title.SetFontColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255));
        }
    }

    private void ShowInitial()
    {
        var tabbedPage = VirtualView;
        if (tabbedPage is null)
            return;
        var first = tabbedPage.SelectedItem as Page
            ?? tabbedPage.Children.OfType<Page>().FirstOrDefault();
        if (first is not null)
            ShowPage(first);
    }

    internal void ShowPage(Page page)
    {
        if (_contentHost is null || ReferenceEquals(_visiblePage, page))
            return;
        _visiblePage?.SendDisappearing();

        var node = GetOrCreateNode(page);
        if (_visibleNode is not null)
            _contentHost.RemoveChild(_visibleNode);
        _contentHost.AddChild(node);
        _visiblePage = page;
        _visibleNode = node;
        page.SendAppearing();

        if (VirtualView is { } tp && !ReferenceEquals(tp.SelectedItem, page))
        {
            try { tp.SelectedItem = page; }
            catch (Exception ex) { HiLog.Warn("HarmonyHost", $"[TabbedPage] SelectedItem 反向同步失败: {ex.GetType().Name}"); }
        }
        ApplyTabBarColors();
    }

    private ArkUINode GetOrCreateNode(Page page)
    {
        if (_nodes.TryGetValue(page, out var cached))
            return cached;
        var handler = HarmonyHandlerFactory.Create((Element)page);
        var node = handler.PlatformView as ArkUINode
            ?? throw new InvalidOperationException(
                $"page handler PlatformView is not an ArkUI node: {handler.PlatformView?.GetType().Name}");
        node.SetWidthPercent(1.0f);
        node.SetHeightPercent(1.0f);
        _nodes[page] = node;
        return node;
    }
}
