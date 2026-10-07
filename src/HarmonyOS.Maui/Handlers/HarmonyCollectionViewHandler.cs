#nullable enable
using System.Collections;
using System.Collections.Specialized;
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using MCollectionView = Microsoft.Maui.Controls.CollectionView;
using ArkList = HarmonyOS.ArkUI.List;
using ArkListItemGroup = HarmonyOS.ArkUI.ListItemGroup;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// CollectionView → ArkUI List/ListItemGroup。
/// 顶层 adapter 负责“组”，组内 adapter 负责 item；组头/组脚挂在 ListItemGroup 的
/// header/footer 属性上，不参与 lane，也不参与选择。
/// </summary>
public class HarmonyCollectionViewHandler : HarmonyViewHandler<MCollectionView, ArkList>
{
    public static PropertyMapper<MCollectionView, HarmonyCollectionViewHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(ItemsView.ItemsSource)] = MapItems,
        [nameof(ItemsView.ItemTemplate)] = MapItems,
        [nameof(GroupableItemsView.IsGrouped)] = MapItems,
        [nameof(GroupableItemsView.GroupHeaderTemplate)] = MapItems,
        [nameof(GroupableItemsView.GroupFooterTemplate)] = MapItems,
        [nameof(StructuredItemsView.Header)] = MapStructure,
        [nameof(StructuredItemsView.HeaderTemplate)] = MapStructure,
        [nameof(StructuredItemsView.Footer)] = MapStructure,
        [nameof(StructuredItemsView.FooterTemplate)] = MapStructure,
        [nameof(ItemsView.EmptyView)] = MapStructure,
        [nameof(ItemsView.EmptyViewTemplate)] = MapStructure,
        [nameof(SelectableItemsView.SelectionMode)] = MapSelection,
        [nameof(SelectableItemsView.SelectedItem)] = MapSelection,
        [nameof(SelectableItemsView.SelectedItems)] = MapSelection,
        [nameof(MCollectionView.ItemsLayout)] = MapItemsLayout,
    };

    private ArkUINodeAdapter? _adapter;
    private List<CollectionViewGroup> _groups = new();
    private readonly Dictionary<nint, GroupState> _groupStates = new();
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
        _adapter = new ArkUINodeAdapter();
        _adapter.SetEventReceiver(OnAdapterEvent);
        platformView.SetNodeAdapter(_adapter.Handle);
        Reload();
    }

    protected override void DisconnectHandler(ArkList platformView)
    {
        UnhookItemsSource();
        foreach (var state in _groupStates.Values.ToArray())
            DisposeGroupState(state);
        _groupStates.Clear();
        if (_adapter is not null)
        {
            platformView.ResetNodeAdapter();
            _adapter.Dispose();
            _adapter = null;
        }
        base.DisconnectHandler(platformView);
    }

    public static void MapItems(HarmonyCollectionViewHandler handler, MCollectionView view)
    {
        handler.UnhookItemsSource();
        if (view.ItemsSource is INotifyCollectionChanged incc)
        {
            handler._observed = incc;
            incc.CollectionChanged += handler.OnItemsCollectionChanged;
        }
        handler.Reload();
    }

    public static void MapStructure(HarmonyCollectionViewHandler handler, MCollectionView view)
        => handler.Reload();

    public static void MapSelection(HarmonyCollectionViewHandler handler, MCollectionView view)
    {
        // 选择状态由 SelectableItemsView 自己维护；这里只保证属性变更进入 mapper 链。
    }

    public static void MapItemsLayout(HarmonyCollectionViewHandler handler, MCollectionView view)
    {
        var list = handler.PlatformView;
        int lanes = 1;
        float gutter = 0f, space = 0f;
        bool horizontal = view.ItemsLayout is ItemsLayout { Orientation: ItemsLayoutOrientation.Horizontal };
        if (view.ItemsLayout is GridItemsLayout g)
        {
            lanes = Math.Max(1, g.Span);
            if (horizontal)
            {
                gutter = (float)g.VerticalItemSpacing;
                space = (float)g.HorizontalItemSpacing;
            }
            else
            {
                gutter = (float)g.HorizontalItemSpacing;
                space = (float)g.VerticalItemSpacing;
            }
        }
        else if (view.ItemsLayout is LinearItemsLayout l)
        {
            space = (float)l.ItemSpacing;
        }
        list.SetDirection(horizontal ? ArkUI_Axis.ARKUI_AXIS_HORIZONTAL : ArkUI_Axis.ARKUI_AXIS_VERTICAL);
        list.SetLanes((uint)lanes, gutter);
        list.SetSpace(space);
    }

    private void UnhookItemsSource()
    {
        if (_observed is null) return;
        _observed.CollectionChanged -= OnItemsCollectionChanged;
        _observed = null;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => Reload();

    private void Reload()
    {
        if (_adapter is null)
            return;
        _groups = BuildGroups(VirtualView);
        _adapter.SetTotalCount(_groups.Count);
        _adapter.ReloadAllItems();
    }

    internal static List<CollectionViewGroup> BuildGroups(MCollectionView view)
    {
        var groups = new List<CollectionViewGroup>();
        if (view is GroupableItemsView { IsGrouped: true } grouped)
        {
            if (view.ItemsSource is null)
                return groups;

            foreach (var group in view.ItemsSource)
            {
                var slots = new List<CollectionViewSlot>();
                if (grouped.GroupHeaderTemplate is not null)
                    slots.Add(new(CollectionViewSlotKind.Header, group, grouped.GroupHeaderTemplate));
                if (group is IEnumerable members)
                {
                    foreach (var member in members)
                        slots.Add(new(CollectionViewSlotKind.Item, member, null));
                }
                if (grouped.GroupFooterTemplate is not null)
                    slots.Add(new(CollectionViewSlotKind.Footer, group, grouped.GroupFooterTemplate));
                groups.Add(new(group, slots));
            }
            return groups;
        }

        var plainSlots = new List<CollectionViewSlot>();
        if (view.Header is not null || view.HeaderTemplate is not null)
            plainSlots.Add(new(CollectionViewSlotKind.Header, view.Header, view.HeaderTemplate));

        var hasItems = false;
        if (view.ItemsSource is not null)
        {
            foreach (var item in view.ItemsSource)
            {
                hasItems = true;
                plainSlots.Add(new(CollectionViewSlotKind.Item, item, null));
            }
        }

        if (!hasItems && (view.EmptyView is not null || view.EmptyViewTemplate is not null))
            plainSlots.Add(new(CollectionViewSlotKind.Empty, view.EmptyView, view.EmptyViewTemplate));

        if (view.Footer is not null || view.FooterTemplate is not null)
            plainSlots.Add(new(CollectionViewSlotKind.Footer, view.Footer, view.FooterTemplate));

        if (plainSlots.Count == 0)
            return groups;

        groups.Add(new(null, plainSlots));
        return groups;
    }

    private void OnAdapterEvent(ArkUI_NodeAdapterEventView ev)
    {
        switch (ev.Type)
        {
            case ArkUI_NodeAdapterEventType.NODE_ADAPTER_EVENT_ON_GET_NODE_ID:
                ev.SetNodeId(ev.ItemIndex);
                break;
            case ArkUI_NodeAdapterEventType.NODE_ADAPTER_EVENT_ON_ADD_NODE_TO_ADAPTER:
                var group = MaterializeGroup(ev.ItemIndex);
                if (group is not null)
                    ev.SetItem(group.Handle);
                break;
            case ArkUI_NodeAdapterEventType.NODE_ADAPTER_EVENT_ON_REMOVE_NODE_FROM_ADAPTER:
                DematerializeGroup(ev.RemovedNode);
                break;
        }
    }

    private ArkListItemGroup? MaterializeGroup(int index)
    {
        if (index < 0 || index >= _groups.Count)
            return null;

        var data = _groups[index];
        var state = new GroupState(data);

        if (data.Slots.FirstOrDefault(s => s.Kind == CollectionViewSlotKind.Header)
            is { } headerSlot)
        {
            var (node, view) = CreateNode(headerSlot, selectable: false);
            state.HeaderNode = node;
            state.HeaderView = view;
            state.Node.SetHeader(node);
        }
        if (data.Slots.FirstOrDefault(s => s.Kind == CollectionViewSlotKind.Footer)
            is { } footerSlot)
        {
            var (node, view) = CreateNode(footerSlot, selectable: false);
            state.FooterNode = node;
            state.FooterView = view;
            state.Node.SetFooter(node);
        }

        state.ItemSlots.AddRange(data.Slots.Where(s => s.Kind == CollectionViewSlotKind.Item));
        state.Adapter.SetEventReceiver(e => OnGroupAdapterEvent(state, e));
        state.Adapter.SetTotalCount(state.ItemSlots.Count);
        state.Node.SetNodeAdapter(state.Adapter.Handle);

        _groupStates[state.Node.Handle.Handle] = state;
        return state.Node;
    }

    private void OnGroupAdapterEvent(GroupState state, ArkUI_NodeAdapterEventView ev)
    {
        switch (ev.Type)
        {
            case ArkUI_NodeAdapterEventType.NODE_ADAPTER_EVENT_ON_GET_NODE_ID:
                ev.SetNodeId(ev.ItemIndex);
                break;
            case ArkUI_NodeAdapterEventType.NODE_ADAPTER_EVENT_ON_ADD_NODE_TO_ADAPTER:
                var node = MaterializeItem(state, ev.ItemIndex);
                if (node is not null)
                    ev.SetItem(node.Handle);
                break;
            case ArkUI_NodeAdapterEventType.NODE_ADAPTER_EVENT_ON_REMOVE_NODE_FROM_ADAPTER:
                DematerializeItem(state, ev.RemovedNode);
                break;
        }
    }

    private ArkUINode? MaterializeItem(GroupState state, int index)
    {
        if (index < 0 || index >= state.ItemSlots.Count)
            return null;
        var (node, view) = CreateNode(state.ItemSlots[index], selectable: true);
        state.Items[node.Handle.Handle] = (node, view);
        return node;
    }

    private (ArkUINode Node, View View) CreateNode(CollectionViewSlot slot, bool selectable)
    {
        var view = slot.Kind is not CollectionViewSlotKind.Item && slot.Context is View directView
            ? directView
            : (slot.Kind == CollectionViewSlotKind.Item
                ? slot.Template ?? VirtualView?.ItemTemplate
                : slot.Template)?.CreateContent() as View
            ?? new Label { Text = slot.Context?.ToString() ?? string.Empty };
        view.BindingContext = slot.Context;

        var handler = HarmonyHandlerFactory.Create((IView)view);
        if (handler.PlatformView is not ArkUINode node)
            throw new InvalidOperationException($"CollectionView slot handler did not create an ArkUI node: {view.GetType().Name}");

        node.SetWidthPercent(1.0f);
        if (selectable)
            node.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, _ => HandleItemClick(slot.Context));
        return (node, view);
    }

    private void HandleItemClick(object? item)
        => ApplySelection(VirtualView as SelectableItemsView, item);

    internal static void ApplySelection(SelectableItemsView? selectable, object? item)
    {
        if (selectable is null || selectable.SelectionMode == SelectionMode.None)
            return;

        if (selectable.SelectionMode == SelectionMode.Single)
        {
            selectable.SelectedItem = item;
            selectable.SelectedItems = new List<object> { item! };
            return;
        }

        var current = selectable.SelectedItems?.ToList() ?? new List<object>();
        if (!current.Remove(item!))
            current.Add(item!);
        selectable.SelectedItems = current;
    }

    private void DematerializeItem(GroupState state, nint nodeHandle)
    {
        if (!state.Items.Remove(nodeHandle, out var entry))
            return;
        ((IElement)entry.View).Handler = null;
        entry.Node.Dispose();
    }

    private void DematerializeGroup(nint nodeHandle)
    {
        if (!_groupStates.Remove(nodeHandle, out var state))
            return;
        DisposeGroupState(state);
    }

    private static void DisposeGroupState(GroupState state)
    {
        state.Node.ResetNodeAdapter();
        state.Adapter.Dispose();

        foreach (var (node, view) in state.Items.Values)
        {
            ((IElement)view).Handler = null;
            node.Dispose();
        }
        state.Items.Clear();

        if (state.HeaderView is not null && state.HeaderNode is not null)
        {
            ((IElement)state.HeaderView).Handler = null;
            state.HeaderNode.Dispose();
        }
        if (state.FooterView is not null && state.FooterNode is not null)
        {
            ((IElement)state.FooterView).Handler = null;
            state.FooterNode.Dispose();
        }
        state.Node.Dispose();
    }

    private sealed class GroupState
    {
        public GroupState(CollectionViewGroup data)
        {
            Data = data;
            Node = new ArkListItemGroup();
            Adapter = new ArkUINodeAdapter();
        }

        public CollectionViewGroup Data { get; }
        public ArkListItemGroup Node { get; }
        public ArkUINodeAdapter Adapter { get; }
        public List<CollectionViewSlot> ItemSlots { get; } = new();
        public Dictionary<nint, (ArkUINode Node, View View)> Items { get; } = new();
        public ArkUINode? HeaderNode { get; set; }
        public View? HeaderView { get; set; }
        public ArkUINode? FooterNode { get; set; }
        public View? FooterView { get; set; }
    }
}

internal enum CollectionViewSlotKind
{
    Header,
    Item,
    Footer,
    Empty,
}

internal readonly record struct CollectionViewSlot(
    CollectionViewSlotKind Kind,
    object? Context,
    DataTemplate? Template);

internal sealed record CollectionViewGroup(
    object? Group,
    IReadOnlyList<CollectionViewSlot> Slots);
