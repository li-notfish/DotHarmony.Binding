using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Xunit;
using HitMode = HarmonyOS.Bindings.NativeNode.ArkUI_HitTestMode;

namespace HarmonyGestureTests;

/// <summary>
/// 通用视觉协议纯规则层（HarmonyViewHandler 之外的 VisualProtocol）：
/// InputTransparent 容器级联 → ArkUI HitTestMode 的映射决策。
/// </summary>
public class VisualProtocolTests
{
    [Fact]
    public void TransparentLeaf_MapsToNone()
    {
        var image = new Image { InputTransparent = true };

        Assert.Equal(HitMode.ARKUI_HIT_TEST_MODE_NONE, VisualProtocol.ResolveHitTestMode(image));
    }

    [Fact]
    public void TransparentContainer_MapsToTransparent()
    {
        var stack = new StackLayout { InputTransparent = true };
        stack.Children.Add(new Label());

        Assert.Equal(HitMode.ARKUI_HIT_TEST_MODE_TRANSPARENT, VisualProtocol.ResolveHitTestMode(stack));
    }

    [Fact]
    public void OpaqueView_MapsToDefault()
    {
        var stack = new StackLayout();
        stack.Children.Add(new Label());

        Assert.Equal(HitMode.ARKUI_HIT_TEST_MODE_DEFAULT, VisualProtocol.ResolveHitTestMode(stack));
    }

    [Fact]
    public void LeafChildOfTransparentContainer_InheritsCascade()
    {
        var stack = new StackLayout { InputTransparent = true };
        var child = new Label();
        stack.Children.Add(child);

        // 子级自身不透明，但祖先透明 → 级联生效（叶子 → NONE，与容器自身映射一致）
        Assert.Equal(HitMode.ARKUI_HIT_TEST_MODE_NONE, VisualProtocol.ResolveHitTestMode(child));
    }

    [Fact]
    public void ContainerChildOfTransparentContainer_InheritsCascade()
    {
        var outer = new StackLayout { InputTransparent = true };
        var inner = new StackLayout();
        outer.Children.Add(inner);
        inner.Children.Add(new Label());

        Assert.Equal(HitMode.ARKUI_HIT_TEST_MODE_TRANSPARENT, VisualProtocol.ResolveHitTestMode(inner));
    }
}
