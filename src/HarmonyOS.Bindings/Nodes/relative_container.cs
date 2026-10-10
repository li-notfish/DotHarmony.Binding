#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class RelativeContainer : ArkUINodeBase
{
    public RelativeContainer() : base(ArkUI_NodeType.ARKUI_NODE_RELATIVE_CONTAINER) { }
}
