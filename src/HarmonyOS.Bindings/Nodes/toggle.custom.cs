// Toggle 手写扩展：生成器未覆盖的 NODE_TOGGLE_SWITCH_POINT_COLOR
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class Toggle
{
    /// <summary>读回开关状态（NODE_TOGGLE_VALUE），用于 native 点击后的 managed 状态兜底同步</summary>
    public bool GetIsOn()
    {
        var item = ArkUINativeApi.GetAttribute(Handle, ArkUI_NodeAttributeType.NODE_TOGGLE_VALUE);
        return item != null && item->value != null && item->size > 0 && item->value[0].i32 == 1;
    }

    /// <summary>Switch 滑块（圆点）颜色（NODE_TOGGLE_SWITCH_POINT_COLOR，0xAARRGGBB）</summary>
    public void SetSwitchPointColor(uint argb)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TOGGLE_SWITCH_POINT_COLOR, ArkUIValue.U(argb));
}
