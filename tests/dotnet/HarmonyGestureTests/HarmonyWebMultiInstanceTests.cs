using System;
using System.Collections.Generic;
using HarmonyOS.Maui.Blazor;
using HarmonyOS.Maui.Web;
using Xunit;

namespace HarmonyGestureTests;

/// <summary>
/// 多实例路由与流式响应的契约测试。
/// 运行时不加载 ArkWeb，因此这里锁定「按 tag 隔离」与「流式/缓冲选择」的纯逻辑，
/// 设备行为由 BlazorHybridApp 的 MultiInstancePage / Upload 页人工验证。
/// </summary>
public sealed class HarmonyWebMultiInstanceTests
{
    [Fact]
    public void GetSurface_IsolatesByTag()
    {
        // 未注册的 tag 必须返回 null，不能回落到默认 surface——
        // 否则两个 BlazorWebView 会共享同一个文件提供者与 IPC 通道。
        Assert.Null(HarmonyWebRuntime.GetSurface("tag-a"));
        Assert.Null(HarmonyWebRuntime.GetSurface("tag-b"));
        Assert.Null(HarmonyWebRuntime.GetSurface(HarmonyWebRuntime.DefaultTag));
    }

    [Fact]
    public void EnsureSurface_DoesNotThrowWithoutHostFactory()
    {
        // 无宿主工厂时只应安静返回 null（单实例场景/未接入 factory 的宿主）
        var surface = HarmonyWebRuntime.EnsureSurface("tag-without-host");
        Assert.Null(surface);
    }

    [Fact]
    public void WebViewFactorySink_IsUnavailableWithoutHost()
    {
        // 测试宿主未提供 libentry.so 导出：能力自检应返回 false 而非抛错
        Assert.False(HarmonyWebViewFactorySink.IsAvailable);
        Assert.False(HarmonyWebViewFactorySink.TryCreate("any-tag"));
    }

    [Fact]
    public void Response_ChoosesStreamWhenProvided()
    {
        // 大文件路径：有 ContentStream 时不写 Body，runtime 走分块响应
        using var stream = new MemoryStream(new byte[1024]);
        var large = new HarmonyWebResponse(200, "application/octet-stream", Array.Empty<byte>())
        {
            ContentStream = stream,
        };
        Assert.True(large.ContentStream is not null);
        Assert.Empty(large.Body);

        // 小文件路径：无 ContentStream，走一次性缓冲
        var small = new HarmonyWebResponse(200, "text/html", "x"u8.ToArray());
        Assert.Null(small.ContentStream);
        Assert.NotEmpty(small.Body);
    }
}
