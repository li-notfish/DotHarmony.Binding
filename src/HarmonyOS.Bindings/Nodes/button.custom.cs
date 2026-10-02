// Button 手写扩展：诊断回读（ArkUI NDK 未公开 label 的 Get 封装，仅调试用）
#nullable enable
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class Button
{
    private Text? _labelChild;

    /// <summary>读回 NODE_BUTTON_LABEL（.string），用于诊断 SetAttribute 是否真正生效</summary>
    public string? GetLabelDebug()
    {
        var item = ArkUINativeApi.GetAttribute(Handle.Handle, ArkUI_NodeAttributeType.NODE_BUTTON_LABEL);
        if (item is null || item->@string is null)
            return null;
        return Marshal.PtrToStringUTF8((nint)item->@string);
    }

    /// <summary>内建 label 属性路径在当前 NDK 上不入排版（SetAttribute 成功但不参与测量/渲染），
    /// 用子 Text 节点承载按钮文字</summary>
    public void SetLabelChild(string text)
    {
        if (_labelChild is null)
        {
            _labelChild = new Text();
            // NDK 原生节点无样式表：不设 fontSize 时按 0 渲染（实测不可见），补平台默认 16fp
            _labelChild.FontSize = 16f;
            // 纯展示节点必须透传命中：否则子 Text 挡住点击，Button 的 CLICK 永远不触发（实测回归）
            _labelChild.SetHitTestBehavior(ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_TRANSPARENT);
            AddChild(_labelChild);
        }
        _labelChild.Content = text;
    }

    /// <summary>转发文字颜色到子 Text（胶囊按钮的 fontColor 属性对子节点不生效时兜底）</summary>
    public void SetLabelChildColor(byte r, byte g, byte b, byte a = 255)
        => _labelChild?.SetFontColor(r, g, b, a);

    /// <summary>转发字号到子 Text</summary>
    public void SetLabelChildFontSize(float size)
    {
        if (_labelChild is not null)
            _labelChild.FontSize = size;
    }

    /// <summary>转发字体族到子 Text。</summary>
    public void SetLabelChildFontFamily(string family)
    {
        _labelChild?.SetFontFamily(family);
    }
}
