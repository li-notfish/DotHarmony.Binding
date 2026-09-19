// IDeviceInfo 鸿蒙实现：@ohos.deviceInfo（同步属性，ApiDemo 已实测 Brand/Model/OsFullName）。
// Name 取 MarketName（用户可见设备名）；Version 从 OsFullName 尾段解析（OpenHarmony-7.0.0.105），
// 失败回退 SdkApiVersion。Virtual/Physical 只能启发式判断（productModel 含 emu/simulator）。
#nullable enable
using Microsoft.Maui.Devices;
using HDeviceInfo = HarmonyOS.Bindings.Api.DeviceInfo;

namespace HarmonyOS.Essentials;

public class HarmonyDeviceInfo : IDeviceInfo
{
    public string Model => HDeviceInfo.ProductModel;

    public string Manufacturer => HDeviceInfo.Manufacture;

    public string Name => HDeviceInfo.MarketName;

    public string VersionString => HDeviceInfo.OsFullName;

    public Version Version => ParseOsVersion();

    public DevicePlatform Platform => DevicePlatform.Create("OpenHarmony");

    public DeviceIdiom Idiom => DeviceIdiom.Phone;

    public DeviceType DeviceType => IsEmulator() ? DeviceType.Virtual : DeviceType.Physical;

    private static Version ParseOsVersion()
    {
        var fullName = HDeviceInfo.OsFullName;
        var tail = fullName[(fullName.LastIndexOf('-') + 1)..];
        if (tail.Length > 0 && tail != fullName && System.Version.TryParse(tail, out var v))
            return v;
        return new Version((int)HDeviceInfo.SdkApiVersion, 0);
    }

    private static bool IsEmulator()
    {
        foreach (var probe in new[] { HDeviceInfo.ProductModel, HDeviceInfo.SoftwareModel, HDeviceInfo.HardwareModel })
        {
            if (probe.Contains("emulator", StringComparison.OrdinalIgnoreCase) ||
                probe.Contains("simulator", StringComparison.OrdinalIgnoreCase) ||
                probe.Contains("emu64", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
