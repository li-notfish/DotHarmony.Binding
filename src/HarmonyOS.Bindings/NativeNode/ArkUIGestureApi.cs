// ArkUI_NativeGestureAPI_1 镜像（native_gesture.h，API 12+）
// 手势识别原语：tap/longpress/pan/pinch/rotation/swipe/组手势 + addGestureToNode。
// 维护约定（同 ArkUINativeApi.cs / ArkUIAnimateApi.cs）：仅枚举手工维护（源头
// native_gesture.h / ui_input_event.h）；结构体与函数表镜像手工维护、成员顺序
// 逐项对照头文件（native_gesture.h 1253 行起，version 字段为首个成员），禁止重排。
#nullable enable
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.NativeNode;

#pragma warning disable CS8500

internal static unsafe partial class ArkUINativeApi
{
    private static ArkUI_NativeGestureAPI_1* _gestureApi;

    /// <summary>ArkUI_NativeGestureAPI_1 函数表（懒获取，进程内稳定）</summary>
    internal static ArkUI_NativeGestureAPI_1* Gesture
    {
        get
        {
            if (_gestureApi == null)
            {
                var name = "ArkUI_NativeGestureAPI_1"u8;
                fixed (byte* p = name)
                {
                    _gestureApi = (ArkUI_NativeGestureAPI_1*)OH_ArkUI_QueryModuleInterfaceByName(
                        (int)ArkUIVariantKind.ARKUI_NATIVE_GESTURE, p);
                }
                if (_gestureApi == null)
                    throw new InvalidOperationException("ArkUI_NativeGestureAPI_1 is not available (UI context required)");
            }
            return _gestureApi;
        }
    }

    // ───────────────────────── 手势事件读取（native_gesture.h 自由函数） ─────────────────────────

    [LibraryImport(ArkuiLib)]
    internal static partial int OH_ArkUI_GestureEvent_GetActionType(ArkUI_GestureEvent* @event);

    /// <summary>取手势事件背后的原始输入事件（ui_input_event.h 读取器适用；vp 坐标）</summary>
    [LibraryImport(ArkuiLib)]
    internal static partial IntPtr OH_ArkUI_GestureEvent_GetRawInputEvent(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial ArkUI_NodeHandle OH_ArkUI_GestureEvent_GetNode(ArkUI_GestureEvent* @event);

    /// <summary>节点相对窗口的偏移（px，native_node.h NodeUtils @since 12）——手势事件坐标换算为视图相对坐标用</summary>
    [LibraryImport(ArkuiLib)]
    internal static partial int OH_ArkUI_NodeUtils_GetLayoutPositionInWindow(ArkUI_NodeHandle node, ArkUI_IntOffset* globalOffset);

    // Pan（offset 单位 px，velocity 单位 px/s）
    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PanGesture_GetVelocity(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PanGesture_GetVelocityX(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PanGesture_GetVelocityY(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PanGesture_GetOffsetX(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PanGesture_GetOffsetY(ArkUI_GestureEvent* @event);

    // Swipe / Rotation / Pinch
    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_SwipeGesture_GetAngle(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_SwipeGesture_GetVelocity(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_RotationGesture_GetAngle(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PinchGesture_GetScale(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PinchGesture_GetCenterX(ArkUI_GestureEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PinchGesture_GetCenterY(ArkUI_GestureEvent* @event);

    // ───────────────────────── 原始输入事件读取（ui_input_event.h） ─────────────────────────

    [LibraryImport(ArkuiLib)]
    internal static partial int OH_ArkUI_UIInputEvent_GetType(ArkUI_UIInputEvent* @event);

    /// <summary>touch → UI_TouchEventAction；mouse → UI_MouseEventAction（见各自枚举）</summary>
    [LibraryImport(ArkuiLib)]
    internal static partial int OH_ArkUI_UIInputEvent_GetAction(ArkUI_UIInputEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial int OH_ArkUI_UIInputEvent_GetSourceType(ArkUI_UIInputEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial uint OH_ArkUI_PointerEvent_GetPointerCount(ArkUI_UIInputEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial int OH_ArkUI_PointerEvent_GetPointerId(ArkUI_UIInputEvent* @event, uint pointerIndex);

    /// <summary>视图（目标节点）相对坐标，vp</summary>
    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PointerEvent_GetX(ArkUI_UIInputEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PointerEvent_GetY(ArkUI_UIInputEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PointerEvent_GetXByIndex(ArkUI_UIInputEvent* @event, uint pointerIndex);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PointerEvent_GetYByIndex(ArkUI_UIInputEvent* @event, uint pointerIndex);

    /// <summary>窗口相对坐标，vp</summary>
    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PointerEvent_GetWindowX(ArkUI_UIInputEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PointerEvent_GetWindowY(ArkUI_UIInputEvent* @event);

    /// <summary>屏幕相对坐标，px（ui_input_event.h）</summary>
    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PointerEvent_GetDisplayX(ArkUI_UIInputEvent* @event);

    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PointerEvent_GetDisplayY(ArkUI_UIInputEvent* @event);

    /// <summary>按压压力（0~1），不支持时返回 0</summary>
    [LibraryImport(ArkuiLib)]
    internal static partial float OH_ArkUI_PointerEvent_GetPressure(ArkUI_UIInputEvent* @event, uint pointerIndex);
}

// ───────────────────────── 枚举（native_gesture.h，顺序与值逐项对照头文件） ─────────────────────────

/// <summary>ArkUI_GestureEventActionType（native_gesture.h ~41 行）：手势事件动作位</summary>
[Flags]
internal enum ArkUI_GestureEventActionType
{
    Accept = 0x01,
    Update = 0x02,
    End = 0x04,
    Cancel = 0x08,
}

/// <summary>ArkUI_GesturePriority（native_gesture.h ~112 行）</summary>
internal enum ArkUI_GesturePriority
{
    Normal = 0,
    Priority = 1,
    Parallel = 2,
}

/// <summary>ArkUI_GroupGestureMode（native_gesture.h ~136 行）</summary>
internal enum ArkUI_GroupGestureMode
{
    SequentialGroup = 0,
    ParallelGroup = 1,
    ExclusiveGroup = 2,
}

/// <summary>ArkUI_GestureDirection（native_gesture.h ~182 行，位掩码）</summary>
[Flags]
internal enum ArkUI_GestureDirection
{
    None = 0,
    Left = 0b0001,
    Right = 0b0010,
    Horizontal = Left | Right,          // 0b0011
    Up = 0b0100,
    Down = 0b1000,
    Vertical = Up | Down,               // 0b1100
    All = Horizontal | Vertical,        // 0b1111
}

/// <summary>ArkUI_GestureMask（native_gesture.h ~236 行）</summary>
internal enum ArkUI_GestureMask
{
    NormalGestureMask = 0,
    IgnoreInternalGestureMask = 1,
}

/// <summary>ArkUI_GestureRecognizerType（native_gesture.h ~262 行）</summary>
internal enum ArkUI_GestureRecognizerType
{
    Tap = 0,
    LongPress = 1,
    Pan = 2,
    Pinch = 3,
    Rotation = 4,
    Swipe = 5,
    Group = 6,
    /// <summary>@since 20</summary>
    Click = 7,
    /// <summary>@since 20</summary>
    DragDrop = 8,
}

/// <summary>ArkUI_GestureInterruptResult（native_gesture.h ~314 行）</summary>
internal enum ArkUI_GestureInterruptResult
{
    Continue = 0,
    Reject = 1,
}

/// <summary>ArkUI_GestureRecognizerState（native_gesture.h ~330 行）</summary>
internal enum ArkUI_GestureRecognizerState
{
    Ready = 0,
    Detecting = 1,
    Pending = 2,
    Blocked = 3,
    Successful = 4,
    Failed = 5,
}

// ───────────────────────── 枚举（ui_input_event.h） ─────────────────────────

/// <summary>ArkUI_UIInputEvent_Type（ui_input_event.h ~100 行）</summary>
internal enum ArkUI_UIInputEventType
{
    Unknown = 0,
    Touch = 1,
    Axis = 2,
    Mouse = 3,
    /// <summary>@since 20</summary>
    Key = 4,
    /// <summary>@since 24</summary>
    DigitalCrown = 5,
}

/// <summary>UI_TOUCH_EVENT_ACTION（ui_input_event.h ~121 行）</summary>
internal enum UI_TouchEventAction
{
    Cancel = 0,
    Down = 1,
    Move = 2,
    Up = 3,
}

/// <summary>UI_INPUT_EVENT_SOURCE_TYPE（ui_input_event.h ~160 行）</summary>
internal enum UI_InputEventSourceType
{
    Unknown = 0,
    Mouse = 1,
    TouchScreen = 2,
    /// <summary>@since 20</summary>
    Key = 4,
    /// <summary>@since 20</summary>
    Joystick = 5,
}

/// <summary>UI_MOUSE_EVENT_ACTION（ui_input_event.h ~236 行，匿名 enum）</summary>
internal enum UI_MouseEventAction
{
    Unknown = 0,
    Press = 1,
    Release = 2,
    Move = 3,
    /// <summary>@since 18</summary>
    Cancel = 13,
}

/// <summary>UI_MOUSE_EVENT_BUTTON（ui_input_event.h ~258 行，匿名 enum）</summary>
internal enum UI_MouseEventButton
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 3,
    /// <summary>@since 18</summary>
    Back = 4,
    /// <summary>@since 18</summary>
    Forward = 5,
}

// ───────────────────────── 不透明句柄/事件类型 ─────────────────────────

/// <summary>ArkUI_GestureRecognizer：8 字节句柄（按值传递时与 C 指针 ABI 一致）</summary>
internal readonly struct ArkUI_GestureRecognizer : IEquatable<ArkUI_GestureRecognizer>
{
    public readonly IntPtr Handle;
    public bool IsNull => Handle == IntPtr.Zero;

    public ArkUI_GestureRecognizer(IntPtr handle) => Handle = handle;

    public static implicit operator IntPtr(ArkUI_GestureRecognizer g) => g.Handle;
    public static implicit operator ArkUI_GestureRecognizer(IntPtr handle) => new(handle);

    public bool Equals(ArkUI_GestureRecognizer other) => Handle == other.Handle;
    public override bool Equals(object? obj) => obj is ArkUI_GestureRecognizer other && Equals(other);
    public override int GetHashCode() => Handle.GetHashCode();
    public override string ToString() => Handle.ToString("X");
}

/// <summary>ArkUI_GestureEvent：不透明事件（内容经 OH_ArkUI_* 自由函数读取）</summary>
internal struct ArkUI_GestureEvent { }

/// <summary>ArkUI_UIInputEvent：不透明输入事件（内容经 ui_input_event.h 的 OH_ArkUI_* 自由函数读取）</summary>
internal struct ArkUI_UIInputEvent { }

/// <summary>ArkUI_GestureInterruptInfo：不透明中断信息（本轮未用，占位保 ABI）</summary>
internal struct ArkUI_GestureInterruptInfo { }

/// <summary>ArkUI_ParallelInnerGestureEvent：不透明并行手势事件（本轮未用，占位保 ABI）</summary>
internal struct ArkUI_ParallelInnerGestureEvent { }

// ───────────────────────── 函数表镜像 ─────────────────────────

/// <summary>
/// ArkUI_NativeGestureAPI_1 函数表镜像（native_gesture.h ~1253 行，@since 12）。
/// 成员顺序与头文件逐项对应（首个成员为 version），禁止重排。
/// C 侧 bool 在 unmanaged 函数指针中按 1 字节处理（显式用 byte 表达）。
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ArkUI_NativeGestureAPI_1
{
    public int version;
    public delegate* unmanaged<int, int, ArkUI_GestureRecognizer> createTapGesture;
    public delegate* unmanaged<int, byte, int, ArkUI_GestureRecognizer> createLongPressGesture;
    public delegate* unmanaged<int, ArkUI_GestureDirection, double, ArkUI_GestureRecognizer> createPanGesture;
    public delegate* unmanaged<int, double, ArkUI_GestureRecognizer> createPinchGesture;
    public delegate* unmanaged<int, double, ArkUI_GestureRecognizer> createRotationGesture;
    public delegate* unmanaged<int, ArkUI_GestureDirection, double, ArkUI_GestureRecognizer> createSwipeGesture;
    public delegate* unmanaged<ArkUI_GroupGestureMode, ArkUI_GestureRecognizer> createGroupGesture;
    public delegate* unmanaged<ArkUI_GestureRecognizer, void> dispose;
    public delegate* unmanaged<ArkUI_GestureRecognizer, ArkUI_GestureRecognizer, int> addChildGesture;
    public delegate* unmanaged<ArkUI_GestureRecognizer, ArkUI_GestureRecognizer, int> removeChildGesture;
    public delegate* unmanaged<ArkUI_GestureRecognizer, ArkUI_GestureEventActionType, void*,
        delegate* unmanaged<ArkUI_GestureEvent*, void*, void>, int> setGestureEventTarget;
    public delegate* unmanaged<ArkUI_NodeHandle, ArkUI_GestureRecognizer, ArkUI_GesturePriority, ArkUI_GestureMask, int> addGestureToNode;
    public delegate* unmanaged<ArkUI_NodeHandle, ArkUI_GestureRecognizer, int> removeGestureFromNode;
    public delegate* unmanaged<ArkUI_NodeHandle, delegate* unmanaged<ArkUI_GestureInterruptInfo*, ArkUI_GestureInterruptResult>, int> setGestureInterrupterToNode;
    public delegate* unmanaged<ArkUI_GestureRecognizer, ArkUI_GestureRecognizerType> getGestureType;
    public delegate* unmanaged<ArkUI_NodeHandle, void*, delegate* unmanaged<ArkUI_ParallelInnerGestureEvent*, ArkUI_GestureRecognizer>, int> setInnerGestureParallelTo;
    public delegate* unmanaged<int, int, double, ArkUI_GestureRecognizer> createTapGestureWithDistanceThreshold;
}
