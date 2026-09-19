using System.Collections.Specialized;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using MCollectionView = Microsoft.Maui.Controls.CollectionView;
using MCarouselView = Microsoft.Maui.Controls.CarouselView;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;
using ArkList = HarmonyOS.ArkUI.List;
using ArkSwiper = HarmonyOS.ArkUI.Swiper;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// CollectionView → ArkUI List（ARKUI_NODE_LIST + NodeAdapter 虚拟化）。
/// 条目按可见范围物化：ON_ADD_NODE_TO_ADAPTER 事件回调创建子节点（ItemTemplate 经
/// CreateContent，AOT 安全），ON_REMOVE 回调处置（Handler 置 null 断连 + 节点 Dispose）——
/// 长列表内存/滚动性能不再随条数线性涨。ItemsSource 变更走 ReloadAllItems 全量重载。
/// </summary>
public class HarmonyCollectionViewHandler : HarmonyViewHandler<MCollectionView, ArkList>
{
    public static PropertyMapper<MCollectionView, HarmonyCollectionViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(ItemsView.ItemsSource)] = MapItems,
        [nameof(ItemsView.ItemTemplate)] = MapItems,
    };

    private ArkUINodeAdapter? _adapter;
    private List<object?> _items = new();
    // 活跃条目：原生节点句柄 → (节点, 虚拟视图)；ON_REMOVE 时按句柄处置
    private readonly Dictionary<nint, (ArkUINode Node, View View)> _live = new();
    // 增量订阅源（ObservableCollection 等；属性引用不变时经 CollectionChanged 触发重载）
    private INotifyCollectionChanged? _observed;

    public HarmonyCollectionViewHandler() : base(Mapper) { }

    protected override ArkList CreatePlatformView()
    {
        var list = new ArkList();
        list.SetWidthPercent(1.0f);
        return list;
    }

    protected override void ConnectHandler(ArkList platformView)
    {
        base.ConnectHandler(platformView);
        // adapter 每次 connect 重建（disconnect 时随节点复位 + Dispose）
        _adapter = new ArkUINodeAdapter();
        _adapter.SetEventReceiver(OnAdapterEvent);
        platformView.SetNodeAdapter(_adapter.Handle);
        Reload();
    }

    protected override void DisconnectHandler(ArkList platformView)
    {
        UnhookItemsSource();
        foreach (var (node, view) in _live.Values)
        {
            ((Microsoft.Maui.IElement)view).Handler = null; // 触发 handler DisconnectHandler（手势/事件注销）
            node.Dispose();
        }
        _live.Clear();
        if (_adapter != null)
        {
            platformView.ResetNodeAdapter();
            _adapter.Dispose();
            _adapter = null;
        }
        base.DisconnectHandler(platformView);
    }

    public static void MapItems(HarmonyCollectionViewHandler handler, MCollectionView view)
    {
        // 挂/换挂增量订阅（CollectionChanged → 全量重载兜底）后再首轮重载
        handler.UnhookItemsSource();
        if (view.ItemsSource is INotifyCollectionChanged incc)
        {
            handler._observed = incc;
            incc.CollectionChanged += handler.OnItemsCollectionChanged;
        }
        handler.Reload();
    }

    private void UnhookItemsSource()
    {
        if (_observed is null) return;
        _observed.CollectionChanged -= OnItemsCollectionChanged;
        _observed = null;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => Reload(); // 虚拟化框架按可见范围物化，全量重载代价可控；InsertItem/RemoveItem 精确增量留待立项

    private void Reload()
    {
        if (_adapter is null)
            return;
        _items.Clear();
        if (VirtualView?.ItemsSource is not null)
        {
            foreach (var item in VirtualView.ItemsSource)
                _items.Add(item);
        }
        _adapter.SetTotalCount(_items.Count);
        _adapter.ReloadAllItems();
    }

    private void OnAdapterEvent(ArkUI_NodeAdapterEventView ev)
    {
        switch (ev.Type)
        {
            case ArkUI_NodeAdapterEventType.NODE_ADAPTER_EVENT_ON_GET_NODE_ID:
                // 全量重载模型：索引即稳定 id（每次 Reload 全部重建）
                ev.SetNodeId(ev.ItemIndex);
                break;
            case ArkUI_NodeAdapterEventType.NODE_ADAPTER_EVENT_ON_ADD_NODE_TO_ADAPTER:
                var node = Materialize(ev.ItemIndex);
                if (node is not null)
                    ev.SetItem(node.Handle);
                break;
            case ArkUI_NodeAdapterEventType.NODE_ADAPTER_EVENT_ON_REMOVE_NODE_FROM_ADAPTER:
                Dematerialize(ev.RemovedNode);
                break;
        }
    }

    /// <summary>物化条目节点（ItemTemplate → 视图 → handler；宽度 100% 填充 List 交叉轴）</summary>
    private ArkUINode? Materialize(int index)
    {
        if (index < 0 || index >= _items.Count)
            return null;
        HiLog.Debug("HarmonyHost", $"[CollectionView] materialize idx={index}");
        var item = _items[index];
        var view = VirtualView?.ItemTemplate?.CreateContent() as View
            ?? new Label { Text = item?.ToString() ?? string.Empty };
        view.BindingContext = item;

        var handler = HarmonyHandlerFactory.Create((Microsoft.Maui.IView)view);
        handler.SetVirtualView(view);
        if (handler.PlatformView is not ArkUINode node)
            return null;
        node.SetWidthPercent(1.0f);
        _live[node.Handle.Handle] = (node, view);
        return node;
    }

    /// <summary>处置条目节点（Handler 置 null 断连 + 节点 Dispose；框架已从列表摘除）</summary>
    private void Dematerialize(nint nodeHandle)
    {
        if (!_live.Remove(nodeHandle, out var entry))
            return;
        ((Microsoft.Maui.IElement)entry.View).Handler = null;
        entry.Node.Dispose();
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
    /// <summary>活跃条目（全量物化模型）：Rebuild/Disconnect 统一处置（Handler 断连 + 节点 Dispose）</summary>
    private readonly List<(ArkUINode Node, View View)> _live = new();

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
        ItemsViewMaterializer.Rebuild(PlatformView, VirtualView.ItemsSource, VirtualView.ItemTemplate, _live, fillHeight: true);
    }
}

/// <summary>ItemsView 的 M1 物化器（CarouselView 用）：ItemsSource + ItemTemplate → 子视图全量添加（AOT 安全，XamlC 模板为编译期工厂）。
/// 替换前统一处置上一批物化视图（Handler 置 null 断连 + 节点 Dispose），ItemsSource 变动不再泄漏旧 handler。</summary>
internal static class ItemsViewMaterializer
{
    public static void Rebuild(ArkUINodeBase container, System.Collections.IEnumerable? items, DataTemplate? template,
        List<(ArkUINode Node, View View)> live, bool fillHeight)
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

            var handler = HarmonyHandlerFactory.Create((Microsoft.Maui.IView)view);
            handler.SetVirtualView(view);
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

    /// <summary>处置一批物化条目（Handler 置 null 触发 DisconnectHandler 注销手势/事件 + 节点 Dispose）</summary>
    public static void DisposeAll(List<(ArkUINode Node, View View)> live)
    {
        foreach (var (node, view) in live)
        {
            ((Microsoft.Maui.IElement)view).Handler = null;
            node.Dispose();
        }
        live.Clear();
    }
}
