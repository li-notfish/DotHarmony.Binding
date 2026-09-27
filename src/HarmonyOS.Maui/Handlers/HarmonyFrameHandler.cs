using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// MAUI Frame / Border 的 HarmonyOS Handler（映射到 ArkUI Stack 容器）。
/// Frame 已废弃，Border 是其替代品；两者都映射到此 Handler。
/// </summary>
public class HarmonyFrameHandler : HarmonyViewHandler<Border, ArkStack>
{
    public static PropertyMapper<Border, HarmonyFrameHandler> Mapper = new(ViewMapper)
    {
        [nameof(Border.Content)] = MapContent,
        [nameof(Border.BackgroundColor)] = MapBackgroundColor,
        [nameof(Border.Background)] = MapBackground,
        [nameof(Border.Padding)] = MapPadding,
        [nameof(VisualElement.WidthRequest)] = MapWidthRequest,
        [nameof(VisualElement.HeightRequest)] = MapHeightRequest,
    };

    public HarmonyFrameHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView() => new();

    private IElementHandler? _contentHandler;

    public static void MapContent(HarmonyFrameHandler h, Border v)
    {
        if (h._contentHandler is not null)
        {
            HarmonyViewHandler<Border, ArkStack>.DisposeContent(h._contentHandler, h.PlatformView);
            h._contentHandler = null;
        }
        if (v.Content is not IView content) return;
        var childHandler = HarmonyHandlerFactory.Create(content);
        childHandler.SetVirtualView(content);
        h._contentHandler = childHandler;
        if (childHandler.PlatformView is ArkUINode node)
        {
            // 内容宽度填满 Border；高度随内容自适应（百分比高度会把内容撑满整个容器）
            node.SetWidthPercent(1.0f);
            if (v.HeightRequest > 0)
                // 显式定高（如磁贴 HeightRequest）时内容跟随填满，网格 Star 行获得可用高度
                node.SetHeightPercent(1.0f);
            h.PlatformView.AddChild(node);
        }
    }

    public static void MapWidthRequest(HarmonyFrameHandler h, Border v)
    {
        // Request 清除（-1）须复位 Auto——同 ContentViewHandler.ApplySizing 的纪律，
        // 否则 XAML/绑定往返一次后旧固定值永久残留在节点上
        if (v.WidthRequest > 0)
            h.PlatformView.SetWidth((float)v.WidthRequest);
        else
            h.PlatformView.SetWidthAuto();
    }

    public static void MapHeightRequest(HarmonyFrameHandler h, Border v)
    {
        if (v.HeightRequest > 0)
            h.PlatformView.SetHeight((float)v.HeightRequest);
        else
            h.PlatformView.SetHeightAuto();
    }

    public static void MapBackgroundColor(HarmonyFrameHandler h, Border v)
    {
        // XAML BackgroundColor 设置的是 VisualElement.BackgroundColor（Color），与 Background（Brush）不互通
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    public static void MapBackground(HarmonyFrameHandler h, Border v)
    {
        BrushHelper.ApplyBackground(h.PlatformView, v.Background);
    }

    public static void MapPadding(HarmonyFrameHandler h, Border v)
    {
        var p = v.Padding;
        if (p.Top > 0 || p.Right > 0 || p.Bottom > 0 || p.Left > 0)
        {
            h.PlatformView.SetPaddingEdges(
                (float)p.Top, (float)p.Right, (float)p.Bottom, (float)p.Left);
        }
    }
}

