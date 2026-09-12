using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// NavDestinationState 枚举
/// </summary>
public enum NavDestinationState
{
    OnShown = 0,
    OnHidden = 1,
    OnAppear = 2,
    OnDisappear = 3,
    OnWillShow = 4,
    OnWillHide = 5,
    OnWillAppear = 6,
    OnWillDisappear = 7,
    OnActive = 8,
    OnInactive = 9,
    OnBackpress = 100
}

/// <summary>
/// RouterPageState 枚举
/// </summary>
public enum RouterPageState
{
    AboutToAppear = 0,
    AboutToDisappear = 1,
    OnPageShow = 2,
    OnPageHide = 3,
    OnBackPress = 4
}

/// <summary>
/// ScrollEventType 枚举
/// </summary>
public enum ScrollEventType
{
    ScrollStart = 0,
    ScrollStop = 1
}

/// <summary>
/// TabContentState 枚举
/// </summary>
public enum TabContentState
{
    OnShow = 0,
    OnHide = 1
}