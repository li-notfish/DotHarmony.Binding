using HarmonyOS.Maui.Hosting;

namespace PerfApp;

public static class Program
{
    public static void Register() => MauiHarmonyHost.RunApplication(() => new App());
}
