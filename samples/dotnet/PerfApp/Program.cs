using HarmonyOS.Maui.Hosting;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Hosting;

namespace PerfApp;

public static class Program
{
    public static void Register()
    {
        MauiApp.CreateBuilder()
            .UseHarmonyApp<App>()
            .Build()
            .RunHarmony();
    }

    [ModuleInitializer]
    internal static void Init() => Register();
}
