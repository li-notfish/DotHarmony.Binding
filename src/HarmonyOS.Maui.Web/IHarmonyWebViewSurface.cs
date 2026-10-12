#nullable enable
using System;
using System.Threading.Tasks;

namespace HarmonyOS.Maui.Web;

public interface IHarmonyWebViewSurface
{
    Task InitializeAsync();
    Task LoadUrlAsync(string url);
    Task LoadDataAsync(string html, string baseUrl);
    Task<string> RunScriptAsync(string script);
    void RegisterMessageHandler(Action<string> handler);
    void SetSchemeHandler(string scheme, Func<HarmonyWebRequest, HarmonyWebResponse> handler);
    HarmonyWebResponse? InterceptRequest(HarmonyWebRequest request);
    void ClearSchemeHandler();
}

public sealed record HarmonyWebRequest(string Url, string Method);

public sealed record HarmonyWebResponse(int StatusCode, string ContentType, byte[] Body);
