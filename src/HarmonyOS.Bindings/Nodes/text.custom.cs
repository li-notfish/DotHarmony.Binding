// Text 手写扩展：生成器未覆盖的 NODE_TEXT_MAX_LINES / NODE_TEXT_ELLIPSIS_MODE
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class Text
{
    /// <summary>字符间距（NODE_TEXT_LETTER_SPACING，单位 vp）</summary>
    public void SetLetterSpacing(float spacing)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TEXT_LETTER_SPACING, ArkUIValue.F(spacing));

    /// <summary>最大行数（NODE_TEXT_MAX_LINES，i32）</summary>
    public void SetMaxLines(int maxLines)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TEXT_MAX_LINES, ArkUIValue.I(maxLines));

    /// <summary>省略模式（NODE_TEXT_ELLIPSIS_MODE，ArkUI_EllipsisMode）。
    /// 注意：该属性仅对设置了 MAX_LINES 的文本生效（ArkUI 文档约束）。</summary>
    public void SetEllipsisMode(ArkUI_EllipsisMode mode)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TEXT_ELLIPSIS_MODE, ArkUIValue.I((int)mode));
}
