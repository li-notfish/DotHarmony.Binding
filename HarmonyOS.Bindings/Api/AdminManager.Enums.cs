using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Policy 枚举
/// </summary>
public enum Policy
{
    BlockList = 0,
    TrustList = 1
}

/// <summary>
/// AdminType 枚举
/// </summary>
public enum AdminType
{
    AdminTypeByod = 2
}

/// <summary>
/// ManagedEvent 枚举
/// </summary>
public enum ManagedEvent
{
    ManagedEventBundleAdded = 0,
    ManagedEventBundleRemoved = 1,
    ManagedEventAppStart = 2,
    ManagedEventAppStop = 3,
    ManagedEventSystemUpdate = 4,
    ManagedEventAccountAdded = 5,
    ManagedEventAccountSwitched = 6,
    ManagedEventAccountRemoved = 7,
    ManagedEventStartupGuideCompleted = 8,
    ManagedEventBootCompleted = 9,
    ManagedEventBundleUpdated = 10,
    ManagedEventPoliciesChanged = 11
}