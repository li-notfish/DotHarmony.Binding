// MainPage.xaml 的 code-behind：XamlC/SourceGen 在编译期生成 InitializeComponent，
// 运行时零反射（NativeAOT 安全）。
using HarmonyDeviceInfo = HarmonyOS.Bindings.Api.DeviceInfo;
using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class MainPage : ContentPage
{
    private int _clicks;
    private int _visits;

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
        HarmonyOS.Maui.Hosting.HarmonyNavigation.Push(new SecondPage(++_visits));
    }
}
