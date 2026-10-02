using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

/// <summary>平局编码：z * 兄弟数 + 声明序下标（原生仲裁详见 scripts/verify-zindex-probe.ps1）。</summary>
public class ZIndexOrderTests
{
    private static (BoxView A, BoxView B, BoxView C) ThreeSiblings()
    {
        var grid = new Grid();
        var a = new BoxView(); var b = new BoxView(); var c = new BoxView();
        grid.Children.Add(a); grid.Children.Add(b); grid.Children.Add(c);
        return (a, b, c);
    }

    [Fact]
    public void SameZ_UniqueByDeclarationOrder()
    {
        var (a, b, c) = ThreeSiblings();
        Assert.Equal(0f, ZIndexOrder.EffectiveZ(a));
        Assert.Equal(1f, ZIndexOrder.EffectiveZ(b));
        Assert.Equal(2f, ZIndexOrder.EffectiveZ(c));
    }

    [Fact]
    public void Raised_StaysOnTop()
    {
        var (a, b, c) = ThreeSiblings();
        b.ZIndex = 10;
        Assert.True(ZIndexOrder.EffectiveZ(b) > ZIndexOrder.EffectiveZ(c));
        Assert.True(ZIndexOrder.EffectiveZ(b) > ZIndexOrder.EffectiveZ(a));
    }

    [Fact]
    public void Restored_ReturnsToDeclarationOrder()
    {
        var (a, b, c) = ThreeSiblings();
        b.ZIndex = 10;
        b.ZIndex = 0; // 回退后必须重新回到声明序：c（后声明）压 b
        Assert.True(ZIndexOrder.EffectiveZ(b) < ZIndexOrder.EffectiveZ(c));
        Assert.True(ZIndexOrder.EffectiveZ(a) < ZIndexOrder.EffectiveZ(b));
    }

    [Fact]
    public void NegativeZ_OrderPreserved()
    {
        var (a, b, c) = ThreeSiblings();
        a.ZIndex = -1;
        b.ZIndex = -1;
        c.ZIndex = -1;
        Assert.True(ZIndexOrder.EffectiveZ(a) < ZIndexOrder.EffectiveZ(b));
        Assert.True(ZIndexOrder.EffectiveZ(b) < ZIndexOrder.EffectiveZ(c));
        Assert.True(ZIndexOrder.EffectiveZ(c) < ZIndexOrder.EffectiveZ(new BoxView())); // -1 组 < 0
    }

    [Fact]
    public void ExtremeZ_NoIntegerOverflow()
    {
        var (a, b, c) = ThreeSiblings();
        a.ZIndex = int.MaxValue;
        b.ZIndex = int.MaxValue;
        c.ZIndex = int.MinValue; // 极端值：编码先截断再 long 乘加，不做 int 环绕
        Assert.True(ZIndexOrder.EffectiveZ(a) < ZIndexOrder.EffectiveZ(b));
        Assert.True(ZIndexOrder.EffectiveZ(c) < ZIndexOrder.EffectiveZ(a));
    }

    [Fact]
    public void ExtremeZ_SaturatesToBoundedOrder()
    {
        var (a, b, _) = ThreeSiblings();
        a.ZIndex = int.MaxValue;
        b.ZIndex = 1;
        Assert.True(ZIndexOrder.EffectiveZ(a) > ZIndexOrder.EffectiveZ(b));
    }
}
