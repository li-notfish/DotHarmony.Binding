// 手势包装层公共类型：托管友好的枚举镜像 + ArkUI_GestureEvent 只读视图。
// 单位约定（头文件实测）：pan offset/velocity=px，指针坐标=px，pinch center=px，
// swipe angle=deg。全部保留 px（与 Android 手势语义一致，MAUI 生态代码零换算迁移）。
#nullable enable
using System;
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

/// <summary>手势事件动作（ArkUI_GestureEventActionType 位标志的托管镜像）</summary>
[Flags]
public enum ArkGestureAction
{
    /// <summary>识别成功（tap 命中 / pan 开始）</summary>
    Accept = 0x01,
    /// <summary>更新（pan/pinch 移动中）</summary>
    Update = 0x02,
    /// <summary>结束</summary>
    End = 0x04,
    /// <summary>取消（被其它手势抢占/中断）</summary>
    Cancel = 0x08,
}

/// <summary>手势方向（ArkUI_GestureDirection 位掩码镜像）</summary>
[Flags]
public enum ArkGestureDirection
{
    None = 0,
    Left = 0b0001,
    Right = 0b0010,
    Horizontal = Left | Right,
    Up = 0b0100,
    Down = 0b1000,
    Vertical = Up | Down,
    All = Horizontal | Vertical,
}

/// <summary>手势优先级（ArkUI_GesturePriority 镜像）</summary>
public enum ArkGesturePriority
{
    /// <summary>与子组件默认手势按声明顺序竞争</summary>
    Normal = 0,
    /// <summary>高于默认手势</summary>
    Priority = 1,
    /// <summary>并行识别（不与默认手势互斥）</summary>
    Parallel = 2,
}

/// <summary>手势掩码（ArkUI_GestureMask 镜像）</summary>
public enum ArkGestureMask
{
    /// <summary>子组件手势照常识别</summary>
    Normal = 0,
    /// <summary>禁用子组件全部手势（含内建）</summary>
    IgnoreInternal = 1,
}

/// <summary>
/// ArkUI_GestureEvent 的托管只读视图。全部坐标/偏移单位为 px（头文件约定），
/// 每类数据仅在其对应手势类型的事件中有效。
/// PositionX/Y 为相对手势宿主节点（Attach 的节点）左上角的坐标——原生事件自带的
/// GetX/GetNode 语义是"事件节点"（实测为根节点），须以挂载节点窗口偏移换算。
/// </summary>
public readonly unsafe struct ArkUIGestureEvent
{
    private readonly ArkUI_GestureEvent* _ptr;
    private readonly ArkUI_NodeHandle _attachedNode;

    internal ArkUIGestureEvent(ArkUI_GestureEvent* ptr, ArkUI_NodeHandle attachedNode)
    {
        _ptr = ptr;
        _attachedNode = attachedNode;
    }

    /// <summary>本次回调的动作类型</summary>
    public ArkGestureAction Action => (ArkGestureAction)ArkUINativeApi.OH_ArkUI_GestureEvent_GetActionType(_ptr);

    // ── Pan（createPanGesture 手势事件专用，px / px·s⁻¹，相对手势起点）──
    // 注意：实测仅在 UPDATE 动作有效，END 返回 0——累计位移请在托管侧自算
    //（HarmonyGestureManager 以 Accept 位置为原点差值累计）

    public float PanOffsetX => ArkUINativeApi.OH_ArkUI_PanGesture_GetOffsetX(_ptr);
    public float PanOffsetY => ArkUINativeApi.OH_ArkUI_PanGesture_GetOffsetY(_ptr);
    public float PanVelocityX => ArkUINativeApi.OH_ArkUI_PanGesture_GetVelocityX(_ptr);
    public float PanVelocityY => ArkUINativeApi.OH_ArkUI_PanGesture_GetVelocityY(_ptr);
    public float PanVelocity => ArkUINativeApi.OH_ArkUI_PanGesture_GetVelocity(_ptr);

    // ── Swipe（createSwipeGesture 手势事件专用）──

    /// <summary>滑动瞬时方向角（deg，水平为 0，顺时针 0~180 / 逆时针 -180~0）</summary>
    public float SwipeAngle => ArkUINativeApi.OH_ArkUI_SwipeGesture_GetAngle(_ptr);
    public float SwipeVelocity => ArkUINativeApi.OH_ArkUI_SwipeGesture_GetVelocity(_ptr);

    // ── Pinch（createPinchGesture 手势事件专用）──

    /// <summary>相对手势起点的累计缩放系数</summary>
    public float PinchScale => ArkUINativeApi.OH_ArkUI_PinchGesture_GetScale(_ptr);
    /// <summary>捏合中心（px，相对组件左上角）</summary>
    public float PinchCenterX => ArkUINativeApi.OH_ArkUI_PinchGesture_GetCenterX(_ptr);
    public float PinchCenterY => ArkUINativeApi.OH_ArkUI_PinchGesture_GetCenterY(_ptr);

    // ── 指针位置（px，相对挂载节点左上角，原生窗口坐标系；无输入事件时为 0）──
    // 实测（模拟器 API 26）：GetX 与 NodeUtils.GetLayoutPositionInWindow 同属窗口坐标系，
    // 自洽成立；与 dumpLayout/屏幕坐标相差一个窗口原点偏移（avoid-area），跨坐标系
    // 换算属上层职责（delta 类计算——pan 累计位移等——不受影响）。

    public float PositionX
    {
        get
        {
            var raw = ArkUINativeApi.OH_ArkUI_GestureEvent_GetRawInputEvent(_ptr);
            return raw == IntPtr.Zero ? 0f : ArkUINativeApi.OH_ArkUI_PointerEvent_GetX((ArkUI_UIInputEvent*)raw);
        }
    }

    public float PositionY
    {
        get
        {
            var raw = ArkUINativeApi.OH_ArkUI_GestureEvent_GetRawInputEvent(_ptr);
            return raw == IntPtr.Zero ? 0f : ArkUINativeApi.OH_ArkUI_PointerEvent_GetY((ArkUI_UIInputEvent*)raw);
        }
    }
}
