using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using MCollectionView = Microsoft.Maui.Controls.CollectionView;
using MCarouselView = Microsoft.Maui.Controls.CarouselView;
using HarmonyOS.Bindings.NativeNode;
using ArkScroll = HarmonyOS.ArkUI.Scroll;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkSwiper = HarmonyOS.ArkUI.Swiper;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// CollectionView → ArkUI Scroll + Column（全量物化子视图，非虚拟化）。
/// M1 限制：无虚拟化/复用（虚拟化需 ArkUI NodeAdapter，ROADMAP 1.4）；仅纵向；ItemsSource 变更全量重建。
/// </summary>
public class HarmonyCollectionViewHandler : ViewHandler<MCollectionView, ArkScroll>
{
    public static PropertyMapper<MCollectionView, HarmonyCollectionViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(ItemsView.ItemsSource)] = MapItems,
        [nameof(ItemsView.ItemTemplate)] = MapItems,
    };

    private ArkColumn? _column;

    public HarmonyCollectionViewHandler() : base(Mapper) { }

    protected override ArkScroll CreatePlatformView()
    {
        var scroll = new ArkScroll();
        scroll.SetWidthPercent(1.0f);
        _column = new ArkColumn();
        _column.SetWidthPercent(1.0f);
        scroll.AddChild(_column);
        return scroll;
    }

    protected override void ConnectHandler(ArkScroll platformView)
    {
        base.ConnectHandler(platformView);
        Rebuild();
    }

    public static void MapItems(HarmonyCollectionViewHandler handler, MCollectionView view)
        => handler.Rebuild();

    private void Rebuild()
    {
        if (_column is null)
            return;
        ItemsViewMaterializer.Rebuild(_column, VirtualView.ItemsSource, VirtualView.ItemTemplate, fillHeight: false);
    }
}

/// <summary>
/// CarouselView → ArkUI Swiper（全量物化子视图，非虚拟化）。
/// M1 限制：无虚拟化（Loop/位置回传等暂略）；Swiper 需显式高度（由使用方 HeightRequest 提供）。
/// </summary>
public class HarmonyCarouselViewHandler : ViewHandler<MCarouselView, ArkSwiper>
{
    public static PropertyMapper<MCarouselView, HarmonyCarouselViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(ItemsView.ItemsSource)] = MapItems,
        [nameof(ItemsView.ItemTemplate)] = MapItems,
    };

    public HarmonyCarouselViewHandler() : base(Mapper) { }

    protected override ArkSwiper CreatePlatformView() => new();

    protected override void ConnectHandler(ArkSwiper platformView)
    {
        base.ConnectHandler(platformView);
        Rebuild();
    }

    public static void MapItems(HarmonyCarouselViewHandler handler, MCarouselView view)
        => handler.Rebuild();

    private void Rebuild()
        => ItemsViewMaterializer.Rebuild(PlatformView, VirtualView.ItemsSource, VirtualView.ItemTemplate, fillHeight: true);
}

/// <summary>ItemsView 的 M1 物化器：ItemsSource + ItemTemplate → 子视图全量添加（AOT 安全，XamlC 模板为编译期工厂）</summary>
internal static class ItemsViewMaterializer
{
    public static void Rebuild(ArkUINodeBase container, System.Collections.IEnumerable? items, DataTemplate? template, bool fillHeight)
    {
        container.RemoveAllChildren();
        if (items is null)
            return;

        foreach (var item in items)
        {
            var view = template?.CreateContent() as View
                ?? new Label { Text = item?.ToString() ?? string.Empty };
            view.BindingContext = item;

            var handler = HarmonyHandlerFactory.Create((Microsoft.Maui.IView)view);
            handler.SetVirtualView(view);
            if (handler.PlatformView is ArkUINode node)
            {
                node.SetWidthPercent(1.0f);
                if (fillHeight)
                    node.SetHeightPercent(1.0f);
                container.AddChild(node);
            }
        }
    }
}
