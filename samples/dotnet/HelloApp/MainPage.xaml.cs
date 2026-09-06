// MainPage.xaml 的 code-behind：XamlC/SourceGen 在编译期生成 InitializeComponent，
// 运行时零反射（NativeAOT 安全）。
using HarmonyDeviceInfo = HarmonyOS.Bindings.Api.DeviceInfo;
using Microsoft.Maui.Controls;

namespace HelloApp;

public partial class MainPage : ContentPage
{
    private int _clicks;

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
}
