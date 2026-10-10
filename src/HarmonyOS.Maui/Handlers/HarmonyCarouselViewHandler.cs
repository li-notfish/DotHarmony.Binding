#nullable enable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using MCarouselView = Microsoft.Maui.Controls.CarouselView;
using ArkSwiper = HarmonyOS.ArkUI.Swiper;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// CarouselView → ArkUI Swiper：全量物化子视图，非虚拟化。
/// Loop / IsSwipeEnabled / CurrentItem / Position 双向同步由原生 onChange 驱动。
/// </summary>
public class HarmonyCarouselViewHandler : HarmonyViewHandler<MCarouselView, ArkSwiper>
{
    private const float DefaultHeight = 200f;

    private bool _syncingFromPlatform;
    private readonly List<(ArkUINode Node, View View)> _live = new();

    public static PropertyMapper<MCarouselView, HarmonyCarouselViewHandler> Mapper = new(HarmonyViewMapper.Base)
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
        ItemsViewMaterializer.DisposeAll(_live);
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
            VirtualView.Position = Math.Max(0, index);
            if (VirtualView.ItemsSource is System.Collections.IList items
                && index >= 0 && index < items.Count
                && !ReferenceEquals(VirtualView.CurrentItem, items[index]))
            {
                VirtualView.CurrentItem = items[index];
            }
        }
        finally { _syncingFromPlatform = false; }
    }

    private void Rebuild()
    {
        if (VirtualView.HeightRequest <= 0)
            VirtualView.HeightRequest = DefaultHeight;
        ItemsViewMaterializer.Rebuild(
            PlatformView, VirtualView.ItemsSource, VirtualView.ItemTemplate, _live, fillHeight: true);
    }
}
