using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CustomComponentLifecycleState 枚举
/// </summary>
public enum CustomComponentLifecycleState
{
    Init = 0,
    Appeared = 1,
    Built = 2,
    Recycled = 3,
    Disappeared = 4
}