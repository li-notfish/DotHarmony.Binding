using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PartnerAgentExtensionAbilityDestroyReason 枚举
/// </summary>
public enum PartnerAgentExtensionAbilityDestroyReason
{
    UnknownReason = 0,
    UserClosedAbility = 1,
    DeviceUnpaired = 2,
    DeviceLost = 3,
    BluetoothDisabled = 4
}