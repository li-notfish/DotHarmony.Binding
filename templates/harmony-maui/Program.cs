// 应用装配（对应 MAUI 模板的 MauiProgram.CreateMauiApp）：
// UI 由 MainPage.xaml 声明（XamlC 编译期生成控件树，NativeAOT 零反射），
// 渲染经 HarmonyOS.Maui Handlers 映射到 ArkUI 原生节点。
// 平台启动代码见 Platforms/HarmonyOS/HarmonyExports.cs。
using HarmonyOS.Maui.Hosting;
using Microsoft.Maui.Controls;

namespace HarmonyMauiApp;

public static class Program
{
    public static void Register() => MauiHarmonyHost.Run(() => new NavigationPage(new MainPage()));
}
