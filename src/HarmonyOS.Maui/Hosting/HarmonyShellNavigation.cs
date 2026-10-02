// HarmonyShellNavigation：MAUI Shell 第一版导航语义宿主。
// MAUI Shell 的平台 fragment 机制依赖各平台 ShellHandler 协议，鸿蒙宿主不具备；
// 本类自持选择状态与 section 内推送栈：视觉切换全部落回 HarmonyShellHandler 的
// 内容区节点交换；MAUI 侧语义（Shell.CurrentItem / ShellItem.CurrentItem /
// ShellSection.CurrentItem）尽力同步——内部设施缺失时异常仅告警，不阻断视觉切换。
//
// 第一版范围：
//   · ShellContent / Tab / TabBar / FlyoutItem 统一作为条目（Flyout 菜单视觉后续子阶段）
//   · GoToAsync 绝对路由（"//item/section/content"，首段也可直接是注册路由）
//   · 相对路由（当前条目/section 内匹配；注册路由推送）
//   · ".." 相对返回（可叠加 "../.."）
//   · RegisterRoute（MAUI Routing 内部表无公开读取通道，自持注册表）
//   · query parameters（IQueryAttributable + [QueryProperty]，值以字符串下发）
//   · 页面 Appearing/Disappearing（切换时透传）
// 系统返回键顺序：模态 → 本类 HandleBack（section 内栈）→ 根级轻量栈（HarmonyNavigation）。
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using HarmonyOS.Interop;
using MPage = Microsoft.Maui.Controls.Page;
using MNavigationProxy = Microsoft.Maui.Controls.Internals.NavigationProxy;

namespace HarmonyOS.Maui.Hosting;

public static class HarmonyShellNavigation
{
    private static readonly Dictionary<string, Func<MPage>> Routes = new(StringComparer.OrdinalIgnoreCase);

    // 当前选择状态（宿主进程内 Shell 实例同一时刻唯一；Shell 页断连时经 Detach 清空
    // 并释放缓存页——重复 Attach 也只保留最新实例）
    private static Shell? _shell;
    private static Handlers.HarmonyShellHandler? _handler;
    private static ShellItem? _item;
    private static ShellSection? _section;
    private static ShellContent? _content;
    private static readonly List<MPage> StackPages = new();

    public static void RegisterRoute(string route,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type pageType)
        => Routes[route] = () => (MPage)Activator.CreateInstance(pageType)!;

    public static void RegisterRoute(string route, Func<MPage> factory)
        => Routes[route] = factory;

    public static Task GoToAsync(Shell shell, string uri)
    {
        try
        {
            GoToCore(shell, uri);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    /// <summary>条目切换入口（TabBar 点击 / 应用代码）。</summary>
    public static void SelectItem(Shell shell, ShellItem item)
    {
        if (ReferenceEquals(_shell, shell))
            SelectItemCore(shell, item);
    }

    /// <summary>MAUI 侧 CurrentItem 变更反向同步（Shell handler mapper 调入）。</summary>
    internal static void SyncFromShell(Shell shell)
    {
        if (ReferenceEquals(_shell, shell)
            && shell.CurrentItem is { } item && !ReferenceEquals(item, _item))
            SelectItemCore(shell, item);
    }

    /// <summary>宿主返回键回调（HarmonyNavigation 转入）：section 内栈非空则弹出返回页。</summary>
    internal static bool HandleBack(Shell shell)
    {
        if (!ReferenceEquals(_shell, shell) || StackPages.Count == 0)
            return false;
        PopCore();
        return true;
    }

    internal static void Attach(Shell shell, Handlers.HarmonyShellHandler handler)
    {
        if (_handler is not null && !ReferenceEquals(_handler, handler))
            Detach(_handler); // 旧实例已被替换宿主但未走 DisconnectHandler 的兜底
        _shell = shell;
        _handler = handler;
        StackPages.Clear();
        _item = shell.CurrentItem ?? shell.Items.FirstOrDefault();
        _section = _item?.CurrentItem ?? _item?.Items.FirstOrDefault();
        _content = _section?.CurrentItem ?? _section?.Items.FirstOrDefault();
        ShowCurrent();
    }

    /// <summary>
    /// Shell 断连清理（HarmonyShellHandler.DisconnectHandler 调入）：释放全部缓存/栈页
    /// 的 handler 与节点，清空静态选择态——不清理则 Shell 重建时页面实例整链泄漏。
    /// 不归还的 handler 调用一律忽略并意为 false。
    /// </summary>
    internal static bool Detach(Handlers.HarmonyShellHandler handler)
    {
        if (!ReferenceEquals(_handler, handler))
            return false;
        var h = _handler;
        ShowEmptyReleaseVisible();
        foreach (var pushed in StackPages)
            h!.ReleasePage(pushed);
        StackPages.Clear();
        foreach (var page in ContentPages.Values)
            h!.ReleasePage(page);
        ContentPages.Clear();
        // Routes are process-lifetime registrations; a Shell reconnect must not discard them.
        _shell = null;
        _handler = null;
        _item = null;
        _section = null;
        _content = null;
        return true;
    }

    // 内容区当前可见页摘除并把节点/handler 释放（Detach 使用；偶发清空由 ShowEmpty 保证）
    private static void ShowEmptyReleaseVisible()
    {
        if (TopPage() is { } visible)
            _handler?.ReleasePage(visible);
        _handler?.ShowEmpty();
    }

    // ───────────────────────── 路由执行 ─────────────────────────

    private static void GoToCore(Shell shell, string uri)
    {
        if (_handler is null || !ReferenceEquals(_shell, shell))
            throw new InvalidOperationException("Shell is not attached to the Harmony host (no Shell root page)");

        var work = uri?.Trim() ?? string.Empty;
        if (work.Length == 0)
            return;

        // 相对返回：".." / "../..";栈空即止（进一步返回交还系统）
        var pops = 0;
        while (work.StartsWith("../"))
        {
            pops++;
            work = work[3..];
        }
        if (work == "..")
        {
            pops++;
            work = string.Empty;
        }
        while (pops-- > 0)
        {
            if (StackPages.Count == 0)
                return;
            PopCore();
        }
        if (work.Length == 0)
            return;

        var match = ResolveRoute(shell, work)
            ?? throw new ArgumentException($"Shell route not found: {uri}", nameof(uri));

        if (match.Item is not null && !ReferenceEquals(match.Item, _item))
            SelectItemCore(shell, match.Item);
        if (match.Section is not null && !ReferenceEquals(match.Section, _section))
            SelectSectionCore(match.Section);
        if (match.Content is not null && !ReferenceEquals(match.Content, _content))
            SelectContentCore(match.Content);

        foreach (var route in match.PushRoutes)
        {
            if (!Routes.TryGetValue(route, out var factory))
                throw new ArgumentException($"Route not registered: {route}", nameof(uri));
            var page = factory();
            if (page.Parent is null)
                page.Parent = _shell; // 资源/BindingContext 继承链
            StackPages.Add(page);
        }

        if (match.Query.Count > 0)
            ApplyQuery(TopPage(), match.Query);

        ShowCurrent();
    }

    private static void SelectItemCore(Shell shell, ShellItem item)
    {
        _item = item;
        _section = item.CurrentItem ?? item.Items.FirstOrDefault();
        _content = _section?.CurrentItem ?? _section?.Items.FirstOrDefault();
        StackPages.Clear(); // 条目切换清空推送栈（每条目一栈的第一版语义）
        TrySet(() => shell.CurrentItem = item);
        _handler?.ApplyTabColors(item);
        ShowCurrent();
    }

    private static void SelectSectionCore(ShellSection section)
    {
        _section = section;
        _content = section.CurrentItem ?? section.Items.FirstOrDefault();
        if (_item is not null)
            TrySet(() => _item.CurrentItem = section);
        ShowCurrent();
    }

    private static void SelectContentCore(ShellContent content)
    {
        _content = content;
        if (_section is not null)
            TrySet(() => _section.CurrentItem = content);
        ShowCurrent();
    }

    private static void TrySet(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            HiLog.Warn("Shell", $"MAUI selection sync failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void PopCore()
    {
        if (StackPages.Count == 0)
            return;
        var popped = StackPages[^1];
        StackPages.RemoveAt(StackPages.Count - 1);
        ShowCurrent(); // 透传 Disappearing(旧)/Appearing(新)
        _handler?.ReleasePage(popped); // 弹出页不可达，节点与 handler 即时释放
    }

    private static void ShowCurrent()
    {
        var page = TopPage();
        if (page is null)
        {
            _handler?.ShowEmpty();
            return;
        }
        // 内容页/推送页统一接 section 导航（Parent 已先接线，OnParentSet 不会重算 Inner）；
        // 根 Shell 页的 Inner 由 HarmonyNavigation.Push 接根适配器，互不影响
        page.NavigationProxy.Inner = SectionNavigation;
        _handler!.ShowPage(page);
    }

    /// <summary>
    /// Shell section 内 INavigation 适配器：计划语义"Navigation.PushAsync/Modal 在 Shell
    /// section 内部接入现有轻量导航栈"。页内 PushAsync（如 Metro 磁贴 → 详情页）落
    /// section 推送栈；模态转发根级模态层（覆盖整个 Shell，与 MAUI Shell 模态语义一致）。
    /// </summary>
    private static readonly SectionNavigationAdapter SectionNavigation = new();

    private sealed class SectionNavigationAdapter : MNavigationProxy
    {
        protected override Task OnPushAsync(MPage page, bool animated)
        {
            if (page.Parent is null && _shell is not null)
                page.Parent = _shell;
            StackPages.Add(page);
            ShowCurrent();
            return Task.CompletedTask;
        }

        protected override Task<MPage> OnPopAsync(bool animated)
        {
            // 空栈明确抛错（与 GoToCore 语义一致），不以 null 伪装非空返回值
            if (StackPages.Count == 0)
                throw new InvalidOperationException("Shell section navigation stack is empty");
            var popped = StackPages[^1];
            PopCore();
            return Task.FromResult(popped);
        }

        protected override Task OnPopToRootAsync(bool animated)
        {
            while (StackPages.Count > 0)
                PopCore();
            return Task.CompletedTask;
        }

        protected override Task OnPushModal(MPage modal, bool animated)
        {
            HarmonyNavigation.PushModal(modal);
            return Task.CompletedTask;
        }

        protected override Task<MPage> OnPopModal(bool animated)
            => Task.FromResult(HarmonyNavigation.PopModal()!);

        protected override System.Collections.Generic.IReadOnlyList<MPage> GetModalStack()
            => HarmonyNavigation.CurrentModals;
    }

    private static MPage? TopPage()
        => StackPages.Count > 0 ? StackPages[^1] : (_content is null ? null : ContentPageOf(_content));

    private static MPage? ContentPageOf(ShellContent content)
    {
        // IShellContentController.Page 对 ContentTemplate 形态返回 null（仅直配 Content
        // 时有值）：模板页在此惰性创建并按 ShellContent 缓存实例——CreateContent 每次
        // 都会产生新页，不缓存则条目切换丢状态、节点泄漏。Parent 补链走资源/BindingContext
        // 继承（宿主自装配）。
        if (ContentPages.TryGetValue(content, out var cached))
            return cached;

        MPage? page = ((IShellContentController)content).Page;
        if (page is null && content.ContentTemplate is { } template)
            page = template.CreateContent() as MPage;
        if (page is null)
            return null;
        if (page.Parent is null)
            page.Parent = content;
        ContentPages[content] = page;
        return page;
    }

    private static readonly Dictionary<ShellContent, MPage> ContentPages = new();

    // ───────────────────────── query ─────────────────────────

    private static void ApplyQuery(MPage? page, Dictionary<string, string> query)
    {
        if (page is null || query.Count == 0)
            return;
        var asObjects = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in query)
            asObjects[kv.Key] = kv.Value;
        if (page is IQueryAttributable attributable)
            attributable.ApplyQueryAttributes(asObjects);
        if (page.BindingContext is IQueryAttributable ctxAttributable)
            ctxAttributable.ApplyQueryAttributes(asObjects);
        foreach (var attr in page.GetType()
                     .GetCustomAttributes(typeof(QueryPropertyAttribute), inherit: true)
                     .Cast<QueryPropertyAttribute>())
        {
            if (query.TryGetValue(attr.QueryId, out var value))
                ApplyPropertyValue(page, attr.Name, value);
        }
    }

    private static void ApplyPropertyValue(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] MPage page,
        string propertyName,
        string value)
    {
        var property = page.GetType().GetProperty(propertyName);
        if (property?.SetMethod is null)
            return;
        var target = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        object converted = value;
        if (target != typeof(string))
        {
            try
            {
                converted = Convert.ChangeType(value, target, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException)
            {
                // 对齐 MAUI 语义：query 值转换失败仅告警，不阻断整次导航
                HiLog.Warn("Shell", $"query value for '{propertyName}' not convertible to {target.Name}: '{value}'");
                return;
            }
        }
        property.SetValue(page, converted);
    }

    // ───────────────────────── 路由解析（纯逻辑，单测覆盖） ─────────────────────────

    internal sealed class ShellRouteMatch
    {
        public ShellItem? Item;
        public ShellSection? Section;
        public ShellContent? Content;
        public List<string> PushRoutes { get; } = new();
        public Dictionary<string, string> Query { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    internal static ShellRouteMatch? ResolveRoute(Shell shell, string uri)
    {
        var (path, queryString) = SplitQuery(uri);
        var match = new ShellRouteMatch();
        if (queryString.Length > 0)
        {
            foreach (var pair in queryString.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = pair.IndexOf('=');
                var key = eq < 0 ? pair : pair[..eq];
                var value = eq < 0 ? string.Empty : pair[(eq + 1)..];
                match.Query[Uri.UnescapeDataString(key)] = Uri.UnescapeDataString(value);
            }
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
            return match; // 仅 query：作用于当前可见页

        if (path.StartsWith("//"))
            return ResolveAbsolute(shell, segments, match);
        return ResolveRelative(shell, segments, match);
    }

    private static ShellRouteMatch? ResolveAbsolute(Shell shell, string[] segments, ShellRouteMatch match)
    {
        // 首段匹配 ShellItem；未命中且为注册路由时整段作为当前条目上的推送
        if (FindByRoute(shell.Items, segments[0]) is not ShellItem item)
        {
            if (Routes.ContainsKey(segments[0]))
            {
                match.PushRoutes.AddRange(segments);
                return match;
            }
            return null;
        }
        match.Item = item;

        var section = segments.Length > 1 ? FindByRoute(item.Items, segments[1]) as ShellSection : null;
        if (section is null)
        {
            for (var i = 1; i < segments.Length; i++)
                match.PushRoutes.Add(segments[i]);
            return match;
        }
        match.Section = section;

        var content = segments.Length > 2 ? FindByRoute(section.Items, segments[2]) as ShellContent : null;
        if (content is null)
        {
            for (var i = 2; i < segments.Length; i++)
                match.PushRoutes.Add(segments[i]);
            return match;
        }
        match.Content = content;

        for (var i = 3; i < segments.Length; i++)
            match.PushRoutes.Add(segments[i]);
        return match;
    }

    private static ShellRouteMatch? ResolveRelative(Shell shell, string[] segments, ShellRouteMatch match)
    {
        if (Routes.ContainsKey(segments[0]))
        {
            match.PushRoutes.AddRange(segments);
            return match;
        }

        var item = shell.CurrentItem;
        if (item is null)
            return null;
        match.Item = item;

        var section = FindByRoute(item.Items, segments[0]) as ShellSection;
        if (section is null)
        {
            // 当前 section 内的 content 路由
            var current = item.CurrentItem ?? item.Items.FirstOrDefault();
            if (current is null)
                return null;
            match.Section = current;
            var content = FindByRoute(current.Items, segments[0]) as ShellContent;
            if (content is null)
                return null;
            match.Content = content;
            for (var i = 1; i < segments.Length; i++)
                match.PushRoutes.Add(segments[i]);
            return match;
        }
        match.Section = section;

        var contentIn = segments.Length > 1
            ? FindByRoute(section.Items, segments[1]) as ShellContent ?? section.CurrentItem ?? section.Items.FirstOrDefault()
            : section.CurrentItem ?? section.Items.FirstOrDefault();
        match.Content = contentIn;
        for (var i = 2; i < segments.Length; i++)
            match.PushRoutes.Add(segments[i]);
        return match;
    }

    private static BaseShellItem? FindByRoute<T>(IEnumerable<T> items, string segment) where T : BaseShellItem
    {
        foreach (var item in items)
            if (string.Equals(RouteKey(item), segment, StringComparison.OrdinalIgnoreCase))
                return item;
        return null;
    }

    private static string RouteKey(BaseShellItem item)
        => string.IsNullOrEmpty(item.Route) ? item.GetType().Name : item.Route;

    private static (string Path, string Query) SplitQuery(string uri)
    {
        var q = uri.IndexOf('?');
        return q < 0 ? (uri, string.Empty) : (uri[..q], uri[(q + 1)..]);
    }
}
