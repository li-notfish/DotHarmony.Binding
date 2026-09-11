// MainPage.xaml 的 code-behind：XamlC/SourceGen 在编译期生成 InitializeComponent，
// 运行时零反射（NativeAOT 安全）。
using HarmonyDeviceInfo = HarmonyOS.Bindings.Api.DeviceInfo;
using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class MainPage : ContentPage
{
    private int _clicks;
    private int _visits;
    private int _appearing;
    private int _disappearing;

    protected override void OnAppearing()
    {
        _appearing++;
        LifecycleLabel.Text = $"Main: {_appearing}A / {_disappearing}D";
    }

    protected override void OnDisappearing()
    {
        _disappearing++;
        // 页面已不可见，仅留痕迹；下一页的 LifecycleLabel 验证模态/导航切换时序
        System.Diagnostics.Debug.WriteLine($"Main disappearing #{_disappearing}");
    }

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

    private void OnTapClicked(object? sender, EventArgs e)
    {
        _clicks++;
        TapButton.Text = $"XAML clicked {_clicks}x";
    }

    private void OnOpenSecondClicked(object? sender, EventArgs e)
    {
        // 传参验证页面状态：每次进入自增，Pop 回来再进应延续（节点保留语义）
        // 走 MAUI 标准 INavigation API（NavigationPage 协议）
        Navigation.PushAsync(new SecondPage(++_visits)).FireAndForgetNavigation();
    }

    private void OnOpenLayoutDemoClicked(object? sender, EventArgs e)
    {
        Navigation.PushAsync(new LayoutDemoPage()).FireAndForgetNavigation();
    }

    private void OnOpenControlsDemoClicked(object? sender, EventArgs e)
    {
        Navigation.PushAsync(new ControlsDemoPage()).FireAndForgetNavigation();
    }

    private void OnOpenModalClicked(object? sender, EventArgs e)
    {
        // 模态：标准 INavigation API（经 RootNavigationAdapter 转接到宿主模态层）
        Navigation.PushModalAsync(new ModalPage()).FireAndForgetNavigation();
    }

    private void OnTsfnTestClicked(object? sender, EventArgs e)
    {
        // M2 TSFN 最小实验：后台 .NET 线程经 TSFN 排队回 JS 线程回调。
        // report 在 JS（宿主主）线程触发，可直接更新控件。
        TsfnLabel.Text = "TSFN: running...";
        HarmonyOS.Bindings.Runtime.TsfnExperiment.Run(s => TsfnLabel.Text = s);
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
