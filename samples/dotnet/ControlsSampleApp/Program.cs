using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace ControlsSampleApp;

public static class Program
{
    public static void Register()
    {
        MauiApp.CreateBuilder()
            .UseHarmonyApp<App>()
            .Build()
            .RunHarmony();
    }
}
