// Swiper 手写扩展（CarouselView 通道：Loop / CurrentIndex / DisableSwipe，2026-09-15）。
// ⚠️ 勿合并回 swiper.cs——该文件由生成器（NativeCodeGenerator）重写，手写内容放此 partial 才不被覆盖。
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class Swiper
{
    /// <summary>单值属性读取（NODE_SWIPER_LOOP/INDEX/DISABLE_SWIPE 手写通道）</summary>
    private int GetAttr(ArkUI_NodeAttributeType attribute)
    {
        var item = ArkUINativeApi.GetAttribute(Handle, attribute);
        return item != null && item->value != null && item->size > 0 ? item->value[0].i32 : 0;
    }

    /// <summary>loop 属性（NODE_SWIPER_LOOP）：是否开启循环轮播</summary>
    public bool Loop
    {
        get => GetAttr(ArkUI_NodeAttributeType.NODE_SWIPER_LOOP) != 0;
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_SWIPER_LOOP, ArkUIValue.I(value ? 1 : 0));
    }

    /// <summary>index 属性（NODE_SWIPER_INDEX）：当前索引（设置即跳转）</summary>
    public int CurrentIndex
    {
        get => GetAttr(ArkUI_NodeAttributeType.NODE_SWIPER_INDEX);
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_SWIPER_INDEX, ArkUIValue.I(value));
    }

    /// <summary>disableSwipe 属性（NODE_SWIPER_DISABLE_SWIPE）：禁用滑动切换</summary>
    public bool DisableSwipe
    {
        get => GetAttr(ArkUI_NodeAttributeType.NODE_SWIPER_DISABLE_SWIPE) != 0;
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_SWIPER_DISABLE_SWIPE, ArkUIValue.I(value ? 1 : 0));
    }
}
