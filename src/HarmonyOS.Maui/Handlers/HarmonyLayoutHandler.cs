using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;
using MLAYOUT = Microsoft.Maui.ILayout;
using MALIGNMENT = Microsoft.Maui.Primitives.LayoutAlignment;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Layout 的 HarmonyOS Handler（ArkUI Column/Row 托管布局）。</summary>
public class HarmonyLayoutHandler : HarmonyViewHandler<MLAYOUT, ArkUINode>
{
    public static PropertyMapper<MLAYOUT, HarmonyLayoutHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(MLAYOUT.Background)] = MapBackground,
        [nameof(MLAYOUT.Padding)] = MapPadding,
        [nameof(Microsoft.Maui.Controls.FlexLayout.Direction)] = MapFlexProperties,
        [nameof(Microsoft.Maui.Controls.FlexLayout.Wrap)] = MapFlexProperties,
        [nameof(Microsoft.Maui.Controls.FlexLayout.JustifyContent)] = MapFlexProperties,
        [nameof(Microsoft.Maui.Controls.FlexLayout.AlignItems)] = MapFlexProperties,
        [nameof(Microsoft.Maui.Controls.FlexLayout.AlignContent)] = MapFlexProperties,
        [nameof(Microsoft.Maui.Controls.FlexLayout.Position)] = MapFlexProperties,
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
        // StackLayout 按 Orientation 选择 ArkUI 容器；其余布局默认 Column。
        // 注意 HorizontalStackLayout 继承自 StackBase 而非 StackLayout，必须合并判断
        if (IsHorizontalStack(VirtualView))
            return new HarmonyOS.ArkUI.Row();
        return new HarmonyOS.ArkUI.Column();
    }

    /// <summary>判定虚拟视图是否为水平栈（StackLayout{Horizontal} / HorizontalStackLayout 都合法）</summary>
    internal static bool IsHorizontalStack(object? view)
        => view is Microsoft.Maui.Controls.HorizontalStackLayout
           || (view is Microsoft.Maui.Controls.StackLayout sl && sl.Orientation == StackOrientation.Horizontal)
           || (view is Microsoft.Maui.Controls.FlexLayout flex
               && flex.Direction is Microsoft.Maui.Layouts.FlexDirection.Row
                       or Microsoft.Maui.Layouts.FlexDirection.RowReverse);

    /// <summary>判定是否为任意栈式布局（StackBase 覆盖 Stack/HStack/VStack）</summary>
    internal static bool IsAnyStack(object? view)
        => view is Microsoft.Maui.Controls.StackBase
           || view is Microsoft.Maui.Controls.FlexLayout;

    private static void MapFlexProperties(HarmonyLayoutHandler h, MLAYOUT v)
    {
        if (v is not Microsoft.Maui.Controls.FlexLayout flex)
            return;

        if (h.PlatformView is HarmonyOS.ArkUI.Flex flexNode)
        {
            flexNode.Direction = FlexSemanticMapper.MapDirection(flex.Direction);
            flexNode.Wrap = FlexSemanticMapper.MapWrap(flex.Wrap);
            flexNode.JustifyContent = FlexSemanticMapper.MapJustify(flex.JustifyContent);
            flexNode.AlignItems = FlexSemanticMapper.MapAlignItems(flex.AlignItems);
            flexNode.AlignContent = FlexSemanticMapper.MapAlignContent(flex.AlignContent);

            if (flex.Position == Microsoft.Maui.Layouts.FlexPosition.Absolute)
                HarmonyOS.Interop.HiLog.Warn(
                    "HarmonyHost",
                    "[FlexLayout] Position=Absolute is not provided by NODE_FLEX_OPTION; managed layout fallback remains.");
            return;
        }

        if (h.PlatformView is HarmonyOS.ArkUI.Row row)
        {
            row.JustifyContent = flex.JustifyContent switch
            {
                Microsoft.Maui.Layouts.FlexJustify.Start => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_START,
                Microsoft.Maui.Layouts.FlexJustify.Center => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_CENTER,
                Microsoft.Maui.Layouts.FlexJustify.End => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_END,
                Microsoft.Maui.Layouts.FlexJustify.SpaceBetween => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_BETWEEN,
                Microsoft.Maui.Layouts.FlexJustify.SpaceAround => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_AROUND,
                Microsoft.Maui.Layouts.FlexJustify.SpaceEvenly => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_EVENLY,
                _ => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_START,
            };
            row.AlignItems = flex.AlignItems switch
            {
                Microsoft.Maui.Layouts.FlexAlignItems.Start => ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_START,
                Microsoft.Maui.Layouts.FlexAlignItems.Center => ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER,
                Microsoft.Maui.Layouts.FlexAlignItems.End => ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_END,
                Microsoft.Maui.Layouts.FlexAlignItems.Stretch => ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_STRETCH,
                _ => ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_STRETCH,
            };
        }
        else if (h.PlatformView is HarmonyOS.ArkUI.Column column)
        {
            column.JustifyContent = flex.JustifyContent switch
            {
                Microsoft.Maui.Layouts.FlexJustify.Start => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_START,
                Microsoft.Maui.Layouts.FlexJustify.Center => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_CENTER,
                Microsoft.Maui.Layouts.FlexJustify.End => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_END,
                Microsoft.Maui.Layouts.FlexJustify.SpaceBetween => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_BETWEEN,
                Microsoft.Maui.Layouts.FlexJustify.SpaceAround => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_AROUND,
                Microsoft.Maui.Layouts.FlexJustify.SpaceEvenly => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_EVENLY,
                _ => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_START,
            };
            column.AlignItems = flex.AlignItems switch
            {
                Microsoft.Maui.Layouts.FlexAlignItems.Start => ArkUI_HorizontalAlignment.ARKUI_HORIZONTAL_ALIGNMENT_START,
                Microsoft.Maui.Layouts.FlexAlignItems.Center => ArkUI_HorizontalAlignment.ARKUI_HORIZONTAL_ALIGNMENT_CENTER,
                Microsoft.Maui.Layouts.FlexAlignItems.End => ArkUI_HorizontalAlignment.ARKUI_HORIZONTAL_ALIGNMENT_END,
                Microsoft.Maui.Layouts.FlexAlignItems.Stretch => ArkUI_HorizontalAlignment.ARKUI_HORIZONTAL_ALIGNMENT_START,
                _ => ArkUI_HorizontalAlignment.ARKUI_HORIZONTAL_ALIGNMENT_START,
            };
        }

        // ArkUI Row/Column 没有 Wrap / AlignContent / Position 的完整 Flexbox 映射。
        HarmonyOS.Interop.HiLog.Warn(
            "HarmonyHost",
            "[FlexLayout] Wrap/AlignContent/Position are degraded on HarmonyOS.");
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
            h.AttachChild(u.View); // M1：中段插入退化为顺序追加（ArkUI_NodeBase.InsertChildAt 可支持真实插入，按需接通）
    }

    public static void MapUpdate(HarmonyLayoutHandler h, MLAYOUT v, object? args)
    {
        if (args is LayoutHandlerUpdate u)
        {
            if (h._children.Remove(u.View, out var old))
                HarmonyViewHandler<MLAYOUT, ArkUINode>.DisposeContent(old, h.PlatformView);
            h.AttachChild(u.View);
        }
    }

    public static void MapUpdateZIndex(HarmonyLayoutHandler h, MLAYOUT v, object? args)
    {
        // NODE_Z_INDEX（值大者在上，flex 与 Stack 通用）
        if (args is LayoutHandlerUpdate u
            && h._children.TryGetValue(u.View, out var ch)
            && ch.PlatformView is ArkUINode node)
        {
            node.SetZIndex(ZIndexOrder.EffectiveZ(u.View));
        }
    }

    private void AttachChild(IView view)
    {
        if (_children.ContainsKey(view)) return;
        var handler = HarmonyHandlerFactory.Create(view);
        if (handler.PlatformView is ArkUINode node)
        {
            // MAUI 显式 WidthRequest/HeightRequest 优先（vp）；view.Width/Height 是布局后的
            // 实测值（未布局时为 -1），不能用来判断显式尺寸
            bool isHorizontalStack = IsHorizontalStack(VirtualView);
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
                switch (view.VerticalLayoutAlignment)
                {
                    case MALIGNMENT.Fill:
                        // ArkUI 的 STRETCH 会参与子项自量测并形成回环（Pivot 表头
                        // 实测 256px 后反向把 Row/Scroll 也撑到 256px）；auto 行先
                        // 保持内容高，显式高仍由 HeightRequest 分支优先处理。
                        node.SetAlignSelf(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_START);
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
            if (VirtualView is Microsoft.Maui.Controls.StackBase sl && sl.Spacing > 0)
            {
                if (isHorizontalStack) mRight += (float)sl.Spacing;
                else mBottom += (float)sl.Spacing;
            }
            if (mTop > 0 || mRight > 0 || mBottom > 0 || mLeft > 0)
                node.SetMarginEdges(mTop, mRight, mBottom, mLeft);

            PlatformView.AddChild(node);
            // 中部插入时 z 编码随兄弟数变化：全体兄弟按声明序重写；
            // 非布局父容器（返回 false）按单节点落值
            if (!ZIndexOrder.TryRewriteSiblings(view))
                node.SetZIndex(ZIndexOrder.EffectiveZ(view));
            _children[view] = handler;
        }
    }

    private void DetachChild(IView view)
    {
        if (_children.Remove(view, out var handler))
            DisposeContent(handler, PlatformView);
    }

    private void ClearChildren()
    {
        foreach (var handler in _children.Values)
            DisposeContent(handler, PlatformView);
        PlatformView.RemoveAllChildren();
        _children.Clear();
    }
}
