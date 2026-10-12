#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HarmonyOS.Maui.Web;
using HarmonyOS.Interop;
using HarmonyOS.Maui.Hosting;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace HarmonyOS.Maui.Blazor;

public sealed class HarmonyBlazorWebViewManager : WebViewManager
{
    public const string AppOrigin = "https://localhost/";
    private static readonly Uri AppOriginUri = new(AppOrigin);

    private readonly IHarmonyWebViewSurface _surface;

    public HarmonyBlazorWebViewManager(
        IHarmonyWebViewSurface surface,
        IServiceProvider services,
        Microsoft.AspNetCore.Components.Dispatcher dispatcher,
        IFileProvider fileProvider,
        JSComponentConfigurationStore jsComponents,
        string hostPageRelativePath)
        : base(services, dispatcher, AppOriginUri, fileProvider, jsComponents, hostPageRelativePath)
    {
        ArgumentNullException.ThrowIfNull(surface);
        _surface = surface;

        _surface.RegisterMessageHandler(message => MessageReceivedInternal(AppOriginUri, message));
        _surface.SetSchemeHandler("https", HandleSchemeRequest);
    }

    protected override void NavigateCore(Uri absoluteUri)
    {
        _ = LoadPageAndStartAsync(absoluteUri);
    }

    private async Task LoadPageAndStartAsync(Uri absoluteUri)
    {
        try
        {
            await _surface.LoadUrlAsync(absoluteUri.ToString()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"[blazor-manager] load url failed: {ex.GetType().Name}: {ex.Message}");
        }
        StartBlazorAsync();
    }

    private void StartBlazorAsync()
    {
        HarmonyDispatcher.Instance.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
        {
            try
            {
                _ = _surface.RunScriptAsync("Blazor.start();");
            }
            catch (Exception ex)
            {
                HiLog.Error("HarmonyWeb", $"[blazor-manager] start failed: {ex.GetType().Name}: {ex.Message}");
            }
        });
    }

    protected override void SendMessage(string message)
    {
        var encoded = JavaScriptEncoder.Default.Encode(message);
        _ = SendToPageAsync(encoded);
    }

    private async Task SendToPageAsync(string encoded)
    {
        try
        {
            await _surface.RunScriptAsync($"window.external.dispatchMessage(\"{encoded}\")").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"[blazor-manager] SendMessage failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    internal HarmonyWebResponse HandleSchemeRequest(HarmonyWebRequest request)
    {
        var hasFileExtension = Path.HasExtension(new Uri(request.Url).AbsolutePath);
        if (TryGetResponseContent(
                request.Url,
                allowFallbackOnHostPage: !hasFileExtension,
                out var statusCode,
                out _,
                out var stream,
                out var headers))
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var contentType = headers.TryGetValue("Content-Type", out var type)
                ? type
                : HarmonyBlazorMimeMap.Resolve(request.Url);
            return new HarmonyWebResponse(statusCode, contentType, ms.ToArray());
        }

        return new HarmonyWebResponse(404, "text/plain", System.Text.Encoding.UTF8.GetBytes("Not Found"));
    }

    internal void MessageReceivedInternal(Uri uri, string message)
    {
        MessageReceived(uri, message);
    }
}

internal static class HarmonyBlazorMimeMap
{
    public static string Resolve(string path)
    {
        var separator = path.IndexOfAny(new[] { '?', '#' });
        if (separator >= 0)
            path = path[..separator];
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".html" => "text/html",
            ".js" => "text/javascript",
            ".css" => "text/css",
            ".json" => "application/json",
            ".wasm" => "application/wasm",
            _ => "application/octet-stream"
        };
    }
}
