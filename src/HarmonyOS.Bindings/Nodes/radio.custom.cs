#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class RadioButton
{
    /// <summary>读回单选状态（NODE_RADIO_CHECKED），用于 native 点击后的 managed 状态兜底同步</summary>
    public bool GetIsChecked()
    {
        var item = ArkUINativeApi.GetAttribute(Handle, ArkUI_NodeAttributeType.NODE_RADIO_CHECKED);
        return item != null && item->value != null && item->size > 0 && item->value[0].i32 == 1;
    }
}
