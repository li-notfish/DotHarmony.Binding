// IVibration 鸿蒙实现：@ohos.vibrator（ApiDemo 已实测该模块）。
// Vibrate 走现代 startVibration（{type:'time', duration:ms} + {usage:'unknown'}），
// 而非 Api 9 起废弃的 vibrate(duration)；Cancel 走 stopVibration() 同步重载（停全部）。
// 时长钳制对齐 MAUI（[0, 5s]，默认 500ms）。VIBRATE 权限为 system_grant，
// 宿主模板 module.json5 已声明（reason: permission_VIBRATE_reason）。
#nullable enable
using Microsoft.Maui.Devices;
using HarmonyOS.Bindings.Runtime;
using HVibrator = HarmonyOS.Bindings.Api.Vibrator;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyVibration : IVibration
{
    const double DefaultDurationMs = 500;
    const double MaxDurationMs = 5000;

    // 振动器在手机设备上恒可用（IsSupportEffect 按效果查询，与 MAUI 的设备级语义不同）
    public bool IsSupported => true;

    public void Vibrate() => Vibrate(TimeSpan.FromMilliseconds(DefaultDurationMs));

    public void Vibrate(TimeSpan duration)
    {
        var ms = Math.Clamp(duration.TotalMilliseconds, 0.0, MaxDurationMs);
        var effect = NativeValue.From(new Dictionary<string, object?>
        {
            ["type"] = "time",
            ["duration"] = ms,
        });
        var attribute = NativeValue.From(new Dictionary<string, object?>
        {
            ["usage"] = "unknown",
        });
        _ = HVibrator.StartVibrationAsync(effect, attribute);
    }

    public void Cancel() => HVibrator.StopVibrationSync();
}
