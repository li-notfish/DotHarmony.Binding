// HarmonyFrameHandler：MAUI Controls.Frame → HarmonyOS.ArkUI.Stack（单 Content 子节点）
// Frame 是 ContentView，无 flex 布局语义——子节点按 100% 铺满。
// Border/CornerRadius 由 ArkUI 通用节点样式（NODE_BORDER_*）处理，M1 暂只接 Background + Content。
using Microsoft.Maui.Handlers;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyFrameHandler : ViewHandler<Microsoft.Maui.Controls.Frame, ArkStack>
{
    public static PropertyMapper<Microsoft.Maui.Controls.Frame, HarmonyFrameHandler> Mapper = new(ViewMapper)
    {
        [nameof(Microsoft.Maui.Controls.Frame.Content)] = MapContent,
        [nameof(Microsoft.Maui.Controls.Frame.BackgroundColor)] = MapBackgroundColor,
    };

    public HarmonyFrameHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformView() => new();

    public static void MapContent(HarmonyFrameHandler h, Microsoft.Maui.Controls.Frame v)
    {
        if (v.Content is not Microsoft.Maui.IView content) return;
        var childHandler = HarmonyHandlerFactory.Create(content);
        childHandler.SetVirtualView(content);
        if (childHandler.PlatformView is ArkUINode node)
        {
            // Content 填满 Frame
            node.SetWidthPercent(1.0f);
            node.SetHeightPercent(1.0f);
            h.PlatformView.AddChild(node);
        }
    }

    public static void MapBackgroundColor(HarmonyFrameHandler h, Microsoft.Maui.Controls.Frame v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }
}
