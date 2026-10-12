using HarmonyOS.Maui.Blazor;
using Microsoft.Maui.Controls;

namespace BlazorHybridApp;

public sealed class BlazorApp : Application
{
    public BlazorApp()
    {
        var webView = new HarmonyBlazorWebView
        {
            HostPage = "wwwroot/index.html",
            StartPath = "/counter"
        };
        webView.RootComponents.Add(new HarmonyRootComponent
        {
            Selector = "#app",
            ComponentType = typeof(Routes)
        });

        MainPage = new ContentPage
        {
            Content = webView
        };
    }
}
