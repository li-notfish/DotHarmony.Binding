#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class LoadingProgress : ArkUINodeBase
{
    public LoadingProgress() : base(ArkUI_NodeType.ARKUI_NODE_LOADING_PROGRESS) { }
}
