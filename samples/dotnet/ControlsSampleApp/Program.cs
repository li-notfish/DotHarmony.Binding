using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace ControlsSampleApp;

public static class Program
{
    public static void Register()
    {
        HarmonyDispatcher.EnsureRegistered();

        var builder = MauiApp.CreateBuilder();
        builder.Build();
        MauiHarmonyHost.RunApplication(() => new App());
    }
}
