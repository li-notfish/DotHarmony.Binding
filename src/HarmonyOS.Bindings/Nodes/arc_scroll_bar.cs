#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class ArcScrollBar : ArkUINodeBase
{
    public ArcScrollBar() : base(ArkUI_NodeType.ARKUI_NODE_ARC_SCROLL_BAR) { }
}
