// Official MauiAppBuilder bridge: keep standard hosting while routing handler
// registrations into HarmonyOS' HarmonyHandlerFactory.
#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyOS.Maui.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace HarmonyOS.Maui.Hosting;

/// <summary>
/// Official MauiAppBuilder extensions for HarmonyOS. Prefer these over the
/// HarmonyMauiAppBuilder shim when third-party libraries expect standard MAUI hosting.
/// </summary>
public static class HarmonyAppBuilderExtensions
{
    /// <summary>Registers TApp as IApplication without adding official platform handlers.</summary>
    public static MauiAppBuilder UseHarmonyApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder)
        where TApp : Microsoft.Maui.Controls.Application
        => builder.UseHarmonyApp<TApp>(null);

    /// <summary>Registers TApp as IApplication with a DI-backed factory.</summary>
    public static MauiAppBuilder UseHarmonyApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(
        this MauiAppBuilder builder,
        Func<IServiceProvider, TApp>? implementationFactory)
        where TApp : Microsoft.Maui.Controls.Application
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (implementationFactory is null)
            builder.Services.TryAddSingleton<TApp>();
        else
            builder.Services.TryAddSingleton<TApp>(implementationFactory);

        builder.Services.TryAddSingleton<IApplication>(sp => sp.GetRequiredService<TApp>());
        return builder;
    }

    /// <summary>
    /// Attaches the built MauiApp to the HarmonyOS host and chains its services
    /// into HarmonyMauiContext.
    /// </summary>
    public static void RunHarmony(this MauiApp app)
    {
        ArgumentNullException.ThrowIfNull(app);
        HarmonyMauiContext.RegisterServiceProvider(app.Services);

        var handlers = app.Services.GetService<IMauiHandlersCollection>();
        if (handlers is not null)
            HarmonyHandlerBridge.Register(app.Services, handlers);

        var application = app.Services.GetRequiredService<IApplication>() as Microsoft.Maui.Controls.Application
            ?? throw new InvalidOperationException("IApplication must resolve to Controls.Application");
        MauiHarmonyHost.RunApplication(() => application);
    }
}

/// <summary>Maps third-party IMauiHandlersCollection registrations into the Harmony registry.</summary>
internal static class HarmonyHandlerBridge
{
    // Official Controls/Core handlers are not usable on HarmonyOS and must not override
    // the built-in Harmony handlers.
    private static readonly HashSet<string> ExcludedAssemblies = new(StringComparer.OrdinalIgnoreCase)
    {
        "Microsoft.Maui.Controls",
        "Microsoft.Maui",
        "Microsoft.Maui.Core",
        "Microsoft.Maui.Essentials",
    };

    public static void Register(IServiceProvider services, IMauiHandlersCollection handlers)
    {
        foreach (var descriptor in handlers)
        {
            var viewType = descriptor.ServiceType;
            if (!typeof(Microsoft.Maui.Controls.Element).IsAssignableFrom(viewType))
                continue;

            Func<IElementHandler> factory;
            if (descriptor.ImplementationType is { } handlerType)
            {
                if (!typeof(IElementHandler).IsAssignableFrom(handlerType) || IsExcluded(handlerType))
                    continue;
                factory = () => (IElementHandler)Activator.CreateInstance(handlerType)!;
            }
            else if (descriptor.ImplementationInstance is IElementHandler instance)
            {
                factory = () => instance;
            }
            else if (descriptor.ImplementationFactory is { } implementationFactory)
            {
                factory = () => (IElementHandler)implementationFactory(services);
            }
            else
            {
                continue;
            }

            HarmonyHandlerFactory.Register(viewType, factory);
        }
    }

    private static bool IsExcluded(Type handlerType)
        => ExcludedAssemblies.Contains(handlerType.Assembly.GetName().Name ?? string.Empty);
}
