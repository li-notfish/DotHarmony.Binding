using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;
using MLAYOUT = Microsoft.Maui.ILayout;
using MALIGNMENT = Microsoft.Maui.Primitives.LayoutAlignment;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Layout 的 HarmonyOS Handler（ArkUI Column/Row 托管布局）。</summary>
public class HarmonyLayoutHandler : ViewHandler<MLAYOUT, ArkUINode>
{
    public static PropertyMapper<MLAYOUT, HarmonyLayoutHandler> Mapper = new(ViewMapper)
    {
        [nameof(MLAYOUT.Background)] = MapBackground,
    };

    // Controls 布局的子树变更协议（字符串命令，经 Handler.Invoke 派发）
    public static CommandMapper<MLAYOUT, HarmonyLayoutHandler> LayoutCommandMapper = new(ViewCommandMapper)
    {
        [nameof(ILayoutHandler.Add)] = MapAdd,
        [nameof(ILayoutHandler.Remove)] = MapRemove,
        [nameof(ILayoutHandler.Clear)] = MapClear,
        [nameof(ILayoutHandler.Insert)] = MapInsert,
        [nameof(ILayoutHandler.Update)] = MapUpdate,
        [nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
    };

    public HarmonyLayoutHandler() : base(Mapper, LayoutCommandMapper) { }

    protected override ArkUINode CreatePlatformView()
    {
        // StackLayout 按 Orientation 选择 ArkUI 容器；其余布局默认 Column
        if (VirtualView is StackLayout sl && sl.Orientation == StackOrientation.Horizontal)
            return new HarmonyOS.ArkUI.Row();
        return new HarmonyOS.ArkUI.Column();
    }

    protected override void ConnectHandler(ArkUINode platformView)
    {
        base.ConnectHandler(platformView);
        // 连接时全量同步已存在的 Children（Controls 侧在 Handler 连接前添加的子节点不会发 Add 命令）
        foreach (var child in ((Layout)VirtualView).Children)
            AttachChild(child);
    }

    private readonly Dictionary<IView, IElementHandler> _children = new();

    public static void MapBackground(HarmonyLayoutHandler h, MLAYOUT v)
    {
        BrushHelper.ApplyBackground(h.PlatformView, v.Background);
    }

    public static void MapAdd(HarmonyLayoutHandler h, MLAYOUT v, object? args)
    {
        if (args is LayoutHandlerUpdate u)
            h.AttachChild(u.View);
    }

    public static void MapRemove(HarmonyLayoutHandler h, MLAYOUT v, object? args)
    {
        if (args is LayoutHandlerUpdate u)
            h.DetachChild(u.View);
    }

    public static void MapClear(HarmonyLayoutHandler h, MLAYOUT v, object? args)
    {
        h.ClearChildren();
    }

    public static void MapInsert(HarmonyLayoutHandler h, MLAYOUT v, object? args)
    {
        if (args is LayoutHandlerUpdate u)
            h.AttachChild(u.View); // M1：中段插入退化为顺序追加
    }

    public static void MapUpdate(HarmonyLayoutHandler h, MLAYOUT v, object? args)
    {
        if (args is LayoutHandlerUpdate u)
        {
            if (h._children.TryGetValue(u.View, out var old) && old.PlatformView is ArkUINode oldNode)
                h.PlatformView.RemoveChild(oldNode);
            h.AttachChild(u.View);
        }
    }

    public static void MapUpdateZIndex(HarmonyLayoutHandler h, MLAYOUT v, object? args)
    {
        // ArkUI flex 按 addChild 顺序，z-index M1 忽略
    }

    private void AttachChild(IView view)
    {
        if (_children.ContainsKey(view)) return;
        var handler = HarmonyHandlerFactory.Create(view);
        handler.SetVirtualView(view);
        if (handler.PlatformView is ArkUINode node)
        {
            // MAUI 显式 WidthRequest/HeightRequest 优先（vp）；否则默认 Fill → 宽度撑满父容器
            if (view.Width is double w && w >= 0)
            {
                node.SetWidth((float)w);
            }
            else if (view.HorizontalLayoutAlignment == MALIGNMENT.Fill)
            {
                node.SetWidthPercent(1.0f);
            }

            if (view.Height is double hgt && hgt >= 0)
            {
                node.SetHeight((float)hgt);
            }

            // MAUI StackLayout.Spacing → 子节点下边距（最后一个子节点略多余，视觉可接受）
            if (VirtualView is StackLayout sl && sl.Spacing > 0)
            {
                node.SetMarginEdges(0, 0, (float)sl.Spacing, 0);
            }

            PlatformView.AddChild(node);
            _children[view] = handler;
        }
    }

    private void DetachChild(IView view)
    {
        if (_children.Remove(view, out var handler) && handler.PlatformView is ArkUINode node)
            PlatformView.RemoveChild(node);
    }

    private void ClearChildren()
    {
        PlatformView.RemoveAllChildren();
        _children.Clear();
    }
}
