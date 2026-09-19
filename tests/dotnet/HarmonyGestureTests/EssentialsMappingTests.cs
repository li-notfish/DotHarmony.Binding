// Essentials 鸿蒙实现的纯映射逻辑单测（无设备依赖）
using HarmonyOS.Essentials;
using Microsoft.Maui.Devices;
using Xunit;

namespace HarmonyGestureTests;

public class EssentialsMappingTests
{
    [Theory]
    [InlineData(0, DisplayRotation.Rotation0)]
    [InlineData(1, DisplayRotation.Rotation90)]
    [InlineData(2, DisplayRotation.Rotation180)]
    [InlineData(3, DisplayRotation.Rotation270)]
    [InlineData(9, DisplayRotation.Rotation0)]   // 未知值兜底
    public void MapRotation_MapsOhCodes(double oh, DisplayRotation expected) =>
        Assert.Equal(expected, HarmonyDeviceDisplay.MapRotation(oh));

    [Fact]
    public void DisplayInfo_DensityFromDpi()
    {
        // 560dpi 屏 → 3.5x（与 Android Essentials DisplayMetrics.Density 同义）
        var info = HarmonyDeviceDisplay.BuildDisplayInfo(width: 1320, height: 2856, dpi: 560, rotation: 0);
        Assert.Equal(1320, info.Width);
        Assert.Equal(3.5, info.Density, precision: 6);
        Assert.Equal(DisplayOrientation.Portrait, info.Orientation);
        Assert.Equal(DisplayRotation.Rotation0, info.Rotation);
    }

    [Fact]
    public void DisplayInfo_LandscapeWhenWider()
    {
        var info = HarmonyDeviceDisplay.BuildDisplayInfo(width: 2856, height: 1320, dpi: 560, rotation: 1);
        Assert.Equal(DisplayOrientation.Landscape, info.Orientation);
        Assert.Equal(DisplayRotation.Rotation90, info.Rotation);
    }
}
