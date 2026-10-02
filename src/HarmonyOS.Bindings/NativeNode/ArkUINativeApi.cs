// ArkUI 原生节点互操作层（libace_ndk.z.so）。
// ABI 镜像部分（ArkUI_NumberValue 等结构体、ArkUI_NativeNodeAPI_1 函数表、
// OH_ArkUI_* PInvoke）由 tools/arkui-bindgen 生成于 ArkUINativeApi.g.cs，请勿手改；
// 本文件只保留托管包装与懒获取逻辑。
#nullable enable
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.NativeNode;

#pragma warning disable CS8500 // 托管类型取指针（此处均为非托管 blittable 结构）

/// <summary>ArkUI 原生节点互操作层（libace_ndk.z.so）</summary>
internal static unsafe partial class ArkUINativeApi
{
    private const string ArkuiLib = "libace_ndk.z.so";

    /// <summary>ArkUI_NativeAPIVariantKind（native_interface.h）</summary>
    private enum ArkUIVariantKind
    {
        ARKUI_NATIVE_NODE = 0,
        ARKUI_NATIVE_DIALOG = 1,
        ARKUI_NATIVE_GESTURE = 2,
        ARKUI_NATIVE_ANIMATE = 3,
        ARKUI_MULTI_THREAD_NATIVE_NODE = 4,
    }

    [LibraryImport(ArkuiLib)]
    private static unsafe partial IntPtr OH_ArkUI_QueryModuleInterfaceByName(int variantKind, byte* structName);

    private static ArkUI_NativeNodeAPI_1* _nodeApi;

    /// <summary>ArkUI_NativeNodeAPI_1 函数表（懒获取，进程内稳定）</summary>
    internal static ArkUI_NativeNodeAPI_1* Node
    {
        get
        {
            if (_nodeApi == null)
            {
                var name = "ArkUI_NativeNodeAPI_1"u8;
                fixed (byte* p = name)
                {
                    _nodeApi = (ArkUI_NativeNodeAPI_1*)OH_ArkUI_QueryModuleInterfaceByName(
                        (int)ArkUIVariantKind.ARKUI_NATIVE_NODE, p);
                }
                if (_nodeApi == null)
                    throw new InvalidOperationException("ArkUI_NativeNodeAPI_1 is not available (UI context required)");
            }
            return _nodeApi;
        }
    }

    // ───────────────────────── 节点生命周期 ─────────────────────────

    internal static ArkUI_NodeHandle CreateNode(ArkUI_NodeType type)
        => Node->CreateNode(type);

    internal static void DisposeNode(ArkUI_NodeHandle node)
        => Node->DisposeNode(node);

    internal static int AddChild(ArkUI_NodeHandle parent, ArkUI_NodeHandle child)
        => Node->AddChild(parent, child);

    internal static int RemoveChild(ArkUI_NodeHandle parent, ArkUI_NodeHandle child)
        => Node->RemoveChild(parent, child);

    internal static int InsertChildAfter(ArkUI_NodeHandle parent, ArkUI_NodeHandle child, ArkUI_NodeHandle sibling)
        => Node->InsertChildAfter(parent, child, sibling);

    internal static int InsertChildBefore(ArkUI_NodeHandle parent, ArkUI_NodeHandle child, ArkUI_NodeHandle sibling)
        => Node->InsertChildBefore(parent, child, sibling);

    internal static int InsertChildAt(ArkUI_NodeHandle parent, ArkUI_NodeHandle child, int position)
        => Node->InsertChildAt(parent, child, position);

    internal static int RemoveAllChildren(ArkUI_NodeHandle parent)
        => Node->RemoveAllChildren(parent);

    internal static ArkUI_NodeHandle GetParent(ArkUI_NodeHandle node)
        => Node->GetParent(node);

    // ───────────────────────── 属性 ─────────────────────────

    internal static int SetAttribute(ArkUI_NodeHandle node, ArkUI_NodeAttributeType attribute, ArkUI_AttributeItem* item)
        => Node->SetAttribute(node, attribute, item);

    internal static ArkUI_AttributeItem* GetAttribute(ArkUI_NodeHandle node, ArkUI_NodeAttributeType attribute)
        => Node->GetAttribute(node, attribute);

    internal static int ResetAttribute(ArkUI_NodeHandle node, ArkUI_NodeAttributeType attribute)
        => Node->ResetAttribute(node, attribute);

    // ───────────────────────── 事件 ─────────────────────────

    // 原生返回 int32_t 错误码（native_node.h）；镜像曾经声明 void 返回静默吞错，
    // 注册失败（重复注册/节点不支持的类型）必须上浮给调用方
    internal static void RegisterNodeEvent(ArkUI_NodeHandle node, ArkUI_NodeEventType eventType, int targetId, void* userData)
    {
        var status = Node->RegisterNodeEvent(node, eventType, targetId, userData);
        if (status != 0)
            throw new InvalidOperationException($"RegisterNodeEvent({eventType}, targetId={targetId}) failed: {status}");
    }

    internal static void UnregisterNodeEvent(ArkUI_NodeHandle node, ArkUI_NodeEventType eventType)
        => Node->UnregisterNodeEvent(node, eventType);

    internal static void RegisterNodeEventReceiver(delegate* unmanaged<ArkUI_NodeEvent*, void> receiver)
        => Node->RegisterNodeEventReceiver(receiver);

    internal static void UnregisterNodeEventReceiver()
        => Node->UnregisterNodeEventReceiver();

    // ───────────────────────── 布局 ─────────────────────────

    internal static int SetMeasuredSize(ArkUI_NodeHandle node, int width, int height)
        => Node->SetMeasuredSize(node, width, height);

    internal static int SetLayoutPosition(ArkUI_NodeHandle node, int positionX, int positionY)
        => Node->SetLayoutPosition(node, positionX, positionY);

    internal static ArkUI_IntSize GetMeasuredSize(ArkUI_NodeHandle node)
        => Node->GetMeasuredSize(node);

    internal static ArkUI_IntOffset GetLayoutPosition(ArkUI_NodeHandle node)
        => Node->GetLayoutPosition(node);

    internal static int MeasureNode(ArkUI_NodeHandle node, ArkUI_LayoutConstraint* constraint)
        => Node->MeasureNode(node, constraint);

    internal static int LayoutNode(ArkUI_NodeHandle node, int positionX, int positionY)
        => Node->LayoutNode(node, positionX, positionY);

    internal static int SetLengthMetricUnit(ArkUI_NodeHandle node, ArkUI_LengthMetricUnit unit)
        => Node->SetLengthMetricUnit(node, unit);

    // ───────────────────────── 树查询 ─────────────────────────

    internal static uint GetTotalChildCount(ArkUI_NodeHandle node)
        => Node->GetTotalChildCount(node);

    internal static ArkUI_NodeHandle GetChildAt(ArkUI_NodeHandle node, int position)
        => Node->GetChildAt(node, position);

    internal static ArkUI_NodeHandle GetFirstChild(ArkUI_NodeHandle node)
        => Node->GetFirstChild(node);

    internal static ArkUI_NodeHandle GetLastChild(ArkUI_NodeHandle node)
        => Node->GetLastChild(node);

    internal static ArkUI_NodeHandle GetPreviousSibling(ArkUI_NodeHandle node)
        => Node->GetPreviousSibling(node);

    internal static ArkUI_NodeHandle GetNextSibling(ArkUI_NodeHandle node)
        => Node->GetNextSibling(node);

    // ───────────────────────── NodeContent（ArkTS 宿主挂载点）─────────────────────────

    /// <summary>
    /// 将 ArkTS 侧传来的 NodeContent napi_value 转换为 NodeContent 句柄。
    /// env 必须已通过 NapiEnv.Initialize 注入。
    /// </summary>
    internal static IntPtr GetNodeContentFromNapiValue(IntPtr env, IntPtr napiValue)
    {
        var status = OH_ArkUI_GetNodeContentFromNapiValue(env, napiValue, out var content);
        if (status != 0)
            throw new InvalidOperationException($"OH_ArkUI_GetNodeContentFromNapiValue failed: {status}");
        return content;
    }

    internal static void NodeContentAddNode(IntPtr content, ArkUI_NodeHandle node)
    {
        var status = OH_ArkUI_NodeContent_AddNode(content, node);
        if (status != 0)
            throw new InvalidOperationException($"OH_ArkUI_NodeContent_AddNode failed: {status}");
    }

    internal static void NodeContentRemoveNode(IntPtr content, ArkUI_NodeHandle node)
    {
        var status = OH_ArkUI_NodeContent_RemoveNode(content, node);
        if (status != 0)
            throw new InvalidOperationException($"OH_ArkUI_NodeContent_RemoveNode failed: {status}");
    }

    internal static void NodeContentInsertNode(IntPtr content, ArkUI_NodeHandle node, int position)
    {
        var status = OH_ArkUI_NodeContent_InsertNode(content, node, position);
        if (status != 0)
            throw new InvalidOperationException($"OH_ArkUI_NodeContent_InsertNode failed: {status}");
    }

    // ───────────────────────── 事件访问器 ─────────────────────────

    internal static ArkUI_NodeEventType GetEventType(ArkUI_NodeEvent* @event)
        => (ArkUI_NodeEventType)OH_ArkUI_NodeEvent_GetEventType(@event);

    internal static int GetTargetId(ArkUI_NodeEvent* @event)
        => OH_ArkUI_NodeEvent_GetTargetId(@event);

    internal static ArkUI_NodeHandle GetEventNodeHandle(ArkUI_NodeEvent* @event)
        => OH_ArkUI_NodeEvent_GetNodeHandle(@event);

    internal static IntPtr GetInputEvent(ArkUI_NodeEvent* @event)
        => OH_ArkUI_NodeEvent_GetInputEvent(@event);

    internal static IntPtr GetNodeComponentEvent(ArkUI_NodeEvent* @event)
        => OH_ArkUI_NodeEvent_GetNodeComponentEvent(@event);

    internal static IntPtr GetStringAsyncEvent(ArkUI_NodeEvent* @event)
        => OH_ArkUI_NodeEvent_GetStringAsyncEvent(@event);

    internal static IntPtr GetTextChangeEvent(ArkUI_NodeEvent* @event)
        => OH_ArkUI_NodeEvent_GetTextChangeEvent(@event);

    internal static IntPtr GetEventUserData(ArkUI_NodeEvent* @event)
        => OH_ArkUI_NodeEvent_GetUserData(@event);

    /// <summary>读取 NodeComponentEvent 附加数据数组中的第 index 项</summary>
    internal static ArkUI_NumberValue GetEventNumber(ArkUI_NodeEvent* @event, int index)
    {
        ArkUI_NumberValue value = default;
        var status = OH_ArkUI_NodeEvent_GetNumberValue(@event, index, &value);
        if (status != 0)
            throw new InvalidOperationException($"OH_ArkUI_NodeEvent_GetNumberValue({index}) failed: {status}");
        return value;
    }

    // ───────────── 拖拽事件（NODE_ON_DRAG_* / NODE_ON_DROP，drag_and_drop.h）─────────────
    // ArkUI_DragEvent 为不透明指针：private partial 走 void*，internal 包装走 IntPtr

    internal static IntPtr GetDragEvent(ArkUI_NodeEvent* @event)
        => OH_ArkUI_NodeEvent_GetDragEvent(@event);

    internal static int SetNodeDraggable(ArkUI_NodeHandle node, bool enabled)
        => OH_ArkUI_SetNodeDraggable(node, enabled);

    /// <summary>放侧放行任意 UDMF 数据类型（等价 ArkTS allowDrop 全类型；不调用则 NODE_ON_DROP 不触发）</summary>
    internal static int AllowNodeAllDropDataTypes(ArkUI_NodeHandle node)
        => OH_ArkUI_AllowNodeAllDropDataTypes(node);

    internal static int DragEventSetData(IntPtr dragEvent, IntPtr data)
        => OH_ArkUI_DragEvent_SetData((void*)dragEvent, data);

    internal static int DragEventGetUdmfData(IntPtr dragEvent, IntPtr data)
        => OH_ArkUI_DragEvent_GetUdmfData((void*)dragEvent, data);

    internal static (float X, float Y) DragEventTouchPointToWindow(IntPtr dragEvent)
        => (OH_ArkUI_DragEvent_GetTouchPointXToWindow((void*)dragEvent),
            OH_ArkUI_DragEvent_GetTouchPointYToWindow((void*)dragEvent));
}

/// <summary>
/// ArkUI_NodeHandle：8 字节句柄（按值传递时与 C 指针 ABI 一致）
/// </summary>
public readonly struct ArkUI_NodeHandle : IEquatable<ArkUI_NodeHandle>
{
    public readonly IntPtr Handle;
    public bool IsNull => Handle == IntPtr.Zero;

    public ArkUI_NodeHandle(IntPtr handle) => Handle = handle;

    public static implicit operator IntPtr(ArkUI_NodeHandle node) => node.Handle;
    public static implicit operator ArkUI_NodeHandle(IntPtr handle) => new(handle);

    public bool Equals(ArkUI_NodeHandle other) => Handle == other.Handle;
    public override bool Equals(object? obj) => obj is ArkUI_NodeHandle other && Equals(other);
    public override int GetHashCode() => Handle.GetHashCode();
    public override string ToString() => Handle.ToString("X");
}
