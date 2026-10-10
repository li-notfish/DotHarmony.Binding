#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class CheckBoxGroup : ArkUINodeBase
{
    public CheckBoxGroup() : base(ArkUI_NodeType.ARKUI_NODE_CHECKBOX_GROUP) { }
}
