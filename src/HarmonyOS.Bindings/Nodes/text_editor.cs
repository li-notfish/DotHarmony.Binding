#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class TextEditor : ArkUINodeBase
{
    public TextEditor() : base(ArkUI_NodeType.ARKUI_NODE_TEXT_EDITOR) { }
}
