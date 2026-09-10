// 手写补充节点类（勿并入生成文件 —— text_picker.d.ts Evo 未被 NativeCodeGenerator 解析为节点）
// 载荷格式依据 native_node.h：NODE_TEXT_PICKER_OPTION_RANGE 为 value[0].i32=RangeType + .string 组合。
#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

/// <summary>TextPicker 组件（ARKUI_NODE_TEXT_PICKER，单列字符串范围）</summary>
public unsafe class TextPicker : ArkUINodeBase
{
    public TextPicker() : base(ArkUI_NodeType.ARKUI_NODE_TEXT_PICKER) { }

    /// <summary>单列选项范围（NODE_TEXT_PICKER_OPTION_RANGE：value[0].i32=1 单列，string 以 ';' 分隔）</summary>
    public void SetRange(IReadOnlyList<string> options)
    {
        var utf8 = Encoding.UTF8.GetBytes(string.Join(";", options));
        var values = new ArkUI_NumberValue[] { ArkUIValue.I(1) }; // ArkUI_TextPickerRangeType: 1 = 单列字符串
        fixed (byte* p = utf8)
        fixed (ArkUI_NumberValue* v = values)
        {
            var item = new ArkUI_AttributeItem { value = v, size = 1, @string = p };
            var status = ArkUINativeApi.SetAttribute(Handle, ArkUI_NodeAttributeType.NODE_TEXT_PICKER_OPTION_RANGE, &item);
            if (status != 0)
                throw new InvalidOperationException($"SetAttribute(NODE_TEXT_PICKER_OPTION_RANGE) failed: {status}");
        }
    }

    /// <summary>当前选中索引（NODE_TEXT_PICKER_SELECTED_INDEX，i32）</summary>
    public int SelectedIndex
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TEXT_PICKER_SELECTED_INDEX, ArkUIValue.I(value));
    }

    /// <summary>选中项变化事件（NODE_TEXT_PICKER_EVENT_ON_CHANGE：data[0..].i32 各列选中索引）</summary>
    public event Action<ArkUINodeEvent>? OnChange
    {
        add => On(ArkUI_NodeEventType.NODE_TEXT_PICKER_EVENT_ON_CHANGE, value!);
        remove => Off(ArkUI_NodeEventType.NODE_TEXT_PICKER_EVENT_ON_CHANGE);
    }
}
