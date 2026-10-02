using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace HarmonyGestureTests;

public class GlobalResourcesTests
{
    [Fact]
    public void EnsureCurrent_MergesGlobalResources()
    {
        var resources = new ResourceDictionary();
        resources.Add("TestAccentColor", Colors.Red);

        HarmonyApplication.EnsureCurrent(resources);

        var label = new Label();
        label.SetDynamicResource(Label.TextColorProperty, "TestAccentColor");

        Assert.Equal(Colors.Red, label.TextColor);
    }

    [Fact]
    public void EnsureCurrent_WithApplication_ReusesInstance()
    {
        var app = new Application();

        var result = HarmonyOS.Maui.Hosting.HarmonyApplication.EnsureCurrent(null, app);

        Assert.Same(app, result);
        Assert.Same(app, Application.Current);
    }
}
