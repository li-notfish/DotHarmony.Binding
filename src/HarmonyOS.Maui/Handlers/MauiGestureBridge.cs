// MAUI 手势桥：TapGestureRecognizer.SendTapped / PointerGestureRecognizer.SendPointer*
// 在 MAUI 10 Controls 程序集里是 internal——事件只能由平台侧经非公开方法触发
// （公开路径不存在：Tapped 是 C# event，无法从外部 raise）。
// NativeAOT 安全性：[DynamicDependency] 把这两个类型的非公开方法收进裁剪根，
// MethodInfo 查一次、CreateDelegate 成强类型委托缓存（AOT 支持，禁止逐次 Invoke）。
// 与"XAML 零反射"铁律不冲突：反射范围仅限这两个类的指定方法。
#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Maui.Controls;
using GraphicsPoint = Microsoft.Maui.Graphics.Point;
using IElement = Microsoft.Maui.IElement;

namespace HarmonyOS.Maui.Handlers;

internal static class MauiGestureBridge
{
    private static readonly Action<TapGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?> _sendTapped = MakeTapSender();

    private static readonly Action<PointerGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformPointerEventArgs?, ButtonsMask> _sendPointerEntered = MakePointerSender("SendPointerEntered");
    private static readonly Action<PointerGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformPointerEventArgs?, ButtonsMask> _sendPointerMoved = MakePointerSender("SendPointerMoved");
    private static readonly Action<PointerGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformPointerEventArgs?, ButtonsMask> _sendPointerExited = MakePointerSender("SendPointerExited");
    private static readonly Action<PointerGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformPointerEventArgs?, ButtonsMask> _sendPointerPressed = MakePointerSender("SendPointerPressed");
    private static readonly Action<PointerGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformPointerEventArgs?, ButtonsMask> _sendPointerReleased = MakePointerSender("SendPointerReleased");

    /// <summary>触发 TapGestureRecognizer.Tapped（含 Command 执行）。</summary>
    internal static void SendTapped(TapGestureRecognizer recognizer, View sender, Func<IElement?, GraphicsPoint?>? getPosition)
        => _sendTapped(recognizer, sender, getPosition);

    internal static void SendPointerEntered(PointerGestureRecognizer r, View sender, Func<IElement?, GraphicsPoint?>? pos)
        => _sendPointerEntered(r, sender, pos, null, 0);

    internal static void SendPointerMoved(PointerGestureRecognizer r, View sender, Func<IElement?, GraphicsPoint?>? pos)
        => _sendPointerMoved(r, sender, pos, null, 0);

    internal static void SendPointerExited(PointerGestureRecognizer r, View sender, Func<IElement?, GraphicsPoint?>? pos)
        => _sendPointerExited(r, sender, pos, null, 0);

    internal static void SendPointerPressed(PointerGestureRecognizer r, View sender, Func<IElement?, GraphicsPoint?>? pos)
        => _sendPointerPressed(r, sender, pos, null, 0);

    internal static void SendPointerReleased(PointerGestureRecognizer r, View sender, Func<IElement?, GraphicsPoint?>? pos)
        => _sendPointerReleased(r, sender, pos, null, 0);

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(TapGestureRecognizer))]
    private static Action<TapGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?> MakeTapSender()
    {
        var m = FindMethod(typeof(TapGestureRecognizer), "SendTapped", 2)
            ?? throw new MissingMethodException(nameof(TapGestureRecognizer), "SendTapped");
        return m.CreateDelegate<Action<TapGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?>>();
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(PointerGestureRecognizer))]
    private static Action<PointerGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformPointerEventArgs?, ButtonsMask> MakePointerSender(string name)
    {
        var m = FindMethod(typeof(PointerGestureRecognizer), name, 4)
            ?? throw new MissingMethodException(nameof(PointerGestureRecognizer), name);
        return m.CreateDelegate<Action<PointerGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformPointerEventArgs?, ButtonsMask>>();
    }

    /// <summary>按名 + 参数个数定位非公开方法（参数类型含 internal 类型，不做精确匹配）</summary>
    private static MethodInfo? FindMethod(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)] Type type,
        string name, int parameterCount)
        => type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == name && m.GetParameters().Length == parameterCount);
}
