using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ResolveStrategy 枚举
/// </summary>
public enum ResolveStrategy
{
    CallingScope = 0,
    LastFocus = 1,
    MaxInstanceId = 2,
    Unique = 3,
    LastForeground = 4,
    Undefined = 5
}

/// <summary>
/// KeyboardAvoidMode 枚举
/// </summary>
public enum KeyboardAvoidMode
{
    Offset = 0,
    Resize = 1,
    OffsetWithCaret = 2,
    ResizeWithCaret = 3,
    None = 4
}

/// <summary>
/// TextSelectionClearPolicy 枚举
/// </summary>
public enum TextSelectionClearPolicy
{
    KeepSelectedTextOnExternalTouch = 0,
    ClearSelectedTextOnExternalTouch = 1
}

/// <summary>
/// SwiperDynamicSyncSceneType 枚举
/// </summary>
public enum SwiperDynamicSyncSceneType
{
    Gesture = 0,
    Animation = 1
}

/// <summary>
/// MarqueeDynamicSyncSceneType 枚举
/// </summary>
public enum MarqueeDynamicSyncSceneType
{
    Animation = 1
}

/// <summary>
/// NodeRenderState 枚举
/// </summary>
public enum NodeRenderState
{
    AboutToRenderIn = 0,
    AboutToRenderOut = 1
}

/// <summary>
/// GestureActionPhase 枚举
/// </summary>
public enum GestureActionPhase
{
    WillStart = 0,
    WillEnd = 1
}

/// <summary>
/// GestureListenerType 枚举
/// </summary>
public enum GestureListenerType
{
    Tap = 0,
    LongPress = 1,
    Pan = 2,
    Pinch = 3,
    Swipe = 4,
    Rotation = 5
}

/// <summary>
/// CustomKeyboardContinueFeature 枚举
/// </summary>
public enum CustomKeyboardContinueFeature
{
    Enabled = 0,
    Disabled = 1
}