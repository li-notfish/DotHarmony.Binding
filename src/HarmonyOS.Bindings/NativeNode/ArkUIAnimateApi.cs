// ArkUI_NativeAnimateAPI_1 镜像（native_animate.h，API 12+）
// 显式动画原语：animateTo(context, option, update, complete)——update 闭包内
// 的属性变更按 option 插值过渡。供页面切换淡入等使用。
// 镜像部分（ArkUI_ContextCallback / ArkUI_AnimateCompleteCallback / ArkUI_NativeAnimateAPI_1
// 函数表与 OH_ArkUI_* PInvoke）由 tools/arkui-bindgen 生成于 ArkUIAnimateApi.g.cs，
// 重出：python -m arkui_bindgen gen（语义规则 config/semantics.yaml）。
// 注意：与其它 ArkUI_Native*API_1 表不同，本表以 animateTo 起首、**没有 version 字段**
//（生成器以 native_animate.h 逐项还原），勿按惯例补 version。
#nullable enable
using System;

namespace HarmonyOS.Bindings.NativeNode;

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
}
