using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI ContentPage 的 HarmonyOS Handler（ArkUI Column 容器 + 单 Content 子节点）。</summary>
public class HarmonyContentPageHandler : ViewHandler<ContentPage, ArkColumn>
{
    public static PropertyMapper<ContentPage, HarmonyContentPageHandler> Mapper = new(ViewMapper)
    {
        [nameof(IContentView.Content)] = MapContent,
        [nameof(IContentView.Background)] = MapBackground,
        [nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
        [nameof(ITitledElement.Title)] = MapTitle,
    };

    public HarmonyContentPageHandler() : base(Mapper) { }

    protected override ArkColumn CreatePlatformView()
    {
        var column = new ArkColumn();
        column.SetWidthPercent(1.0f);
        column.SetHeightPercent(1.0f);
        return column;
    }

    public static void MapContent(HarmonyContentPageHandler handler, ContentPage page)
    {
        if (page.Content is not IView content) return;
        handler.UpdateContent(content);
    }

    public static void MapBackground(HarmonyContentPageHandler h, ContentPage v)
    {
        BrushHelper.ApplyBackground(h.PlatformView, v.Background);
    }

    public static void MapBackgroundColor(HarmonyContentPageHandler h, ContentPage v)
    {
        // XAML BackgroundColor 设置的是 VisualElement.BackgroundColor（Color），与 Background（Brush）不互通
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(
                (byte)(c.Red * 255), (byte)(c.Green * 255),
                (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }

    public static void MapTitle(HarmonyContentPageHandler h, ContentPage v)
    {
        // HarmonyOS 标题栏由宿主 UIAbility 承担，M1 忽略
    }

    private IElementHandler? _contentHandler;

    private void UpdateContent(object? content)
    {
        if (_contentHandler is not null)
        {
            PlatformView.RemoveAllChildren();
            _contentHandler = null;
        }

        if (content is not IView view) return;

        var childHandler = HarmonyHandlerFactory.Create(view);
        childHandler.SetVirtualView(view);
        _contentHandler = childHandler;

        if (childHandler.PlatformView is ArkUINode node)
        {
            node.SetWidthPercent(1.0f);
            node.SetHeightPercent(1.0f);
            PlatformView.AddChild(node);
        }
    }
}
