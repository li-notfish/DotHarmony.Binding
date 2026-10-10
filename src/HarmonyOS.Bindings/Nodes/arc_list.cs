#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class ArcList : ArkUINodeBase
{
    public ArcList() : base(ArkUI_NodeType.ARKUI_NODE_ARC_LIST) { }
}
