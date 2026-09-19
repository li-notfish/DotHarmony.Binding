using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DeviceConnectState 枚举
/// </summary>
public enum DeviceConnectState
{
    Idle = 0,
    Connecting = 1,
    Connected = 2,
    Disconnecting = 3
}

/// <summary>
/// ContinuationMode 枚举
/// </summary>
public enum ContinuationMode
{
    CollaborationSingle = 0,
    CollaborationMultiple = 1
}