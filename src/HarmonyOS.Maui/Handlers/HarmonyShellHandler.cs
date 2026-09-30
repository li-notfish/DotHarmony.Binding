// HarmonyShellHandler：MAUI Shell 第一版视觉宿主。
// 平台结构：根 Column = 内容区（ArkStack，FlexGrow 占满）+ 底部 TabBar（ArkRow，多条目时显示）。
// ShellContent 页经 IShellContentController.Page 惰性创建，节点按 Page 缓存（条目切换保留
// 状态）；注册路由推送页同驻内容区。路由/栈语义在 HarmonyShellNavigation（自持，不依赖
// MAUI Shell 的平台 fragment 机制）；MAUI 侧 CurrentItem 变更经 mapper 反向同步。
// TabBar 点击用通用 NODE_ON_CLICK（生成组件的 Click 包装仅覆盖部分节点类型）。
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkRow = HarmonyOS.ArkUI.Row;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkText = HarmonyOS.ArkUI.Text;
using ArkImage = HarmonyOS.ArkUI.Image;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyShellHandler : ViewHandler<Shell, ArkColumn>
{
    public static PropertyMapper<Shell, HarmonyShellHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        // 应用/MAUI 内部改 CurrentItem 时反向同步（选择状态由 HarmonyShellNavigation 自持；
        // Controls Shell 的 BP 变更不保证触发 UpdateValue，此条为尽力而为）
        [nameof(Shell.CurrentItem)] = MapCurrentItem,
    };

    public HarmonyShellHandler() : base(Mapper) { }

    private ArkStack? _contentHost;
    private ArkRow? _tabBar;
    private readonly Dictionary<Page, ArkUINode> _nodes = new();
    private readonly List<(ArkUINode Node, ShellItem Item, ArkText Title)> _tabEntries = new();
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
        _tabBar.SetHeight(56f);
        platformView.AddChild(_tabBar);

        BuildTabBar();
        Hosting.HarmonyShellNavigation.Attach(VirtualView, this);
    }

    protected override void DisconnectHandler(ArkColumn platformView)
    {
        // 先通知 HarmonyShellNavigation 归还其缓存/栈页（Shell 重建不留静态残留）；
        // 仍留在 _nodes 里的残余页（如曾 ShowPage 但未入库的边缘值）逐项走 ReleasePage 释放
        Hosting.HarmonyShellNavigation.Detach(this);
        foreach (var (node, _, _) in _tabEntries)
            node.Dispose();
        _tabEntries.Clear();
        while (_nodes.Count > 0)
        {
            foreach (var page in _nodes.Keys.ToArray())
                ReleasePage(page); // 断连 handler 链并 Dispose 节点
        }
        _visibleNode = null;
        _visiblePage = null;
        base.DisconnectHandler(platformView);
    }

    private static void MapCurrentItem(HarmonyShellHandler handler, Shell shell)
        => Hosting.HarmonyShellNavigation.SyncFromShell(shell);

    // ───────────────────────── TabBar ─────────────────────────

    private void BuildTabBar()
    {
        var shell = VirtualView;
        if (_tabBar is null || shell is null)
            return;
        var items = shell.Items.ToList();
        // 单条目不显示 TabBar（对齐主流平台 Shell 行为）
        _tabBar.Visible = items.Count > 1;
        foreach (var item in items)
        {
            var entry = new ArkColumn();
            entry.SetFlexGrow(1f);
            entry.SetHeightPercent(1.0f);
            entry.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);

            // 图标（FileImageSource 等可解析来源）；Icon 缺省时仅渲染标题
            if (ImageSourceResolver.Resolve(item.Icon) is { } iconSrc)
            {
                var icon = new ArkImage();
                icon.Src = iconSrc;
                icon.SetWidth(20f);
                icon.SetHeight(20f);
                icon.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
                entry.AddChild(icon);
            }

            var title = new ArkText();
            title.Content = string.IsNullOrEmpty(item.Title) ? DisplayKey(item) : item.Title;
            title.TextAlign = ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_CENTER;
            title.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
            entry.AddChild(title);

            entry.SubscribeEvent(
                ArkUI_NodeEventType.NODE_ON_CLICK,
                _ => Hosting.HarmonyShellNavigation.SelectItem(shell, item));
            _tabBar.AddChild(entry);
            _tabEntries.Add((entry, item, title));
        }
        ApplyTabColors(shell.CurrentItem);
    }

    private static string DisplayKey(ShellItem item)
        => string.IsNullOrEmpty(item.Route) ? item.GetType().Name : item.Route;

    internal void ApplyTabColors(ShellItem? selected)
    {
        // selected/unselected 色走静态兜底（选中黑 / 未选灰）：MAUI 的 Shell attached
        // 颜色访问器非公开 API，完整主题接入随 Shell 子阶段进行
        foreach (var (entry, item, title) in _tabEntries)
        {
            title.SetFontColor(ReferenceEquals(item, selected)
                ? Microsoft.Maui.Graphics.Colors.Black
                : Microsoft.Maui.Graphics.Colors.Gray);
        }
    }

    // ───────────────────────── 内容区 ─────────────────────────

    /// <summary>切换可见页（由 HarmonyShellNavigation 驱动）；页面节点按 Page 缓存保留。</summary>
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
    }

    internal void ShowEmpty()
    {
        if (_contentHost is null)
            return;
        _visiblePage?.SendDisappearing();
        if (_visibleNode is not null)
            _contentHost.RemoveChild(_visibleNode);
        _visibleNode = null;
        _visiblePage = null;
    }

    /// <summary>释放弹出页（不可达）：断连 handler 并释放平台节点子树。</summary>
    internal void ReleasePage(Page page)
    {
        if (!_nodes.Remove(page, out var node))
            return;
        if (ReferenceEquals(_visibleNode, node))
        {
            _contentHost?.RemoveChild(node);
            _visibleNode = null;
        }
        if (ReferenceEquals(_visiblePage, page))
            _visiblePage = null;
        page.Handler = null; // 触发 DisconnectHandler（手势/事件订阅清理）
        node.Dispose();
    }

    private ArkUINode GetOrCreateNode(Page page)
    {
        if (_nodes.TryGetValue(page, out var cached))
            return cached;
        var handler = HarmonyHandlerFactory.Create((Element)page);
        handler.SetVirtualView(page);
        var node = handler.PlatformView as ArkUINode
            ?? throw new InvalidOperationException(
                $"page handler PlatformView is not an ArkUI node: {handler.PlatformView?.GetType().Name}");
        node.SetWidthPercent(1.0f);
        node.SetHeightPercent(1.0f);
        _nodes[page] = node;
        return node;
    }
}
