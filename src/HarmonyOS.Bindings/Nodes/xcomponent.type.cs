#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class XComponent
{
    internal static ArkUI_NodeType ToNodeType(ArkUI_XComponentType type) => type switch
    {
        ArkUI_XComponentType.ARKUI_XCOMPONENT_TYPE_SURFACE => ArkUI_NodeType.ARKUI_NODE_XCOMPONENT,
        ArkUI_XComponentType.ARKUI_XCOMPONENT_TYPE_TEXTURE => ArkUI_NodeType.ARKUI_NODE_XCOMPONENT_TEXTURE,
        _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, "Unsupported XComponent type"),
    };

    public XComponent(ArkUI_XComponentType type)
        : base(ToNodeType(type))
    {
    }
}
