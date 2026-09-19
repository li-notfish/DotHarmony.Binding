using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// FormError 枚举
/// </summary>
public enum FormError
{
    ErrCommon = 1,
    ErrPermissionDeny = 2,
    ErrGetInfoFailed = 4,
    ErrGetBundleFailed = 5,
    ErrGetLayoutFailed = 6,
    ErrAddInvalidParam = 7,
    ErrCfgNotMatchId = 8,
    ErrNotExistId = 9,
    ErrBindProviderFailed = 10,
    ErrMaxSystemForms = 11,
    ErrMaxInstancesPerForm = 12,
    ErrOperationFormNotSelf = 13,
    ErrProviderDelFail = 14,
    ErrMaxFormsPerClient = 15,
    ErrMaxSystemTempForms = 16,
    ErrFormNoSuchModule = 17,
    ErrFormNoSuchAbility = 18,
    ErrFormNoSuchDimension = 19,
    ErrFormFaNotInstalled = 20,
    ErrSystemResponsesFailed = 30,
    ErrFormDuplicateAdded = 31,
    ErrInRecovery = 36
}