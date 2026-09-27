using HarmonyOS.Maui.Hosting;

namespace WeatherTwentyOne;

public static class Program
{
    public static void Register() => MauiHarmonyHost.RunApplication(() => new App());
}
