// HarmonyNavigationPageHandler：MAUI NavigationPage 的鸿蒙 Handler（转接层）。
// NavigationPage 的 Push/Pop 经 MauiNavigationImpl → SendHandlerUpdateAsync 打包整包
// NavigationRequest 经 Handler.Invoke("RequestNavigation") 到达本 handler；
// 平台侧把 ArkUI 节点栈同步成请求的栈后，必须回调 IStackNavigation.NavigationFinished，
// 否则 SendHandlerUpdateAsync 内 await 永久挂起（PushAsync 不返回）。
// 子页 NavigationProxy.Inner 由 NavigableElement.OnParentSet 沿父链自动接线，无需干预。
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI NavigationPage 的 HarmonyOS Handler（ArkUI Stack 承载页面栈）。</summary>
public class HarmonyNavigationPageHandler : ViewHandler<NavigationPage, ArkStack>
{
    public static readonly PropertyMapper<NavigationPage, HarmonyNavigationPageHandler> Mapper =
        new(ViewMapper)
        {
            [nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
        };

    public static readonly CommandMapper<NavigationPage, HarmonyNavigationPageHandler> Commands =
        new(ViewCommandMapper)
        {
            [nameof(IStackNavigation.RequestNavigation)] = MapRequestNavigation,
        };

    public HarmonyNavigationPageHandler() : base(Mapper, Commands) { }

    protected override ArkStack CreatePlatformView()
    {
        var stack = new ArkStack();
        stack.SetWidthPercent(1.0f);
        stack.SetHeightPercent(1.0f);
        return stack;
    }

    // 平台侧当前栈：顺序 = MAUI 导航栈；节点摘除保留句柄（返回时重挂，状态不丢）
    readonly List<Page> _mounted = new();
    readonly Dictionary<Page, ArkUINode> _nodes = new();

    static void MapRequestNavigation(HarmonyNavigationPageHandler handler, NavigationPage page, object? args)
    {
        if (args is not NavigationRequest request)
            return;

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
                PlatformView.RemoveChild(node);
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

        // 仅栈顶挂入 ArkStack；低层页节点摘除保留（与 HarmonyNavigation 同一策略）
        PlatformView.RemoveAllChildren();
        if (target.Count > 0)
            PlatformView.AddChild(_nodes[target[^1]]);
    }

    static void MapBackgroundColor(HarmonyNavigationPageHandler h, NavigationPage v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(
                (byte)(c.Red * 255), (byte)(c.Green * 255),
                (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }
}
