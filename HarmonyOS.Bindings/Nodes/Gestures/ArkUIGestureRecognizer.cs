// ArkUI 原生手势识别器的托管包装（native_gesture.h，API 12+）。
// 生命周期：ctor 创建原生 recognizer → Attach 到节点（GCHandle 作 extraParams，
// 单一 [UnmanagedCallersOnly] 跳板经句柄路由回实例，同 NodeEventBus 模式）→
// Detach / Dispose（先摘除再 dispose，最后释放 GCHandle）。
// 回调一律在 UI（宿主主）线程进入，异常必须吞掉记日志——不得穿透原生帧。
#nullable enable
using System;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ArkUI 原生手势识别器基类。子类在构造时创建原生 recognizer（须在 UI 线程），
/// Attach 后原生手势回调经 <see cref="GestureEvent"/> 进入托管。
/// </summary>
public abstract unsafe class ArkUIGestureRecognizer : IDisposable
{
    private ArkUI_GestureRecognizer _recognizer;
    private ArkUI_NodeHandle _attachedNode;
    private GCHandle _extraParams;
    private bool _disposed;
    private Action<ArkUIGestureEvent>? _callback;

    private static readonly delegate* unmanaged<ArkUI_GestureEvent*, void*, void> Trampoline = &Dispatch;

    private protected ArkUIGestureRecognizer(ArkUI_GestureRecognizer recognizer)
        => _recognizer = recognizer;

    /// <summary>手势事件回调（每动作一次，Action 区分类型；UI 线程）</summary>
    public Action<ArkUIGestureEvent>? GestureEvent
    {
        get => _callback;
        set => _callback = value;
    }

    /// <summary>池化复用键（类型+构造参数）；Dispose 不可在事件分发内调用（原生 UAF），
    /// 重建场景应 Detach 后按此键入池复用</summary>
    private protected abstract string ConfigKey { get; }

    internal string Key => ConfigKey;

    /// <summary>是否已挂到某个节点</summary>
    public bool IsAttached => !_attachedNode.IsNull;

    /// <summary>
    /// 把手势挂到节点上。默认 Parallel + Normal：与节点内建手势（如 onClick）并行识别，
    /// 子组件手势不受影响——MAUI 语义下 Button.Click 与 TapGestureRecognizer 应同发。
    /// </summary>
    public void Attach(ArkUINodeBase node,
        ArkGesturePriority priority = ArkGesturePriority.Parallel,
        ArkGestureMask mask = ArkGestureMask.Normal)
    {
        ThrowIfDisposed();
        if (!_attachedNode.IsNull)
            throw new InvalidOperationException(GetType().Name + " is already attached; Detach first");

        if (!_extraParams.IsAllocated)
            _extraParams = GCHandle.Alloc(this);

        var api = ArkUINativeApi.Gesture;
        const ArkUI_GestureEventActionType allActions =
            ArkUI_GestureEventActionType.Accept | ArkUI_GestureEventActionType.Update |
            ArkUI_GestureEventActionType.End | ArkUI_GestureEventActionType.Cancel;
        var rc = api->setGestureEventTarget(
            _recognizer, allActions, (void*)GCHandle.ToIntPtr(_extraParams), Trampoline);
        if (rc != 0)
            throw new InvalidOperationException($"setGestureEventTarget failed: {rc}");

        rc = api->addGestureToNode(node.Handle, _recognizer,
            (ArkUI_GesturePriority)priority, (ArkUI_GestureMask)mask);
        if (rc != 0)
            throw new InvalidOperationException($"addGestureToNode failed: {rc}");

        _attachedNode = node.Handle;
    }

    /// <summary>从当前挂载节点摘除（保留 recognizer，可重新 Attach）</summary>
    public void Detach()
    {
        if (_attachedNode.IsNull || _disposed) return;
        ArkUINativeApi.Gesture->removeGestureFromNode(_attachedNode, _recognizer);
        _attachedNode = default;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Detach();
        if (!_recognizer.IsNull)
        {
            ArkUINativeApi.Gesture->dispose(_recognizer);
            _recognizer = default;
        }
        if (_extraParams.IsAllocated)
        {
            _extraParams.Free();
            _extraParams = default;
        }
        _callback = null;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    [UnmanagedCallersOnly]
    private static void Dispatch(ArkUI_GestureEvent* eventPtr, void* extraParams)
    {
        try
        {
            var target = GCHandle.FromIntPtr((IntPtr)extraParams).Target as ArkUIGestureRecognizer;
            if (target?._callback is not { } cb)
                return;
            cb(new ArkUIGestureEvent(eventPtr, target._attachedNode));
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyGestures", $"gesture dispatch error: {ex.GetType().Name}: {ex.Message}");
        }
    }
}

/// <summary>单击/连击手势（createTapGesture）。Accept 动作即命中。</summary>
public sealed unsafe class ArkTapGesture : ArkUIGestureRecognizer
{
    private protected override string ConfigKey => $"tap:{_count}:{_fingers}";

    private readonly int _count;
    private readonly int _fingers;

    internal int TapCount => _count;

    /// <param name="consecutiveTaps">连续点击次数（MAUI NumberOfTapsRequired）</param>
    /// <param name="fingers">手指数</param>
    public ArkTapGesture(int consecutiveTaps = 1, int fingers = 1)
        : base(ArkUINativeApi.Gesture->createTapGesture(consecutiveTaps, fingers))
    {
        _count = consecutiveTaps;
        _fingers = fingers;
    }
}

/// <summary>拖动手势（createPanGesture）。Accept=开始，Update=移动，End/Cancel=收尾。</summary>
public sealed unsafe class ArkPanGesture : ArkUIGestureRecognizer
{
    private protected override string ConfigKey => $"pan:{_directions}:{_distance}:{_fingers}";

    private readonly ArkGestureDirection _directions;
    private readonly double _distance;
    private readonly int _fingers;

    /// <param name="directions">有效方向掩码</param>
    /// <param name="minDistancePx">触发阈值（px）</param>
    /// <param name="fingers">手指数</param>
    public ArkPanGesture(ArkGestureDirection directions = ArkGestureDirection.All,
        double minDistancePx = 10, int fingers = 1)
        : base(ArkUINativeApi.Gesture->createPanGesture(fingers, (ArkUI_GestureDirection)directions, minDistancePx))
    {
        _directions = directions;
        _distance = minDistancePx;
        _fingers = fingers;
    }
}

/// <summary>捏合手势（createPinchGesture）。Scale 相对手势起点累计。</summary>
public sealed unsafe class ArkPinchGesture : ArkUIGestureRecognizer
{
    private protected override string ConfigKey => "pinch:2";

    public ArkPinchGesture(int fingers = 2, double minDistancePx = 5)
        : base(ArkUINativeApi.Gesture->createPinchGesture(fingers, minDistancePx)) { }
}

/// <summary>轻扫手势（createSwipeGesture）：速度达标即一次性触发（Accept）。</summary>
public sealed unsafe class ArkSwipeGesture : ArkUIGestureRecognizer
{
    private protected override string ConfigKey => "swipe";

    public ArkSwipeGesture(ArkGestureDirection directions = ArkGestureDirection.All,
        double minSpeedPxS = 100, int fingers = 1)
        : base(ArkUINativeApi.Gesture->createSwipeGesture(fingers, (ArkUI_GestureDirection)directions, minSpeedPxS)) { }
}

/// <summary>长按手势（createLongPressGesture）。</summary>
public sealed unsafe class ArkLongPressGesture : ArkUIGestureRecognizer
{
    private protected override string ConfigKey => "longpress";

    public ArkLongPressGesture(int fingers = 1, bool repeat = false, int durationMs = 500)
        : base(ArkUINativeApi.Gesture->createLongPressGesture(fingers, repeat ? (byte)1 : (byte)0, durationMs)) { }
}
