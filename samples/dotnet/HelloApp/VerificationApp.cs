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
        HiLog.Info("VProbe", $"[V][HOST] VerificationApp built (shell={Program.UseShell})");
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => Program.UseShell ? new Window(new AppShell()) : new Window(new NavigationPage(new MainPage()));
}
