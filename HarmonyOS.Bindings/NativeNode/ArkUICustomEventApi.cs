#nullable enable
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>
/// ArkUINativeApi 自绘节点扩展：RegisterNodeCustomEvent 系列（native_node.h API 11+）、
/// markDirty、ArkUI_LayoutConstraint 访问器与 ArkUI_NodeCustomEvent 访问器。
/// </summary>
internal static unsafe partial class ArkUINativeApi
{
    // ───────────────────────── 自绘节点事件（函数表后段）─────────────────────────

    internal static void RegisterNodeCustomEvent(ArkUI_NodeHandle node, ArkUI_NodeCustomEventType eventType, int targetId, void* userData)
        => Node->RegisterNodeCustomEvent(node, (int)eventType, targetId, userData);

    internal static void UnregisterNodeCustomEvent(ArkUI_NodeHandle node, ArkUI_NodeCustomEventType eventType)
        => Node->UnregisterNodeCustomEvent(node, (int)eventType);

    internal static void RegisterCustomEventReceiver(delegate* unmanaged<IntPtr, void> receiver)
        => Node->RegisterNodeCustomEventReceiver(receiver);

    internal static void UnregisterCustomEventReceiver()
        => Node->UnregisterNodeCustomEventReceiver();

    internal static void AddNodeCustomEventReceiver(ArkUI_NodeHandle node, delegate* unmanaged<IntPtr, void> receiver)
        => Node->AddNodeCustomEventReceiver(node, receiver);

    internal static void RemoveNodeCustomEventReceiver(ArkUI_NodeHandle node, delegate* unmanaged<IntPtr, void> receiver)
        => Node->RemoveNodeCustomEventReceiver(node, receiver);

    // ───────────────────────── 自绘事件访问器（native_node.h，@since 12）─────────────────────────

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeCustomEvent_GetEventType(ArkUI_NodeCustomEvent* @event);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_NodeCustomEvent_GetEventTargetId(ArkUI_NodeCustomEvent* @event);

    [LibraryImport(ArkuiLib)]
    private static partial IntPtr OH_ArkUI_NodeCustomEvent_GetUserData(ArkUI_NodeCustomEvent* @event);

    [LibraryImport(ArkuiLib)]
    private static partial IntPtr OH_ArkUI_NodeCustomEvent_GetDrawContextInDraw(ArkUI_NodeCustomEvent* @event);

    [LibraryImport(ArkuiLib)]
    private static partial ArkUI_LayoutConstraint* OH_ArkUI_NodeCustomEvent_GetLayoutConstraintInMeasure(
        ArkUI_NodeCustomEvent* @event);

    internal static int NodeCustomEventGetEventType(ArkUI_NodeCustomEvent* @event)
        => OH_ArkUI_NodeCustomEvent_GetEventType(@event);

    internal static int NodeCustomEventGetEventTargetId(ArkUI_NodeCustomEvent* @event)
        => OH_ArkUI_NodeCustomEvent_GetEventTargetId(@event);

    internal static IntPtr NodeCustomEventGetUserData(ArkUI_NodeCustomEvent* @event)
        => OH_ArkUI_NodeCustomEvent_GetUserData(@event);

    /// <summary>NODE_ON_DRAW 的绘制上下文（ArkUI_DrawContext*）</summary>
    internal static IntPtr NodeCustomEventGetDrawContextInDraw(ArkUI_NodeCustomEvent* @event)
        => OH_ArkUI_NodeCustomEvent_GetDrawContextInDraw(@event);

    /// <summary>NODE_ON_MEASURE 的布局约束</summary>
    internal static ArkUI_LayoutConstraint* NodeCustomEventGetLayoutConstraintInMeasure(ArkUI_NodeCustomEvent* @event)
        => OH_ArkUI_NodeCustomEvent_GetLayoutConstraintInMeasure(@event);

    // ───────────────────────── markDirty / 布局约束与绘制上下文访问器 ─────────────────────────

    internal static int MarkDirty(ArkUI_NodeHandle node, ArkUI_NodeDirtyFlag flags)
        => Node->MarkDirty(node, flags);

    // native_type.h：约束最大/最小尺寸为 int（px 域）

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_LayoutConstraint_GetMaxWidth(ArkUI_LayoutConstraint* constraint);

    [LibraryImport(ArkuiLib)]
    private static partial int OH_ArkUI_LayoutConstraint_GetMaxHeight(ArkUI_LayoutConstraint* constraint);

    internal static int ConstraintMaxWidth(ArkUI_LayoutConstraint* constraint)
        => OH_ArkUI_LayoutConstraint_GetMaxWidth(constraint);

    internal static int ConstraintMaxHeight(ArkUI_LayoutConstraint* constraint)
        => OH_ArkUI_LayoutConstraint_GetMaxHeight(constraint);

    // native_type.h：ArkUI_DrawContext 不透明，经访问器读取画布与绘制区尺寸

    [LibraryImport(ArkuiLib)]
    private static partial IntPtr OH_ArkUI_DrawContext_GetCanvas(IntPtr context);

    [LibraryImport(ArkuiLib)]
    private static partial ArkUI_IntSize OH_ArkUI_DrawContext_GetSize(IntPtr context);

    internal static IntPtr DrawContextGetCanvas(IntPtr context)
        => OH_ArkUI_DrawContext_GetCanvas(context);

    internal static ArkUI_IntSize DrawContextGetSize(IntPtr context)
        => OH_ArkUI_DrawContext_GetSize(context);
}