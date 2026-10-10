// App composition (the counterpart of MauiProgram.CreateMauiApp in the MAUI template).
// Platform startup code lives in Platforms/HarmonyOS/HarmonyExports.cs.
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using HarmonyOS.Maui.Hosting;

namespace HarmonyApp1;

public static class Program
{
    public static void Register()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseHarmonyApp<App>();
        // builder.Services.AddSingleton<IMyService, MyService>();
        builder.Build().RunHarmony();
    }
}

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
        => new(new MainPage());
}
