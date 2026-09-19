using System;
using System.Threading;
using System.Threading.Tasks;
using HarmonyOS.Bindings.Api;
using HarmonyOS.Interop;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace HarmonyOS.Maui.Essentials;

/// <summary>
/// Geolocation：@ohos.geoLocationManager 支撑。
/// GetCurrentLocationAsync/GetLastLocation 句柄 → MAUI Location；
/// locationChange/locationError 事件桥接 LocationChanged/ListeningFailed。
/// Cancellation token 暂不支持（OHOS getCurrentLocation 无取消通道，超时由 TimeoutMs 控制）。
/// </summary>
internal sealed class HarmonyGeolocation : IGeolocation
{
    private Action<IntPtr>? _onLocationChange;
    private Action<HarmonyOS.ArkUI.LocationError>? _onLocationError;
    private bool _listening;

    public bool IsEnabled => GeoLocationManager.IsLocationEnabled();
    public bool IsListeningForeground => _listening;

    public event EventHandler<GeolocationLocationChangedEventArgs>? LocationChanged;
    public event EventHandler<GeolocationListeningFailedEventArgs>? ListeningFailed;

    public Task<Location?> GetLastKnownLocationAsync()
    {
        if (!GeoLocationManager.IsLocationEnabled())
            return Task.FromResult<Location?>(null);
        var handle = GeoLocationManager.GetLastLocation();
        return Task.FromResult<Location?>(handle != IntPtr.Zero ? FromNative(handle) : null);
    }

    public async Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken)
    {
        if (!GeoLocationManager.IsLocationEnabled())
            throw new FeatureNotEnabledException("Location is not enabled on this device.");
        var req = new CurrentLocationRequest(
            MaxAccuracy: MaxAccuracyOf(request.DesiredAccuracy),
            TimeoutMs: request.Timeout.TotalMilliseconds);
        // OHOS getCurrentLocation 无取消通道：WaitAsync 只取消托管等待（底层定位继续、结果丢弃）
        var task = GeoLocationManager.GetCurrentLocationAsync(req);
        var handle = cancelToken.CanBeCanceled ? await task.WaitAsync(cancelToken) : await task;
        return FromNative(handle);
    }

    public Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request)
    {
        if (_listening)
            throw new InvalidOperationException("Already listening for location updates");
        if (!GeoLocationManager.IsLocationEnabled())
            return Task.FromResult(false);

        _onLocationChange = handle =>
            LocationChanged?.Invoke(this, new GeolocationLocationChangedEventArgs(FromNative(handle)));

        _onLocationError = error =>
        {
            StopListeningForeground();
            ListeningFailed?.Invoke(this, new GeolocationListeningFailedEventArgs(
                error == HarmonyOS.ArkUI.LocationError.LocatingFailedLocationPermissionDenied
                || error == HarmonyOS.ArkUI.LocationError.LocatingFailedBackgroundPermissionDenied
                    ? GeolocationError.Unauthorized
                    : GeolocationError.PositionUnavailable));
        };

        GeoLocationManager.On("locationChange", _onLocationChange, new LocationRequest(
            Scenario: ScenarioOf(request.DesiredAccuracy),
            TimeInterval: request.MinimumTime.TotalMilliseconds,
            MaxAccuracy: MaxAccuracyOf(request.DesiredAccuracy)));
        GeoLocationManager.On("locationError", _onLocationError);
        _listening = true;
        return Task.FromResult(true);
    }

    public void StopListeningForeground()
    {
        if (!_listening) return;
        GeoLocationManager.Off("locationChange", _onLocationChange);
        GeoLocationManager.Off("locationError", _onLocationError);
        _onLocationChange = null;
        _onLocationError = null;
        _listening = false;
    }

    /// <summary>OHOS Location 对象句柄 → MAUI Location（可选字段缺失时读值抛错 → null）</summary>
    private static Location FromNative(IntPtr handle)
    {
        var location = new Location(
            NativeValue.ToDouble(NodeApi.GetProperty(handle, "latitude")),
            NativeValue.ToDouble(NodeApi.GetProperty(handle, "longitude")),
            DateTimeOffset.FromUnixTimeMilliseconds((long)NativeValue.ToDouble(NodeApi.GetProperty(handle, "timeStamp"))));
        location.Altitude = OptionalDouble(handle, "altitude");
        location.Accuracy = OptionalDouble(handle, "accuracy");
        location.Speed = OptionalDouble(handle, "speed");
        location.Course = OptionalDouble(handle, "direction");
        return location;
    }

    private static double? OptionalDouble(IntPtr handle, string name)
    {
        try { return NativeValue.ToDouble(NodeApi.GetProperty(handle, name)); }
        catch { return null; }
    }

    /// <summary>GeolocationAccuracy → OHOS MaxAccuracy（米）</summary>
    private static double MaxAccuracyOf(GeolocationAccuracy accuracy) => accuracy switch
    {
        GeolocationAccuracy.Best => 1,
        GeolocationAccuracy.High => 10,
        GeolocationAccuracy.Lowest => 3000,
        GeolocationAccuracy.Low => 1000,
        _ => 100,
    };

    /// <summary>GeolocationAccuracy → OHOS 定位场景</summary>
    private static HarmonyOS.ArkUI.LocationRequestScenario ScenarioOf(GeolocationAccuracy accuracy) => accuracy switch
    {
        GeolocationAccuracy.Best or GeolocationAccuracy.High => HarmonyOS.ArkUI.LocationRequestScenario.Navigation,
        GeolocationAccuracy.Lowest or GeolocationAccuracy.Low => HarmonyOS.ArkUI.LocationRequestScenario.NoPower,
        _ => HarmonyOS.ArkUI.LocationRequestScenario.DailyLifeService,
    };
}
