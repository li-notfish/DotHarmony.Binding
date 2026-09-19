// MAUI Essentials 鸿蒙实现装配器（ROADMAP 2.4）。
// MAUI 10 的 Essentials 静态入口（DeviceInfo.Current / DeviceDisplay.MainDisplayInfo /
// AppInfo.Name / Clipboard.SetTextAsync ...）在 netstandard 产物里缺省实现全部 throw——
// 每个静态类留有 internal 注入点（SetCurrent / SetDefault）。
// 本桥经 [DynamicDependency] 收根 + CreateDelegate 缓存强类型委托完成注入（AOT 安全，
// 与 MauiGestureBridge 同款模式，禁止逐次 MethodInfo.Invoke）。
// 范围说明：依赖宿主 DI 容器（MauiAppBuilder.ConfigureEssentials）的解析路径本宿主不使用——
// 我们直接驱动静态入口，静态入口又同步驱动接口消费方，二者一致。
#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using HarmonyOS.Interop;

namespace HarmonyOS.Maui.Essentials;

public static class HarmonyEssentials
{
    static bool _installed;

    /// <summary>注入全部已实现的 Essentials 服务（幂等；MauiHarmonyHost.Run 时调用）。</summary>
    public static void Install()
    {
        if (_installed)
            return;
        _installed = true;

        SetImplementation(typeof(global::Microsoft.Maui.Devices.DeviceInfo), "SetCurrent",
            new HarmonyDeviceInfo(), static (m, impl) => m.CreateDelegate<Action<IDeviceInfo>>()(impl));
        SetImplementation(typeof(DeviceDisplay), "SetCurrent",
            new HarmonyDeviceDisplay(), static (m, impl) => m.CreateDelegate<Action<IDeviceDisplay>>()(impl));
        SetImplementation(typeof(AppInfo), "SetCurrent",
            new HarmonyAppInfo(), static (m, impl) => m.CreateDelegate<Action<IAppInfo>>()(impl));
        SetImplementation(typeof(Clipboard), "SetDefault",
            new HarmonyClipboard(), static (m, impl) => m.CreateDelegate<Action<IClipboard>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.Storage.Preferences), "SetDefault",
            new HarmonyPreferences(), static (m, impl) => m.CreateDelegate<Action<IPreferences>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.Devices.Battery), "SetDefault",
            new HarmonyBattery(), static (m, impl) => m.CreateDelegate<Action<IBattery>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.Devices.Vibration), "SetDefault",
            new HarmonyVibration(), static (m, impl) => m.CreateDelegate<Action<IVibration>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.Networking.Connectivity), "SetCurrent",
            new HarmonyConnectivity(), static (m, impl) => m.CreateDelegate<Action<IConnectivity>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.Storage.FileSystem), "SetCurrent",
            new HarmonyFileSystem(), static (m, impl) => m.CreateDelegate<Action<IFileSystem>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.ApplicationModel.Launcher), "SetDefault",
            new HarmonyLauncher(), static (m, impl) => m.CreateDelegate<Action<ILauncher>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.ApplicationModel.Browser), "SetDefault",
            new HarmonyBrowser(), static (m, impl) => m.CreateDelegate<Action<IBrowser>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.ApplicationModel.Communication.PhoneDialer), "SetDefault",
            new HarmonyPhoneDialer(), static (m, impl) => m.CreateDelegate<Action<IPhoneDialer>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.ApplicationModel.DataTransfer.Share), "SetDefault",
            new HarmonyShare(), static (m, impl) => m.CreateDelegate<Action<IShare>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.ApplicationModel.Communication.Email), "SetDefault",
            new HarmonyEmail(), static (m, impl) => m.CreateDelegate<Action<IEmail>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.Storage.SecureStorage), "SetDefault",
            new HarmonySecureStorage(), static (m, impl) => m.CreateDelegate<Action<ISecureStorage>>()(impl));
        // 传感器族（@ohos.sensor）：五服务 SetDefault 注入
        SetImplementation(typeof(Accelerometer), "SetDefault",
            new HarmonyAccelerometer(), static (m, impl) => m.CreateDelegate<Action<IAccelerometer>>()(impl));
        SetImplementation(typeof(Magnetometer), "SetDefault",
            new HarmonyMagnetometer(), static (m, impl) => m.CreateDelegate<Action<IMagnetometer>>()(impl));
        SetImplementation(typeof(Gyroscope), "SetDefault",
            new HarmonyGyroscope(), static (m, impl) => m.CreateDelegate<Action<IGyroscope>>()(impl));
        SetImplementation(typeof(Compass), "SetDefault",
            new HarmonyCompass(), static (m, impl) => m.CreateDelegate<Action<ICompass>>()(impl));
        SetImplementation(typeof(OrientationSensor), "SetDefault",
            new HarmonyOrientationSensor(), static (m, impl) => m.CreateDelegate<Action<IOrientationSensor>>()(impl));
        // 定位（@ohos.geoLocationManager）与媒体选择（@ohos.file.picker + camera.picker）
        SetImplementation(typeof(global::Microsoft.Maui.Devices.Sensors.Geolocation), "SetDefault",
            new HarmonyGeolocation(), static (m, impl) => m.CreateDelegate<Action<IGeolocation>>()(impl));
        SetImplementation(typeof(global::Microsoft.Maui.Media.MediaPicker), "SetDefault",
            new HarmonyMediaPicker(), static (m, impl) => m.CreateDelegate<Action<IMediaPicker>>()(impl));

        // IMainThread 不注入：MAUI 10.0.11 的 MainThread 没有注入点（PlatformIsMainThread 直接 throw，
        // SetCustomImplementation 是 .NET 11 main 才加的 API）——曾误判为 AOT 裁剪，反编译 net10.0 产物实锤。
        // 本宿主 .NET 代码全在原生 UI 线程上跑（TSFN 回调同线程），无需 MainThread 静态入口；
        // 升级到含 SetCustomImplementation 的 MAUI 版本后再接回。

        HiLog.Info("Essentials",
            "HarmonyOS Essentials installed: DeviceInfo / DeviceDisplay / AppInfo / Clipboard / Preferences / Battery / Vibration / Connectivity / FileSystem / Launcher / Browser / PhoneDialer / Share / Email / SecureStorage / Accelerometer / Magnetometer / Gyroscope / Compass / OrientationSensor / Geolocation / MediaPicker");
    }

    private delegate void Setter<TInterface>(MethodInfo m, TInterface impl);

    private static void SetImplementation<TInterface>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)] Type staticClass,
        string setterName,
        TInterface implementation,
        Setter<TInterface> invoke)
    {
        var m = staticClass.GetMethod(setterName, BindingFlags.NonPublic | BindingFlags.Static, null, [typeof(TInterface)], null)
            ?? throw new MissingMethodException(staticClass.Name, setterName);
        invoke(m, implementation);
    }
}
