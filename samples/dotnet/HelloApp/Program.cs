// 应用装配（对应 MAUI 模板的 MauiProgram.CreateMauiApp）：
// RunApplication 路径 = 真实 Application 宿主（Resources/UserAppTheme 随应用自带，
// 根 Window/Page 经 IApplication.CreateWindow(null) 取得）。
// 双入口开关（计划：同一代码切换 NavigationPage/AppShell 两种入口）：
//   UseShell = true  → AppShell（Shell 导航：tab/路由/query/模态）
//   UseShell = false → NavigationPage（轻量页面栈）
// UI 声明：MainPage.xaml（既有入口）+ VerificationShell.cs（Shell 入口，代码装配）。
// 平台启动代码见 Platforms/HarmonyOS/HarmonyExports.cs。
using Microsoft.Maui.Controls;
using HarmonyOS.Maui.Hosting;

namespace HelloApp;

public static class Program
{
    /// <summary>双入口开关：true = AppShell；false = NavigationPage。</summary>
    internal static bool UseShell = false;

    public static void Register() => MauiHarmonyHost.RunApplication(() => new VerificationApp());
}
