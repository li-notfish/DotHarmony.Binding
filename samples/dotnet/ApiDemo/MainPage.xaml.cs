// MainPage 的 code-behind：API 绑定 demo 的入口菜单。
using HarmonyDeviceInfo = HarmonyOS.Bindings.Api.DeviceInfo;
using Microsoft.Maui.Controls;

namespace ApiDemo;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        try
        {
            DeviceLabel.Text = $"{HarmonyDeviceInfo.Brand} {HarmonyDeviceInfo.ProductModel} · {HarmonyDeviceInfo.OsFullName}";
        }
        catch (Exception ex)
        {
            DeviceLabel.Text = "deviceInfo FAILED: " + ex.Message;
        }
    }

    private void OnTsfnTestClicked(object? sender, EventArgs e)
    {
        // M2 TSFN 最小实验：后台 .NET 线程经 TSFN 排队回 JS 线程回调。
        // report 在 JS（宿主主）线程触发，可直接更新控件。
        TsfnLabel.Text = "TSFN: running...";
        HarmonyOS.Bindings.Runtime.TsfnExperiment.Run(s => TsfnLabel.Text = s);
    }

    private void OnAsyncDemoClicked(object? sender, EventArgs e)
    {
        Navigation.PushAsync(new AsyncDemoPage()).FireAndForgetNavigation();
    }

    private void OnModuleVerifyClicked(object? sender, EventArgs e)
    {
        Navigation.PushAsync(new ModuleVerifyPage()).FireAndForgetNavigation();
    }
}

internal static class NavigationFireAndForget
{
    /// <summary>PushAsync 未观察异常的兜底（导航失败时打到 hilog 而非静默）</summary>
    public static void FireAndForgetNavigation(this Task task)
    {
        task.ContinueWith(t =>
        {
            if (t.Exception is not null)
                System.Diagnostics.Debug.WriteLine($"navigation failed: {t.Exception.InnerException}");
        }, TaskScheduler.Default);
    }
}
