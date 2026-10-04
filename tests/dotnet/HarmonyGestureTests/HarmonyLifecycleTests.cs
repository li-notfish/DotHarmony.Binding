using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

public class HarmonyLifecycleTests
{
    [Fact]
    public void LifecycleController_MapsAbilityEvents_WithoutRepeatingTerminalEvents()
    {
        HarmonyLifecycleController.Reset();
        var window = new Window();
        var events = new List<string>();
        window.Created += (_, _) => events.Add("Created");
        window.Resumed += (_, _) => events.Add("Resumed");
        window.Activated += (_, _) => events.Add("Activated");
        window.Deactivated += (_, _) => events.Add("Deactivated");
        window.Stopped += (_, _) => events.Add("Stopped");
        window.Destroying += (_, _) => events.Add("Destroying");

        HarmonyLifecycleController.Attach(window);
        var controller = HarmonyLifecycleController.Current;
        Assert.NotNull(controller);

        controller!.Created();
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Foreground);
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Foreground);
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Background);
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Background);
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Destroy);
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Destroy);

        Assert.Equal(
            new[] { "Created", "Resumed", "Activated", "Deactivated", "Stopped", "Destroying" },
            events);
    }

    [Fact]
    public void LifecycleController_DefersForegroundUntilWindowIsCreated()
    {
        HarmonyLifecycleController.Reset();
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Foreground);

        var window = new Window();
        var events = new List<string>();
        window.Created += (_, _) => events.Add("Created");
        window.Resumed += (_, _) => events.Add("Resumed");
        window.Activated += (_, _) => events.Add("Activated");

        HarmonyLifecycleController.Attach(window);
        HarmonyLifecycleController.Current!.Created();

        Assert.Equal(new[] { "Created", "Resumed", "Activated" }, events);
    }

    [Fact]
    public void LifecycleController_ClearsPendingForegroundWhenTerminalEventArrives()
    {
        HarmonyLifecycleController.Reset();
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Foreground);
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Background);
        HarmonyLifecycleController.Notify(HarmonyLifecycleController.Destroy);

        var window = new Window();
        var events = new List<string>();
        window.Created += (_, _) => events.Add("Created");
        window.Resumed += (_, _) => events.Add("Resumed");
        window.Activated += (_, _) => events.Add("Activated");

        HarmonyLifecycleController.Attach(window);
        HarmonyLifecycleController.Current!.Created();

        Assert.Equal(new[] { "Created" }, events);
    }
}
