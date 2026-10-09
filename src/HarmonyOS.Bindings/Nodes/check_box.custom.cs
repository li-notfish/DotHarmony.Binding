// CheckBox 手写扩展：生成器未覆盖的 NODE_CHECKBOX_SELECT_COLOR
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class CheckBox
{
    /// <summary>读回复选状态（NODE_CHECKBOX_SELECT），用于 native 点击后的 managed 状态兜底同步</summary>
    public bool GetIsChecked()
    {
        var item = ArkUINativeApi.GetAttribute(Handle, ArkUI_NodeAttributeType.NODE_CHECKBOX_SELECT);
        return item != null && item->value != null && item->size > 0 && item->value[0].i32 == 1;
    }

    /// <summary>选中态颜色（NODE_CHECKBOX_SELECT_COLOR，0xAARRGGBB）</summary>
    public void SetSelectColor(uint argb)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_CHECKBOX_SELECT_COLOR, ArkUIValue.U(argb));
}
