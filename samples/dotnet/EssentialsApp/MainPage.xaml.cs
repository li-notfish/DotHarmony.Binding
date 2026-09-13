// Essentials 四服务的标准入口验证：DeviceInfo / DeviceDisplay / AppInfo / Clipboard。
// 实现由 HarmonyOS.Maui 启动时注入；netstandard 缺省实现会 throw——本页任何一栏有值即注入生效。
// 剪贴板读权限（READ_PASTEBOARD，user_grant）：首次点击会弹系统授权对话框，
// 允许后回环显示写入文本；拒绝则显示失败原因（可到系统设置改为"始终允许"）。
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace EssentialsApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        RefreshDeviceInfo();
        RefreshDisplayInfo();
        RefreshAppInfo();
    }

    private void RefreshDeviceInfo()
    {
        try
        {
            DeviceLabel.Text =
                $"{DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model} · {DeviceInfo.Current.VersionString} · {DeviceInfo.Current.Idiom}";
        }
        catch (Exception ex)
        {
            DeviceLabel.Text = "DeviceInfo FAILED: " + ex.Message;
        }
    }

    private void RefreshDisplayInfo()
    {
        try
        {
            var d = DeviceDisplay.Current.MainDisplayInfo;
            DisplayLabel.Text =
                $"display {(int)d.Width}x{(int)d.Height} @{d.Density:F1}x · {d.Orientation}";
        }
        catch (Exception ex)
        {
            DisplayLabel.Text = "DeviceDisplay FAILED: " + ex.Message;
        }
    }

    private void RefreshAppInfo()
    {
        try
        {
            AppInfoLabel.Text =
                $"pkg {AppInfo.Current.PackageName} v{AppInfo.Current.VersionString} (build {AppInfo.Current.BuildString}) · {AppInfo.Current.PackagingModel}";
        }
        catch (Exception ex)
        {
            AppInfoLabel.Text = "AppInfo FAILED: " + ex.Message;
        }
    }

    private async void OnClipboardClicked(object? sender, EventArgs e)
    {
        try
        {
            await Clipboard.Default.SetTextAsync("hello-essentials");
            var text = await Clipboard.Default.GetTextAsync();
            ClipboardLabel.Text = $"roundtrip: hasText={Clipboard.Default.HasText} text={text}";
        }
        catch (Exception ex)
        {
            ClipboardLabel.Text = "clipboard FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private void OnRefreshDisplayClicked(object? sender, EventArgs e)
    {
        RefreshDisplayInfo();
        RefreshDeviceInfo();
    }
}
