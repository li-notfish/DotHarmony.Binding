// Toggle 手写扩展：生成器未覆盖的 NODE_TOGGLE_SWITCH_POINT_COLOR
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class Toggle
{
    /// <summary>Switch 滑块（圆点）颜色（NODE_TOGGLE_SWITCH_POINT_COLOR，0xAARRGGBB）</summary>
    public void SetSwitchPointColor(uint argb)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TOGGLE_SWITCH_POINT_COLOR, ArkUIValue.U(argb));
}
