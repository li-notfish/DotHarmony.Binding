using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public static class Program
{
    public static void Register()
    {
        var builder = HarmonyMauiAppBuilder.CreateBuilder();
        builder.Build();
        MauiHarmonyHost.RunApplication(() => new App());
    }
}
