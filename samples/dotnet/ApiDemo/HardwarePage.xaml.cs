// ApiDemo 的“真实系统能力”页：HTTP、文件、权限、定位、相机/相册。
// 与 ModuleVerifyPage 的模块加载探针不同，这里的按钮都会走完整调用路径并回报结果。
using System.Text;
using HarmonyOS.Bindings.Api.Net;
using HarmonyOS.Interop;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace ApiDemo;

public partial class HardwarePage : ContentPage
{
    public HardwarePage()
    {
        InitializeComponent();
    }

    private void Report(string message) => ResultLabel.Text = message;

    private async void OnHttpClicked(object? sender, EventArgs e)
    {
        Report("HTTP: requesting https://example.com ...");
        HttpRequest? http = null;
        try
        {
            http = Http.CreateHttp();
            var handle = await http.RequestAsync("https://example.com");
            using var response = new HttpResponseView(handle);
            var (code, body) = response.Read();
            var snippet = body.Replace('\n', ' ').Trim();
            if (snippet.Length > 80) snippet = snippet[..80] + "…";
            Report($"HTTP {code:F0} · {body.Length} chars · {snippet}");
        }
        catch (Exception ex)
        {
            Report($"HTTP FAILED: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            http?.Destroy();
        }
    }

    private async void OnFileClicked(object? sender, EventArgs e)
    {
        try
        {
            var dir = FileSystem.Current.AppDataDirectory;
            var path = Path.Combine(dir, "apidemo-roundtrip.txt");
            var text = $"hello-harmony {DateTime.UtcNow:O}";
            await File.WriteAllTextAsync(path, text);
            var read = await File.ReadAllTextAsync(path);
            Report($"FILE ok · wrote {text.Length}, read {read.Length} chars · {path}");
        }
        catch (Exception ex)
        {
            Report($"FILE FAILED: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private async void OnPermissionClicked(object? sender, EventArgs e)
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            Report($"PERMISSION LocationWhenInUse = {status}");
        }
        catch (Exception ex)
        {
            Report($"PERMISSION FAILED: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private async void OnLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            location ??= await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
            Report(location is null
                ? "GEO no fix (last-known empty and current request returned null)"
                : $"GEO {location.Latitude:F5},{location.Longitude:F5} ±{location.Accuracy:F0}m");
        }
        catch (Exception ex)
        {
            Report($"GEO FAILED: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            var files = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = "ApiDemo pick" });
            var file = files?.FirstOrDefault();
            if (file is null)
            {
                Report("PICK canceled");
                return;
            }
            using var stream = await file.OpenReadAsync();
            Report($"PICK ok · {file.FileName} · {stream.Length} bytes");
        }
        catch (Exception ex)
        {
            Report($"PICK FAILED: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private async void OnCapturePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            var file = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "ApiDemo capture" });
            if (file is null)
            {
                Report("CAMERA canceled");
                return;
            }
            using var stream = await file.OpenReadAsync();
            Report($"CAMERA ok · {file.FileName} · {stream.Length} bytes");
        }
        catch (Exception ex)
        {
            Report($"CAMERA FAILED: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// @ohos.net.http 的 HttpResponse 是 TS interface（生成器未产出包装类，返回值退化为 IntPtr）。
    /// 这里用 JsObject 读取 responseCode/result，同时演示“降级点”的正确手工收口方式。
    /// </summary>
    private sealed class HttpResponseView : JsObject
    {
        private static ReadOnlySpan<byte> CodeName => "responseCode"u8;
        private static ReadOnlySpan<byte> ResultName => "result"u8;

        public HttpResponseView(IntPtr handle) : base(handle) { }

        public (double Code, string Body) Read()
        {
            double code = GetProperty<double>(CodeName);
            string body;
            try
            {
                body = GetProperty<string>(ResultName) ?? string.Empty;
            }
            catch
            {
                var bytes = GetProperty<byte[]>(ResultName);
                body = bytes is null ? string.Empty : Encoding.UTF8.GetString(bytes);
            }
            return (code, body);
        }
    }
}
