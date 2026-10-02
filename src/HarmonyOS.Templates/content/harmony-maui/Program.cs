// App composition (the counterpart of MauiProgram.CreateMauiApp in the MAUI template).
// Platform startup code lives in Platforms/HarmonyOS/HarmonyExports.cs.
using Microsoft.Maui.Controls;
using HarmonyOS.Maui.Hosting;

namespace HarmonyApp1;

public static class Program
{
    public static void Register()
    {
        // 最小 UseMauiApp 等价引导：ConfigureMauiHandlers 注册自定义/第三方 Handler，
        // ConfigureServices 注册应用服务（DI 容器接入 HarmonyMauiContext）
        var builder = HarmonyMauiAppBuilder.CreateBuilder();
        builder.ConfigureMauiHandlers(handlers =>
        {
            // handlers.AddHandler<MyView, MyHandler>();
        });
        builder.ConfigureServices(services =>
        {
            // services.AddSingleton<IMyService, MyService>();
        });
        builder.Build();
        MauiHarmonyHost.RunApplication(() => new App());
    }
}

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
        => new(new MainPage());
}
