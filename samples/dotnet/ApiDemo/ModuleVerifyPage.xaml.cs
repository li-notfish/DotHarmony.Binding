using HarmonyOS.Bindings.Api;
using HarmonyOS.Bindings.Runtime;

namespace ApiDemo;

public partial class ModuleVerifyPage : ContentPage
{
    public ModuleVerifyPage()
    {
        InitializeComponent();
    }

    private void ShowResult(string moduleName, string result)
    {
        ResultLabel.Text = $"[{moduleName}] {result}";
    }

    private void ShowError(string moduleName, Exception ex)
    {
        ResultLabel.Text = $"[{moduleName}] Error: {ex.Message}";
    }

    // Basic Info
    private void OnVerifyDeviceInfo(object? sender, EventArgs e)
    {
        try
        {
            var brand = HarmonyOS.Bindings.Api.DeviceInfo.Brand;
            var manufacture = HarmonyOS.Bindings.Api.DeviceInfo.Manufacture;
            var model = HarmonyOS.Bindings.Api.DeviceInfo.ProductModel;
            var osFullName = HarmonyOS.Bindings.Api.DeviceInfo.OsFullName;
            ShowResult("DeviceInfo", $"Brand={brand}, Model={model}, OS={osFullName}");
        }
        catch (Exception ex) { ShowError("DeviceInfo", ex); }
    }

    private void OnVerifyBatteryInfo(object? sender, EventArgs e)
    {
        try
        {
            var soc = HarmonyOS.Bindings.Api.BatteryInfo.BatterySoc;
            var charging = HarmonyOS.Bindings.Api.BatteryInfo.ChargingStatus;
            ShowResult("BatteryInfo", $"SOC={soc}, Charging={charging}");
        }
        catch (Exception ex) { ShowError("BatteryInfo", ex); }
    }

    private async void OnVerifyDisplay(object? sender, EventArgs e)
    {
        try
        {
            // .NET 风格标准化后的实例包装：Promise<Array<Display>> → Task<DisplayObject[]>
            var display = await HarmonyOS.Bindings.Api.Display.GetDefaultDisplayAsync();

            // 事件模型演示：.NET event 访问器自动配对 on/off（订阅 → 退订，真实事件需等待触发）
            System.Action<double> onChange = _ => { };
            HarmonyOS.Bindings.Api.Display.Change += onChange;
            HarmonyOS.Bindings.Api.Display.Change -= onChange;

            ShowResult("Display", $"#{(int)display.Id} {display.Name} {display.Width}x{display.Height} @{display.DensityDpi}dpi · event ok");
        }
        catch (Exception ex) { ShowError("Display", ex); }
    }

    private void OnVerifySettings(object? sender, EventArgs e)
    {
        try
        {
            // Settings 需要 UIContext，这里仅测试模块加载
            ShowResult("Settings", "Module loaded OK (needs UIContext for API call)");
        }
        catch (Exception ex) { ShowError("Settings", ex); }
    }

    private void OnVerifySensor(object? sender, EventArgs e)
    {
        try
        {
            // 事件触发路径实测：订阅加速度计 → JS 持续触发 → handler 收到类型化载荷 →
            // 第 5 次时在 handler 内退订（全程 JS 线程，无跨线程 NAPI）
            _sensorEventCount = 0;
            void OnAccel(HarmonyOS.Bindings.Api.AccelerometerResponse r)
            {
                _sensorEventCount++;
                if (_sensorEventCount < 5)
                {
                    if (_sensorEventCount == 1)
                        ShowResult("Sensor", $"event #1 x={r.X:F2} y={r.Y:F2} z={r.Z:F2}");
                    return;
                }
                HarmonyOS.Bindings.Api.Sensor.Accelerometer -= OnAccel;
                ShowResult("Sensor", $"event #{_sensorEventCount} x={r.X:F2} y={r.Y:F2} z={r.Z:F2} · received 5, auto-unsubscribed");
            }
            HarmonyOS.Bindings.Api.Sensor.Accelerometer += OnAccel;
            ShowResult("Sensor", "subscribed, waiting for events...");
        }
        catch (Exception ex) { ShowError("Sensor", ex); }
    }

    private int _sensorEventCount;

    // Network
    private void OnVerifyHttp(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Http", "Module loaded OK (needs async for createHttp)");
        }
        catch (Exception ex) { ShowError("Http", ex); }
    }

    private void OnVerifyConnection(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Connection", "Module loaded OK (needs callback for getStatus)");
        }
        catch (Exception ex) { ShowError("Connection", ex); }
    }

    // File
    private void OnVerifyFs(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Fs", "Module loaded OK (needs path for openSync)");
        }
        catch (Exception ex) { ShowError("Fs", ex); }
    }

    private void OnVerifyPicker(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Picker", "Module loaded OK (needs UIContext for select)");
        }
        catch (Exception ex) { ShowError("Picker", ex); }
    }

    private void OnVerifyPreferences(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Preferences", "Module loaded OK (needs context for getPreferences)");
        }
        catch (Exception ex) { ShowError("Preferences", ex); }
    }

    // Multimedia
    private void OnVerifyMedia(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Media", "Module loaded OK (needs async for createMediaLibrary)");
        }
        catch (Exception ex) { ShowError("Media", ex); }
    }

    private void OnVerifyImage(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Image", "Module loaded OK (needs source for createPicture)");
        }
        catch (Exception ex) { ShowError("Image", ex); }
    }

    private void OnVerifyCamera(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Camera", "Module loaded OK (needs context for getCameraManager)");
        }
        catch (Exception ex) { ShowError("Camera", ex); }
    }

    // UI
    private void OnVerifyWindow(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Window", "Module loaded OK (needs context for getWindow)");
        }
        catch (Exception ex) { ShowError("Window", ex); }
    }

    private void OnVerifyRouter(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Router", "Module loaded OK (needs url for pushUrl)");
        }
        catch (Exception ex) { ShowError("Router", ex); }
    }

    private void OnVerifyPromptAction(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("PromptAction", "Module loaded OK (needs UIContext for showDialog)");
        }
        catch (Exception ex) { ShowError("PromptAction", ex); }
    }

    // Others
    private void OnVerifyPasteboard(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Pasteboard", "Module loaded OK (needs context for getPasteboard)");
        }
        catch (Exception ex) { ShowError("Pasteboard", ex); }
    }

    private void OnVerifyRequest(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Request", "Module loaded OK (needs context for upload)");
        }
        catch (Exception ex) { ShowError("Request", ex); }
    }

    private void OnVerifyVibrator(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Vibrator", "Module loaded OK (needs context for startVibrating)");
        }
        catch (Exception ex) { ShowError("Vibrator", ex); }
    }

    private void OnVerifyGeolocation(object? sender, EventArgs e)
    {
        try
        {
            ShowResult("Geolocation", "Module loaded OK (needs callback for getLocation)");
        }
        catch (Exception ex) { ShowError("Geolocation", ex); }
    }

    // 封送专项（2.6 用例）：byte[] → JS ArrayBuffer → byte[] 往返，内容必须一致
    private async void OnVerifyArrayBuffer(object? sender, EventArgs e)
    {
        try
        {
            byte[] payload = [0x01, 0x02, 0x03, 0xF0, 0x0F, 0x7F, 0x80, 0xFF];
            var echoed = await NodeApi.CallMethodAsync<byte[]>(NodeApi.GetGlobal(), "echoArrayBuffer", payload);
            bool ok = echoed.AsSpan().SequenceEqual(payload);
            ShowResult("ArrayBuffer", ok
                ? $"byte[{payload.Length}] round-trip OK: {Convert.ToHexString(echoed)}"
                : $"MISMATCH ({echoed.Length} bytes): {Convert.ToHexString(echoed)}");
        }
        catch (Exception ex) { ShowError("ArrayBuffer", ex); }
    }

    // 封送专项（2.6 用例）：JS Map → JsMap 活视图读取（Count/TryGet/Entries）；
    // JsMap.Create 写入 → JS 侧 forEach 求和回读
    private async void OnVerifyJsMap(object? sender, EventArgs e)
    {
        try
        {
            var map = await NodeApi.CallMethodAsync(
                NodeApi.GetGlobal(), "makeMap"u8,
                static h => new JsMap<double, string>(h));
            string? two = map.TryGet(2, out var twoValue) ? twoValue : null;
            bool readOk = map.Count == 3
                && two == "two"
                && map.Entries().Any(kv => kv.Key == 3 && kv.Value == "three");
            var readMsg = $"read: Count={map.Count} TryGet(2)={two ?? "null"}";

            var written = JsMap<double, double>.Create();
            written.Set(1, 10).Set(2, 20);
            var sum = await NodeApi.CallMethodAsync<double>(NodeApi.GetGlobal(), "sumMap", written);
            var writeOk = sum == 30;

            ShowResult("JsMap", $"{readMsg} · readOk={readOk} · write sum={sum} writeOk={writeOk}");
        }
        catch (Exception ex) { ShowError("JsMap", ex); }
    }
}
