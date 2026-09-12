// 应用装配（对应 MAUI 模板的 MauiProgram.CreateMauiApp）：
// UI 由 MainPage.xaml 声明（XamlC 编译期生成控件树，NativeAOT 零反射），
// 渲染经 HarmonyOS.Maui Handlers 映射到 ArkUI 原生节点。
// 根页为 NavigationPage：页面导航走 MAUI 标准 Navigation.PushAsync/PopAsync
// （经 HarmonyNavigationPageHandler 转接 IStackNavigation 协议到 ArkUI）。
// 平台启动代码见 Platforms/HarmonyOS/HarmonyExports.cs。
using Microsoft.Maui.Controls;
using HarmonyOS.Maui.Hosting;

namespace HelloApp;

public static class Program
{
    public static void Register() => MauiHarmonyHost.Run(() => new NavigationPage(new MainPage()));
}
