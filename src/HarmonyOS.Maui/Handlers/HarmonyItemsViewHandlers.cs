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
public class HarmonyCollectionViewHandler : HarmonyViewHandler<MCollectionView, ArkScroll>
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
/// Loop/IsSwipeEnabled/CurrentItem/Position 双向接通：用户滑动经 Swiper onChange（data[0]=index）
/// 回传 Position/CurrentItem——MAUI 的 Position/CurrentItem 绑定属性回调自动触发
/// PositionChanged/CurrentItemChanged，handler 无需手动 raise；程序化设置经映射下发
/// NODE_SWIPER_INDEX，_syncingFromPlatform 守卫防回环。
/// Swiper 无内容自撑高（子项 100% 填充）——HeightRequest 未设置时给默认高，裸用不塌陷。
/// </summary>
public class HarmonyCarouselViewHandler : HarmonyViewHandler<MCarouselView, ArkSwiper>
{
    /// <summary>HeightRequest 未设置时的默认高（vp）：Swiper 子项 100% 填充不自撑</summary>
    private const float DefaultHeight = 200f;

    private bool _syncingFromPlatform;

    public static PropertyMapper<MCarouselView, HarmonyCarouselViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(ItemsView.ItemsSource)] = MapItems,
        [nameof(ItemsView.ItemTemplate)] = MapItems,
        [nameof(MCarouselView.Loop)] = MapLoop,
        [nameof(MCarouselView.IsSwipeEnabled)] = MapIsSwipeEnabled,
        [nameof(MCarouselView.CurrentItem)] = MapCurrentItem,
        [nameof(MCarouselView.Position)] = MapPosition,
    };

    public HarmonyCarouselViewHandler() : base(Mapper) { }

    protected override ArkSwiper CreatePlatformView() => new();

    protected override void ConnectHandler(ArkSwiper platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Change += OnSwiperChange;
        SyncAll();
    }

    protected override void DisconnectHandler(ArkSwiper platformView)
    {
        platformView.Change -= OnSwiperChange;
        base.DisconnectHandler(platformView);
    }

    public static void MapItems(HarmonyCarouselViewHandler handler, MCarouselView view)
        => handler.Rebuild();

    public static void MapLoop(HarmonyCarouselViewHandler handler, MCarouselView view)
        => handler.PlatformView.Loop = view.Loop;

    public static void MapIsSwipeEnabled(HarmonyCarouselViewHandler handler, MCarouselView view)
        => handler.PlatformView.DisableSwipe = !view.IsSwipeEnabled;

    public static void MapCurrentItem(HarmonyCarouselViewHandler handler, MCarouselView view)
        => handler.SyncIndexFromCurrentItem();

    public static void MapPosition(HarmonyCarouselViewHandler handler, MCarouselView view)
        => handler.SyncIndexFromPosition();

    private void SyncAll()
    {
        PlatformView.Loop = VirtualView.Loop;
        PlatformView.DisableSwipe = !VirtualView.IsSwipeEnabled;
        SyncIndexFromPosition();
    }

    private void SyncIndexFromPosition()
    {
        if (_syncingFromPlatform) return;
        _syncingFromPlatform = true;
        try { PlatformView.CurrentIndex = Math.Max(0, VirtualView.Position); }
        finally { _syncingFromPlatform = false; }
    }

    private void SyncIndexFromCurrentItem()
    {
        if (_syncingFromPlatform) return;
        if (VirtualView.ItemsSource is not System.Collections.IList list || VirtualView.CurrentItem is null)
            return;
        for (int i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], VirtualView.CurrentItem))
            {
                _syncingFromPlatform = true;
                try { PlatformView.CurrentIndex = i; }
                finally { _syncingFromPlatform = false; }
                return;
            }
        }
    }

    private void OnSwiperChange(ArkUINodeEvent e)
    {
        if (_syncingFromPlatform) return;
        int index = e.ComponentData(0).i32;
        _syncingFromPlatform = true;
        try
        {
            // 属性回调自动触发 PositionChanged/CurrentItemChanged（CarouselView 内部 propertyChanged）
            VirtualView.Position = Math.Max(0, index);
            if (VirtualView.ItemsSource is System.Collections.IList list
                && index >= 0 && index < list.Count
                && !ReferenceEquals(VirtualView.CurrentItem, list[index]))
            {
                VirtualView.CurrentItem = list[index];
            }
        }
        finally { _syncingFromPlatform = false; }
    }

    private void Rebuild()
    {
        if (VirtualView.HeightRequest <= 0)
            VirtualView.HeightRequest = DefaultHeight;
        ItemsViewMaterializer.Rebuild(PlatformView, VirtualView.ItemsSource, VirtualView.ItemTemplate, fillHeight: true);
    }
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
