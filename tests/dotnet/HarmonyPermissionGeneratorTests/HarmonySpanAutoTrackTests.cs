using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyPermissionGeneratorTests;

public sealed class HarmonySpanAutoTrackTests
{
    [Fact]
    public void PureAutoSpan_deficit_distributed_evenly()
    {
        var units = new[] { GridUnitType.Auto, GridUnitType.Auto };
        var autoSizes = new float[] { 0, 0 };
        HarmonyManagedLayoutHandler.DistributeSpanDeficit(autoSizes, units, 0, 2, 100);
        Assert.Equal(50f, autoSizes[0], 1);
        Assert.Equal(50f, autoSizes[1], 1);
    }

    [Fact]
    public void AutoPlusStarSpan_deficit_skipped()
    {
        var units = new[] { GridUnitType.Auto, GridUnitType.Star };
        var autoSizes = new float[] { 0, 0 };
        HarmonyManagedLayoutHandler.DistributeSpanDeficit(autoSizes, units, 0, 2, 100);
        Assert.Equal(0f, autoSizes[0]);
    }

    [Fact]
    public void SpanBeyondBounds_clamped_to_track_length()
    {
        var units = new[] { GridUnitType.Auto };
        var autoSizes = new float[] { 0 };
        HarmonyManagedLayoutHandler.DistributeSpanDeficit(autoSizes, units, 0, 5, 100);
        Assert.Equal(100f, autoSizes[0], 1);
    }

    [Fact]
    public void SpannedAutoCount_counts_only_auto_tracks()
    {
        var units = new[] { GridUnitType.Auto, GridUnitType.Star, GridUnitType.Auto };
        Assert.Equal(2, HarmonyManagedLayoutHandler.SpannedAutoCount(units, 0, 3));
        Assert.Equal(1, HarmonyManagedLayoutHandler.SpannedAutoCount(units, 1, 2));
        Assert.Equal(1, HarmonyManagedLayoutHandler.SpannedAutoCount(units, 2, 1));
    }

    [Fact]
    public void ExistingAutoSize_not_reduced_by_deficit()
    {
        var units = new[] { GridUnitType.Auto, GridUnitType.Auto };
        var autoSizes = new float[] { 80, 0 };
        HarmonyManagedLayoutHandler.DistributeSpanDeficit(autoSizes, units, 0, 2, 100);
        Assert.Equal(90f, autoSizes[0], 1);
        Assert.Equal(10f, autoSizes[1], 1);
    }
}
