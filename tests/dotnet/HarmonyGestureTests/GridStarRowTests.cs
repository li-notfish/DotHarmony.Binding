using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

/// <summary>
/// HarmonyManagedLayoutHandler.GridHasStarRow：无 Star 行的 Grid 首帧后改写为内容显式高
/// （Auto 卡片不被百分比高撑成整屏）；未定义行 = 单行 Star（MAUI 默认）。
/// </summary>
public class GridStarRowTests
{
    [Fact]
    public void No_RowDefinitions_Is_Single_Star()
        => Assert.True(HarmonyManagedLayoutHandler.GridHasStarRow(new Grid()));

    [Fact]
    public void Explicit_Star_Row()
        => Assert.True(HarmonyManagedLayoutHandler.GridHasStarRow(new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
        }));

    [Fact]
    public void Auto_And_Absolute_Only_No_Star()
        => Assert.False(HarmonyManagedLayoutHandler.GridHasStarRow(new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(40)) }
        }));

    [Fact]
    public void Star_With_Multiplier()
        => Assert.True(HarmonyManagedLayoutHandler.GridHasStarRow(new Grid
        {
            RowDefinitions = { new RowDefinition(new GridLength(2, GridUnitType.Star)) }
        }));
}
