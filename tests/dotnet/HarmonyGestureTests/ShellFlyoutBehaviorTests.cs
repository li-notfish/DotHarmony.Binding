using HarmonyOS.Maui.Handlers;
using Xunit;

namespace HarmonyGestureTests;

/// <summary>
/// FlyoutBehavior.Locked 并排让宽的纯规则层：主列宽 = 根宽 - 面板宽，下限 0。
/// </summary>
public class ShellFlyoutBehaviorTests
{
    [Fact]
    public void LockedContentWidth_NormalLayout_SubtractsFlyoutWidth()
        => Assert.Equal(520f, HarmonyShellHandler.ResolveLockedContentWidth(800f, 280f));

    [Fact]
    public void LockedContentWidth_FlyoutExceedsRoot_ClampsToZero()
        => Assert.Equal(0f, HarmonyShellHandler.ResolveLockedContentWidth(200f, 280f));

    [Fact]
    public void LockedContentWidth_ExactFit_ReturnsZero()
        => Assert.Equal(0f, HarmonyShellHandler.ResolveLockedContentWidth(280f, 280f));

    [Fact]
    public void LockedFlyoutWidth_WideRoot_KeepsThemeWidth()
        => Assert.Equal(280f, HarmonyShellHandler.ResolveLockedFlyoutWidth(800f, 280f));

    [Fact]
    public void LockedFlyoutWidth_NarrowPhone_ClampsTo60Percent()
        => Assert.Equal(216.0, (double)HarmonyShellHandler.ResolveLockedFlyoutWidth(360f, 280f), 3);
}
