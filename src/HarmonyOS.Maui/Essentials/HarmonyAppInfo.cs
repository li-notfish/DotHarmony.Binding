// IAppInfo 鸿蒙实现：@ohos.bundle.bundleManager 的 getBundleInfoForSelfSync。
// flag 1 = GET_BUNDLE_INFO_WITH_APPLICATION（带出 appInfo.label 即应用显示名）。
// RequestedTheme 经 ability 上下文 → getApplicationContext().getColorMode()（COLOR_MODE_DARK=1）；
// ShowSettingsUI 经 startAbility({uri:'ohos.settings'}) 拉起系统设置；LayoutDirection 无系统通道按 Ltr。
#nullable enable
using Microsoft.Maui.ApplicationModel;
using HBundleManager = HarmonyOS.Bindings.Api.Bundle.BundleManager;
using HBundleInfo = HarmonyOS.Bindings.Api.Enterprise.BundleInfo;
using HarmonyOS.Interop;

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

    public void ShowSettingsUI()
    {
        var want = NativeValue.From(new Dictionary<string, object?>
        {
            ["uri"] = "ohos.settings",
        });
        _ = NodeApi.CallMethodAsync<object?>(HarmonyPreferences.Context, "startAbility", want);
    }

    public AppTheme RequestedTheme
    {
        get
        {
            try
            {
                var appCtx = NodeApi.CallMethod<IntPtr>(HarmonyPreferences.Context, "getApplicationContext"u8);
                var mode = NodeApi.CallMethod<double>(appCtx, "getColorMode"u8);
                return mode == 1 ? AppTheme.Dark : AppTheme.Light; // COLOR_MODE_DARK=1, COLOR_MODE_LIGHT=0
            }
            catch (Exception)
            {
                return AppTheme.Unspecified;
            }
        }
    }

    public AppPackagingModel PackagingModel => AppPackagingModel.Packaged; // HAP 分发

    public LayoutDirection RequestedLayoutDirection => LayoutDirection.LeftToRight;

    private static Version ParseVersion(string versionName, long versionCode)
    {
        if (System.Version.TryParse(versionName, out var v))
            return v;
        return new Version((int)Math.Min(versionCode, int.MaxValue), 0);
    }
}
