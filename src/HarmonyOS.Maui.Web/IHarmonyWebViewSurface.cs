#nullable enable
using System;
using System.Threading.Tasks;

namespace HarmonyOS.Maui.Web;

public interface IHarmonyWebViewSurface
{
    /// <summary>ArkWeb JS proxy 注册窗口信号（NDK valid 回调）。实测语义是"现在可以注册 proxy"，
    /// 不是"渲染就绪"——不能用它门控 Blazor.start()，保留供 NDK 通道化（RegisterJavaScriptProxy）使用。</summary>
    event Action? ProxyReady;

    /// <summary>proxy valid 回调是否已发生过。仅诊断/重放用途，同上不用于启动门控。</summary>
    bool IsProxyReady { get; }

    Task InitializeAsync();
    Task LoadUrlAsync(string url);
    Task LoadDataAsync(string html, string baseUrl);
    Task<string> RunScriptAsync(string script);
    void SetCustomUserAgent(string userAgent);
    void RegisterMessageHandler(Action<string> handler);
    void SetSchemeHandler(string scheme, Func<HarmonyWebRequest, HarmonyWebResponse> handler);
    HarmonyWebResponse? InterceptRequest(HarmonyWebRequest request);
    void ClearSchemeHandler();
}

public sealed record HarmonyWebRequest(string Url, string Method);

public sealed record HarmonyWebResponse(int StatusCode, string ContentType, byte[] Body);
