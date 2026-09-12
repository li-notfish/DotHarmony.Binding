using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ApplicationFormInfoFormType 枚举
/// </summary>
public enum ApplicationFormInfoFormType
{
    Js = 1
}

/// <summary>
/// ApplicationFormInfoColorMode 枚举
/// </summary>
public enum ApplicationFormInfoColorMode
{
    [Description("-1")]
    ModeAuto,
    ModeDark = 0,
    ModeLight = 1
}

/// <summary>
/// ApplicationFormInfoFormState 枚举
/// </summary>
public enum ApplicationFormInfoFormState
{
    [Description("-1")]
    Unknown,
    Default = 0,
    Ready = 1
}

/// <summary>
/// ApplicationFormInfoFormParam 枚举
/// </summary>
public enum ApplicationFormInfoFormParam
{
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
    TemporaryKey
}