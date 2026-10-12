#nullable enable
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Controls;

namespace HarmonyOS.Maui.Blazor;

public sealed class HarmonyBlazorWebView : View
{
    private readonly JSComponentConfigurationStore _jsComponents = new();
    private readonly HarmonyRootComponentsCollection _rootComponents;

    public HarmonyBlazorWebView()
    {
        _rootComponents = new HarmonyRootComponentsCollection(_jsComponents);
    }

    public string? HostPage { get; set; }

    public string StartPath { get; set; } = "/";

    public HarmonyRootComponentsCollection RootComponents => _rootComponents;

    public JSComponentConfigurationStore JSComponents => _jsComponents;

    public IFileProvider CreateFileProvider(string contentRootDir)
        => new HarmonyBlazorFileProvider(contentRootDir);
}
