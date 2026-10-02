// HarmonyViewMapper：所有 HarmonyOS Handler 的共享基座 Mapper。
// 收口 IView 级通用属性（WidthRequest/HeightRequest），消除逐 handler 重复映射、
// 漏映射（Image 图标按 PNG 原尺寸自撑溢出 Auto 轨道即此洞）的问题。
// 纪律：
//  - 各 handler 的 Mapper 改为 new(HarmonyViewMapper.Base)；自身条目优先于基座
//    （PropertyMapper 链式语义：外层覆盖内层），需要特化（ContentView/Frame 的
//    水平栈 auto 宽等）的 handler 保留自己的条目即可。
//  - 视觉属性（Visibility/Opacity/Transform 等）不在这里——HarmonyViewHandler.UpdateValue
//    已拦截处理（支持运行时变更），mapper 重复映射会双写。
//  - MinimumWidth/HeightRequest 不在此：ArkUI NDK 无 min-size 属性（审计基线记录）。
#nullable enable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public static class HarmonyViewMapper
{
    public static readonly PropertyMapper<IView, IViewHandler> Base = new(ViewHandler.ViewMapper)
    {
        [nameof(VisualElement.WidthRequest)] = (h, v) => ApplyWidthRequest(h, v),
        [nameof(VisualElement.HeightRequest)] = (h, v) => ApplyHeightRequest(h, v),
        [nameof(IView.Background)] = (h, v) => ApplyBackground(h, v),
        [nameof(IPadding.Padding)] = (h, v) => ApplyPadding(h, v),
    };

    /// <summary>IPadding.Padding 默认落地（四边 vp）；容器类（Layout/Frame/ManagedLayout）
    /// 有自家 Padding 条目或 Arrange 口径，按链式语义覆盖本默认。</summary>
    internal static void ApplyPadding(IViewHandler handler, IView view)
    {
        if (handler.PlatformView is not ArkUINode node || view is not IPadding p) return;
        var t = p.Padding;
        if (t.Top > 0 || t.Right > 0 || t.Bottom > 0 || t.Left > 0)
            node.SetPaddingEdges((float)t.Top, (float)t.Right, (float)t.Bottom, (float)t.Left);
    }

    /// <summary>IView.Background（Brush）默认落地：纯色/渐变/图片经 BrushHelper；
    /// 各 handler 自有 Background 条目（Label/Frame/Layout 等）按链式语义覆盖本默认。</summary>
    internal static void ApplyBackground(IViewHandler handler, IView view)
    {
        if (handler.PlatformView is ArkUINode node)
            BrushHelper.ApplyBackground(node, view.Background);
    }

    /// <summary>显式 WidthRequest 落节点（vp）；清除（-1）复位 Auto，与 FrameHandler 同一纪律。</summary>
    internal static void ApplyWidthRequest(IViewHandler handler, IView view)
    {
        if (handler.PlatformView is not ArkUINode node || view is not VisualElement ve)
            return;
        if (ve.WidthRequest >= 0)
            node.SetWidth((float)ve.WidthRequest);
        else
            node.SetWidthAuto();
    }

    internal static void ApplyHeightRequest(IViewHandler handler, IView view)
    {
        if (handler.PlatformView is not ArkUINode node || view is not VisualElement ve)
            return;
        if (ve.HeightRequest >= 0)
            node.SetHeight((float)ve.HeightRequest);
        else
            node.SetHeightAuto();
    }
}
