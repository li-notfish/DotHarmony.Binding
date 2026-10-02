// List 手写扩展：生成器未覆盖的 NODE_LIST_LANES / NODE_LIST_SPACE
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class List
{
    /// <summary>车道模式（NODE_LIST_LANES）：value[0].u32=车道数，value[1]?.f32=车道间距(vp)。
    /// 注意 NDK 文档中 4 值形态是 min/max 车道宽语义（设置了 min/max 车道数即失效），
    /// 固定车道数 + 间距按 ArkTS lanes(count,gutter) 直接映射用 2 值形态（未传 gutter 时 1 值）。</summary>
    public void SetLanes(uint lanes, float gutter = 0f)
    {
        if (gutter > 0f)
            SetNumericAttribute(ArkUI_NodeAttributeType.NODE_LIST_LANES, ArkUIValue.U(lanes), ArkUIValue.F(gutter));
        else
            SetNumericAttribute(ArkUI_NodeAttributeType.NODE_LIST_LANES, ArkUIValue.U(lanes));
    }

    /// <summary>主轴条目间距（NODE_LIST_SPACE），单位 vp</summary>
    public void SetSpace(float space)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_LIST_SPACE, ArkUIValue.F(space));

    /// <summary>列表方向（NODE_LIST_DIRECTION），value[0].i32=ArkUI_Axis：
    /// HORIZONTAL 即横向滚动列表（CollectionView 横向 ItemsLayout 映射）</summary>
    public void SetDirection(ArkUI_Axis axis)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_LIST_DIRECTION, ArkUIValue.I((int)axis));
}
