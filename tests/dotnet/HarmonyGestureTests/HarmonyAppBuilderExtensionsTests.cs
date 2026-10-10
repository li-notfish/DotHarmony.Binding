using HarmonyOS.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Xunit;

namespace HarmonyGestureTests;

public class HarmonyAppBuilderExtensionsTests
{
    private sealed class TestApp : Application { }

    private sealed class CustomView : View { }

    private sealed class CustomHandler : IElementHandler
    {
        public object? PlatformView => null;
        public IElement? VirtualView => null;
        public IMauiContext? MauiContext => null;
        public void SetMauiContext(IMauiContext mauiContext) { }
        public void SetVirtualView(IElement view) { }
        public void UpdateValue(string property) { }
        public void Invoke(string command, object? args) { }
        public void DisconnectHandler() { }
    }

    [Fact]
    public void UseHarmonyApp_RegistersApplicationFromServices()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseHarmonyApp<TestApp>();
        using var app = builder.Build();

        Assert.IsType<TestApp>(app.Services.GetRequiredService<IApplication>());
    }

    [Fact]
    public void HandlerBridge_RegistersThirdPartyHandlers()
    {
        HarmonyOS.Maui.Handlers.HarmonyHandlerFactory.Unregister<CustomView>();
        var builder = MauiApp.CreateBuilder();
        builder.UseHarmonyApp<TestApp>();
        builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<CustomView, CustomHandler>());
        using var app = builder.Build();

        HarmonyHandlerBridge.Register(
            app.Services,
            app.Services.GetRequiredService<IMauiHandlersCollection>());

        var factory = HarmonyOS.Maui.Handlers.HarmonyHandlerFactory.TryResolveRegistered(typeof(CustomView));
        Assert.NotNull(factory);
        Assert.IsType<CustomHandler>(factory());
    }
}
