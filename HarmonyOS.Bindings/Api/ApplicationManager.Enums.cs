using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// KioskFeature 枚举
/// </summary>
public enum KioskFeature
{
    AllowNotificationCenter = 1,
    AllowControlCenter = 2,
    AllowGestureControl = 3,
    AllowSideDock = 4
}

/// <summary>
/// ApplicationManagerServiceType 枚举
/// </summary>
public enum ApplicationManagerServiceType
{
    CollaborationService = 0
}

/// <summary>
/// WindowState 枚举
/// </summary>
public enum WindowState
{
    Disconnect = 0,
    Connect = 1,
    Foreground = 2,
    Active = 3,
    Inactive = 4,
    Background = 5
}