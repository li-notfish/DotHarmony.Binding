// 应用装配（对应 MAUI 模板的 MauiProgram.CreateMauiApp）：
// RunApplication 路径 = 真实 Application 宿主（Resources/UserAppTheme 随应用自带，
// 根 Window/Page 经 IApplication.CreateWindow(null) 取得）。
// 双入口开关（同一代码切换 NavigationPage/AppShell 两种入口）：
//   UseShell = false → NavigationPage(MainPage)：默认教学入口（XAML/导航/手势/布局）
//   UseShell = true  → AppShell（VerificationShell：Shell 导航回归，供脚本/专项验证）
// UI 声明：MainPage.xaml（默认入口）+ VerificationShell.cs（Shell 入口，代码装配）。
// 平台启动代码见 Platforms/HarmonyOS/HarmonyExports.cs。
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using HarmonyOS.Maui.Hosting;

namespace HelloApp;

public static class Program
{
    /// <summary>双入口开关：false = NavigationPage(MainPage，默认)；true = Shell 验证宿主。</summary>
    internal static bool UseShell = false;

    public static void Register()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseHarmonyApp<VerificationApp>();
        // builder.Services.AddSingleton<IMyService, MyService>();
        builder.Build().RunHarmony();
    }
}
