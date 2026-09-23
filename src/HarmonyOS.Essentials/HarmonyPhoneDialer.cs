// IPhoneDialer 鸿蒙实现：Open 经 startAbility({uri:'tel:'+number})；IsSupported 经
// @ohos.telephony.sim.getSimStateSync（卡槽 0；无 SIM/未知 = 不支持，模拟器实测 NotPresent）。
#nullable enable
using Microsoft.Maui.ApplicationModel.Communication;
using HSim = HarmonyOS.Bindings.Api.Telephony.Sim;
using HSimState = HarmonyOS.ArkUI.SimState;

namespace HarmonyOS.Essentials;

public class HarmonyPhoneDialer : IPhoneDialer
{
    public bool IsSupported =>
        HSim.GetSimStateSync(0) is HSimState.SimStateReady or HSimState.SimStateLoaded;

    public void Open(string number) =>
        _ = HarmonyLauncher.StartAbilityAsync(new Uri("tel:" + Uri.EscapeDataString(number)));
}
