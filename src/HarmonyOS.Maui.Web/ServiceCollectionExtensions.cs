#nullable enable
using HarmonyOS.Maui.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace HarmonyOS.Maui.Web;

public static class ServiceCollectionExtensions
{
    public static void UseHarmonyWebView(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        HarmonyHandlerFactory.Register<WebView>(() => new HarmonyWebViewHandler());
    }
}
