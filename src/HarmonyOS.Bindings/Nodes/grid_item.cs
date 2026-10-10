#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class GridItem : ArkUINodeBase
{
    public GridItem() : base(ArkUI_NodeType.ARKUI_NODE_GRID_ITEM) { }
}
