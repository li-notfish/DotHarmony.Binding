// HarmonyShellHandler：MAUI Shell 视觉宿主（Flyout + TabBar + 主题色）。
// 平台结构：根 Stack = 主 Column（顶部导航栏 + 内容区 + 底部 TabBar）+ 遮罩 + Flyout 面板
//（后两者为根直接子节点，z 序在上；FlyoutBehavior.Locked 时主列右移收窄与面板并排）。
// ShellContent 页经 IShellContentController.Page 惰性创建，节点按 Page 缓存（条目切换保留
// 状态）；注册路由推送页同驻内容区。路由/栈语义在 HarmonyShellNavigation（自持，不依赖
// MAUI Shell 的平台 fragment 机制）；MAUI 侧 CurrentItem 变更经 mapper 反向同步。
// TabBar 点击用通用 NODE_ON_CLICK（生成组件的 Click 包装仅覆盖部分节点类型）。
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

public class HarmonyShellHandler : ViewHandler<Shell, ArkStack>
{
    public static PropertyMapper<Shell, HarmonyShellHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(Shell.CurrentItem)] = MapCurrentItem,
        [nameof(Shell.FlyoutIsPresented)] = MapFlyoutIsPresented,
        [nameof(Shell.FlyoutBehavior)] = MapFlyoutBehavior,
        // NavBarIsVisible/TabBarIsVisible 是 attached 属性，mapper 键取属性名
        [Shell.NavBarIsVisibleProperty.PropertyName] = MapChromeVisibility,
        [Shell.TabBarIsVisibleProperty.PropertyName] = MapChromeVisibility,
    };

    public HarmonyShellHandler() : base(Mapper) { }

    private ArkColumn? _mainColumn;
    private ArkRow? _topBar;
    private ArkText? _hamburger;
    private ArkText? _topBarTitle;
    private ArkStack? _contentHost;
    private ArkRow? _tabBar;
    private ArkStack? _backdrop;
    private ArkColumn? _flyoutPanel;
    private bool _flyoutVisible;
    private readonly Dictionary<Page, ArkUINode> _nodes = new();
    private readonly List<(ArkUINode Node, ShellItem Item, ArkText Title)> _tabEntries = new();
    private readonly List<(ArkUINode Node, ShellItem Item)> _flyoutEntries = new();
    private IElementHandler? _flyoutHeaderHandler;
    private Page? _visiblePage;
    private ArkUINode? _visibleNode;
    private bool _tabBarShown = true; // 初值 true：单条目 Shell 首次 ApplyChromeVisibility 才会产生"变更"并落 NONE
    private bool _navBarShown = true;
    private float _lastColumnHeight;
    private float _lastRootWidth;
    private bool _syncingFlyout; // FlyoutIsPresented 双向同步回环防护

    // 内容区高度显式回填（NODE_FLEX_GROW 在 Column 内不可靠，见 ConnectHandler 注释）：
    // 列高 - （可见时）顶栏 - （可见时）TabBar
    private void RelayoutContent()
    {
        if (_contentHost is null || _lastColumnHeight <= 0)
            return;
        var contentHeight = _lastColumnHeight
            - (_navBarShown ? HarmonyShellTheme.TopBarHeightVp : 0f)
            - (_tabBarShown ? HarmonyShellTheme.TabBarHeightVp : 0f);
        if (contentHeight > 0)
            _contentHost.SetHeight(contentHeight);
    }

    private void OnMainColumnSizeChange(ArkUINodeEvent e)
    {
        _lastColumnHeight = e.SizeChangeHeight;
        RelayoutContent();
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        => ApplyThemeColors();

    protected override ArkStack CreatePlatformView()
    {
        var root = new ArkStack();
        root.SetWidthPercent(1.0f);
        root.SetHeightPercent(1.0f);
        return root;
    }

    protected override void ConnectHandler(ArkStack platformView)
    {
        base.ConnectHandler(platformView);

        // 主内容列：顶部导航栏 + 内容区 + 底部 TabBar
        _mainColumn = new ArkColumn();
        _mainColumn.SetWidthPercent(1.0f);
        _mainColumn.SetHeightPercent(1.0f);
        platformView.AddChild(_mainColumn);

        // 顶部导航栏：汉堡按钮 + 标题
        _topBar = new ArkRow();
        _topBar.SetWidthPercent(1.0f);
        _topBar.SetHeight(HarmonyShellTheme.TopBarHeightVp);
        _mainColumn.AddChild(_topBar);

        _hamburger = new ArkText();
        _hamburger.Content = "\u2630";
        _hamburger.FontSize = HarmonyShellTheme.HamburgerFontSize;
        _hamburger.SetMarginEdges(0, HarmonyShellTheme.HamburgerMarginRight, 0, HarmonyShellTheme.HamburgerMarginLeft);
        _hamburger.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
        _hamburger.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, _ => ToggleFlyout());
        _topBar.AddChild(_hamburger);

        _topBarTitle = new ArkText();
        _topBarTitle.FontSize = HarmonyShellTheme.TopBarTitleFontSize;
        _topBarTitle.SetFlexGrow(1f);
        _topBarTitle.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
        _topBar.AddChild(_topBarTitle);

        _contentHost = new ArkStack();
        _contentHost.SetWidthPercent(1.0f);
        _mainColumn.AddChild(_contentHost);

        _tabBar = new ArkRow();
        _tabBar.SetWidthPercent(1.0f);
        _tabBar.SetHeight(HarmonyShellTheme.TabBarHeightVp);
        _mainColumn.AddChild(_tabBar);

        // ArkUI NDK 的 NODE_FLEX_GROW 在 Column 内不按"剩余空间"收缩（实测内容区拿了
        // 整列高，把固定高 TabBar 挤出可视区）——改为列尺寸就绪后显式回填内容区高度：
        // 列高 - 顶栏 - （可见时）TabBar
        _mainColumn.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_SIZE_CHANGE, OnMainColumnSizeChange);

        // 根尺寸：Locked 并排让宽的数据源
        platformView.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_SIZE_CHANGE, OnRootSizeChange);

        // 遮罩（全屏，仅 Flyout 模式展开时可见；点击关闭）。面板与遮罩同为根直接
        // 子节点而非覆盖层容器，Locked 并排时无全屏容器遮挡内容区命中测试。
        _backdrop = new ArkStack();
        _backdrop.SetWidthPercent(1.0f);
        _backdrop.SetHeightPercent(1.0f);
        _backdrop.SetBackgroundColor(0, 0, 0, HarmonyShellTheme.BackdropAlpha);
        _backdrop.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, _ => ToggleFlyout());
        _backdrop.Visible = false;
        platformView.AddChild(_backdrop);

        // Flyout 面板（左侧，宽度/背景/内边距走主题；关闭时隐藏）
        _flyoutPanel = new ArkColumn();
        _flyoutPanel.SetWidth(HarmonyShellTheme.FlyoutWidthVp);
        _flyoutPanel.SetHeightPercent(1.0f);
        _flyoutPanel.SetPosition(0f, 0f);
        _flyoutPanel.SetTranslate(-HarmonyShellTheme.FlyoutWidthVp, 0f);
        _flyoutPanel.Visible = false;
        _flyoutPanel.SetPaddingEdges(
            HarmonyShellTheme.FlyoutPaddingTop,
            HarmonyShellTheme.FlyoutPaddingRight,
            HarmonyShellTheme.FlyoutPaddingBottom,
            HarmonyShellTheme.FlyoutPaddingLeft);
        platformView.AddChild(_flyoutPanel);

        BuildTabBar();
        BuildFlyout();
        ApplyThemeColors();
        ApplyFlyoutBehavior();
        ApplyChromeVisibility();
        if (Application.Current is { } application)
            application.RequestedThemeChanged += OnRequestedThemeChanged;
        Hosting.HarmonyShellNavigation.Attach(VirtualView, this);
    }

    protected override void DisconnectHandler(ArkStack platformView)
    {
        if (Application.Current is { } application)
            application.RequestedThemeChanged -= OnRequestedThemeChanged;
        Hosting.HarmonyShellNavigation.Detach(this);
        // 关闭动画可能被断开打断，强制复位遮罩/面板，避免悬挂在 Visible=true
        _flyoutVisible = false;
        if (_backdrop is { } backdrop)
            backdrop.Visible = false;
        if (_flyoutPanel is { } panel)
            panel.Visible = false;
        // FlyoutHeader 的 handler 不入 _flyoutEntries，单独断连并释放节点子树
        if (_flyoutHeaderHandler is { } headerHandler)
        {
            if (headerHandler.VirtualView is { } headerView)
                headerView.Handler = null;
            (headerHandler.PlatformView as ArkUINode)?.Dispose();
            _flyoutHeaderHandler = null;
        }
        foreach (var (node, _, _) in _tabEntries)
            node.Dispose();
        _tabEntries.Clear();
        foreach (var (node, _) in _flyoutEntries)
            node.Dispose();
        _flyoutEntries.Clear();
        while (_nodes.Count > 0)
        {
            foreach (var page in _nodes.Keys.ToArray())
                ReleasePage(page);
        }
        _visibleNode = null;
        _visiblePage = null;
        // 外壳子树（主列/顶栏/内容区/TabBar/遮罩/Flyout 面板）随根 Stack 级联释放——
        // ViewHandler 不自动 Dispose PlatformView，不释放则 Shell 重建时整树泄漏
        _flyoutPanel = null;
        _backdrop = null;
        _contentHost = null;
        _tabBar = null;
        _topBarTitle = null;
        _hamburger = null;
        _topBar = null;
        _mainColumn = null;
        platformView.Dispose();
        base.DisconnectHandler(platformView);
    }

    private static void MapCurrentItem(HarmonyShellHandler handler, Shell shell)
        => Hosting.HarmonyShellNavigation.SyncFromShell(shell);

    // ───────────────────────── TabBar / Flyout ─────────────────────────

    private void BuildTabBar()
    {
        var shell = VirtualView;
        if (_tabBar is null || shell is null)
            return;
        var items = shell.Items.ToList();
        // 可见性统一由 ApplyChromeVisibility 合成（条目数 > 1 且页级 TabBarIsVisible），
        // None（不占位）而非 Hidden：显式高度布局下 Hidden 仍占 56vp 会把内容区顶出界
        foreach (var item in items)
        {
            var entry = new ArkColumn();
            entry.SetFlexGrow(1f);
            entry.SetHeightPercent(1.0f);
            entry.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);

            if (ImageSourceResolver.Resolve(item.Icon) is { } iconSrc)
            {
                var icon = new ArkImage();
                icon.Src = iconSrc;
                icon.SetWidth(HarmonyShellTheme.TabIconSizeVp);
                icon.SetHeight(HarmonyShellTheme.TabIconSizeVp);
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
    }

    private void BuildFlyout()
    {
        var shell = VirtualView;
        if (_flyoutPanel is null || shell is null)
            return;

        // Flyout 背景色（先落白色兜底，再叠加自定义画刷）
        var defaultBg = HarmonyShellTheme.FlyoutBackground;
        _flyoutPanel.SetBackgroundColor(
            (byte)(defaultBg.Red * 255), (byte)(defaultBg.Green * 255),
            (byte)(defaultBg.Blue * 255), (byte)(defaultBg.Alpha * 255));
        var flyoutBg = shell.GetValue(Shell.FlyoutBackgroundProperty) as Brush;
        if (flyoutBg is { } bg)
            BrushHelper.ApplyBackground(_flyoutPanel, bg);

        // Flyout 前景色
        var fgColor = HarmonyShellTheme.FlyoutForeground;

        // FlyoutHeader（View 或 DataTemplate）
        if (shell.GetValue(Shell.FlyoutHeaderProperty) is { } header)
        {
            if (header is View headerView)
                AttachFlyoutHeader(headerView, shell);
            else if (header is DataTemplate template && template.CreateContent() is View templateView)
                AttachFlyoutHeader(templateView, shell);
        }

        // Flyout 条目（FlyoutItem / ShellContent 均可作为菜单项）
        foreach (var item in shell.Items)
        {
            var row = new ArkRow();
            row.SetWidthPercent(1.0f);
            row.SetHeight(HarmonyShellTheme.FlyoutItemHeightVp);
            row.SetMarginEdges(4f, 12f, 4f, HarmonyShellTheme.FlyoutItemMarginLeft);
            row.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);

            if (ImageSourceResolver.Resolve(item.Icon) is { } iconSrc)
            {
                var icon = new ArkImage();
                icon.Src = iconSrc;
                icon.SetWidth(HarmonyShellTheme.FlyoutIconSizeVp);
                icon.SetHeight(HarmonyShellTheme.FlyoutIconSizeVp);
                icon.SetMarginEdges(0, 8f, 0, 0);
                icon.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
                row.AddChild(icon);
            }

            var label = new ArkText();
            label.Content = string.IsNullOrEmpty(item.Title) ? DisplayKey(item) : item.Title;
            label.FontSize = HarmonyShellTheme.FlyoutItemFontSize;
            label.SetMarginEdges(0, 8f, 0, 0);
            label.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
            label.SetFontColor((byte)(fgColor.Red * 255), (byte)(fgColor.Green * 255),
                (byte)(fgColor.Blue * 255));
            row.AddChild(label);

            var captured = item;
            row.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, _ =>
            {
                ToggleFlyout();
                Hosting.HarmonyShellNavigation.SelectItem(shell, captured);
            });
            _flyoutPanel.AddChild(row);
            _flyoutEntries.Add((row, item));
        }
    }

    /// <summary>装配 FlyoutHeader：统一补 Parent、创建 handler 并持有引用（Disconnect 时释放）。</summary>
    private void AttachFlyoutHeader(View headerView, Shell shell)
    {
        if (_flyoutPanel is null)
            return;
        headerView.Parent ??= shell;
        _flyoutHeaderHandler = HarmonyHandlerFactory.Create((Element)headerView);
        if (_flyoutHeaderHandler.PlatformView is ArkUINode headerNode)
        {
            headerNode.SetWidthPercent(1.0f);
            _flyoutPanel.AddChild(headerNode);
        }
    }

    private static string DisplayKey(ShellItem item)
        => string.IsNullOrEmpty(item.Route) ? item.GetType().Name : item.Route;

    private void ToggleFlyout()
    {
        SetFlyoutVisible(!_flyoutVisible, syncToShell: true);
    }

    /// <summary>Flyout 开关统一入口：UI 手势与 Shell.FlyoutIsPresented 双向同源。</summary>
    private void SetFlyoutVisible(bool visible, bool syncToShell)
    {
        var shell = VirtualView;
        var behavior = shell?.FlyoutBehavior ?? FlyoutBehavior.Flyout;
        if (behavior == FlyoutBehavior.Disabled && visible)
            return; // Disabled：拒绝打开
        if (behavior == FlyoutBehavior.Locked && !visible)
            return; // Locked：常驻，拒绝关闭
        if (_flyoutVisible == visible)
            return;
        _flyoutVisible = visible;
        if (_backdrop is not { } backdrop || _flyoutPanel is not { } panel)
            return;

        // 反向同步 MAUI 侧（回环由 MapFlyoutIsPresented 的 _syncingFlyout 守卫）
        if (syncToShell && shell is not null)
        {
            _syncingFlyout = true;
            try { shell.FlyoutIsPresented = visible; }
            finally { _syncingFlyout = false; }
        }

        if (_flyoutVisible)
        {
            panel.Visible = true;
            // Locked 并排：无遮罩（遮罩会阻断内容区命中测试）
            backdrop.Visible = behavior == FlyoutBehavior.Flyout;
            panel.SetTranslate(-HarmonyShellTheme.FlyoutWidthVp, 0f);
            panel.Animate(
                () => panel.SetTranslate(0f, 0f),
                null,
                HarmonyShellTheme.FlyoutAnimationMs);
        }
        else
        {
            backdrop.Visible = false;
            panel.Animate(
                () => panel.SetTranslate(-HarmonyShellTheme.FlyoutWidthVp, 0f),
                // 关闭动画期间被重新打开时不得隐藏（完成回调不可取消，加状态守卫）
                () => { if (!_flyoutVisible) panel.Visible = false; },
                HarmonyShellTheme.FlyoutAnimationMs);
        }
    }

    // ───────────────────────── Shell 语义映射（M2） ─────────────────────────

    private static void MapFlyoutIsPresented(HarmonyShellHandler handler, Shell shell)
    {
        if (handler._syncingFlyout)
            return; // 本侧 SetFlyoutVisible 触发的回写，忽略
        handler.SetFlyoutVisible(shell.FlyoutIsPresented, syncToShell: false);
    }

    private static void MapFlyoutBehavior(HarmonyShellHandler handler, Shell shell)
        => handler.ApplyFlyoutBehavior();

    private static void MapChromeVisibility(HarmonyShellHandler handler, Shell shell)
        => handler.ApplyChromeVisibility();

    /// <summary>FlyoutBehavior：Disabled 隐藏汉堡并强制关闭；Locked 常驻展开并与内容区
    /// 并排（无遮罩、主列右移让宽）；Flyout 常规覆盖。</summary>
    private void ApplyFlyoutBehavior()
    {
        var shell = VirtualView;
        if (shell is null)
            return;
        var behavior = shell.FlyoutBehavior;
        if (_hamburger is { } ham)
            ham.SetVisibility(behavior == FlyoutBehavior.Flyout
                ? ArkUI_Visibility.ARKUI_VISIBILITY_VISIBLE
                : ArkUI_Visibility.ARKUI_VISIBILITY_NONE);
        if (behavior == FlyoutBehavior.Disabled)
        {
            ApplyLockedLayout(locked: false);
            SetFlyoutVisible(false, syncToShell: true);
        }
        else if (behavior == FlyoutBehavior.Locked)
        {
            ApplyLockedLayout(locked: true);
            SetFlyoutVisible(true, syncToShell: true);
        }
        else
        {
            ApplyLockedLayout(locked: false);
            // 从 Locked 切回：面板仍处展开态时补回遮罩，恢复覆盖式语义
            if (_flyoutVisible && _backdrop is { } bd)
                bd.Visible = true;
        }
    }

    /// <summary>Locked 并排布局：主列右移 FlyoutWidthVp 并显式收窄；解除时恢复满宽。</summary>
    private void ApplyLockedLayout(bool locked)
    {
        if (_mainColumn is null)
            return;
        if (locked && _lastRootWidth > 0)
        {
            _mainColumn.SetWidth(ResolveLockedContentWidth(_lastRootWidth, HarmonyShellTheme.FlyoutWidthVp));
            _mainColumn.SetPosition(HarmonyShellTheme.FlyoutWidthVp, 0f);
        }
        else if (!locked)
        {
            _mainColumn.SetWidthPercent(1.0f);
            _mainColumn.SetPosition(0f, 0f);
        }
        // locked 但根尺寸未就绪：等 OnRootSizeChange 回填
    }

    /// <summary>Locked 并排时主列宽度：根宽 - 面板宽，下限 0（窄屏/超宽面板不设负宽）。</summary>
    internal static float ResolveLockedContentWidth(float rootWidth, float flyoutWidth)
        => Math.Max(0f, rootWidth - flyoutWidth);

    private void OnRootSizeChange(ArkUINodeEvent e)
    {
        _lastRootWidth = e.SizeChangeWidth;
        // Locked 并排：根宽变化（旋转/折叠屏展开）需重算主列让宽
        if (VirtualView?.FlyoutBehavior == FlyoutBehavior.Locked)
            ApplyLockedLayout(locked: true);
    }

    /// <summary>NavBar/TabBar 可见性：Shell 级与可见页 attached 值合成（页级优先），
    /// 变更后按可见性重新回填内容区高度（隐藏用 NONE 不占位）。</summary>
    internal void ApplyChromeVisibility()
    {
        var shell = VirtualView;
        if (shell is null)
            return;
        var navShown = _visiblePage is { } p ? Shell.GetNavBarIsVisible(p) : true;
        var tabShown = _tabEntries.Count > 1
            && (_visiblePage is { } p2 ? Shell.GetTabBarIsVisible(p2) : true);
        if (navShown == _navBarShown && tabShown == _tabBarShown)
            return;
        _navBarShown = navShown;
        _tabBarShown = tabShown;
        _topBar?.SetVisibility(navShown ? ArkUI_Visibility.ARKUI_VISIBILITY_VISIBLE : ArkUI_Visibility.ARKUI_VISIBILITY_NONE);
        _tabBar?.SetVisibility(tabShown ? ArkUI_Visibility.ARKUI_VISIBILITY_VISIBLE : ArkUI_Visibility.ARKUI_VISIBILITY_NONE);
        RelayoutContent();
    }

    internal void ApplyThemeColors()
    {
        var shell = VirtualView;
        if (shell is null)
            return;

        if (_mainColumn is { } mainColumn)
            mainColumn.SetBackgroundColor(HarmonyShellTheme.ShellBackground.ToUint());

        var bg = Shell.GetBackgroundColor(shell);
        var fg = Shell.GetForegroundColor(shell);
        var title = Shell.GetTitleColor(shell);
        var unselected = Shell.GetUnselectedColor(shell);

        // 顶部导航栏背景色
        if (_topBar is { } bar)
        {
            if (bg is { } bgColor)
                bar.SetBackgroundColor(bgColor.ToUint());
            else
                bar.SetBackgroundColor(HarmonyShellTheme.TopBarBackground.ToUint());
        }

        // 汉堡按钮前景色 / 标题颜色（缺省回退主题值）
        var fgColor = fg ?? HarmonyShellTheme.FlyoutForeground;
        var titleColor = title ?? fgColor;
        if (_hamburger is { } ham)
            ham.SetFontColor((byte)(fgColor.Red * 255), (byte)(fgColor.Green * 255), (byte)(fgColor.Blue * 255));
        if (_topBarTitle is { } barTitle)
            barTitle.SetFontColor((byte)(titleColor.Red * 255), (byte)(titleColor.Green * 255), (byte)(titleColor.Blue * 255));

        // TabBar 未选中色（公开 API；缺省灰）
        var unselectedColor = unselected ?? HarmonyShellTheme.TabUnselected;
        ApplyTabColors(shell.CurrentItem, unselectedColor);
    }

    internal void ApplyTabColors(ShellItem? selected)
        => ApplyTabColors(selected, null);

    private void ApplyTabColors(ShellItem? selected, MGraphicsColor? unselectedOverride)
    {
        var shell = VirtualView;
        var fg = shell is not null ? Shell.GetForegroundColor(shell) : null;
        var fgColor = fg ?? HarmonyShellTheme.TabSelected;
        var unselected = unselectedOverride ?? HarmonyShellTheme.TabUnselected;

        foreach (var (entry, item, title) in _tabEntries)
        {
            var c = ReferenceEquals(item, selected) ? fgColor : unselected;
            title.SetFontColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255));
        }
    }

    // ───────────────────────── 内容区 ─────────────────────────

    /// <summary>切换可见页（由 HarmonyShellNavigation 驱动）；页面节点按 Page 缓存保留。</summary>
    internal void ShowPage(Page page)
    {
        if (_contentHost is null || ReferenceEquals(_visiblePage, page))
            return;
        if (_visiblePage is { } old)
            old.PropertyChanged -= OnVisiblePagePropertyChanged;
        _visiblePage?.SendDisappearing();

        var node = GetOrCreateNode(page);
        if (_visibleNode is not null)
            _contentHost.RemoveChild(_visibleNode);
        _contentHost.AddChild(node);
        _visiblePage = page;
        _visibleNode = node;
        // 跟踪可见页 Title / NavBarIsVisible / TabBarIsVisible 变更（attached 属性
        // 变更走元素 PropertyChanged，不经 Shell 的 mapper）
        page.PropertyChanged += OnVisiblePagePropertyChanged;
        page.SendAppearing();
        UpdateTopBarTitle(page);
        ApplyChromeVisibility();
    }

    private void OnVisiblePagePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Page.Title))
            UpdateTopBarTitle(sender as Page);
        else if (e.PropertyName is "NavBarIsVisible" or "TabBarIsVisible")
            ApplyChromeVisibility();
    }

    internal void ShowEmpty()
    {
        if (_contentHost is null)
            return;
        _visiblePage?.SendDisappearing();
        if (_visiblePage is { } old)
            old.PropertyChanged -= OnVisiblePagePropertyChanged;
        if (_visibleNode is not null)
            _contentHost.RemoveChild(_visibleNode);
        _visibleNode = null;
        _visiblePage = null;
    }

    private void UpdateTopBarTitle(Page? page)
    {
        if (_topBarTitle is null)
            return;
        _topBarTitle.Content = page?.Title ?? string.Empty;
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
        var node = handler.PlatformView as ArkUINode
            ?? throw new InvalidOperationException(
                $"page handler PlatformView is not an ArkUI node: {handler.PlatformView?.GetType().Name}");
        node.SetWidthPercent(1.0f);
        node.SetHeightPercent(1.0f);
        _nodes[page] = node;
        return node;
    }
}
