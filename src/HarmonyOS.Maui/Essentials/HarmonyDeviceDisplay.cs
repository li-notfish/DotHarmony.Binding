// IDeviceDisplay 鸿蒙实现：@ohos.display 的 getDefaultDisplaySync（避免异步 promise 在
// 同步 getter 里内联等待——续体在 JS 线程内联恢复，同步阻塞会死锁）。
// DisplayInfo 语义对齐 Android Essentials：Width/Height 为 px，Density = DPI/160 缩放系数。
// KeepScreenOn 需要 window 通道（宿主 ability 上下文），M2.4 首批仅记录状态不生效。
#nullable enable
using Microsoft.Maui.Devices;
using HDisplay = HarmonyOS.Bindings.Api.Display;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyDeviceDisplay : IDeviceDisplay
{
    bool _keepScreenOn;

    public HarmonyDeviceDisplay()
    {
        // 模块级 .NET 事件（2.7 已实测订阅/退订回路），驱动 MainDisplayInfoChanged
        HDisplay.Change += OnDisplayChanged;
    }

    public bool KeepScreenOn
    {
        get => _keepScreenOn;
        set
        {
            if (_keepScreenOn == value)
                return;
            _keepScreenOn = value;
            HiLog.Warn("Essentials", "KeepScreenOn state recorded but not yet effective (window channel pending)");
        }
    }

    public DisplayInfo MainDisplayInfo => BuildDisplayInfo(HDisplay.GetDefaultDisplaySync());

    public event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChanged;

    private void OnDisplayChanged(double _) =>
        MainDisplayInfoChanged?.Invoke(this, new DisplayInfoChangedEventArgs(BuildDisplayInfo(HDisplay.GetDefaultDisplaySync())));

    internal static DisplayInfo BuildDisplayInfo(HarmonyOS.Bindings.Api.DisplayObject d) =>
        BuildDisplayInfo(d.Width, d.Height, d.DensityDpi, d.Rotation);

    internal static DisplayInfo BuildDisplayInfo(double width, double height, double dpi, double rotation)
    {
        var density = dpi / 160.0;
        return new DisplayInfo(
            width, height, density,
            width >= height ? DisplayOrientation.Landscape : DisplayOrientation.Portrait,
            MapRotation(rotation));
    }

    internal static DisplayRotation MapRotation(double rotation) => rotation switch
    {
        1 => DisplayRotation.Rotation90,
        2 => DisplayRotation.Rotation180,
        3 => DisplayRotation.Rotation270,
        _ => DisplayRotation.Rotation0,
    };
}
