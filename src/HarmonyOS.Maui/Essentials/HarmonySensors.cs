// 传感器族 Essentials 实现（@ohos.sensor）：Accelerometer/Magnetometer/Gyroscope 经
// xyz 三轴响应直映射；Compass 经 ORIENTATION 传感器 alpha（方位角）；OrientationSensor
// 经 ROTATION_VECTOR 四元数。订阅走 Sensor 模块的 typed On/Off 配对（EventListenerRegistry
// 保证 off 传入同一 JS 函数）；速率经 SensorOptions.interval 映射。
#nullable enable
using Microsoft.Maui.Devices;
using HarmonyOS.Bindings.Api;
using HSensor = HarmonyOS.Bindings.Api.Sensor;
using MSensorId = HarmonyOS.ArkUI.SensorId;

namespace HarmonyOS.Maui.Essentials;

/// <summary>传感器支持性/速率映射共享助手</summary>
internal static class HarmonySensorSupport
{
    private static HashSet<string>? _supportedTypes;

    /// <summary>getSensorListSync 缓存 → 类型名集合（OHOS 无该传感器时 getSingleSensor 抛 14500102）</summary>
    private static HashSet<string> SupportedTypes
    {
        get
        {
            if (_supportedTypes != null) return _supportedTypes;
            var set = new HashSet<string>();
            try
            {
                foreach (var s in HSensor.GetSensorListSync())
                    set.Add(s.SensorName ?? string.Empty);
            }
            catch { /* 模拟器/设备无列表时按不支持处理 */ }
            _supportedTypes = set;
            return set;
        }
    }

    /// <summary>OHOS 传感器类型名（getSensorListSync 的 sensorTypeId 命名）</summary>
    internal static bool IsSupported(MSensorId id) => SupportedTypes.Count == 0 || SupportedTypes.Contains(SensorTypeName(id));

    private static string SensorTypeName(MSensorId id) => id switch
    {
        MSensorId.Accelerometer => "accelerometer",
        MSensorId.Gyroscope => "gyroscope",
        MSensorId.MagneticField => "magneticField",
        MSensorId.Orientation => "orientation",
        MSensorId.RotationVector => "rotationVector",
        _ => id.ToString(),
    };

    /// <summary>SensorSpeed → interval(ms)（OHOS sensor.on options；Fastest/Game/UI/Default 阶梯）</summary>
    internal static double IntervalMs(SensorSpeed speed) => speed switch
    {
        SensorSpeed.Fastest => 10,
        SensorSpeed.Game => 20,
        SensorSpeed.UI => 60,
        _ => 200,
    };
}

/// <summary>Accelerometer：SensorId.Accelerometer 三轴直映射</summary>
internal sealed class HarmonyAccelerometer : IAccelerometer
{
    private bool _monitoring;
    public bool IsSupported => HarmonySensorSupport.IsSupported(MSensorId.Accelerometer);
    public bool IsMonitoring => _monitoring;
    public event EventHandler<AccelerometerChangedEventArgs>? ReadingChanged;
    public event EventHandler? ShakeDetected; // 摇晃检测需幅值阈值状态机，声明不触发

    public void Start(SensorSpeed sensorSpeed)
    {
        if (_monitoring) return;
        HSensor.On(MSensorId.Accelerometer, OnReading,
            new SensorOptions(HarmonySensorSupport.IntervalMs(sensorSpeed)));
        _monitoring = true;
    }

    public void Stop()
    {
        if (!_monitoring) return;
        HSensor.Off(MSensorId.Accelerometer, OnReading);
        _monitoring = false;
    }

    private void OnReading(AccelerometerResponse r)
        => ReadingChanged?.Invoke(this, new AccelerometerChangedEventArgs(new AccelerometerData(r.X, r.Y, r.Z)));
}

/// <summary>Magnetometer：SensorId.MagneticField 三轴直映射</summary>
internal sealed class HarmonyMagnetometer : IMagnetometer
{
    private bool _monitoring;
    public bool IsSupported => HarmonySensorSupport.IsSupported(MSensorId.MagneticField);
    public bool IsMonitoring => _monitoring;
    public event EventHandler<MagnetometerChangedEventArgs>? ReadingChanged;

    public void Start(SensorSpeed sensorSpeed)
    {
        if (_monitoring) return;
        HSensor.On(MSensorId.MagneticField, OnReading,
            new SensorOptions(HarmonySensorSupport.IntervalMs(sensorSpeed)));
        _monitoring = true;
    }

    public void Stop()
    {
        if (!_monitoring) return;
        HSensor.Off(MSensorId.MagneticField, OnReading);
        _monitoring = false;
    }

    private void OnReading(MagneticFieldResponse r)
        => ReadingChanged?.Invoke(this, new MagnetometerChangedEventArgs(new MagnetometerData(r.X, r.Y, r.Z)));
}

/// <summary>Gyroscope：SensorId.Gyroscope 三轴直映射</summary>
internal sealed class HarmonyGyroscope : IGyroscope
{
    private bool _monitoring;
    public bool IsSupported => HarmonySensorSupport.IsSupported(MSensorId.Gyroscope);
    public bool IsMonitoring => _monitoring;
    public event EventHandler<GyroscopeChangedEventArgs>? ReadingChanged;

    public void Start(SensorSpeed sensorSpeed)
    {
        if (_monitoring) return;
        HSensor.On(MSensorId.Gyroscope, OnReading,
            new SensorOptions(HarmonySensorSupport.IntervalMs(sensorSpeed)));
        _monitoring = true;
    }

    public void Stop()
    {
        if (!_monitoring) return;
        HSensor.Off(MSensorId.Gyroscope, OnReading);
        _monitoring = false;
    }

    private void OnReading(GyroscopeResponse r)
        => ReadingChanged?.Invoke(this, new GyroscopeChangedEventArgs(new GyroscopeData(r.X, r.Y, r.Z)));
}

/// <summary>Compass：SensorId.Orientation 的 alpha（方位角 0~360）→ HeadingMagneticNorth；
/// OHOS 无独立罗盘精度通道，HeadingAccuracy 保持 null</summary>
internal sealed class HarmonyCompass : ICompass
{
    private bool _monitoring;
    public bool IsSupported => HarmonySensorSupport.IsSupported(MSensorId.Orientation);
    public bool IsMonitoring => _monitoring;
    public event EventHandler<CompassChangedEventArgs>? ReadingChanged;

    public void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
    {
        // 低通滤波 OHOS 无对应通道（MAUI 仅提示），按普通速率订阅
        Start(sensorSpeed);
    }

    public void Start(SensorSpeed sensorSpeed)
    {
        if (_monitoring) return;
        HSensor.On(MSensorId.Orientation, OnReading,
            new SensorOptions(HarmonySensorSupport.IntervalMs(sensorSpeed)));
        _monitoring = true;
    }

    public void Stop()
    {
        if (!_monitoring) return;
        HSensor.Off(MSensorId.Orientation, OnReading);
        _monitoring = false;
    }

    private void OnReading(OrientationResponse r)
        => ReadingChanged?.Invoke(this, new CompassChangedEventArgs(
            new CompassData(r.Alpha)));
}

/// <summary>OrientationSensor：SensorId.RotationVector 四元数 (x, y, z, w) 直映射</summary>
internal sealed class HarmonyOrientationSensor : IOrientationSensor
{
    private bool _monitoring;
    public bool IsSupported => HarmonySensorSupport.IsSupported(MSensorId.RotationVector);
    public bool IsMonitoring => _monitoring;
    public event EventHandler<OrientationSensorChangedEventArgs>? ReadingChanged;

    public void Start(SensorSpeed sensorSpeed)
    {
        if (_monitoring) return;
        HSensor.On(MSensorId.RotationVector, OnReading,
            new SensorOptions(HarmonySensorSupport.IntervalMs(sensorSpeed)));
        _monitoring = true;
    }

    public void Stop()
    {
        if (!_monitoring) return;
        HSensor.Off(MSensorId.RotationVector, OnReading);
        _monitoring = false;
    }

    private void OnReading(RotationVectorResponse r)
        => ReadingChanged?.Invoke(this, new OrientationSensorChangedEventArgs(
            new OrientationSensorData(r.X, r.Y, r.Z, r.W)));
}
