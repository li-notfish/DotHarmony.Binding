#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class WaterFlow : ArkUINodeBase
{
    public WaterFlow() : base(ArkUI_NodeType.ARKUI_NODE_WATER_FLOW) { }
}
