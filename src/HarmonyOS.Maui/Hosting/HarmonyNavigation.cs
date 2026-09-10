// HarmonyNavigation：轻量 Page 栈（M1 范围：Push/Pop，节点树保留式切换）
// 页面 platform view 挂入宿主根容器；Push 摘除当前页（句柄保留）、Pop 重新挂载。
// "摘除-恢复"的属性完整性以实测为准（ROADMAP 难点：removeChild 后 addNode 的节点状态）。
// 当前页若是 NavigationPage，返回键优先消费其内部导航栈（IStackNavigation 协议）。
using Microsoft.Maui;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;
using MContentPage = Microsoft.Maui.Controls.ContentPage;
using MPage = Microsoft.Maui.Controls.Page;

namespace HarmonyOS.Maui.Hosting;

public static class HarmonyNavigation
{
    private static ArkColumn? _container;
    private static ArkUINode? _currentPage;
    private static MPage? _currentPageMaui;
    private static readonly Stack<ArkUINode> BackStack = new();

    /// <summary>是否存在可返回的页面</summary>
    public static bool CanPop => BackStack.Count > 0;

    internal static void Attach(ArkColumn container)
    {
        _container = container;
        BackStack.Clear();
        _currentPage = null;
        _currentPageMaui = null;
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
        if (_currentPage is not null)
        {
            BackStack.Push(_currentPage);
            _container.RemoveChild(_currentPage);
        }
        _currentPageMaui = page;
        _currentPage = AssemblePage(page);
        _container.AddChild(_currentPage);
    }

    /// <summary>
    /// 系统返回键/返回手势请求（由宿主 onBackPress 转入）。
    /// 优先消费当前 NavigationPage 的内部导航栈；其次根级 BackStack；
    /// 都为空时返回 false 交还系统默认行为。
    /// </summary>
    public static bool OnBackRequested()
    {
        if (_currentPageMaui is Microsoft.Maui.Controls.NavigationPage navPage &&
            navPage.Navigation.NavigationStack.Count > 1)
        {
            navPage.PopAsync().FireAndForget(navPage.Handler);
            return true;
        }

        if (!CanPop)
            return false;
        Pop();
        return true;
    }

    /// <summary>返回上一页（当前页被移除并释放，上一页恢复挂载）</summary>
    public static void Pop()
    {
        if (_container is null || _currentPage is null || BackStack.Count == 0)
            return;

        _container.RemoveChild(_currentPage);
        _currentPage.Dispose();
        _currentPage = BackStack.Pop();
        _currentPageMaui = null;
        _container.AddChild(_currentPage);
    }
}

internal static class TaskFireAndForgetExtensions
{
    /// <summary>吞掉后台导航任务的异常（沿用 MAUI FireAndForget 语义，避免未观察 Task 崩溃）</summary>
    public static void FireAndForget(this Task task, IElementHandler? handler)
    {
        task.ContinueWith(t =>
        {
            if (t.Exception is not null)
                System.Diagnostics.Debug.WriteLine($"FireAndForget navigation failed: {t.Exception.InnerException}");
        }, TaskScheduler.Default);
    }
}
