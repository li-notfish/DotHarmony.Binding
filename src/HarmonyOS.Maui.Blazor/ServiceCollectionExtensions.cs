#nullable enable
using System.Linq;
using HarmonyOS.Maui.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HarmonyOS.Maui.Blazor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseHarmonyBlazorWebView(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        HarmonyIpcJsonOptions.Configure();
        services.AddMauiBlazorWebView();
        HarmonyHandlerFactory.Register<HarmonyBlazorWebView>(() => new HarmonyBlazorWebViewHandler());
        return services;
    }
}
