using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DataFlowType 枚举
/// </summary>
public enum DataFlowType
{
    DataFlowTypeNone = 0,
    DataFlowTypeDown = 1,
    DataFlowTypeUp = 2,
    DataFlowTypeUpDown = 3,
    DataFlowTypeDormant = 4
}

/// <summary>
/// DataConnectState 枚举
/// </summary>
public enum DataConnectState
{
    [Description("-1")]
    DataStateUnknown,
    DataStateDisconnected = 0,
    DataStateConnecting = 1,
    DataStateConnected = 2,
    DataStateSuspended = 3
}