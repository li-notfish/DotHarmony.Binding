#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HarmonyOS.Maui.Web;

/// <summary>
/// 平台 WebView surface 抽象。实现方为 ArkWeb（ArkTS Web 组件 + NDK 通道）。
/// 生命周期信号只描述「ArkWeb 侧事实」：PageEnded（主框架加载完成）、NavigationStarting（导航拦截）、
/// ProxyReady（proxy 注册窗口）。Blazor 启动门控一律以 PageEnded + __bwv 握手为准。
/// </summary>
public interface IHarmonyWebViewSurface
{
    /// <summary>ArkWeb JS proxy 注册窗口信号（NDK valid 回调）。实测语义是"现在可以注册 proxy"，
    /// 不是"渲染就绪"——不能用它门控 Blazor.start()，保留供 NDK 通道化（RegisterJavaScriptProxy）使用。</summary>
    event Action? ProxyReady;

    /// <summary>proxy valid 回调是否已发生过。仅诊断/重放用途，同上不用于启动门控。</summary>
    bool IsProxyReady { get; }

    /// <summary>主框架页面加载完成（Web 组件 onPageEnd）。Blazor 启动的确定性信号源。</summary>
    event Action<string>? PageEnded;

    /// <summary>导航请求开始（Web 组件 onLoadIntercept）；返回 true 表示拦截该请求。</summary>
    event Func<string, bool>? NavigationStarting;

    /// <summary>
    /// 把 BuilderNode 产出的 Web 节点挂进 .NET 侧的容器节点（parent），使 Web 参与 MAUI 布局，
    /// 而不是以全屏覆盖方式存在于宿主页。parent/child 均为 ArkUI_NodeHandle。
    /// 返回 true 表示挂载成功；surface 未由 BuilderNode 承载时返回 false（回退全屏模式）。
    /// </summary>
    bool TryAttachPlatformNode(IntPtr parentHandle);

    /// <summary>从容器节点摘除 Web 节点（handler 断连时调用）。</summary>
    void DetachPlatformNode();

    /// <summary>把 MAUI 布局算出的容器尺寸同步给 Web 节点（vp）。</summary>
    void SetPlatformNodeSize(float widthVp, float heightVp);

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

public sealed record HarmonyWebResponse(int StatusCode, string ContentType, byte[] Body)
{
    /// <summary>响应头（可为 null：仅 Content-Type 走 ContentType 字段）。</summary>
    public IDictionary<string, string>? Headers { get; init; }
}
