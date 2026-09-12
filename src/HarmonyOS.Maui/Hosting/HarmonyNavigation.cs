// HarmonyNavigation：两级导航宿主
// ① 轻量 Page 栈（Push/Pop，节点树保留式切换）：页面 platform view 挂入宿主根容器；
//    Push 摘除当前页（句柄保留）、Pop 重新挂载。"摘除-恢复"的属性完整性已实测。
// ② 模态层（PushModal/PopModal）：宿主根容器为 ArkStack，模态页覆盖在页面之上。
// ③ RootNavigationAdapter：作为根页 NavigationProxy.Inner，使无 Window/NavigationPage
//    场景下 Navigation.PushModalAsync/PopModalAsync/PushAsync/PopAsync 标准可用——
//    NavigationPage 的模态调用经 MauiNavigationImpl 未覆写的 OnPushModal 落到同一适配器。
// 生命周期：页面级 Appearing/Disappearing 经 Page.SendAppearing/SendDisappearing 透传
// （仅轻量栈/模态层切换时；NavigationPage 内部切换由 MAUI 自行触发，不重复）。
using Microsoft.Maui;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;
using MApplication = Microsoft.Maui.Controls.Application;
using MNavigationProxy = Microsoft.Maui.Controls.Internals.NavigationProxy;
using MPage = Microsoft.Maui.Controls.Page;
using MWindow = Microsoft.Maui.Controls.Window;

namespace HarmonyOS.Maui.Hosting;

public static class HarmonyNavigation
{
    private static ArkStack? _container;
    private static (MPage Page, ArkUINode Node)? _currentPage;
    private static readonly Stack<(MPage Page, ArkUINode Node)> BackStack = new();
    private static readonly List<(MPage Page, ArkUINode Node)> Modals = new();
    private static readonly RootNavigationAdapter Adapter = new();

    // 生命周期最小逻辑链：Page.SendAppearing 有守卫（FindParentOfType<IWindow>().Parent
    // 须非空），宿主无 MAUI 引导时 Appearing/Disappearing 会静默不触发。
    // 建立 Page → Window → Application 链（均为 public API）即可放行；
    // 不设 Window.Page——其 setter 会把页面的 NavigationProxy.Inner 覆写为
    // Window.NavigationImpl（PushAsync 直接抛异常的那个）。
    private static MWindow? _window;

    /// <summary>根级栈是否存在可返回的页面（不含模态与 NavigationPage 内栈）</summary>
    public static bool CanPop => BackStack.Count > 0;

    /// <summary>当前打开的模态数量</summary>
    public static int ModalCount => Modals.Count;

    internal static void Attach(ArkStack container)
    {
        _container = container;
        BackStack.Clear();
        Modals.Clear();
        _currentPage = null;

        if (_window is null)
        {
            var app = new MApplication();
            MApplication.SetCurrentApplication(app);
            _window = new MWindow { Parent = app };
        }
    }

    private static ArkUINode AssemblePage(MPage page)
    {
        var handler = Handlers.HarmonyHandlerFactory.Create((Microsoft.Maui.Controls.Element)page);
        handler.SetVirtualView(page);
        return handler.PlatformView as ArkUINode
            ?? throw new InvalidOperationException(
                $"page handler PlatformView is not an ArkUI node: {handler.PlatformView?.GetType().Name}");
    }

    /// <summary>压入新页面（当前页面摘除但保留，可 Pop 返回）；首页也经此挂载</summary>
    public static void Push(MPage page)
    {
        if (_container is null)
            throw new InvalidOperationException("HarmonyNavigation not initialized (MauiHarmonyHost.Run first)");

        // 根级 INavigation 接线：无父链时 NavigationProxy.Inner 为 null（标准 API 会静默丢弃），
        // 接到适配器后 PushAsync/PushModalAsync 经适配器回到本类。
        // Parent 接线必须在前——NavigableElement.OnParentSet 会把 Inner 清空重算。
        if (page.RealParent is null && _window is not null)
            page.Parent = _window;
        page.NavigationProxy.Inner = Adapter;

        if (_currentPage is not null)
        {
            BackStack.Push(_currentPage.Value);
            _currentPage.Value.Page.SendDisappearing();
            _container.RemoveChild(_currentPage.Value.Node);
        }

        var node = AssemblePage(page);
        _currentPage = (page, node);
        _container.AddChild(node);
        FadeIn(node);
        page.SendAppearing();
    }

    /// <summary>
    /// 系统返回键/返回手势请求（由宿主 onBackPress 转入）。
    /// 优先级：模态 → 当前 NavigationPage 内部栈 → 根级轻量栈；均空返回 false 交还系统。
    /// </summary>
    public static bool OnBackRequested()
    {
        if (Modals.Count > 0)
        {
            PopModal();
            return true;
        }

        if (_currentPage?.Page is Microsoft.Maui.Controls.NavigationPage navPage &&
            navPage.Navigation.NavigationStack.Count > 1)
        {
            navPage.PopAsync().FireAndForget();
            return true;
        }

        if (!CanPop)
            return false;
        Pop();
        return true;
    }

    /// <summary>返回上一页（当前页淡出后被移除并释放，上一页恢复挂载）；动画期间忽略重入</summary>
    public static void Pop() => PopCore(animated: true);

    private static bool _transitioning;

    private static void PopCore(bool animated)
    {
        if (_container is null || _currentPage is null || BackStack.Count == 0 || _transitioning)
            return;

        var current = _currentPage.Value;
        current.Page.SendDisappearing();

        void Finish()
        {
            _container!.RemoveChild(current.Node);
            current.Node.Dispose();
            _currentPage = BackStack.Pop();
            _currentPage.Value.Page.SendAppearing();
            _container.AddChild(_currentPage.Value.Node);
            _transitioning = false;
        }

        if (animated)
        {
            _transitioning = true;
            current.Node.Animate(() => current.Node.SetOpacity(0f), Finish);
        }
        else
        {
            Finish();
        }
    }

    // ───────────────────────── 模态层 ─────────────────────────

    /// <summary>推入模态页（覆盖当前页面之上；容器为 ArkStack，后挂者覆盖先挂者）</summary>
    public static void PushModal(MPage modal)
    {
        if (_container is null)
            throw new InvalidOperationException("HarmonyNavigation not initialized (MauiHarmonyHost.Run first)");

        TopPage()?.SendDisappearing();

        if (modal.RealParent is null && _window is not null)
            modal.Parent = _window;
        modal.NavigationProxy.Inner = Adapter;
        var node = AssemblePage(modal);
        Modals.Add((modal, node));
        _container.AddChild(node);
        FadeIn(node);
        modal.SendAppearing();
    }

    /// <summary>关闭栈顶模态并返回其实例（无模态时返回 null）；淡出动画期间忽略重入</summary>
    public static MPage? PopModal()
    {
        if (_container is null || Modals.Count == 0 || _transitioning)
            return null;

        var (page, node) = Modals[^1];
        Modals.RemoveAt(Modals.Count - 1);

        page.SendDisappearing();
        _transitioning = true;
        node.Animate(() => node.SetOpacity(0f), () =>
        {
            _container!.RemoveChild(node);
            node.Dispose();
            _transitioning = false;
        });

        TopPage()?.SendAppearing();
        return page;
    }

    /// <summary>当前最上层可见的 MAUI 页（栈顶模态优先，其次根级当前页）</summary>
    private static MPage? TopPage()
        => Modals.Count > 0 ? Modals[^1].Page : _currentPage?.Page;

    private static void FadeIn(ArkUINode node)
    {
        node.SetOpacity(0f);
        node.AnimateAsync(() => node.SetOpacity(1f)).FireAndForget();
    }

    /// <summary>
    /// 根级 INavigation 适配器：模态 → 模态层、压栈/出栈 → 轻量栈。
    /// 挂在根页（含 NavigationPage 的 MauiNavigationImpl）的 NavigationProxy.Inner 上，
    /// 未被上层类型覆写的模态调用经 NavigationProxy 基类转发至此。
    /// </summary>
    private sealed class RootNavigationAdapter : MNavigationProxy
    {
        protected override Task OnPushModal(MPage modal, bool animated)
        {
            PushModal(modal);
            return Task.CompletedTask;
        }

        protected override Task<MPage> OnPopModal(bool animated)
            => Task.FromResult(PopModal()!);

        protected override System.Collections.Generic.IReadOnlyList<MPage> GetModalStack()
            => Modals.Select(m => m.Page).ToArray();

        protected override Task OnPushAsync(MPage page, bool animated)
        {
            Push(page);
            return Task.CompletedTask;
        }

        protected override Task<MPage> OnPopAsync(bool animated)
        {
            Pop();
            return Task.FromResult<MPage>(null!);
        }

        protected override Task OnPopToRootAsync(bool animated)
        {
            // PopToRoot 一次性清栈：同步无动画路径（动画版 Pop 有过渡互斥，不能循环）
            while (CanPop)
                PopCore(animated: false);
            return Task.CompletedTask;
        }
    }
}

internal static class TaskFireAndForgetExtensions
{
    /// <summary>吞掉导航任务的异常（沿用 MAUI FireAndForget 语义，避免未观察 Task 崩溃）</summary>
    public static void FireAndForget(this Task task)
    {
        task.ContinueWith(t =>
        {
            if (t.Exception is not null)
                System.Diagnostics.Debug.WriteLine($"navigation failed: {t.Exception.InnerException}");
        }, TaskScheduler.Default);
    }
}
