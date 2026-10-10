#nullable enable
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui.Layouts;

namespace HarmonyOS.Maui.Handlers;

internal static class FlexSemanticMapper
{
    public static ArkUI_FlexDirection MapDirection(FlexDirection direction) => direction switch
    {
        FlexDirection.Row => ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_ROW,
        FlexDirection.RowReverse => ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_ROW_REVERSE,
        FlexDirection.Column => ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_COLUMN,
        FlexDirection.ColumnReverse => ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_COLUMN_REVERSE,
        _ => ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_ROW,
    };

    public static ArkUI_FlexWrap MapWrap(FlexWrap wrap) => wrap switch
    {
        FlexWrap.Wrap => ArkUI_FlexWrap.ARKUI_FLEX_WRAP_WRAP,
        FlexWrap.Reverse => ArkUI_FlexWrap.ARKUI_FLEX_WRAP_WRAP_REVERSE,
        _ => ArkUI_FlexWrap.ARKUI_FLEX_WRAP_NO_WRAP,
    };

    public static ArkUI_FlexAlignment MapJustify(FlexJustify justify) => justify switch
    {
        FlexJustify.Center => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_CENTER,
        FlexJustify.End => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_END,
        FlexJustify.SpaceBetween => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_BETWEEN,
        FlexJustify.SpaceAround => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_AROUND,
        FlexJustify.SpaceEvenly => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_EVENLY,
        _ => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_START,
    };

    public static ArkUI_ItemAlignment MapAlignItems(FlexAlignItems align) => align switch
    {
        FlexAlignItems.Start => ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_START,
        FlexAlignItems.Center => ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER,
        FlexAlignItems.End => ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_END,
        _ => ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_STRETCH,
    };

    public static ArkUI_FlexAlignment MapAlignContent(FlexAlignContent align) => align switch
    {
        FlexAlignContent.Center => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_CENTER,
        FlexAlignContent.End => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_END,
        FlexAlignContent.SpaceBetween => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_BETWEEN,
        FlexAlignContent.SpaceAround => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_AROUND,
        FlexAlignContent.SpaceEvenly => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_EVENLY,
        _ => ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_START,
    };
}
