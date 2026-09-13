// Essentials 六服务的标准入口验证：DeviceInfo / DeviceDisplay / AppInfo / Clipboard / Preferences / Battery。
// 实现由 HarmonyOS.Maui 启动时注入；netstandard 缺省实现会 throw——本页任何一栏有值即注入生效。
// 剪贴板读权限（READ_PASTEBOARD，user_grant）：首次点击会弹系统授权对话框，
// 允许后回环显示写入文本；拒绝则显示失败原因（可到系统设置改为"始终允许"）。
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

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

    private void OnPreferencesClicked(object? sender, EventArgs e)
    {
        try
        {
            // 先读旧值再写新值：计数器跨重启延续（进程内变量重启即失，能延续的只有持久层）
            var clicks = Preferences.Default.Get("prefs.clicks", 0) + 1;
            Preferences.Default.Set("prefs.clicks", clicks);
            Preferences.Default.Set("prefs.time", DateTime.Now);
            Preferences.Default.Set("prefs.text", $"written at #{clicks}");

            var time = Preferences.Default.Get("prefs.time", DateTime.MinValue);
            var text = Preferences.Default.Get("prefs.text", "missing");
            PreferencesLabel.Text =
                $"clicks={clicks} (persisted across restarts) · time={time:HH:mm:ss.ffffff} · text={text}";
        }
        catch (Exception ex)
        {
            PreferencesLabel.Text = "preferences FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private void OnBatteryClicked(object? sender, EventArgs e)
    {
        try
        {
            var b = Battery.Default;
            BatteryLabel.Text = $"battery: level={b.ChargeLevel:P0} · state={b.State} · source={b.PowerSource} · saver={b.EnergySaverStatus}";

            // 事件订阅验证：模拟器改电量/接拔充电线即可触发 BATTERY_CHANGED
            Battery.Default.BatteryInfoChanged -= OnBatteryChanged;
            Battery.Default.BatteryInfoChanged += OnBatteryChanged;
        }
        catch (Exception ex)
        {
            BatteryLabel.Text = "battery FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private void OnBatteryChanged(object? sender, BatteryInfoChangedEventArgs e)
    {
        BatteryLabel.Text = $"event! level={e.ChargeLevel:P0} · state={e.State} · source={e.PowerSource}";
    }

    private void OnRefreshDisplayClicked(object? sender, EventArgs e)
    {
        RefreshDisplayInfo();
        RefreshDeviceInfo();
    }
}
