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
        // HarmonyBlazorWebView 继承 WebView：注册表沿继承链向上找，这里显式注册具体类型，
        // 保证解析时命中 Blazor handler 而非 HarmonyWebViewHandler。
        HarmonyHandlerFactory.Register<HarmonyBlazorWebView>(() => new HarmonyBlazorWebViewHandler());
        return services;
    }
}
