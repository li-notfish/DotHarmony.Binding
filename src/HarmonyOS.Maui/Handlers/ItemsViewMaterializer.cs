#nullable enable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using HarmonyOS.Bindings.NativeNode;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>CarouselView 的全量物化器：ItemsSource + ItemTemplate → 子视图。</summary>
internal static class ItemsViewMaterializer
{
    public static void Rebuild(
        ArkUINode container,
        System.Collections.IEnumerable? items,
        DataTemplate? template,
        List<(ArkUINode Node, View View)> live,
        bool fillHeight)
    {
        DisposeAll(live);
        container.RemoveAllChildren();
        if (items is null)
            return;

        foreach (var item in items)
        {
            var view = template?.CreateContent() as View
                ?? new Label { Text = item?.ToString() ?? string.Empty };
            view.BindingContext = item;

            var handler = HarmonyHandlerFactory.Create((IView)view);
            if (handler.PlatformView is ArkUINode node)
            {
                node.SetWidthPercent(1.0f);
                if (fillHeight)
                    node.SetHeightPercent(1.0f);
                container.AddChild(node);
                live.Add((node, view));
            }
        }
    }

    public static void DisposeAll(List<(ArkUINode Node, View View)> live)
    {
        foreach (var (node, view) in live)
        {
            ((IElement)view).Handler = null;
            node.Dispose();
        }
        live.Clear();
    }
}
