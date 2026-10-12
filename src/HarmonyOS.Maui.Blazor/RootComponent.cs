#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;

namespace HarmonyOS.Maui.Blazor;

public sealed class HarmonyRootComponent
{
    public string? Selector { get; set; }
    public Type? ComponentType { get; set; }
    public IDictionary<string, object?>? Parameters { get; set; }

    internal Task AddToWebViewManagerAsync(WebViewManager manager)
    {
        if (string.IsNullOrWhiteSpace(Selector))
            throw new InvalidOperationException($"{nameof(HarmonyRootComponent)} requires a Selector.");
        if (ComponentType is null)
            throw new InvalidOperationException($"{nameof(HarmonyRootComponent)} requires a ComponentType.");

        var parameters = Parameters is null
            ? ParameterView.Empty
            : ParameterView.FromDictionary(Parameters);
        return manager.AddRootComponentAsync(ComponentType, Selector, parameters);
    }

    internal Task RemoveFromWebViewManagerAsync(WebViewManager manager)
    {
        if (string.IsNullOrWhiteSpace(Selector))
            throw new InvalidOperationException($"{nameof(HarmonyRootComponent)} requires a Selector.");
        return manager.RemoveRootComponentAsync(Selector);
    }
}
