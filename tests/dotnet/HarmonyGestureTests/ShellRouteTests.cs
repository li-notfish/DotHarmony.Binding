using HarmonyOS.Maui.Hosting;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

/// <summary>
/// Shell 第一版路由解析（HarmonyShellNavigation.ResolveRoute，纯逻辑）：
/// 绝对/相对路由、注册路由推送、query 解析、ShellContent 页面托管契约。
/// </summary>
public class ShellRouteTests
{
    private static Shell BuildShell()
    {
        var shell = new Shell();
        shell.Items.Add(new ShellItem
        {
            Route = "home",
            Title = "Home",
            Items =
            {
                new Tab
                {
                    Route = "homeTabs",
                    Items = { new ShellContent { Route = "main", Title = "Main", Content = new ContentPage() } }
                }
            }
        });
        shell.Items.Add(new ShellItem
        {
            Route = "browse",
            Title = "Browse",
            Items =
            {
                new Tab
                {
                    Route = "browseTabs",
                    Items =
                    {
                        new ShellContent { Route = "list", Title = "List", Content = new ContentPage() },
                        new ShellContent { Route = "grid", Title = "Grid", Content = new ContentPage() },
                    }
                }
            }
        });
        return shell;
    }

    private sealed class TestDetailPage : ContentPage
    {
    }

    [Fact]
    public void Absolute_ItemRoute_ResolvesItem()
    {
        var shell = BuildShell();

        var match = HarmonyShellNavigation.ResolveRoute(shell, "//browse");

        Assert.NotNull(match);
        Assert.Equal("browse", match!.Item!.Route);
        Assert.Null(match.Section);
        Assert.Null(match.Content);
        Assert.Empty(match.PushRoutes);
    }

    [Fact]
    public void Absolute_FullPath_ResolvesContent()
    {
        var shell = BuildShell();

        var match = HarmonyShellNavigation.ResolveRoute(shell, "//home/homeTabs/main");

        Assert.NotNull(match);
        Assert.Equal("home", match!.Item!.Route);
        Assert.Equal("homeTabs", match.Section!.Route);
        Assert.Equal("main", match.Content!.Route);
    }

    [Fact]
    public void Absolute_RegisteredRoute_PushesOntoCurrentSelection()
    {
        HarmonyShellNavigation.RegisterRoute("route_test_detail", typeof(TestDetailPage));
        var shell = BuildShell();

        var match = HarmonyShellNavigation.ResolveRoute(shell, "//route_test_detail");

        Assert.NotNull(match);
        Assert.Null(match!.Item);
        var route = Assert.Single(match.PushRoutes);
        Assert.Equal("route_test_detail", route);
    }

    [Fact]
    public void Relative_RegisteredRoute_Pushes()
    {
        HarmonyShellNavigation.RegisterRoute("route_test_relative", typeof(TestDetailPage));
        var shell = BuildShell();

        var match = HarmonyShellNavigation.ResolveRoute(shell, "route_test_relative");

        Assert.NotNull(match);
        Assert.Null(match!.Item);
        Assert.Contains("route_test_relative", match.PushRoutes);
    }

    [Fact]
    public void Relative_ContentWithinCurrentItem_Resolves()
    {
        var shell = BuildShell();
        shell.CurrentItem = shell.Items[1]; // browse

        var match = HarmonyShellNavigation.ResolveRoute(shell, "grid");

        Assert.NotNull(match);
        Assert.Equal("browse", match!.Item!.Route);
        Assert.Equal("browseTabs", match.Section!.Route);
        Assert.Equal("grid", match.Content!.Route);
    }

    [Fact]
    public void Absolute_UnknownRoute_ReturnsNull()
    {
        var shell = BuildShell();

        Assert.Null(HarmonyShellNavigation.ResolveRoute(shell, "//no_such_route"));
    }

    [Fact]
    public void Query_IsSplitAndDecoded()
    {
        var shell = BuildShell();

        var match = HarmonyShellNavigation.ResolveRoute(shell, "//home/homeTabs/main?id=42&title=Hello%20World");

        Assert.NotNull(match);
        Assert.Equal("42", match!.Query["id"]);
        Assert.Equal("Hello World", match.Query["title"]);
    }

    [Fact]
    public void ShellContentPage_HostingContract()
    {
        var shell = BuildShell();

        var content = shell.Items[0].Items[0].Items[0];
        var page = ((IShellContentController)content).Page;

        Assert.IsType<ContentPage>(page);
    }
}
