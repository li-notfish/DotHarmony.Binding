// 自绘链路纯逻辑单测：颜色打包与圆角钳制（不触原生，可在桌面跑）。
using Xunit;

namespace HarmonyGestureTests;

public class DrawingMappingTests
{
    [Theory]
    [InlineData(255, 255, 0, 0, 0xFFFF0000u)]   // 红
    [InlineData(255, 0, 255, 0, 0xFF00FF00u)]   // 绿
    [InlineData(0, 255, 255, 255, 0x00FFFFFFu)] // 全透明
    [InlineData(128, 17, 34, 51, 0x80112233u)]  // 混合分量
    public void PackArgb_MapsComponents(byte a, byte r, byte g, byte b, uint expected)
        => Assert.Equal(expected, HarmonyOS.Bindings.NativeNode.OHDrawingColorHelper.PackArgb(a, r, g, b));

    [Theory]
    [InlineData(8, 100, 50, 8)]    // 半径未超短边一半 → 原值
    [InlineData(30, 100, 50, 25)]  // 超短边一半 → 钳制
    [InlineData(10, 0, 50, 0)]     // 零尺寸 → 0
    [InlineData(-1, 100, 50, 0)]   // 负半径 → 0
    public void ClampRadius_ClampsToHalfMinSide(float radius, float w, float h, float expected)
        => Assert.Equal(expected, HarmonyOS.Maui.Handlers.HarmonyDrawingCanvas.ClampRadius(radius, w, h));
}
