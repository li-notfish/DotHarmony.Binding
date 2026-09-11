using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Action 枚举
/// </summary>
public enum Action
{
    [Description("ohos.want.action.home")]
    ActionHome,
    [Description("ohos.want.action.dial")]
    ActionDial,
    [Description("ohos.want.action.search")]
    ActionSearch,
    [Description("ohos.settings.wireless")]
    ActionWirelessSettings,
    [Description("ohos.settings.manage.applications")]
    ActionManageApplicationsSettings,
    [Description("ohos.settings.application.details")]
    ActionApplicationDetailsSettings,
    [Description("ohos.want.action.setAlarm")]
    ActionSetAlarm,
    [Description("ohos.want.action.showAlarms")]
    ActionShowAlarms,
    [Description("ohos.want.action.snoozeAlarm")]
    ActionSnoozeAlarm,
    [Description("ohos.want.action.dismissAlarm")]
    ActionDismissAlarm,
    [Description("ohos.want.action.dismissTimer")]
    ActionDismissTimer,
    [Description("ohos.want.action.sendSms")]
    ActionSendSms,
    [Description("ohos.want.action.choose")]
    ActionChoose,
    [Description("ohos.want.action.imageCapture")]
    ActionImageCapture,
    [Description("ohos.want.action.videoCapture")]
    ActionVideoCapture,
    [Description("ohos.want.action.select")]
    ActionSelect,
    [Description("ohos.want.action.sendData")]
    ActionSendData,
    [Description("ohos.want.action.sendMultipleData")]
    ActionSendMultipleData,
    [Description("ohos.want.action.scanMediaFile")]
    ActionScanMediaFile,
    [Description("ohos.want.action.viewData")]
    ActionViewData,
    [Description("ohos.want.action.editData")]
    ActionEditData,
    [Description("ability.want.params.INTENT")]
    IntentParamsIntent,
    [Description("ability.want.params.TITLE")]
    IntentParamsTitle,
    [Description("ohos.action.fileSelect")]
    ActionFileSelect,
    [Description("ability.params.stream")]
    ParamsStream,
    [Description("ohos.account.appAccount.action.oauth")]
    ActionAppAccountOauth
}

/// <summary>
/// Entity 枚举
/// </summary>
public enum Entity
{
    [Description("entity.system.default")]
    EntityDefault,
    [Description("entity.system.home")]
    EntityHome,
    [Description("entity.system.voice")]
    EntityVoice,
    [Description("entity.system.browsable")]
    EntityBrowsable,
    [Description("entity.system.video")]
    EntityVideo
}

/// <summary>
/// Flags 枚举
/// </summary>
public enum Flags : long
{
    FlagAuthReadUriPermission = 1,
    FlagAuthWriteUriPermission = 2,
    FlagAbilityForwardResult = 4,
    FlagAbilityContinuation = 8,
    FlagNotOhosComponent = 16,
    FlagAbilityFormEnabled = 32,
    FlagAbilitysliceMultiDevice = 256,
    FlagStartForegroundAbility = 512,
    FlagInstallOnDemand = 2048,
    FlagInstallWithBackgroundMode = 2147483648,
    FlagAbilityClearMission = 32768,
    FlagAbilityNewMission = 268435456,
    FlagAbilityMissionTop = 536870912
}