using HarmonyOS.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui;
using Xunit;

namespace HarmonyGestureTests;

public class ScrollViewContentSizingTests
{
    [Theory]
    [InlineData(ScrollOrientation.Vertical, true, false)]
    [InlineData(ScrollOrientation.Horizontal, false, true)]
    [InlineData(ScrollOrientation.Both, false, false)]
    // Neither 官方 CrossPlatformMeasure：宽高均不约束
    [InlineData(ScrollOrientation.Neither, false, false)]
    public void Orientation_SelectsExpectedCrossAxisSizing(
        ScrollOrientation orientation,
        bool stretchWidth,
        bool stretchHeight)
    {
        var (actualWidth, actualHeight) = HarmonyScrollViewHandler.GetContentSizing(orientation);

        Assert.Equal(stretchWidth, actualWidth);
        Assert.Equal(stretchHeight, actualHeight);
    }

    [Theory]
    [InlineData(ScrollBarVisibility.Default, ArkUI_ScrollBarDisplayMode.ARKUI_SCROLL_BAR_DISPLAY_MODE_AUTO)]
    [InlineData(ScrollBarVisibility.Always, ArkUI_ScrollBarDisplayMode.ARKUI_SCROLL_BAR_DISPLAY_MODE_ON)]
    [InlineData(ScrollBarVisibility.Never, ArkUI_ScrollBarDisplayMode.ARKUI_SCROLL_BAR_DISPLAY_MODE_OFF)]
    public void ScrollBarVisibility_MapsToNativeDisplayMode(
        ScrollBarVisibility visibility,
        ArkUI_ScrollBarDisplayMode expected)
    {
        Assert.Equal(expected, HarmonyScrollViewHandler.GetScrollBarDisplayMode(visibility));
    }
}
