// MAUI 手势桥：TapGestureRecognizer.SendTapped / PointerGestureRecognizer.SendPointer* /
// DragGestureRecognizer.SendDragStarting/SendDropCompleted /
// DropGestureRecognizer.SendDragLeave/SendDrop 在 MAUI 10 Controls 程序集里是 internal——
// 事件只能由平台侧经非公开方法触发（公开路径不存在：Tapped 等是 C# event，无法从外部 raise）。
// NativeAOT 安全性：[DynamicDependency] 把这些类型的非公开方法收进裁剪根，
// MethodInfo 查一次、CreateDelegate 成强类型委托缓存（AOT 支持，禁止逐次 Invoke）。
// 与"XAML 零反射"铁律不冲突：反射范围仅限这几个类的指定方法。
#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
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

    private static readonly Func<DragGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformDragStartingEventArgs?, DragStartingEventArgs> _sendDragStarting = MakeDragStartingSender();
    private static readonly Action<DragGestureRecognizer, DropCompletedEventArgs> _sendDropCompleted = MakeDropCompletedSender();
    private static readonly Action<DropGestureRecognizer, DragEventArgs> _sendDragLeave = MakeDragLeaveSender();
    private static readonly Func<DropGestureRecognizer, DropEventArgs, Task> _sendDrop = MakeDropSender();

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

    /// <summary>触发 DragGestureRecognizer.DragStarting，返回事件参数（Data 可由应用填充）。</summary>
    internal static DragStartingEventArgs SendDragStarting(DragGestureRecognizer recognizer, View sender)
        => _sendDragStarting(recognizer, sender, null, null);

    /// <summary>触发 DragGestureRecognizer.DropCompleted（含 Command 执行）。</summary>
    internal static void SendDropCompleted(DragGestureRecognizer recognizer)
        => _sendDropCompleted(recognizer, new DropCompletedEventArgs());

    /// <summary>触发 DropGestureRecognizer.DragLeave（含 Command 执行）。</summary>
    internal static void SendDragLeave(DropGestureRecognizer recognizer, DragEventArgs args)
        => _sendDragLeave(recognizer, args);

    /// <summary>触发 DropGestureRecognizer.Drop（含 Command 执行）。</summary>
    internal static Task SendDrop(DropGestureRecognizer recognizer, DropEventArgs args)
        => _sendDrop(recognizer, args);

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(DragGestureRecognizer))]
    private static Func<DragGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformDragStartingEventArgs?, DragStartingEventArgs> MakeDragStartingSender()
    {
        var m = FindMethod(typeof(DragGestureRecognizer), "SendDragStarting", 3)
            ?? throw new MissingMethodException(nameof(DragGestureRecognizer), "SendDragStarting");
        return m.CreateDelegate<Func<DragGestureRecognizer, View, Func<IElement?, GraphicsPoint?>?, PlatformDragStartingEventArgs?, DragStartingEventArgs>>();
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(DragGestureRecognizer))]
    private static Action<DragGestureRecognizer, DropCompletedEventArgs> MakeDropCompletedSender()
    {
        var m = FindMethod(typeof(DragGestureRecognizer), "SendDropCompleted", 1)
            ?? throw new MissingMethodException(nameof(DragGestureRecognizer), "SendDropCompleted");
        return m.CreateDelegate<Action<DragGestureRecognizer, DropCompletedEventArgs>>();
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(DropGestureRecognizer))]
    private static Action<DropGestureRecognizer, DragEventArgs> MakeDragLeaveSender()
    {
        var m = FindMethod(typeof(DropGestureRecognizer), "SendDragLeave", 1)
            ?? throw new MissingMethodException(nameof(DropGestureRecognizer), "SendDragLeave");
        return m.CreateDelegate<Action<DropGestureRecognizer, DragEventArgs>>();
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(DropGestureRecognizer))]
    private static Func<DropGestureRecognizer, DropEventArgs, Task> MakeDropSender()
    {
        var m = FindMethod(typeof(DropGestureRecognizer), "SendDrop", 1)
            ?? throw new MissingMethodException(nameof(DropGestureRecognizer), "SendDrop");
        return m.CreateDelegate<Func<DropGestureRecognizer, DropEventArgs, Task>>();
    }

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
