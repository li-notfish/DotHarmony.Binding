// 布局遗留修复的纯逻辑单测：
// 1) Grid 单元格内对齐（MAUI 官方语义：非 Fill 按实测尺寸在 frame 内摆放）；
// 2) Swipe 方向映射（保留自手势专项）。
using HarmonyOS.Maui.Handlers;
using Xunit;
using LayoutAlignment = Microsoft.Maui.Primitives.LayoutAlignment;

namespace HarmonyGestureTests;

public class GridCellAlignmentTests
{
    private const float FrameOffset = 10f;
    private const float FrameSize = 100f;

    [Fact]
    public void Fill_KeepsFrame()
    {
        var (offset, size) = HarmonyManagedLayoutHandler.ApplyAlignment(
            LayoutAlignment.Fill, FrameOffset, FrameSize, 30);
        Assert.Equal(FrameOffset, offset);
        Assert.Equal(FrameSize, size);
    }

    [Fact]
    public void Start_ShrinksToContent_AtFrameOrigin()
    {
        var (offset, size) = HarmonyManagedLayoutHandler.ApplyAlignment(
            LayoutAlignment.Start, FrameOffset, FrameSize, 30);
        Assert.Equal(FrameOffset, offset);
        Assert.Equal(30f, size);
    }

    [Fact]
    public void Center_OffsetsByHalf()
    {
        var (offset, size) = HarmonyManagedLayoutHandler.ApplyAlignment(
            LayoutAlignment.Center, FrameOffset, FrameSize, 30);
        Assert.Equal(FrameOffset + 35f, offset);
        Assert.Equal(30f, size);
    }

    [Fact]
    public void End_OffsetsByRemainder()
    {
        var (offset, size) = HarmonyManagedLayoutHandler.ApplyAlignment(
            LayoutAlignment.End, FrameOffset, FrameSize, 30);
        Assert.Equal(FrameOffset + 70f, offset);
        Assert.Equal(30f, size);
    }

    [Fact]
    public void UnmeasuredContent_KeepsFrame()
    {
        // 首帧内容未量测：保持 Fill，AREA_CHANGE 触发下一轮后生效
        var (offset, size) = HarmonyManagedLayoutHandler.ApplyAlignment(
            LayoutAlignment.Center, FrameOffset, FrameSize, 0);
        Assert.Equal(FrameOffset, offset);
        Assert.Equal(FrameSize, size);
    }

    [Fact]
    public void ContentLargerThanFrame_KeepsFrame()
    {
        var (offset, size) = HarmonyManagedLayoutHandler.ApplyAlignment(
            LayoutAlignment.Center, FrameOffset, FrameSize, 150);
        Assert.Equal(FrameOffset, offset);
        Assert.Equal(FrameSize, size);
    }
}
