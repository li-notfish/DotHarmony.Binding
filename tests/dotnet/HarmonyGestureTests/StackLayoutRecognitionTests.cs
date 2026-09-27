using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

public class StackLayoutRecognitionTests
{
    [Fact]
    public void HorizontalStackLayout_IsHorizontal()
    {
        Assert.True(HarmonyLayoutHandler.IsHorizontalStack(new HorizontalStackLayout()));
    }

    [Fact]
    public void VerticalStackLayout_IsNotHorizontal()
    {
        Assert.False(HarmonyLayoutHandler.IsHorizontalStack(new VerticalStackLayout()));
    }

    [Fact]
    public void HorizontalStackLayout_Orientation_IsHorizontal()
    {
        Assert.True(HarmonyLayoutHandler.IsHorizontalStack(
            new StackLayout { Orientation = StackOrientation.Horizontal }));
    }

    [Fact]
    public void VerticalStackLayout_Orientation_IsNotHorizontal()
    {
        Assert.False(HarmonyLayoutHandler.IsHorizontalStack(
            new StackLayout { Orientation = StackOrientation.Vertical }));
    }

    [Fact]
    public void StackLayoutFamily_IsAnyStack()
    {
        Assert.True(HarmonyLayoutHandler.IsAnyStack(new StackLayout()));
        Assert.True(HarmonyLayoutHandler.IsAnyStack(new HorizontalStackLayout()));
        Assert.True(HarmonyLayoutHandler.IsAnyStack(new VerticalStackLayout()));
    }
}
