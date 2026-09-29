// App composition (the counterpart of MauiProgram.CreateMauiApp in the MAUI template).
// Platform startup code lives in Platforms/HarmonyOS/HarmonyExports.cs.
using Microsoft.Maui.Controls;
using HarmonyOS.Maui.Hosting;

namespace HarmonyApp1;

public static class Program
{
    public static void Register()
        => MauiHarmonyHost.RunApplication(() => new App());
}

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
        => new(new MainPage());
}
