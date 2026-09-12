using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// DragStatus 枚举
/// </summary>
public enum DragStatus
{
    Started = 0,
    Ended = 1
}

/// <summary>
/// DragStartRequestStatus 枚举
/// </summary>
public enum DragStartRequestStatus
{
    Waiting = 0,
    Ready = 1
}

/// <summary>
/// DragSpringLoadingState 枚举
/// </summary>
public enum DragSpringLoadingState
{
    Begin,
    Update,
    End,
    Cancel
}