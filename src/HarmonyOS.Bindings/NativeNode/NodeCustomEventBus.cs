#nullable enable
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using HarmonyOS.Interop;
namespace HarmonyOS.Bindings.NativeNode;

/// <summary>ArkUI_NodeCustomEvent（不透明，经访问器读取）</summary>
public struct ArkUI_NodeCustomEvent { }

/// <summary>ArkUI_NodeCustomEvent 的托管只读视图（自绘节点事件载荷）</summary>
public readonly unsafe struct ArkUICustomEvent
{
    private readonly ArkUI_NodeCustomEvent* _ptr;

    internal ArkUICustomEvent(ArkUI_NodeCustomEvent* ptr) => _ptr = ptr;

    /// <summary>自绘事件类型</summary>
    public ArkUI_NodeCustomEventType EventType
        => (ArkUI_NodeCustomEventType)ArkUINativeApi.NodeCustomEventGetEventType(_ptr);

    /// <summary>注册自绘事件时指定的 targetId</summary>
    public int TargetId => ArkUINativeApi.NodeCustomEventGetEventTargetId(_ptr);

    /// <summary>注册时透传的 userData</summary>
    public IntPtr UserData => ArkUINativeApi.NodeCustomEventGetUserData(_ptr);

    /// <summary>NODE_ON_MEASURE 的布局约束（ArkUI_LayoutConstraint*，其它事件为零指针）</summary>
    public ArkUI_LayoutConstraint* ConstraintInMeasure
        => ArkUINativeApi.NodeCustomEventGetLayoutConstraintInMeasure(_ptr);

    /// <summary>解析 NODE_ON_DRAW 的画布与绘制区尺寸（px，经 ArkUI_DrawContext 访问器）</summary>
    public (OHDrawingCanvas Canvas, int Width, int Height) GetDrawContext()
    {
        var ctx = ArkUINativeApi.NodeCustomEventGetDrawContextInDraw(_ptr);
        if (ctx == IntPtr.Zero)
            throw new InvalidOperationException("custom event carries no draw context");
        unsafe
        {
            var size = ArkUINativeApi.DrawContextGetSize(ctx);
            return (new OHDrawingCanvas((OH_Drawing_Canvas*)ArkUINativeApi.DrawContextGetCanvas(ctx)),
                size.width, size.height);
        }
    }
}

/// <summary>
/// 自绘节点事件分发总线：RegisterNodeCustomEventReceiver 全进程只允许一个原生接收器，
/// 以 (targetId, eventType) 复合键路由到各节点（与 NodeEventBus 同模式）。
/// </summary>
internal static unsafe class NodeCustomEventBus
{
    private static readonly ConcurrentDictionary<(int TargetId, int EventType), Action<ArkUICustomEvent>> Handlers = new();
    private static delegate* unmanaged<IntPtr, void> _receiverPtr;
    private static readonly object Gate = new();
    private static bool _registered;

    /// <summary>分配进程内唯一的自绘事件 targetId（与普通节点事件共用 NodeEventBus 的计数空间，
    /// 保证跨两类事件注册簿的键全局唯一，杜绝同号歧义）</summary>
    internal static int NextTargetId() => NodeEventBus.NextTargetId();

    internal static void Register(int targetId, ArkUI_NodeCustomEventType eventType, Action<ArkUICustomEvent> handler)
    {
        EnsureRegistered();
        Handlers[(targetId, (int)eventType)] = handler;
    }

    internal static void Unregister(int targetId, ArkUI_NodeCustomEventType eventType)
    {
        Handlers.TryRemove((targetId, (int)eventType), out _);
    }

    private static void EnsureRegistered()
    {
        // 注册动作是单例一次性：加锁保证多视图并发首挂时原生 receiver 只注册一次
        lock (Gate)
        {
            if (_registered) return;
            _receiverPtr = &Dispatch;
            ArkUINativeApi.RegisterCustomEventReceiver(_receiverPtr);
            _registered = true;
            HiLog.Debug("HarmonyHost", $"[CustomEventBus] receiver registered, fnPtr=0x{(long)_receiverPtr:X}");
        }
    }

    [UnmanagedCallersOnly]
    private static void Dispatch(IntPtr eventPtr)
    {
        // JS 线程热点：同 NodeEventBus，顺带批量回收终结器线程暂存的 napi_ref
        NapiFinalizationQueue.Drain();
        try
        {
            var ev = (ArkUI_NodeCustomEvent*)eventPtr;
            var key = (ArkUINativeApi.NodeCustomEventGetEventTargetId(ev),
                (int)ArkUINativeApi.NodeCustomEventGetEventType(ev));
            if (Handlers.TryGetValue(key, out var handler))
                handler(new ArkUICustomEvent(ev));
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyHost", $"[CustomEventBus] dispatch error: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
