using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ShortMessageClass 枚举
/// </summary>
public enum ShortMessageClass
{
    Unknown,
    InstantMessage,
    OptionalMessage,
    SimMessage,
    ForwardMessage
}

/// <summary>
/// SendSmsResult 枚举
/// </summary>
public enum SendSmsResult
{
    SendSmsSuccess = 0,
    SendSmsFailureUnknown = 1,
    SendSmsFailureRadioOff = 2,
    SendSmsFailureServiceUnavailable = 3
}