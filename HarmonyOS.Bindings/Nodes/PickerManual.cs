// 手写补充节点类（勿并入生成文件 —— picker 系 d.ts Evo 未被 NativeCodeGenerator 解析为节点）
// 载荷格式依据 native_node.h：DATE/TIME_PICKER_SELECTED 均为 .string（"yyyy-MM-dd" / "HH:mm"）。
#nullable enable
using System;
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

/// <summary>DatePicker 组件（ARKUI_NODE_DATE_PICKER，内嵌滚轮）</summary>
public unsafe class DatePicker : ArkUINodeBase
{
    public DatePicker() : base(ArkUI_NodeType.ARKUI_NODE_DATE_PICKER) { }

    /// <summary>选中日期（NODE_DATE_PICKER_SELECTED，.string "yyyy-MM-dd"）</summary>
    public string SelectedDate
    {
        set => SetStringAttribute(ArkUI_NodeAttributeType.NODE_DATE_PICKER_SELECTED, value);
    }

    /// <summary>最小日期（NODE_DATE_PICKER_START，.string "yyyy-MM-dd"）</summary>
    public string StartDate
    {
        set => SetStringAttribute(ArkUI_NodeAttributeType.NODE_DATE_PICKER_START, value);
    }

    /// <summary>最大日期（NODE_DATE_PICKER_END，.string "yyyy-MM-dd"）</summary>
    public string EndDate
    {
        set => SetStringAttribute(ArkUI_NodeAttributeType.NODE_DATE_PICKER_END, value);
    }

    /// <summary>日期变化事件（NODE_DATE_PICKER_EVENT_ON_DATE_CHANGE：data[0..2].i32 = 年/月[0-11]/日）</summary>
    public event Action<ArkUINodeEvent>? OnDateChange
    {
        add => On(ArkUI_NodeEventType.NODE_DATE_PICKER_EVENT_ON_DATE_CHANGE, value!);
        remove => Off(ArkUI_NodeEventType.NODE_DATE_PICKER_EVENT_ON_DATE_CHANGE);
    }
}

/// <summary>TimePicker 组件（ARKUI_NODE_TIME_PICKER，内嵌滚轮）</summary>
public unsafe class TimePicker : ArkUINodeBase
{
    public TimePicker() : base(ArkUI_NodeType.ARKUI_NODE_TIME_PICKER) { }

    /// <summary>选中时间（NODE_TIME_PICKER_SELECTED，.string "HH:mm"）</summary>
    public string SelectedTime
    {
        set => SetStringAttribute(ArkUI_NodeAttributeType.NODE_TIME_PICKER_SELECTED, value);
    }

    /// <summary>时间变化事件（NODE_TIME_PICKER_EVENT_ON_CHANGE：data[0].i32=时[0-23]、data[1].i32=分[0-59]）</summary>
    public event Action<ArkUINodeEvent>? OnTimeChange
    {
        add => On(ArkUI_NodeEventType.NODE_TIME_PICKER_EVENT_ON_CHANGE, value!);
        remove => Off(ArkUI_NodeEventType.NODE_TIME_PICKER_EVENT_ON_CHANGE);
    }
}
