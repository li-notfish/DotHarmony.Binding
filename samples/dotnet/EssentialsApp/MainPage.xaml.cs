// Essentials 全部 16 服务的标准入口验证：DeviceInfo / DeviceDisplay / AppInfo / Clipboard / Preferences / Battery /
// Vibration / Connectivity / FileSystem / Launcher / Browser / PhoneDialer / Share / Email / SecureStorage / MainThread。
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
        RefreshFileSystem();
    }

    private void RefreshFileSystem()
    {
        try
        {
            var data = FileSystem.Current.AppDataDirectory;
            var cache = FileSystem.Current.CacheDirectory;
            FileSystemLabel.Text = $"files=…{data[(data.LastIndexOf('/') + 1)..]} · cache=…{cache[(cache.LastIndexOf('/') + 1)..]}";
        }
        catch (Exception ex)
        {
            FileSystemLabel.Text = "FileSystem FAILED: " + ex.Message;
        }
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
                $"pkg {AppInfo.Current.PackageName} v{AppInfo.Current.VersionString} (build {AppInfo.Current.BuildString}) · {AppInfo.Current.PackagingModel} · theme={AppInfo.Current.RequestedTheme}";
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

    private void OnVibrateClicked(object? sender, EventArgs e)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            VibrationLabel.Text = "vibrating 500ms (touch the device to feel it)";
        }
        catch (Exception ex)
        {
            VibrationLabel.Text = "vibration FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private void OnConnectivityClicked(object? sender, EventArgs e)
    {
        try
        {
            var c = Connectivity.Current;
            var profiles = string.Join("/", c.ConnectionProfiles);
            ConnectivityLabel.Text = $"network: {c.NetworkAccess} · [{profiles}]";

            // 事件订阅验证：模拟器开关 Wi-Fi / 飞行模式即可触发
            Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        }
        catch (Exception ex)
        {
            ConnectivityLabel.Text = "connectivity FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var profiles = string.Join("/", e.ConnectionProfiles);
        ConnectivityLabel.Text = $"event! {e.NetworkAccess} · [{profiles}]";
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        try
        {
            AppInfo.Current.ShowSettingsUI();
        }
        catch (Exception ex)
        {
            KeepScreenOnLabel.Text = "settings FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private void OnKeepScreenOnClicked(object? sender, EventArgs e)
    {
        try
        {
            DeviceDisplay.Current.KeepScreenOn = !DeviceDisplay.Current.KeepScreenOn;
            KeepScreenOnLabel.Text = $"keepScreenOn={DeviceDisplay.Current.KeepScreenOn}";
        }
        catch (Exception ex)
        {
            KeepScreenOnLabel.Text = "keepScreenOn FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private void OnBrowserClicked(object? sender, EventArgs e)
    {
        _ = Browser.Default.OpenAsync("https://example.com");
    }

    private async void OnShareClicked(object? sender, EventArgs e)
    {
        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = "hello from DotHarmony.Binding Essentials on HarmonyOS",
                Title = "Share via HarmonyOS",
            });
        }
        catch (Exception ex)
        {
            SecureStorageLabel.Text = "share FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private async void OnEmailClicked(object? sender, EventArgs e)
    {
        try
        {
            await Email.Default.ComposeAsync(new EmailMessage
            {
                Subject = "hello-essentials",
                Body = "composed on HarmonyOS",
                To = ["example@example.com"],
            });
        }
        catch (Exception ex)
        {
            SecureStorageLabel.Text = "email FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private async void OnSecureStorageClicked(object? sender, EventArgs e)
    {
        try
        {
            await SecureStorage.Default.SetAsync("secure.demo", $"secret-{DateTime.Now:HHmmss}");
            var value = await SecureStorage.Default.GetAsync("secure.demo");
            SecureStorageLabel.Text = $"secure roundtrip: {(value is null ? "MISSING" : value)}";
        }
        catch (Exception ex)
        {
            SecureStorageLabel.Text = "secure storage FAILED: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    private void OnRefreshDisplayClicked(object? sender, EventArgs e)
    {
        RefreshDisplayInfo();
        RefreshDeviceInfo();
    }
}
