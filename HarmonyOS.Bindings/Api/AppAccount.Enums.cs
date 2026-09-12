using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Constants 枚举
/// </summary>
public enum Constants
{
    [Description("addAccountImplicitly")]
    ActionAddAccountImplicitly,
    [Description("authenticate")]
    ActionAuthenticate,
    [Description("createAccountImplicitly")]
    ActionCreateAccountImplicitly,
    [Description("auth")]
    ActionAuth,
    [Description("verifyCredential")]
    ActionVerifyCredential,
    [Description("setAuthenticatorProperties")]
    ActionSetAuthenticatorProperties,
    [Description("name")]
    KeyName,
    [Description("owner")]
    KeyOwner,
    [Description("token")]
    KeyToken,
    [Description("action")]
    KeyAction,
    [Description("authType")]
    KeyAuthType,
    [Description("sessionId")]
    KeySessionId,
    [Description("callerPid")]
    KeyCallerPid,
    [Description("callerUid")]
    KeyCallerUid,
    [Description("callerBundleName")]
    KeyCallerBundleName,
    [Description("requiredLabels")]
    KeyRequiredLabels,
    [Description("booleanResult")]
    KeyBooleanResult
}

/// <summary>
/// ResultCode 枚举
/// </summary>
public enum ResultCode
{
    Success = 0,
    ErrorAccountNotExist = 10001,
    ErrorAppAccountServiceException = 10002,
    ErrorInvalidPassword = 10003,
    ErrorInvalidRequest = 10004,
    ErrorInvalidResponse = 10005,
    ErrorNetworkException = 10006,
    ErrorOauthAuthenticatorNotExist = 10007,
    ErrorOauthCanceled = 10008,
    ErrorOauthListTooLarge = 10009,
    ErrorOauthServiceBusy = 10010,
    ErrorOauthServiceException = 10011,
    ErrorOauthSessionNotExist = 10012,
    ErrorOauthTimeout = 10013,
    ErrorOauthTokenNotExist = 10014,
    ErrorOauthTokenTooMany = 10015,
    ErrorOauthUnsupportAction = 10016,
    ErrorOauthUnsupportAuthType = 10017,
    ErrorPermissionDenied = 10018
}