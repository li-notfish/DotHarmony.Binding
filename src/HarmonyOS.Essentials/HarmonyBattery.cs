// IBattery 鸿蒙实现：@ohos.batteryInfo 静态属性（同步通道，ApiDemo 已实测）+ commonEvent 变化通知。
// ChargeLevel 从 batterySOC(0-100) 折算（无电池返回 -1，对齐 MAUI 语义）；EnergySaverStatus
// 经 @ohos.power.getPowerMode()（Api 9 起）映射省电模式。
// @ohos.batteryInfo 本身无变化回调：事件走 usual.event.BATTERY_CHANGED / usual.event.POWER_SAVE_MODE_CHANGED
// commonEvent 订阅，回调内重读属性（事件载荷字段不解析——属性即真相）。
// 首次订阅时缓存当前值做去重，避免首事件误报（MAUI BatteryImplementation 同款语义）。
#nullable enable
using System.Runtime.InteropServices;
using Microsoft.Maui.Devices;
using HarmonyOS.Interop;
using HBatteryInfo = HarmonyOS.Bindings.Api.BatteryInfo;
using HPower = HarmonyOS.Bindings.Api.Power;
using HPowerMode = HarmonyOS.ArkUI.DevicePowerMode;
using HChargeState = HarmonyOS.ArkUI.BatteryChargeState;
using HPluggedType = HarmonyOS.ArkUI.BatteryPluggedType;
using HCommonEvent = HarmonyOS.Bindings.Api.CommonEventManager;

namespace HarmonyOS.Essentials;

public class HarmonyBattery : IBattery
{
    const string BatteryChangedEvent = "usual.event.BATTERY_CHANGED";
    const string PowerSaveChangedEvent = "usual.event.POWER_SAVE_MODE_CHANGED";

    public double ChargeLevel
    {
        get
        {
            if (!HBatteryInfo.IsBatteryPresent)
                return -1;
            return Math.Clamp(HBatteryInfo.BatterySoc / 100.0, 0.0, 1.0);
        }
    }

    public BatteryState State => !HBatteryInfo.IsBatteryPresent
        ? BatteryState.NotPresent
        : MapState(HBatteryInfo.ChargingStatus);

    public BatteryPowerSource PowerSource => MapPowerSource(State, HBatteryInfo.PluggedType);

    public EnergySaverStatus EnergySaverStatus => MapSaverStatus(HPower.GetPowerMode());

    event EventHandler<BatteryInfoChangedEventArgs>? _batteryChanged;
    event EventHandler<EnergySaverStatusChangedEventArgs>? _saverChanged;

    readonly EventListenerRegistry _batteryListeners = new();
    readonly EventListenerRegistry _saverListeners = new();
    IntPtr _batterySubscriber, _saverSubscriber;

    // 去重缓存：commonEvent 回调内重读属性，仅变化时才触发
    double _lastLevel;
    BatteryState _lastState;
    BatteryPowerSource _lastSource;
    EnergySaverStatus _lastSaver;

    public event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged
    {
        add
        {
            bool first = _batteryChanged is null;
            _batteryChanged += value;
            if (first)
                StartBatteryListeners();
        }
        remove
        {
            _batteryChanged -= value;
            if (_batteryChanged is null)
                StopBatteryListeners();
        }
    }

    public event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
    {
        add
        {
            bool first = _saverChanged is null;
            _saverChanged += value;
            if (first)
                StartSaverListeners();
        }
        remove
        {
            _saverChanged -= value;
            if (_saverChanged is null)
                StopSaverListeners();
        }
    }

    void StartBatteryListeners()
    {
        try
        {
            var info = NativeValue.From(new Dictionary<string, object?>
            {
                ["events"] = new[] { BatteryChangedEvent },
            });
            _batterySubscriber = HCommonEvent.CreateSubscriberSync(info);
            _batteryListeners.Add(_batterySubscriber,
                _ => OnBatteryChanged(),
                js => _ = HCommonEvent.SubscribeToEventAsync(_batterySubscriber, js));
            _lastLevel = ChargeLevel;
            _lastState = State;
            _lastSource = PowerSource;
        }
        catch (Exception ex)
        {
            HiLog.Warn("Essentials", $"[battery] subscribe {BatteryChangedEvent} failed: {ex.Message}");
        }
    }

    void StopBatteryListeners()
    {
        if (_batteryListeners.Remove(_batterySubscriber, js => _ = HCommonEvent.UnsubscribeAsync(js)))
            _batterySubscriber = IntPtr.Zero;
    }

    void StartSaverListeners()
    {
        try
        {
            var info = NativeValue.From(new Dictionary<string, object?>
            {
                ["events"] = new[] { PowerSaveChangedEvent },
            });
            _saverSubscriber = HCommonEvent.CreateSubscriberSync(info);
            _saverListeners.Add(_saverSubscriber,
                _ => OnSaverChanged(),
                js => _ = HCommonEvent.SubscribeToEventAsync(_saverSubscriber, js));
            _lastSaver = EnergySaverStatus;
        }
        catch (Exception ex)
        {
            HiLog.Warn("Essentials", $"[battery] subscribe {PowerSaveChangedEvent} failed: {ex.Message}");
        }
    }

    void StopSaverListeners()
    {
        if (_saverListeners.Remove(_saverSubscriber, js => _ = HCommonEvent.UnsubscribeAsync(js)))
            _saverSubscriber = IntPtr.Zero;
    }

    void OnBatteryChanged()
    {
        try
        {
            var level = ChargeLevel;
            var state = State;
            var source = PowerSource;
            if (level == _lastLevel && state == _lastState && source == _lastSource)
                return;
            _lastLevel = level;
            _lastState = state;
            _lastSource = source;
            _batteryChanged?.Invoke(this, new BatteryInfoChangedEventArgs(level, state, source));
        }
        catch (Exception ex)
        {
            HiLog.Warn("Essentials", $"[battery] info changed handler failed: {ex.Message}");
        }
    }

    void OnSaverChanged()
    {
        try
        {
            var saver = EnergySaverStatus;
            if (saver == _lastSaver)
                return;
            _lastSaver = saver;
            _saverChanged?.Invoke(this, new EnergySaverStatusChangedEventArgs(saver));
        }
        catch (Exception ex)
        {
            HiLog.Warn("Essentials", $"[battery] saver changed handler failed: {ex.Message}");
        }
    }

    internal static BatteryState MapState(HChargeState status) => status switch
    {
        HChargeState.Enable => BatteryState.Charging,
        HChargeState.Disable => BatteryState.Discharging,
        HChargeState.Full => BatteryState.Full,
        // 状态无效但电池存在（实测模拟器 chargingStatus=0）：按"未充电"处理更贴近 MAUI 语义
        _ => BatteryState.Discharging,
    };

    internal static BatteryPowerSource MapPowerSource(BatteryState state, HPluggedType plugged) => state switch
    {
        BatteryState.Charging or BatteryState.Full => plugged switch
        {
            HPluggedType.Ac => BatteryPowerSource.AC,
            HPluggedType.Usb => BatteryPowerSource.Usb,
            HPluggedType.Wireless => BatteryPowerSource.Wireless,
            _ => BatteryPowerSource.Unknown,
        },
        BatteryState.Discharging => BatteryPowerSource.Battery,
        _ => BatteryPowerSource.Unknown,
    };

    internal static EnergySaverStatus MapSaverStatus(HPowerMode mode) => mode
        is HPowerMode.ModePowerSave or HPowerMode.ModeExtremePowerSave or HPowerMode.ModeCustomPowerSave
            ? EnergySaverStatus.On
            : EnergySaverStatus.Off;
}
