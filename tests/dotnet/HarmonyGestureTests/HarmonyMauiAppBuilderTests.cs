using HarmonyOS.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

/// <summary>
/// HarmonyMauiAppBuilder 最小 UseMauiApp 等价引导：
/// ConfigureServices → DI 容器链式解析；ConfigureMauiHandlers → 开放注册表；
/// 内置 AnimationManager 不被覆盖。
/// </summary>
public class HarmonyMauiAppBuilderTests
{
    private interface IGreeting { string Text { get; } }
    private sealed class Greeting : IGreeting { public string Text => "hi"; }

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
    public void ConfigureServices_Build_ServiceResolvableFromMauiContext()
    {
        var builder = HarmonyMauiAppBuilder.CreateBuilder();
        builder.ConfigureServices(s => s.AddSingleton<IGreeting>(new Greeting()));
        builder.Build();

        var resolved = HarmonyMauiContext.Shared.Services.GetService(typeof(IGreeting));
        Assert.IsType<Greeting>(resolved);
    }

    [Fact]
    public void Build_AnimationManagerStillBuiltin_NotOverriddenByDi()
    {
        var builder = HarmonyMauiAppBuilder.CreateBuilder();
        builder.Build();

        // 内置服务优先于 DI 容器
        Assert.IsType<AnimationManager>(HarmonyMauiContext.Shared.Services.GetService(typeof(IAnimationManager)));
    }

    [Fact]
    public void ConfigureMauiHandlers_AddHandler_LandsInOpenRegistry()
    {
        var builder = HarmonyMauiAppBuilder.CreateBuilder();
        builder.ConfigureMauiHandlers(h => h.AddHandler<CustomView, CustomHandler>());

        var factory = HarmonyOS.Maui.Handlers.HarmonyHandlerFactory.TryResolveRegistered(typeof(CustomView));
        Assert.NotNull(factory);
        Assert.IsType<CustomHandler>(factory());
    }
}
