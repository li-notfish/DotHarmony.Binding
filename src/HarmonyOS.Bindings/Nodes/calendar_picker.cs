#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public sealed class CalendarPicker : ArkUINodeBase
{
    public CalendarPicker() : base(ArkUI_NodeType.ARKUI_NODE_CALENDAR_PICKER) { }
}
