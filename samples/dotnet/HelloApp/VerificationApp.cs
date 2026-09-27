// 真实 Application 宿主验证（RunApplication 路径）：
// 应用自带 Resources/UserAppTheme/MainPage，宿主只负责 CreateWindow(null) → 挂屏。
using HarmonyOS.Interop;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace HelloApp;

public class VerificationApp : Application
{
    public VerificationApp()
    {
        Resources["ProbeAccent"] = Colors.Purple;
        UserAppTheme = AppTheme.Light;
        MainPage = Program.UseShell ? new AppShell() : new NavigationPage(new MainPage());
        HiLog.Info("VProbe", $"[V][HOST] VerificationApp built (shell={Program.UseShell})");
    }
}
