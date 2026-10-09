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

    public string VersionString => Version.ToString();

    public Version Version => ParseOsVersion();

    public DevicePlatform Platform => DevicePlatform.Create("OpenHarmony");

    // deviceInfo.deviceType 英文字符串（"phone"/"tablet"/"wearable"/"tv"/"car"/"2in1"...）。
    // 未知值保持 Unknown，避免向 MAUI 侧给出过强的 Phone 结论。
    public DeviceIdiom Idiom => MapIdiom(HDeviceInfo.DeviceType);

    public DeviceType DeviceType =>
        IsEmulator() ? DeviceType.Virtual :
        IsKnownDeviceType() ? DeviceType.Physical :
        DeviceType.Unknown;

    internal static DeviceIdiom MapIdiom(string deviceType) => deviceType switch
    {
        "phone" or "mobile" => DeviceIdiom.Phone,
        "tablet" or "tablet_pc" => DeviceIdiom.Tablet,
        "tv" or "television" => DeviceIdiom.TV,
        "wearable" or "watch" => DeviceIdiom.Watch,
        "desktop" or "2in1" or "pc" => DeviceIdiom.Desktop,
        _ => DeviceIdiom.Unknown,
    };

    internal static Version ParseOsVersion()
    {
        return ParseOsVersion(HDeviceInfo.OsFullName, (int)HDeviceInfo.SdkApiVersion);
    }

    internal static Version ParseOsVersion(string fullName, int sdkApiVersion)
    {
        var tail = fullName[(fullName.LastIndexOf('-') + 1)..];
        if (tail.Length > 0 && tail != fullName && System.Version.TryParse(tail, out var v))
            return v;
        return new Version(sdkApiVersion, 0);
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

    private static bool IsKnownDeviceType()
    {
        return HDeviceInfo.DeviceType is
            "phone" or "mobile" or
            "tablet" or "tablet_pc" or
            "tv" or "television" or
            "wearable" or "watch" or
            "desktop" or "2in1" or "pc";
    }
}
