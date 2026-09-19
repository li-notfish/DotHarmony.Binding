#nullable enable
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>ArkUI_NodeAdapterEvent（不透明，经访问器读取）</summary>
public struct ArkUI_NodeAdapterEvent { }

/// <summary>
/// ArkUINativeApi NodeAdapter 扩展：List/Swiper/WaterFlow 虚拟化 adapter（native_node.h API 12+）。
/// 流程：Create → SetTotalNodeCount → ReloadAllItems；事件 receiver 按类型分派
/// （ON_GET_NODE_ID → SetNodeId 稳定 id；ON_ADD_NODE_TO_ADAPTER → SetItem 物化子节点，每条目一次；
/// ON_REMOVE_NODE_FROM_ADAPTER → GetRemovedNode 处置）。adapter 经 NODE_LIST_NODE_ADAPTER
/// 属性（item.@object）挂到 List 节点。
/// </summary>
internal static unsafe partial class ArkUINativeApi
{
    [LibraryImport(ArkuiLib)]
    private static partial IntPtr OH_ArkUI_NodeAdapter_Create();

    [LibraryImport(ArkuiLib)]
    private static partial void OH_ArkUI_NodeAdapter_Dispose(IntPtr handle);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeAdapter_SetTotalNodeCount(IntPtr handle, uint size);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeAdapter_RegisterEventReceiver(
        IntPtr handle, IntPtr userData, delegate* unmanaged<ArkUI_NodeAdapterEvent*, void> receiver);

    [LibraryImport(ArkuiLib)]
    private static partial void OH_ArkUI_NodeAdapter_UnregisterEventReceiver(IntPtr handle);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeAdapter_ReloadAllItems(IntPtr handle);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeAdapter_ReloadItem(IntPtr handle, uint startPosition, uint itemCount);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeAdapter_RemoveItem(IntPtr handle, uint startPosition, uint itemCount);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeAdapter_InsertItem(IntPtr handle, uint startPosition, uint itemCount);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeAdapterEvent_SetItem(ArkUI_NodeAdapterEvent* @event, IntPtr node);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeAdapterEvent_SetNodeId(ArkUI_NodeAdapterEvent* @event, int id);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeAdapterEvent_GetType(ArkUI_NodeAdapterEvent* @event);

    [LibraryImport(ArkuiLib)]
    private static partial IntPtr OH_ArkUI_NodeAdapterEvent_GetRemovedNode(ArkUI_NodeAdapterEvent* @event);

    [LibraryImport(ArkuiLib)]
    private static partial uint OH_ArkUI_NodeAdapterEvent_GetItemIndex(ArkUI_NodeAdapterEvent* @event);

    [LibraryImport(ArkuiLib)]
    private static partial IntPtr OH_ArkUI_NodeAdapterEvent_GetUserData(ArkUI_NodeAdapterEvent* @event);

    internal static IntPtr NodeAdapterCreate() => OH_ArkUI_NodeAdapter_Create();

    internal static void NodeAdapterDispose(IntPtr handle) => OH_ArkUI_NodeAdapter_Dispose(handle);

    internal static int NodeAdapterSetTotalCount(IntPtr handle, int count)
        => OH_ArkUI_NodeAdapter_SetTotalNodeCount(handle, (uint)count);

    internal static int NodeAdapterRegisterEventReceiver(
        IntPtr handle, IntPtr userData, delegate* unmanaged<ArkUI_NodeAdapterEvent*, void> receiver)
        => OH_ArkUI_NodeAdapter_RegisterEventReceiver(handle, userData, receiver);

    internal static void NodeAdapterUnregisterEventReceiver(IntPtr handle)
        => OH_ArkUI_NodeAdapter_UnregisterEventReceiver(handle);

    internal static int NodeAdapterReloadAllItems(IntPtr handle) => OH_ArkUI_NodeAdapter_ReloadAllItems(handle);

    internal static int NodeAdapterReloadItem(IntPtr handle, int start, int count)
        => OH_ArkUI_NodeAdapter_ReloadItem(handle, (uint)start, (uint)count);

    internal static int NodeAdapterRemoveItem(IntPtr handle, int start, int count)
        => OH_ArkUI_NodeAdapter_RemoveItem(handle, (uint)start, (uint)count);

    internal static int NodeAdapterInsertItem(IntPtr handle, int start, int count)
        => OH_ArkUI_NodeAdapter_InsertItem(handle, (uint)start, (uint)count);

    internal static int NodeAdapterEventSetItem(ArkUI_NodeAdapterEvent* @event, IntPtr node)
        => OH_ArkUI_NodeAdapterEvent_SetItem(@event, node);

    internal static int NodeAdapterEventSetNodeId(ArkUI_NodeAdapterEvent* @event, int id)
        => OH_ArkUI_NodeAdapterEvent_SetNodeId(@event, id);

    internal static ArkUI_NodeAdapterEventType NodeAdapterEventGetType(ArkUI_NodeAdapterEvent* @event)
        => (ArkUI_NodeAdapterEventType)OH_ArkUI_NodeAdapterEvent_GetType(@event);

    internal static IntPtr NodeAdapterEventGetRemovedNode(ArkUI_NodeAdapterEvent* @event)
        => OH_ArkUI_NodeAdapterEvent_GetRemovedNode(@event);

    internal static int NodeAdapterEventGetItemIndex(ArkUI_NodeAdapterEvent* @event)
        => (int)OH_ArkUI_NodeAdapterEvent_GetItemIndex(@event);

    internal static IntPtr NodeAdapterEventGetUserData(ArkUI_NodeAdapterEvent* @event)
        => OH_ArkUI_NodeAdapterEvent_GetUserData(@event);
}

/// <summary>
/// ArkUI NodeAdapter 包装类：List 等滚动容器的虚拟化适配器。
/// 物化回调经 GCHandle + 单一 [UnmanagedCallersOnly] 跳板（同 NodeEventBus 模式）；
/// 拆除时 UnregisterEventReceiver + Dispose。
/// </summary>
public sealed unsafe class ArkUINodeAdapter : IDisposable
{
    private readonly IntPtr _handle;
    private Action<ArkUI_NodeAdapterEventView>? _receiver;
    private GCHandle _self;
    private bool _registered;

    public ArkUINodeAdapter() => _handle = ArkUINativeApi.NodeAdapterCreate();

    internal IntPtr Handle => _handle;

    /// <summary>注册物化回调（覆盖式；传 null 注销）。回调在 UI 线程执行，禁止分配重对象。</summary>
    public void SetEventReceiver(Action<ArkUI_NodeAdapterEventView>? receiver)
    {
        _receiver = receiver;
        if (receiver != null && !_registered)
        {
            _self = GCHandle.Alloc(this);
            _registered = true;
            ArkUINativeApi.NodeAdapterRegisterEventReceiver(_handle, GCHandle.ToIntPtr(_self), &Trampoline);
        }
        else if (receiver == null && _registered)
        {
            ArkUINativeApi.NodeAdapterUnregisterEventReceiver(_handle);
            _self.Free();
            _registered = false;
        }
    }

    /// <summary>设置条目总数（下一次 Reload 生效；负数直接拒绝——(uint) 隐式收缩会把 -1 绕成天量）</summary>
    public void SetTotalCount(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArkUINativeApi.NodeAdapterSetTotalCount(_handle, count);
    }

    /// <summary>全量重载（框架按可见范围物化，虚拟化核心）</summary>
    public void ReloadAllItems() => ArkUINativeApi.NodeAdapterReloadAllItems(_handle);

    [UnmanagedCallersOnly]
    private static void Trampoline(ArkUI_NodeAdapterEvent* eventPtr)
    {
        try
        {
            // GetUserData 返回注册时透传的 userData（即本实例 GCHandle）
            var adapter = GCHandle.FromIntPtr(ArkUINativeApi.NodeAdapterEventGetUserData(eventPtr)).Target
                as ArkUINodeAdapter;
            adapter?.DispatchEvent(new ArkUI_NodeAdapterEventView(eventPtr));
        }
        catch (Exception ex)
        {
            Runtime.HiLog.Error("HarmonyHost", $"[NodeAdapter] dispatch error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void DispatchEvent(ArkUI_NodeAdapterEventView ev) => _receiver?.Invoke(ev);

    public void Dispose()
    {
        if (_registered)
        {
            ArkUINativeApi.NodeAdapterUnregisterEventReceiver(_handle);
            _self.Free();
            _registered = false;
        }
        if (_handle != IntPtr.Zero)
            ArkUINativeApi.NodeAdapterDispose(_handle);
    }
}

/// <summary>ArkUI_NodeAdapterEvent 的托管只读视图（adapter 物化事件载荷）</summary>
public readonly unsafe struct ArkUI_NodeAdapterEventView
{
    private readonly ArkUI_NodeAdapterEvent* _ptr;

    internal ArkUI_NodeAdapterEventView(ArkUI_NodeAdapterEvent* ptr) => _ptr = ptr;

    /// <summary>adapter 事件类型</summary>
    public ArkUI_NodeAdapterEventType Type
        => ArkUINativeApi.NodeAdapterEventGetType(_ptr);

    /// <summary>操作条目的索引（ON_GET_NODE_ID / ON_ADD 的条目位置）</summary>
    public int ItemIndex => ArkUINativeApi.NodeAdapterEventGetItemIndex(_ptr);

    /// <summary>被移除的节点（ON_REMOVE_NODE_FROM_ADAPTER；已从列表摘除，等待处置）</summary>
    public IntPtr RemovedNode => ArkUINativeApi.NodeAdapterEventGetRemovedNode(_ptr);

    /// <summary>把物化好的节点交给 adapter（ON_ADD；status != 0 时抛出）</summary>
    public void SetItem(IntPtr node)
    {
        var status = ArkUINativeApi.NodeAdapterEventSetItem(_ptr, node);
        if (status != 0)
            throw new InvalidOperationException($"NodeAdapterEvent SetItem failed: {status}");
    }

    /// <summary>设置条目稳定 id（ON_GET_NODE_ID；status != 0 时抛出）</summary>
    public void SetNodeId(int id)
    {
        var status = ArkUINativeApi.NodeAdapterEventSetNodeId(_ptr, id);
        if (status != 0)
            throw new InvalidOperationException($"NodeAdapterEvent SetNodeId failed: {status}");
    }
}
