#nullable enable
using System;
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

/// <summary>ArkUI ListItemGroup：List 的 section 容器（组头/组脚/组内 adapter）。</summary>
public unsafe partial class ListItemGroup : ArkUINodeBase
{
    public ListItemGroup() : base(ArkUI_NodeType.ARKUI_NODE_LIST_ITEM_GROUP) { }

    /// <summary>组头节点（NODE_LIST_ITEM_GROUP_SET_HEADER，item.object=node handle）</summary>
    public void SetHeader(ArkUINodeBase header)
        => SetObjectAttribute(ArkUI_NodeAttributeType.NODE_LIST_ITEM_GROUP_SET_HEADER,
            (void*)header.Handle.Handle);

    /// <summary>组脚节点（NODE_LIST_ITEM_GROUP_SET_FOOTER，item.object=node handle）</summary>
    public void SetFooter(ArkUINodeBase footer)
        => SetObjectAttribute(ArkUI_NodeAttributeType.NODE_LIST_ITEM_GROUP_SET_FOOTER,
            (void*)footer.Handle.Handle);

    /// <summary>挂载组内虚拟化 adapter（item.object=adapter handle）</summary>
    public void SetNodeAdapter(IntPtr adapterHandle)
        => SetObjectAttribute(ArkUI_NodeAttributeType.NODE_LIST_ITEM_GROUP_NODE_ADAPTER,
            (void*)adapterHandle);

    /// <summary>摘除组内 adapter（dispose 前必须先复位）</summary>
    public void ResetNodeAdapter()
        => ResetAttribute(ArkUI_NodeAttributeType.NODE_LIST_ITEM_GROUP_NODE_ADAPTER);
}
