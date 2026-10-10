using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;
using Xunit;

namespace HarmonyGestureTests;

public sealed class MainThreadBridgeTests
{
    [Fact]
    public async Task StandardMauiAppBuilder_UsesHarmonyMainThreadBridge()
    {
        HarmonyDispatcher.EnsureRegistered();

        using var app = MauiApp.CreateBuilder().Build();

        Assert.True(MainThread.IsMainThread);

        var executed = false;
        await MainThread.InvokeOnMainThreadAsync(() => executed = true);

        Assert.True(executed);
    }
}
