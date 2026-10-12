using HarmonyOS.Maui.Blazor;
using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace BlazorHybridApp;

public static class Program
{
    public static void Register()
    {
        var builder = MauiApp.CreateBuilder();
        builder.Services.UseHarmonyBlazorWebView();
        builder.UseHarmonyApp<BlazorApp>();
        builder.Build().RunHarmony();
    }
}
