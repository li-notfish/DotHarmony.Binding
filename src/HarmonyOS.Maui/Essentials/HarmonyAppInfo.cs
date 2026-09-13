// IAppInfo 鸿蒙实现：@ohos.bundle.bundleManager 的 getBundleInfoForSelfSync。
// flag 1 = GET_BUNDLE_INFO_WITH_APPLICATION（带出 appInfo.label 即应用显示名）。
// RequestedTheme/LayoutDirection/设置页 URI 无系统通道，按 Unspecified/Ltr/无操作记录。
#nullable enable
using Microsoft.Maui.ApplicationModel;
using HBundleManager = HarmonyOS.Bindings.Api.BundleManager;
using HBundleInfo = HarmonyOS.Bindings.Api.BundleInfo;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyAppInfo : IAppInfo
{
    readonly HBundleInfo _info;

    public HarmonyAppInfo()
    {
        _info = new HBundleInfo(HBundleManager.GetBundleInfoForSelfSync(1));
    }

    public string Name => _info.AppInfo.Label;

    public string PackageName => _info.Name;

    public string VersionString => _info.VersionName;

    public Version Version => ParseVersion(_info.VersionName, (long)_info.VersionCode);

    public string BuildString => $"{_info.VersionName} ({(long)_info.VersionCode})";

    public void ShowSettingsUI() =>
        HiLog.Warn("Essentials", "ShowSettingsUI: OpenHarmony has no standard per-app settings URI");

    public AppTheme RequestedTheme => AppTheme.Unspecified;

    public AppPackagingModel PackagingModel => AppPackagingModel.Packaged; // HAP 分发

    public LayoutDirection RequestedLayoutDirection => LayoutDirection.LeftToRight;

    private static Version ParseVersion(string versionName, long versionCode)
    {
        if (System.Version.TryParse(versionName, out var v))
            return v;
        return new Version((int)Math.Min(versionCode, int.MaxValue), 0);
    }
}
