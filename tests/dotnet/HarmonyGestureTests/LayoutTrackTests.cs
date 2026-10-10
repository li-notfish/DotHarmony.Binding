using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;
using MALIGNMENT = Microsoft.Maui.Primitives.LayoutAlignment;

namespace HarmonyGestureTests;

/// <summary>
/// 托管布局纯规则层夹具：Star 列、Auto+Star 混合轨道解析、对齐收缩、
/// span 亏空分摊、AbsoluteLayout 比例定位（W2 布局补强）。
/// </summary>
public class LayoutTrackTests
{
    private static float[] Resolve(string[] units, float[] values, float[] auto, float container)
    {
        var u = new GridUnitType[units.Length];
        for (int i = 0; i < units.Length; i++)
            u[i] = units[i] switch
            {
                "*" => GridUnitType.Star,
                "A" => GridUnitType.Absolute,
                _ => GridUnitType.Auto,
            };
        return HarmonyManagedLayoutHandler.ResolveTracks(u, values, auto, container);
    }

    // ── Star 列/轨道解析 ──

    [Fact]
    public void SingleStar_TakesWholeContainer()
        => Assert.Equal(new[] { 400f }, Resolve(new[] { "*" }, new[] { 1f }, new[] { 0f }, 400f));

    [Fact]
    public void WeightedStars_SplitRemainingByWeight()
        => Assert.Equal(new[] { 100f, 300f }, Resolve(new[] { "*", "*" }, new[] { 1f, 3f }, new[] { 0f, 0f }, 400f));

    [Fact]
    public void AutoPlusStar_StarGetsRemainderAfterAuto()
        => Assert.Equal(new[] { 80f, 320f }, Resolve(new[] { "Auto", "*" }, new[] { 0f, 1f }, new[] { 80f, 0f }, 400f));

    [Fact]
    public void AbsoluteAutoStar_FixedFirst_ThenStarRemainder()
        => Assert.Equal(new[] { 50f, 70f, 280f },
            Resolve(new[] { "A", "Auto", "*" }, new[] { 50f, 0f, 1f }, new[] { 0f, 70f, 0f }, 400f));

    [Fact]
    public void NoStar_RemainingSpaceLeftUnused()
        => Assert.Equal(new[] { 50f, 70f },
            Resolve(new[] { "A", "Auto" }, new[] { 50f, 0f }, new[] { 0f, 70f }, 400f));

    [Fact]
    public void FixedOverflow_StarClampsToZero()
        => Assert.Equal(new[] { 500f, 0f },
            Resolve(new[] { "A", "*" }, new[] { 500f, 1f }, new[] { 0f, 0f }, 400f));

    [Fact]
    public void ZeroWeightStar_GetsNothing()
        => Assert.Equal(new[] { 0f, 400f },
            Resolve(new[] { "*", "*" }, new[] { 0f, 1f }, new[] { 0f, 0f }, 400f));

    // ── 对齐收缩（非 auto 维在 frame 内按实测尺寸摆放） ──

    [Fact]
    public void Alignment_Fill_KeepsFrame()
        => Assert.Equal((10f, 200f), HarmonyManagedLayoutHandler.ApplyAlignment(MALIGNMENT.Fill, 10f, 200f, 50.0));

    [Fact]
    public void Alignment_Center_ShrinksAndCenters()
        => Assert.Equal((85f, 50f), HarmonyManagedLayoutHandler.ApplyAlignment(MALIGNMENT.Center, 10f, 200f, 50.0));

    [Fact]
    public void Alignment_End_ShrinksToTrailingEdge()
        => Assert.Equal((160f, 50f), HarmonyManagedLayoutHandler.ApplyAlignment(MALIGNMENT.End, 10f, 200f, 50.0));

    [Fact]
    public void Alignment_Start_ShrinksToLeadingEdge()
        => Assert.Equal((10f, 50f), HarmonyManagedLayoutHandler.ApplyAlignment(MALIGNMENT.Start, 10f, 200f, 50.0));

    [Fact]
    public void Alignment_ContentNotMeasured_KeepsFrame()
        => Assert.Equal((10f, 200f), HarmonyManagedLayoutHandler.ApplyAlignment(MALIGNMENT.Center, 10f, 200f, 0.0));

    [Fact]
    public void Alignment_ContentExceedsFrame_KeepsFrame()
        => Assert.Equal((10f, 200f), HarmonyManagedLayoutHandler.ApplyAlignment(MALIGNMENT.End, 10f, 200f, 250.0));

    // ── span 亏空分摊 ──

    [Fact]
    public void SpanDeficit_PureAuto_SplitsShortfall()
    {
        var auto = new[] { 20f, 10f };
        var units = new[] { GridUnitType.Auto, GridUnitType.Auto };
        HarmonyManagedLayoutHandler.DistributeSpanDeficit(auto, units, 0, 2, 90f);
        Assert.Equal(new[] { 50f, 40f }, auto);
    }

    [Fact]
    public void SpanDeficit_ContainsStar_Skipped()
    {
        var auto = new[] { 20f, 0f };
        var units = new[] { GridUnitType.Auto, GridUnitType.Star };
        HarmonyManagedLayoutHandler.DistributeSpanDeficit(auto, units, 0, 2, 90f);
        Assert.Equal(new[] { 20f, 0f }, auto); // Star 吸收剩余空间，不得吹大 Auto
    }

    [Fact]
    public void SpanDeficit_AlreadyEnough_NoOp()
    {
        var auto = new[] { 60f, 40f };
        var units = new[] { GridUnitType.Auto, GridUnitType.Auto };
        HarmonyManagedLayoutHandler.DistributeSpanDeficit(auto, units, 0, 2, 90f);
        Assert.Equal(new[] { 60f, 40f }, auto);
    }

    // ── AbsoluteLayout 比例定位 ──

    [Fact]
    public void Absolute_ProportionalSizeAndPosition_AnchorsOnPlaceableArea()
    {
        // 50% 宽 @ X=1.0（右对齐贴边）：x = 1.0 * (400 - 200) = 200
        var (x, _, w, _, wAuto, _) = HarmonyManagedLayoutHandler.ResolveAbsoluteBounds(
            AbsoluteLayoutFlags.All, new Rect(1.0, 0.5, 0.5, 0.25), 400f, 800f, new Thickness(0));
        Assert.False(wAuto);
        Assert.Equal(200f, w);
        Assert.Equal(200f, x);
    }

    [Fact]
    public void Absolute_AutoSize_StaysAutoAndUnshrunk()
    {
        var (_, _, _, h, _, hAuto) = HarmonyManagedLayoutHandler.ResolveAbsoluteBounds(
            AbsoluteLayoutFlags.PositionProportional, new Rect(0, 0, 100, AbsoluteLayout.AutoSize),
            400f, 800f, new Thickness(4, 8, 4, 8));
        Assert.True(hAuto);
        Assert.Equal(AbsoluteLayout.AutoSize, h); // auto 维不内缩 Margin
    }

    [Fact]
    public void Absolute_ExplicitSize_MarginShrinksBothAxes()
    {
        var (x, y, w, h, _, _) = HarmonyManagedLayoutHandler.ResolveAbsoluteBounds(
            AbsoluteLayoutFlags.None, new Rect(10, 20, 100, 50), 400f, 800f, new Thickness(4, 8, 4, 8));
        Assert.Equal((14f, 28f, 92f, 34f), (x, y, w, h));
    }
}
