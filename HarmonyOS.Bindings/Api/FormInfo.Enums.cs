using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// FormType 枚举
/// </summary>
public enum FormType
{
    Js = 1,
    ETs = 2
}

/// <summary>
/// FormInfoColorMode 枚举
/// </summary>
public enum FormInfoColorMode
{
    [Description("-1")]
    ModeAuto,
    ModeDark = 0,
    ModeLight = 1
}

/// <summary>
/// FormState 枚举
/// </summary>
public enum FormState
{
    [Description("-1")]
    Unknown,
    Default = 0,
    Ready = 1
}

/// <summary>
/// FormUpdateReason 枚举
/// </summary>
public enum FormUpdateReason
{
    [Description("-1")]
    Unknown,
    FormNodeReuse = 0
}

/// <summary>
/// FormParam 枚举
/// </summary>
public enum FormParam
{
    [Description("ohos.extra.param.key.form_identity")]
    IdentityKey,
    [Description("ohos.extra.param.key.form_dimension")]
    DimensionKey,
    [Description("ohos.extra.param.key.form_name")]
    NameKey,
    [Description("ohos.extra.param.key.module_name")]
    ModuleNameKey,
    [Description("ohos.extra.param.key.form_width")]
    WidthKey,
    [Description("ohos.extra.param.key.form_height")]
    HeightKey,
    [Description("ohos.extra.param.key.form_temporary")]
    TemporaryKey,
    [Description("ohos.extra.param.key.bundle_name")]
    BundleNameKey,
    [Description("ohos.extra.param.key.ability_name")]
    AbilityNameKey,
    [Description("ohos.extra.param.key.form_launch_reason")]
    LaunchReasonKey,
    [Description("ohos.extra.param.key.form_customize")]
    ParamFormCustomizeKey,
    [Description("ohos.extra.param.key.form_location")]
    FormLocationKey,
    [Description("ohos.extra.param.key.form_rendering_mode")]
    FormRenderingModeKey,
    [Description("ohos.extra.param.key.host_bg_inverse_color")]
    HostBgInverseColorKey,
    [Description("ohos.extra.param.key.permission_name")]
    FormPermissionNameKey,
    [Description("ohos.extra.param.key.permission_granted")]
    FormPermissionGrantedKey,
    [Description("ohos.extra.param.key.original_form_id")]
    OriginalFormKey,
    [Description("ohos.extra.param.key.edit_form_id")]
    EditFormKey,
    [Description("ohos.extra.param.key.update_form_reason")]
    UpdateFormReasonKey
}

/// <summary>
/// FormDimension 枚举
/// </summary>
public enum FormDimension
{
    Dimension12 = 1,
    Dimension22 = 2,
    Dimension24 = 3,
    Dimension44 = 4,
    Dimension21,
    Dimension11 = 6,
    Dimension64 = 7,
    Dimension23 = 8,
    Dimension33 = 9
}

/// <summary>
/// FormShape 枚举
/// </summary>
public enum FormShape
{
    Rect = 1,
    Circle = 2
}

/// <summary>
/// VisibilityType 枚举
/// </summary>
public enum VisibilityType
{
    Unknown = 0,
    FormVisible = 1,
    FormInvisible = 2
}

/// <summary>
/// FormInfoLaunchReason 枚举
/// </summary>
public enum FormInfoLaunchReason
{
    FormDefault = 1,
    FormShare = 2,
    FormSizeChange = 3
}

/// <summary>
/// FormLocation 枚举
/// </summary>
public enum FormLocation
{
    Desktop = 0,
    FormCenter = 1,
    FormManager = 2,
    NegativeScreen = 3,
    ScreenLock = 6,
    AISuggestion = 7,
    Standby = 8
}