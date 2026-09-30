// CheckBox 手写扩展：生成器未覆盖的 NODE_CHECKBOX_SELECT_COLOR
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class CheckBox
{
    /// <summary>选中态颜色（NODE_CHECKBOX_SELECT_COLOR，0xAARRGGBB）</summary>
    public void SetSelectColor(uint argb)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_CHECKBOX_SELECT_COLOR, ArkUIValue.U(argb));
}
