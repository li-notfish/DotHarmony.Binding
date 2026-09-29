using HarmonyOS.Maui.Hosting;
using System.Runtime.CompilerServices;

namespace PerfApp;

public static class Program
{
    public static void Register() => MauiHarmonyHost.RunApplication(() => new App());

    [ModuleInitializer]
    internal static void Init() => Register();
}
