// HarmonyBattery 纯逻辑单测：OHOS 枚举 → MAUI 枚举的映射（不依赖真机）。
// OHOS 侧语义：BatteryChargeState.Enable=充电中 / Disable=未充电 / Full=充满 / None=无效；
// BatteryPluggedType 仅在充电状态下有意义；DevicePowerMode 600=普通，601/603/650=省电类。
using HarmonyOS.ArkUI;
using HarmonyOS.Essentials;
using Microsoft.Maui.Devices;
using Xunit;

namespace HarmonyGestureTests;

public class BatteryMappingTests
{
    // ── State：chargingStatus → MAUI BatteryState ──

    [Theory]
    [InlineData(BatteryChargeState.Enable, BatteryState.Charging)]
    [InlineData(BatteryChargeState.Disable, BatteryState.Discharging)]
    [InlineData(BatteryChargeState.Full, BatteryState.Full)]
    public void State_MapsChargeStatus(BatteryChargeState status, BatteryState expected)
        => Assert.Equal(expected, HarmonyBattery.MapState(status));

    [Fact]
    public void State_InvalidChargeStatus_WithBattery_IsDischarging()
        // 实测模拟器 chargingStatus=0（None）：状态无效但电池存在，按未充电处理
        => Assert.Equal(BatteryState.Discharging, HarmonyBattery.MapState(BatteryChargeState.None));

    // ── PowerSource：状态 + pluggedType → MAUI BatteryPowerSource ──

    [Theory]
    [InlineData(BatteryPluggedType.Ac, BatteryPowerSource.AC)]
    [InlineData(BatteryPluggedType.Usb, BatteryPowerSource.Usb)]
    [InlineData(BatteryPluggedType.Wireless, BatteryPowerSource.Wireless)]
    public void PowerSource_Charging_MapsPluggedType(BatteryPluggedType plugged, BatteryPowerSource expected)
        => Assert.Equal(expected, HarmonyBattery.MapPowerSource(BatteryState.Charging, plugged));

    [Fact]
    public void PowerSource_Charging_UnknownPlugged_FallsBackToUnknown()
        => Assert.Equal(BatteryPowerSource.Unknown, HarmonyBattery.MapPowerSource(BatteryState.Charging, BatteryPluggedType.None));

    [Fact]
    public void PowerSource_Discharging_IsBattery()
        => Assert.Equal(BatteryPowerSource.Battery, HarmonyBattery.MapPowerSource(BatteryState.Discharging, BatteryPluggedType.None));

    [Theory]
    [InlineData(BatteryState.Unknown)]
    [InlineData(BatteryState.NotPresent)]
    public void PowerSource_NoBattery_IsUnknown(BatteryState state)
        => Assert.Equal(BatteryPowerSource.Unknown, HarmonyBattery.MapPowerSource(state, BatteryPluggedType.Ac));

    // ── EnergySaverStatus：DevicePowerMode → MAUI EnergySaverStatus ──

    [Fact]
    public void Saver_NormalMode_IsOff()
        => Assert.Equal(EnergySaverStatus.Off, HarmonyBattery.MapSaverStatus(DevicePowerMode.ModeNormal));

    [Fact]
    public void Saver_PerformanceMode_IsOff()
        => Assert.Equal(EnergySaverStatus.Off, HarmonyBattery.MapSaverStatus(DevicePowerMode.ModePerformance));

    [Theory]
    [InlineData(DevicePowerMode.ModePowerSave)]
    [InlineData(DevicePowerMode.ModeExtremePowerSave)]
    [InlineData(DevicePowerMode.ModeCustomPowerSave)]
    public void Saver_PowerSaveModes_AreOn(DevicePowerMode mode)
        => Assert.Equal(EnergySaverStatus.On, HarmonyBattery.MapSaverStatus(mode));

    // ── 枚举底值锁死：OHOS DevicePowerMode.ModeNormal=600（Api 9 起稳定值），映射依赖顺序连续 ──

    [Fact]
    public void DevicePowerMode_Normal_Is600()
        => Assert.Equal(600, (int)DevicePowerMode.ModeNormal);

    [Fact]
    public void BatteryChargeState_Values_AreStable()
    {
        Assert.Equal(0, (int)BatteryChargeState.None);
        Assert.Equal(1, (int)BatteryChargeState.Enable);
        Assert.Equal(2, (int)BatteryChargeState.Disable);
        Assert.Equal(3, (int)BatteryChargeState.Full);
    }
}
