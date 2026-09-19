// ArkUI_UIInputEvent 的公共只读包装。HarmonyOS.Maui 等外部程序集拿不到
// ArkUINativeApi internals，经 ArkUINodeEvent.InputEvent（IntPtr）→ 本包装读取。
// 坐标单位：**vp**（相对组件左上角；ui_input_event.h 标注 px 系笔误——模拟器 API 26
// 实测 (displayX−节点窗口偏移px)/3.5 == GetX，即 vp）。
#nullable enable
using System;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>触摸动作（UI_TOUCH_EVENT_ACTION 镜像）</summary>
public enum ArkPointerTouchAction
{
    Canceled = 0,
    Pressed = 1,
    Moved = 2,
    Released = 3,
}

/// <summary>输入源（UI_INPUT_EVENT_SOURCE_TYPE 镜像）</summary>
public enum ArkPointerSource
{
    Unknown = 0,
    Mouse = 1,
    TouchScreen = 2,
}

/// <summary>ArkUI_UIInputEvent 只读视图（指针事件坐标/动作）</summary>
public readonly unsafe struct ArkUIPointerEvent
{
    private readonly ArkUI_UIInputEvent* _ptr;

    private ArkUIPointerEvent(ArkUI_UIInputEvent* ptr) => _ptr = ptr;

    /// <summary>从 ArkUINodeEvent.InputEvent 构造；IntPtr.Zero 得到 IsNull 视图</summary>
    public static ArkUIPointerEvent From(IntPtr inputEvent)
        => inputEvent == IntPtr.Zero ? default : new ArkUIPointerEvent((ArkUI_UIInputEvent*)inputEvent);

    public bool IsNull => _ptr == null;

    /// <summary>原始动作值（touch 按 ArkPointerTouchAction，mouse 按 UI_MOUSE_EVENT_ACTION 语义）</summary>
    public int RawAction => ArkUINativeApi.OH_ArkUI_UIInputEvent_GetAction(_ptr);

    /// <summary>触摸动作语义（touch 事件用；mouse 事件请读 RawAction）</summary>
    public ArkPointerTouchAction TouchAction => (ArkPointerTouchAction)ArkUINativeApi.OH_ArkUI_UIInputEvent_GetAction(_ptr);

    public ArkPointerSource Source => (ArkPointerSource)ArkUINativeApi.OH_ArkUI_UIInputEvent_GetSourceType(_ptr);

    /// <summary>X（vp，相对组件左上角）</summary>
    public float X => ArkUINativeApi.OH_ArkUI_PointerEvent_GetX(_ptr);

    /// <summary>Y（vp，相对组件左上角）</summary>
    public float Y => ArkUINativeApi.OH_ArkUI_PointerEvent_GetY(_ptr);

    public uint PointerCount => ArkUINativeApi.OH_ArkUI_PointerEvent_GetPointerCount(_ptr);

    /// <summary>按压压力（0~1）</summary>
    public float Pressure => ArkUINativeApi.OH_ArkUI_PointerEvent_GetPressure(_ptr, 0);
}
