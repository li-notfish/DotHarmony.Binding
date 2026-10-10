#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class FlowItem : ArkUINodeBase
{
    public FlowItem() : base(ArkUI_NodeType.ARKUI_NODE_FLOW_ITEM) { }
}
