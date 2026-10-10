// HarmonyMauiAppBuilder：UseMauiApp 的最小等价引导。
// 鸿蒙宿主不走 Microsoft.Maui.Hosting.MauiAppBuilder（其平台引导与原生宿主耦合），
// 此处提供 API 形状对齐的最小实现：ConfigureMauiHandlers / ConfigureServices / Build。
// Handler 注册落到 HarmonyHandlerFactory 开放注册表；服务落到 MS.DI 容器并作为
// HarmonyMauiContext 的链式解析口（内置 AnimationManager → 注册表 → DI 容器）。
// 第三方库（如 CommunityToolkit 的平台无关组件）的 ConfigureMauiHandlers 委托可直接复用。
#nullable enable
using System;
using HarmonyOS.Maui.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace HarmonyOS.Maui.Hosting;

/// <summary>Handler 注册收集器（MauiHandlersCollectionExtensions.AddHandler 的最小等价）。</summary>
public sealed class HarmonyMauiHandlersCollection
{
    /// <summary>注册 TView 的 Handler（无参构造）。同类型后注册覆盖先注册。</summary>
    public void AddHandler<TView, THandler>()
        where TView : Microsoft.Maui.Controls.Element
        where THandler : IElementHandler, new()
        => HarmonyHandlerFactory.Register<TView>(() => new THandler());

    /// <summary>注册 TView 的 Handler 工厂（需要构造参数/容器解析时用）。</summary>
    public void AddHandler<TView>(Func<IElementHandler> factory)
        where TView : Microsoft.Maui.Controls.Element
        => HarmonyHandlerFactory.Register<TView>(factory);
}

/// <summary>最小 UseMauiApp 等价引导：服务收集 + Handler 注册 + Build。</summary>
public sealed class HarmonyMauiAppBuilder
{
    private readonly HarmonyMauiHandlersCollection _handlers = new();

    private HarmonyMauiAppBuilder() { }

    public static HarmonyMauiAppBuilder CreateBuilder() => new();

    /// <summary>应用服务集合（Build 时构建为 ServiceProvider 并接入 HarmonyMauiContext）。</summary>
    public IServiceCollection Services { get; } = new ServiceCollection();

    /// <summary>注册自定义/第三方 Handler（对齐 MauiAppBuilder.ConfigureMauiHandlers）。</summary>
    public HarmonyMauiAppBuilder ConfigureMauiHandlers(Action<HarmonyMauiHandlersCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_handlers);
        return this;
    }

    /// <summary>注册应用服务（对齐 MauiAppBuilder 的 Services 配置委托）。</summary>
    public HarmonyMauiAppBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Services);
        return this;
    }

    /// <summary>构建服务提供者并接入 HarmonyMauiContext（链式解析末段）。</summary>
    public HarmonyMauiApp Build()
    {
        var provider = Services.BuildServiceProvider();
        HarmonyMauiContext.RegisterServiceProvider(provider);
        return new HarmonyMauiApp(provider);
    }
}

/// <summary>HarmonyMauiAppBuilder.Build() 的产物：持有应用级服务提供者。</summary>
public sealed class HarmonyMauiApp
{
    internal HarmonyMauiApp(IServiceProvider services) => Services = services;

    public IServiceProvider Services { get; }
}
