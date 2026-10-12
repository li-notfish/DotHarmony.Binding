using HarmonyOS.Maui.Web;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

public sealed class HarmonyWebViewHandlerTests
{
    [Fact]
    public void Mapper_Contains_WebViewSemantics()
    {
        var keys = HarmonyWebViewHandler.Mapper.GetKeys().ToArray();

        Assert.Contains(nameof(WebView.Source), keys);
        Assert.Contains(nameof(WebView.UserAgent), keys);
    }

    [Fact]
    public void Commands_Contain_NavigationAndScriptSemantics()
    {
        var commands = HarmonyWebViewHandler.Commands.GetCommand(nameof(IWebView.GoBack));

        Assert.NotNull(commands);
        Assert.NotNull(HarmonyWebViewHandler.Commands.GetCommand(nameof(IWebView.GoForward)));
        Assert.NotNull(HarmonyWebViewHandler.Commands.GetCommand(nameof(IWebView.Reload)));
        Assert.NotNull(HarmonyWebViewHandler.Commands.GetCommand(nameof(IWebView.Eval)));
        Assert.NotNull(HarmonyWebViewHandler.Commands.GetCommand(nameof(IWebView.EvaluateJavaScriptAsync)));
    }
}
