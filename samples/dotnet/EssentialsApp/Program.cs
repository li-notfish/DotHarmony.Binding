// 应用装配（对应 MAUI 模板的 MauiProgram.CreateMauiApp）：
// 单页 Essentials 验证应用——DeviceInfo / DeviceDisplay / AppInfo / Clipboard 四服务，
// 实现由 HarmonyOS.Maui 启动时注入（RootBuilder 时机，见 HANDLERS.md §6.2）。
// 平台启动代码见 Platforms/HarmonyOS/HarmonyExports.cs。
using Microsoft.Maui.Controls;
using HarmonyOS.Maui.Hosting;

namespace EssentialsApp;

public static class Program
{
    public static void Register() => MauiHarmonyHost.Run(() => new MainPage());
}
