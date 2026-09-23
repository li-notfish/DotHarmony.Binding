using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Params 枚举
/// </summary>
public enum Params
{
    [Description("ability.params.backToOtherMissionStack")]
    AbilityBackToOtherMissionStack,
    [Description("ohos.ability.params.abilityRecoveryRestart")]
    AbilityRecoveryRestart,
    [Description("ohos.extra.param.key.contentTitle")]
    ContentTitleKey,
    [Description("ohos.extra.param.key.shareAbstract")]
    ShareAbstractKey,
    [Description("ohos.extra.param.key.shareUrl")]
    ShareUrlKey,
    [Description("ohos.extra.param.key.supportContinuePageStack")]
    SupportContinuePageStackKey,
    [Description("ohos.extra.param.key.supportContinueSourceExit")]
    SupportContinueSourceExitKey,
    [Description("ohos.extra.param.key.showMode")]
    ShowModeKey,
    [Description("ability.params.stream")]
    ParamsStream,
    [Description("ohos.extra.param.key.appCloneIndex")]
    AppCloneIndexKey,
    [Description("ohos.extra.param.key.callerRequestCode")]
    CallerRequestCode,
    [Description("ohos.param.atomicservice.pagePath")]
    PagePath,
    [Description("ohos.param.atomicservice.routerName")]
    RouterName,
    [Description("ohos.param.atomicservice.pageSourceFile")]
    PageSourceFile,
    [Description("ohos.param.atomicservice.buildFunction")]
    BuildFunction,
    [Description("ohos.param.atomicservice.subpackageName")]
    SubPackageName,
    [Description("ohos.extra.param.key.appInstance")]
    AppInstanceKey,
    [Description("ohos.extra.param.key.createAppInstance")]
    CreateAppInstanceKey,
    [Description("ohos.param.callerAppCloneIndex")]
    CallerAppCloneIndex,
    [Description("ohos.params.pluginAbility")]
    DestinationPluginAbility,
    [Description("ohos.params.appLaunchTrustList")]
    AppLaunchTrustlist,
    [Description("ohos.params.atomicservice.shareRouter")]
    AtomicServiceShareRouter,
    [Description("ohos.params.launchReasonMessage")]
    LaunchReasonMessage,
    [Description("ohos.param.ability.udKey")]
    AbilityUnifiedDataKey
}

/// <summary>
/// AppAbilityWantConstantFlags 枚举
/// </summary>
public enum AppAbilityWantConstantFlags
{
    FlagAuthReadUriPermission = 1,
    FlagAuthWriteUriPermission = 2,
    FlagAuthPersistableUriPermission = 64,
    FlagInstallOnDemand = 2048,
    FlagAbilityOnCollaborate = 8192,
    FlagStartWithoutTips = 1073741824
}

/// <summary>
/// ShowMode 枚举
/// </summary>
public enum ShowMode
{
    Window = 0,
    EmbeddedFull = 1,
    EmbeddedHalf = 2
}

/// <summary>
/// AppAbilityWantConstantAction 枚举
/// </summary>
public enum AppAbilityWantConstantAction
{
    [Description("ohos.want.action.sendToData")]
    ActionSendToData
}