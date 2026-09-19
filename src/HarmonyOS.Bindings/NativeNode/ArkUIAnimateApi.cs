// ArkUI_NativeAnimateAPI_1 镜像（native_animate.h，API 12+）
// 显式动画原语：animateTo(context, option, update, complete)——update 闭包内
// 的属性变更按 option 插值过渡。供页面切换淡入等使用。
// 维护约定（同 ArkUINativeApi.cs）：仅枚举由 extract_arkui_types.py 生成
// （ArkUI_AnimationCurve / ArkUI_FinishCallbackType 等，源头 native_animate.h /
// native_type_visual.h）；结构体与函数表镜像手工维护、成员顺序逐项对照头文件。</auto-generated>
#nullable enable
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.NativeNode;

#pragma warning disable CS8500

internal static unsafe partial class ArkUINativeApi
{
    private static ArkUI_NativeAnimateAPI_1* _animateApi;

    /// <summary>ArkUI_NativeAnimateAPI_1 函数表（懒获取，进程内稳定）</summary>
    internal static ArkUI_NativeAnimateAPI_1* Animate
    {
        get
        {
            if (_animateApi == null)
            {
                var name = "ArkUI_NativeAnimateAPI_1"u8;
                fixed (byte* p = name)
                {
                    _animateApi = (ArkUI_NativeAnimateAPI_1*)OH_ArkUI_QueryModuleInterfaceByName(
                        (int)ArkUIVariantKind.ARKUI_NATIVE_ANIMATE, p);
                }
                if (_animateApi == null)
                    throw new InvalidOperationException("ArkUI_NativeAnimateAPI_1 is not available (UI context required)");
            }
            return _animateApi;
        }
    }

    /// <summary>由任意节点取 UIContext（animateTo 的 context 参数）</summary>
    [LibraryImport(ArkuiLib)]
    internal static partial IntPtr OH_ArkUI_GetContextByNode(ArkUI_NodeHandle node);

    [LibraryImport(ArkuiLib)]
    internal static partial IntPtr OH_ArkUI_AnimateOption_Create();

    [LibraryImport(ArkuiLib)]
    internal static partial void OH_ArkUI_AnimateOption_Dispose(IntPtr option);

    [LibraryImport(ArkuiLib)]
    internal static partial void OH_ArkUI_AnimateOption_SetDuration(IntPtr option, int value);

    [LibraryImport(ArkuiLib)]
    internal static partial void OH_ArkUI_AnimateOption_SetCurve(IntPtr option, ArkUI_AnimationCurve value);
}

/// <summary>ArkUI_ContextCallback（common_type.h）：{ userData, callback }</summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ArkUI_ContextCallback
{
    public void* UserData;
    public delegate* unmanaged<void*, void> Callback;
}

/// <summary>ArkUI_AnimateCompleteCallback（native_animate.h）：{ type, callback, userData }</summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ArkUI_AnimateCompleteCallback
{
    public ArkUI_FinishCallbackType Type;
    public delegate* unmanaged<void*, void> Callback;
    public void* UserData;
}

/// <summary>
/// ArkUI_NativeAnimateAPI_1 函数表镜像。
/// 成员顺序与 native_animate.h 逐项对应，禁止重排。
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ArkUI_NativeAnimateAPI_1
{
    public delegate* unmanaged<IntPtr, IntPtr, ArkUI_ContextCallback*, ArkUI_AnimateCompleteCallback*, int> animateTo;
    public delegate* unmanaged<IntPtr, IntPtr, int> keyframeAnimateTo;
    public delegate* unmanaged<IntPtr, IntPtr, IntPtr> createAnimator;
    public delegate* unmanaged<IntPtr, void> disposeAnimator;
}
