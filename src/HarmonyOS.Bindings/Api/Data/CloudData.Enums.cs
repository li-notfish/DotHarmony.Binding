using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// StrategyType 枚举
/// </summary>
public enum StrategyType
{
    Network
}

/// <summary>
/// NetWorkStrategy 枚举
/// </summary>
public enum NetWorkStrategy
{
    Wifi = 1,
    Cellular = 2
}

/// <summary>
/// AutoSyncTriggerMode 枚举
/// </summary>
public enum AutoSyncTriggerMode
{
    AccountLogin = 0,
    CloudSwitchOn = 1,
    NetworkRecover = 2,
    CloudDataChange = 3,
    UserChange = 4
}