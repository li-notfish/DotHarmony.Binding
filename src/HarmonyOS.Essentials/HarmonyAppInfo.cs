// IAppInfo 鸿蒙实现：@ohos.bundle.bundleManager 的 getBundleInfoForSelfSync。
// flag 1 = GET_BUNDLE_INFO_WITH_APPLICATION（带出 appInfo.label 即应用显示名）。
// RequestedTheme 优先用 onConfigurationUpdate 回调的 colorMode，缺省回退 UiAppearance.GetDarkMode()；
// ShowSettingsUI 经 startAbility({uri:'ohos.settings'}) 拉起系统设置；LayoutDirection 无系统通道按 Ltr。
#nullable enable
using System.Threading;
using Microsoft.Maui.ApplicationModel;
using HBundleManager = HarmonyOS.Bindings.Api.Bundle.BundleManager;
using HBundleInfo = HarmonyOS.Bindings.Api.Enterprise.BundleInfo;
using HUiAppearance = HarmonyOS.Bindings.Api.UiAppearance;
using HarmonyOS.Interop;

namespace HarmonyOS.Essentials;

public class HarmonyAppInfo : IAppInfo
{
    readonly HBundleInfo _info;

    // UIAbility.onConfigurationUpdate 提供的 colorMode
    // （ConfigurationConstant.ColorMode：-1=NOT_SET, 0=DARK, 1=LIGHT）。
    // 回调发生时 UiAppearance 轮询可能仍返回旧值，因此运行中以此权威值优先。
    // native 回调线程写入、UI 线程读取，经 Volatile 保证可见性；-1 = 尚未收到回调。
    private static int _configurationColorMode = -1;

    internal static void SetConfigurationColorMode(int colorMode)
        => Volatile.Write(ref _configurationColorMode, colorMode);

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
                var mode = Volatile.Read(ref _configurationColorMode);
                if (mode is 0 or 1)
                    return mode switch
                    {
                        0 => AppTheme.Dark,   // COLOR_MODE_DARK
                        1 => AppTheme.Light,  // COLOR_MODE_LIGHT
                        _ => HUiAppearance.GetDarkMode() == global::HarmonyOS.ArkUI.DarkMode.AlwaysDark
                            ? AppTheme.Dark
                            : AppTheme.Light,
                    };

                return HUiAppearance.GetDarkMode() == global::HarmonyOS.ArkUI.DarkMode.AlwaysDark
                    ? AppTheme.Dark
                    : AppTheme.Light;
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
