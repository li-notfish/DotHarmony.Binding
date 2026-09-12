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
    };

    public HarmonyFrameHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView() => new();

    public static void MapContent(HarmonyFrameHandler h, Border v)
    {
        if (v.Content is not IView content) return;
        var childHandler = HarmonyHandlerFactory.Create(content);
        childHandler.SetVirtualView(content);
        if (childHandler.PlatformView is ArkUINode node)
        {
            // 内容宽度填满 Border；高度随内容自适应（百分比高度会把内容撑满整个容器）
            node.SetWidthPercent(1.0f);
            h.PlatformView.AddChild(node);
        }
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
