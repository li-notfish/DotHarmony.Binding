// ArkUI_NativeGestureAPI_1 镜像（native_gesture.h，API 12+）
// 手势识别原语：tap/longpress/pan/pinch/rotation/swipe/组手势 + addGestureToNode。
// 镜像部分（13 个手势/输入枚举、4 个不透明占位、ArkUI_NativeGestureAPI_1 函数表、
// OH_ArkUI_* PInvoke）由 tools/arkui-bindgen 生成于 ArkUIGestureApi.g.cs，请勿手改；
// 本文件只保留懒获取属性与 ArkUI_GestureRecognizer 托管包装。
#nullable enable
using System;

namespace HarmonyOS.Bindings.NativeNode;

internal static unsafe partial class ArkUINativeApi
{
    private static ArkUI_NativeGestureAPI_1* _gestureApi;

    /// <summary>ArkUI_NativeGestureAPI_1 函数表（懒获取，进程内稳定）</summary>
    internal static ArkUI_NativeGestureAPI_1* Gesture
    {
        get
        {
            if (_gestureApi == null)
            {
                var name = "ArkUI_NativeGestureAPI_1"u8;
                fixed (byte* p = name)
                {
                    _gestureApi = (ArkUI_NativeGestureAPI_1*)OH_ArkUI_QueryModuleInterfaceByName(
                        (int)ArkUIVariantKind.ARKUI_NATIVE_GESTURE, p);
                }
                if (_gestureApi == null)
                    throw new InvalidOperationException("ArkUI_NativeGestureAPI_1 is not available (UI context required)");
            }
            return _gestureApi;
        }
    }
}

/// <summary>ArkUI_GestureRecognizer：8 字节句柄（按值传递时与 C 指针 ABI 一致）</summary>
internal readonly struct ArkUI_GestureRecognizer : IEquatable<ArkUI_GestureRecognizer>
{
    public readonly IntPtr Handle;
    public bool IsNull => Handle == IntPtr.Zero;

    public ArkUI_GestureRecognizer(IntPtr handle) => Handle = handle;

    public static implicit operator IntPtr(ArkUI_GestureRecognizer g) => g.Handle;
    public static implicit operator ArkUI_GestureRecognizer(IntPtr handle) => new(handle);

    public bool Equals(ArkUI_GestureRecognizer other) => Handle == other.Handle;
    public override bool Equals(object? obj) => obj is ArkUI_GestureRecognizer other && Equals(other);
    public override int GetHashCode() => Handle.GetHashCode();
    public override string ToString() => Handle.ToString("X");
}
