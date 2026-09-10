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
        [nameof(MLAYOUT.Padding)] = MapPadding,
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

    public static void MapPadding(HarmonyLayoutHandler h, MLAYOUT v)
    {
        var p = v.Padding;
        if (p.Top > 0 || p.Right > 0 || p.Bottom > 0 || p.Left > 0)
        {
            h.PlatformView.SetPaddingEdges(
                (float)p.Top, (float)p.Right, (float)p.Bottom, (float)p.Left);
        }
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
            // MAUI 显式 WidthRequest/HeightRequest 优先（vp）；view.Width/Height 是布局后的
            // 实测值（未布局时为 -1），不能用来判断显式尺寸
            bool isHorizontalStack = VirtualView is StackLayout { Orientation: StackOrientation.Horizontal };
            if (view is VisualElement ve && ve.WidthRequest >= 0)
            {
                node.SetWidth((float)ve.WidthRequest);
            }
            else if (!isHorizontalStack)
            {
                // 竖直 Stack（Column）交叉轴 = 水平：HorizontalOptions 逐子项生效
                // （ArkUI alignItems 是容器级，per-child 用 alignSelf/NODE_ALIGN_SELF 折衷）
                switch (view.HorizontalLayoutAlignment)
                {
                    case MALIGNMENT.Fill:
                        node.SetWidthPercent(1.0f);
                        break;
                    case MALIGNMENT.Start:
                        node.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_START);
                        break;
                    case MALIGNMENT.Center:
                        node.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
                        break;
                    case MALIGNMENT.End:
                        node.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_END);
                        break;
                }
            }

            if (view is VisualElement vep && vep.HeightRequest >= 0)
            {
                node.SetHeight((float)vep.HeightRequest);
            }
            else if (isHorizontalStack)
            {
                // 水平 Stack（Row）交叉轴 = 垂直：VerticalOptions 逐子项生效。
                // Fill 落成百分比高仅当 Row 自身高度受约束（显式 HeightRequest）——
                // auto 高 Row 的子项百分比会退化为 0/未定义，Row 高度本应由内容决定
                bool rowHeightBounded = VirtualView is VisualElement rve && rve.HeightRequest >= 0;
                switch (view.VerticalLayoutAlignment)
                {
                    case MALIGNMENT.Fill:
                        if (rowHeightBounded)
                            node.SetHeightPercent(1.0f);
                        break;
                    case MALIGNMENT.Start:
                        node.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_START);
                        break;
                    case MALIGNMENT.Center:
                        node.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER);
                        break;
                    case MALIGNMENT.End:
                        node.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_END);
                        break;
                }
            }

            // 子节点自身 Margin（四边）；StackLayout.Spacing 以同侧外边距叠加
            var margin = view.Margin;
            float mTop = (float)margin.Top, mRight = (float)margin.Right,
                  mBottom = (float)margin.Bottom, mLeft = (float)margin.Left;
            if (VirtualView is StackLayout sl && sl.Spacing > 0)
            {
                if (isHorizontalStack) mRight += (float)sl.Spacing;
                else mBottom += (float)sl.Spacing;
            }
            if (mTop > 0 || mRight > 0 || mBottom > 0 || mLeft > 0)
                node.SetMarginEdges(mTop, mRight, mBottom, mLeft);

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
