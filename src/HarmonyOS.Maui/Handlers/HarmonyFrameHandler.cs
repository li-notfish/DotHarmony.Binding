using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Frame 的 HarmonyOS Handler（Frame 已废弃，映射到 ArkUI Stack 容器）。</summary>
public class HarmonyFrameHandler : ViewHandler<Frame, ArkStack>
{
    public static PropertyMapper<Frame, HarmonyFrameHandler> Mapper = new(ViewMapper)
    {
        [nameof(Frame.Content)] = MapContent,
        [nameof(Frame.BackgroundColor)] = MapBackgroundColor,
    };

    public HarmonyFrameHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView() => new();

    public static void MapContent(HarmonyFrameHandler h, Frame v)
    {
        if (v.Content is not IView content) return;
        var childHandler = HarmonyHandlerFactory.Create(content);
        childHandler.SetVirtualView(content);
        if (childHandler.PlatformView is ArkUINode node)
        {
            node.SetWidthPercent(1.0f);
            node.SetHeightPercent(1.0f);
            h.PlatformView.AddChild(node);
        }
    }

    public static void MapBackgroundColor(HarmonyFrameHandler h, Frame v)
    {
        if (v.BackgroundColor is { } c)
            BrushHelper.ApplyBackground(h.PlatformView, v.Background);
    }
}
