using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// LinkType 枚举
/// </summary>
public enum LinkType
{
    LinkBr = 0
}

/// <summary>
/// ChannelState 枚举
/// </summary>
public enum ChannelState
{
    ChannelWaitResume = 0,
    ChannelResume = 1,
    ChannelExceptionSoftwareFailed = 2,
    ChannelBrNoPaired = 3
}