#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class Flex
{
    private struct FlexOptions
    {
        public ArkUI_FlexDirection Direction;
        public ArkUI_FlexWrap Wrap;
        public ArkUI_FlexAlignment JustifyContent;
        public ArkUI_ItemAlignment AlignItems;
        public ArkUI_FlexAlignment AlignContent;
    }

    private FlexOptions _options = new()
    {
        Direction = ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_ROW,
        Wrap = ArkUI_FlexWrap.ARKUI_FLEX_WRAP_NO_WRAP,
        JustifyContent = ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_START,
        AlignItems = ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_STRETCH,
        AlignContent = ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_START,
    };

    public ArkUI_FlexDirection Direction
    {
        get => _options.Direction;
        set
        {
            _options.Direction = value;
            ApplyFlexOptions();
        }
    }

    public ArkUI_FlexWrap Wrap
    {
        get => _options.Wrap;
        set
        {
            _options.Wrap = value;
            ApplyFlexOptions();
        }
    }

    public ArkUI_FlexAlignment JustifyContent
    {
        get => _options.JustifyContent;
        set
        {
            _options.JustifyContent = value;
            ApplyFlexOptions();
        }
    }

    public ArkUI_ItemAlignment AlignItems
    {
        get => _options.AlignItems;
        set
        {
            _options.AlignItems = value;
            ApplyFlexOptions();
        }
    }

    public ArkUI_FlexAlignment AlignContent
    {
        get => _options.AlignContent;
        set
        {
            _options.AlignContent = value;
            ApplyFlexOptions();
        }
    }

    private void ApplyFlexOptions()
    {
        SetNumericAttribute(
            ArkUI_NodeAttributeType.NODE_FLEX_OPTION,
            ArkUIValue.I((int)_options.Direction),
            ArkUIValue.I((int)_options.Wrap),
            ArkUIValue.I((int)_options.JustifyContent),
            ArkUIValue.I((int)_options.AlignItems),
            ArkUIValue.I((int)_options.AlignContent));
    }
}
