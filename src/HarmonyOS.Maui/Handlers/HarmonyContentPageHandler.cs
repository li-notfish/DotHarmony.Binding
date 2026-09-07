// HarmonyContentPageHandler：MAUI ContentPage → ArkUI Column（100%×100% 容器 + 单 Content 子节点）
// 协议对齐官方：IPageHandler : IContentViewHandler : IViewHandler；
// Content 经 PropertyMapper（nameof(IContentView.Content)）自动增量派发。
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyContentPageHandler : ViewHandler<Microsoft.Maui.Controls.ContentPage, ArkColumn>
{
    public static PropertyMapper<Microsoft.Maui.Controls.ContentPage, HarmonyContentPageHandler> Mapper =
        new(ViewHandler.ViewMapper)
        {
            [nameof(IContentView.Content)] = MapContent,
            [nameof(ITitledElement.Title)] = (h, v) => { /* 鸿蒙标题栏由宿主 UIAbility 承担，M1 忽略 */ },
            [nameof(IContentView.Background)] = (h, v) =>
                BrushHelper.ApplyBackground(h.PlatformView, v.Background),
        };

    public HarmonyContentPageHandler() : base(Mapper) { }

    protected override ArkColumn CreatePlatformView()
    {
        var column = new ArkColumn();
        // Page 语义：铺满窗口
        column.SetWidthPercent(1.0f);
        column.SetHeightPercent(1.0f);
        return column;
    }

    private static void MapContent(HarmonyContentPageHandler handler, IContentView page)
    {
        if (page.Content is not IView content) return;
        var childHandler = HarmonyHandlerFactory.Create(content);
        childHandler.SetVirtualView(content);
        if (childHandler.PlatformView is ArkUINode node)
        {
            // Content 填满 Page
            node.SetWidthPercent(1.0f);
            node.SetHeightPercent(1.0f);
            handler.PlatformView.AddChild(node);
        }
    }
}
