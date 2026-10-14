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

        // 走自研 AOT 安全 IPC 接收器——官方 WebViewManager.MessageReceived → IpcReceiver 的
        // JsonElement[] 反序列化在 NativeAOT 下依赖本程序集注册的 converter，只作兜底不可靠。
        _surface.RegisterMessageHandler(message => HarmonyIpcReceiver.Dispatch(this, message));
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
            return;
        }
        StartBlazorAsync();
    }

    private void StartBlazorAsync()
    {
        // 实测（2026-10-14 模拟器）：NDK proxy-valid 回调是"可注册 proxy"的窗口信号，
        // 先于渲染流水线就绪——用它触发 Blazor.start 会复现白屏（OnRenderCompleted TypeError）。
        // 保留 500ms 实测安全值起步；以 __bwv IPC 回执（AttachPage 为首条）做自验证，未确认则重试。
        HarmonyDispatcher.Instance.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
        {
            _ = StartUntilConfirmedAsync();
        });
    }

    private volatile bool _ipcConfirmed;

    internal void NoteBlazorIpc() => _ipcConfirmed = true;

    private async Task StartUntilConfirmedAsync()
    {
        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts && !_ipcConfirmed; attempt++)
        {
            HiLog.Info("HarmonyWeb", $"[blazor-manager] Blazor.start attempt {attempt}");
            try
            {
                await _surface.RunScriptAsync("Blazor.start();").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HiLog.Error("HarmonyWeb", $"[blazor-manager] start failed: {ex.GetType().Name}: {ex.Message}");
            }
            await Task.Delay(1500).ConfigureAwait(false);
        }
        if (!_ipcConfirmed)
            HiLog.Error("HarmonyWeb", "[blazor-manager] Blazor.start unconfirmed after retries; page may stay blank");
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
            using var ms = stream.CanSeek
                ? new MemoryStream((int)Math.Min(stream.Length, int.MaxValue))
                : new MemoryStream();
            stream.CopyTo(ms);
            var contentType = headers.TryGetValue("Content-Type", out var type)
                ? type
                : HarmonyBlazorMimeMap.Resolve(request.Url);
            return new HarmonyWebResponse(statusCode, contentType, ms.ToArray());
        }

        return new HarmonyWebResponse(404, "text/plain", System.Text.Encoding.UTF8.GetBytes("Not Found"));
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
