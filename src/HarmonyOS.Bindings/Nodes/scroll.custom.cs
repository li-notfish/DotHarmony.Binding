// Scroll 手写扩展：生成器未覆盖的 NODE_SCROLL_ENABLE_SCROLL_INTERACTION
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class Scroll
{
    /// <summary>允许手势滚动（NODE_SCROLL_ENABLE_SCROLL_INTERACTION）</summary>
    public bool EnableScrollInteraction
    {
        set => SetNumericAttribute(
            ArkUI_NodeAttributeType.NODE_SCROLL_ENABLE_SCROLL_INTERACTION,
            ArkUIValue.I(value ? 1 : 0));
    }

    /// <summary>嵌套滚动模式（NODE_SCROLL_NESTED_SCROLL）：value[0]=向前、value[1]=向后，
    /// 均为 ArkUI_ScrollNestedMode。NDK 要求两值成对下发，单值返回 401（实测）</summary>
    public void SetNestedScroll(ArkUI_ScrollNestedMode forward, ArkUI_ScrollNestedMode backward)
        => SetNumericAttribute(
            ArkUI_NodeAttributeType.NODE_SCROLL_NESTED_SCROLL,
            ArkUIValue.I((int)forward), ArkUIValue.I((int)backward));

    /// <summary>滚动条显示模式（NODE_SCROLL_BAR_DISPLAY_MODE）。</summary>
    public ArkUI_ScrollBarDisplayMode ScrollBarDisplayMode
    {
        set => SetNumericAttribute(
            ArkUI_NodeAttributeType.NODE_SCROLL_BAR_DISPLAY_MODE,
            ArkUIValue.I((int)value));
    }

    /// <summary>相对滚动（NODE_SCROLL_BY）：value[0].f32=x 增量、value[1].f32=y 增量，单位 vp。
    /// NODE_SCROLL_OFFSET 绝对定位在横向 Scroll 上实测被钳到边缘回弹，相对滚动走 NDK 推荐通道。</summary>
    public void ScrollBy(float dx, float dy)
        => SetNumericAttribute(
            ArkUI_NodeAttributeType.NODE_SCROLL_BY,
            ArkUIValue.F(dx), ArkUIValue.F(dy));
}
