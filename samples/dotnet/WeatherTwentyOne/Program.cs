using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.Hosting;

namespace WeatherTwentyOne;

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
