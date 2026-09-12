// MAUI GestureRecognizers → ArkUI 原生手势的翻译层。
// 架构镜像官方 GesturePlatformManager.Android：监听虚拟视图手势集合变化，
// 按识别器类型挂原生通道，回送到识别器的公开 Send* 协议。
// 通道分派（模拟器 API 26 实测沉淀）：
//   Tap / Pinch → NDK 手势识别器（native_gesture.h，事件数据可靠，位置 px）；
//   Pan / Swipe / Pointer → NODE_TOUCH_EVENT 触摸流（Android 同款 touch listener 模型——
//   pan 原生手势的 GetOffsetX/Y 在 END 返回 0、原始输入位置在 UPDATE/END 不可靠）。
// 单位（实测）：触摸通道 GetX/Y 为 **vp**（头文件标注 px 系笔误），故 PanUpdatedEventArgs
// 的 TotalX/Y 为 vp（等价 iOS points 语义；跨平台代码注意与 Android px 的差异）。
// 仅 TapGestureRecognizer.SendTapped / PointerGestureRecognizer.SendPointer* 走反射桥
// （见 MauiGestureBridge），Pan/Pinch/Swipe 全部走公开控制器接口。
#nullable enable
using System.Collections.Specialized;
using System.ComponentModel;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.ArkUI;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using GraphicsPoint = Microsoft.Maui.Graphics.Point;

namespace HarmonyOS.Maui.Handlers;

/// <summary>单视图手势管理器：View.GestureRecognizers ↔ ArkUI 原生手势生命周期</summary>
internal sealed class HarmonyGestureManager : IDisposable
{
    private readonly View _view;
    private readonly ArkUINodeBase _node;
    private readonly List<ArkUIGestureRecognizer> _attached = new();
    private readonly List<ArkUIGestureRecognizer> _parked = new();
    private INotifyCollectionChanged? _observedCollection;
    // Rebuild 时快照（触摸事件按显示刷新率到达，避免每事件 LINQ 迭代器分配）
    private List<PointerGestureRecognizer> _pointers = new();
    private bool _disposed;

    internal HarmonyGestureManager(View view, ArkUINodeBase node)
    {
        _view = view;
        _node = node;
        HookCompositeCollection();
        view.PropertyChanged += OnViewPropertyChanged;
        Rebuild();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _observedCollection?.CollectionChanged -= OnRecognizersChanged; // C# 14 null-conditional assignment
        _view.PropertyChanged -= OnViewPropertyChanged;
        if (_touchSubscribed)
            _node.UnsubscribeEvent(ArkUI_NodeEventType.NODE_TOUCH_EVENT);
        foreach (var g in _attached.Concat(_parked))
            g.Dispose();
        _attached.Clear();
        _parked.Clear();
    }

    // ───────────────────────── 集合/属性监听 ─────────────────────────

    private void HookCompositeCollection()
    {
        var composite = ((IGestureController)_view).CompositeGestureRecognizers;
        _observedCollection?.CollectionChanged -= OnRecognizersChanged;
        _observedCollection = composite as INotifyCollectionChanged;
        if (_observedCollection is not null)
            _observedCollection.CollectionChanged += OnRecognizersChanged;
    }

    private void OnRecognizersChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void OnViewPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(View.IsEnabled) or nameof(View.InputTransparent))
            Rebuild();
    }

    /// <summary>全量重建：摘下已挂手势入池复用（**不可在事件分发内 dispose 原生 recognizer**——
    /// 实测 dispose 后原生管线仍派发事件，UAF 崩溃；dispose 仅在 Handler 断连时统一执行）</summary>
    private void Rebuild()
    {
        ThrowIfDisposed();
        foreach (var g in _attached)
        {
            g.Detach();
            g.GestureEvent = null;
            _parked.Add(g);
        }
        _attached.Clear();
        _panStates.Clear();
        _swipeStates.Clear();
        _attachedTapCounts.Clear();
        _pointers.Clear();
        // 触摸通道按需重挂：无 Pan/Swipe/Pointer 识别器时注销，避免空跑事件流
        if (_touchSubscribed)
        {
            _node.UnsubscribeEvent(ArkUI_NodeEventType.NODE_TOUCH_EVENT);
            _touchSubscribed = false;
        }

        if (!_view.IsEnabled || _view.InputTransparent)
            return;

        foreach (var recognizer in _view.GestureRecognizers)
            Attach(recognizer);
    }

    private void Attach(IGestureRecognizer recognizer)
    {
        switch (recognizer)
        {
            case TapGestureRecognizer tap:
                AttachTap(tap);
                break;
            case PanGestureRecognizer pan:
                AttachPan(pan);
                break;
            case PinchGestureRecognizer pinch:
                AttachPinch(pinch);
                break;
            case SwipeGestureRecognizer swipe:
                AttachSwipe(swipe);
                break;
            case PointerGestureRecognizer pointer:
                _pointers.Add(pointer);
                EnsureTouchChannel();
                break;
            default:
                // DragGestureRecognizer/DropGestureRecognizer 等：暂不支持（见 ROADMAP）
                break;
        }
    }

    /// <summary>挂载（调用方先从 _parked 取复用实例；停靠实例不 dispose，仅 Handler 断连时统一释放）</summary>
    private void AttachNative(ArkUIGestureRecognizer native)
    {
        native.Attach(_node);
        _attached.Add(native);
    }

    private ArkTapGesture TakeTapFromPool(int count)
    {
        for (int i = 0; i < _parked.Count; i++)
        {
            if (_parked[i] is ArkTapGesture t && t.TapCount == count)
            {
                _parked.RemoveAt(i);
                return t;
            }
        }
        return new ArkTapGesture(count);
    }

    // ───────────────────────── Tap（NDK 手势） ─────────────────────────

    private void AttachTap(TapGestureRecognizer tap)
    {
        var count = tap.NumberOfTapsRequired;
        // 同视图按连击数分组：每组只挂一个原生手势（RaiseTap 会广播给组内全部识别器，
        // 否则 N 个识别器 × N 个原生回调 = N² 次触发）
        if (!_attachedTapCounts.Add(count))
            return;
        var native = TakeTapFromPool(count);
        native.GestureEvent = e =>
        {
            if (e.Action == ArkGestureAction.Accept)
                RaiseTap(count, e);
        };
        AttachNative(native);
    }

    private void RaiseTap(int tapCount, ArkUIGestureEvent e)
    {
        var position = new GraphicsPoint(e.PositionX, e.PositionY);
        var posFunc = GetPositionFunc(position); // 每次点击一个闭包，循环内复用

        // 子元素手势（Label Span 等 GestureElement）优先：命中则由子元素消费本次点击
        var raised = false;
        foreach (var child in ChildGesturesFor<TapGestureRecognizer>(_view.GetChildElements(position),
                     r => r.NumberOfTapsRequired == tapCount))
        {
            MauiGestureBridge.SendTapped(child, _view, posFunc);
            raised = true;
        }
        if (raised)
            return;

        foreach (var own in GesturesFor<TapGestureRecognizer>(_view.GestureRecognizers, r =>
                     r.NumberOfTapsRequired == tapCount &&
                     (r.Buttons & ButtonsMask.Primary) != 0))
        {
            MauiGestureBridge.SendTapped(own, _view, posFunc);
        }
    }

    // ───────────────────────── Pinch（NDK 手势） ─────────────────────────

    private ArkPinchGesture TakePinchFromPool()
    {
        for (int i = 0; i < _parked.Count; i++)
        {
            if (_parked[i] is ArkPinchGesture p)
            {
                _parked.RemoveAt(i);
                return p;
            }
        }
        return new ArkPinchGesture();
    }

    private void AttachPinch(PinchGestureRecognizer pinch)
    {
        var native = TakePinchFromPool();
        native.GestureEvent = e =>
        {
            var controller = (IPinchGestureController)pinch;
            var center = new GraphicsPoint(e.PinchCenterX, e.PinchCenterY);
            switch (e.Action)
            {
                case ArkGestureAction.Accept:
                    controller.SendPinchStarted(_view, center);
                    break;
                case ArkGestureAction.Update:
                    controller.SendPinch(_view, e.PinchScale, center);
                    break;
                case ArkGestureAction.End:
                    controller.SendPinchEnded(_view);
                    break;
                case ArkGestureAction.Cancel:
                    controller.SendPinchCanceled(_view);
                    break;
            }
        };
        AttachNative(native);
    }

    // ──────────────── 触摸事件通道（Pan / Swipe / Pointer 共用） ────────────────

    private bool _touchSubscribed;
    private readonly HashSet<int> _attachedTapCounts = new();
    private readonly Dictionary<PanGestureRecognizer, PanTouchState> _panStates = new();
    private readonly Dictionary<SwipeGestureRecognizer, SwipeTouchState> _swipeStates = new();

    private sealed class PanTouchState
    {
        public bool Started;
        public double StartX, StartY;
        public double LastX, LastY;
    }

    private sealed class SwipeTouchState
    {
        public double StartX, StartY;
        public double LastX, LastY;
    }

    private void AttachPan(PanGestureRecognizer pan)
    {
        _panStates[pan] = new PanTouchState();
        EnsureTouchChannel();
    }

    private void AttachSwipe(SwipeGestureRecognizer swipe)
    {
        _swipeStates[swipe] = new SwipeTouchState();
        EnsureTouchChannel();
    }

    private void EnsureTouchChannel()
    {
        if (_touchSubscribed) return;
        _node.SubscribeEvent(ArkUI_NodeEventType.NODE_TOUCH_EVENT, OnNodeTouch);
        _touchSubscribed = true;
    }

    private void OnNodeTouch(ArkUINodeEvent e)
    {
        var pe = ArkUIPointerEvent.From(e.InputEvent);
        if (pe.IsNull)
            return;

        var position = new GraphicsPoint(pe.X, pe.Y);
        switch (pe.TouchAction)
        {
            case ArkPointerTouchAction.Pressed:
                RaisePointerPressed(position);
                ResetTouchStates(position);
                break;
            case ArkPointerTouchAction.Moved:
                RaisePointerMoved(position);
                UpdateTouchStates(position);
                break;
            case ArkPointerTouchAction.Released:
                RaisePointerReleased(position);
                CompleteTouchStates(canceled: false);
                break;
            case ArkPointerTouchAction.Canceled:
                RaisePointerReleased(position);
                CompleteTouchStates(canceled: true);
                break;
        }
    }

    private void ResetTouchStates(GraphicsPoint position)
    {
        foreach (var state in _panStates.Values)
        {
            state.Started = false;
            state.StartX = position.X;
            state.StartY = position.Y;
            state.LastX = state.LastY = 0;
        }
        foreach (var state in _swipeStates.Values)
        {
            state.StartX = position.X;
            state.StartY = position.Y;
            state.LastX = state.LastY = 0;
        }
    }

    private void UpdateTouchStates(GraphicsPoint position)
    {
        foreach (var (pan, state) in _panStates)
        {
            state.LastX = position.X - state.StartX;
            state.LastY = position.Y - state.StartY;
            var controller = (IPanGestureController)pan;
            if (!state.Started)
            {
                state.Started = true;
                controller.SendPanStarted(_view, PanGestureRecognizer.CurrentId.Value);
            }
            controller.SendPan(_view, state.LastX, state.LastY, PanGestureRecognizer.CurrentId.Value);
        }
        foreach (var state in _swipeStates.Values)
        {
            state.LastX = position.X - state.StartX;
            state.LastY = position.Y - state.StartY;
        }
    }

    private void CompleteTouchStates(bool canceled)
    {
        foreach (var (pan, state) in _panStates)
        {
            if (!state.Started) continue;
            var controller = (IPanGestureController)pan;
            if (canceled)
                controller.SendPanCanceled(_view, PanGestureRecognizer.CurrentId.Value);
            else
                controller.SendPanCompleted(_view, PanGestureRecognizer.CurrentId.Value);
        }
        if (!canceled)
        {
            foreach (var (swipe, state) in _swipeStates)
                DetectSwipe(swipe, state.LastX, state.LastY);
        }
    }

    private void DetectSwipe(SwipeGestureRecognizer swipe, double totalX, double totalY)
    {
        var direction = MapSwipeDirection(totalX, totalY);
        ((ISwipeGestureController)swipe).SendSwipe(_view, totalX, totalY);
        // 阈值判定在识别器内部（未达 Threshold 静默），命中即触发 Swiped/Command
        ((ISwipeGestureController)swipe).DetectSwipe(_view, direction);
    }

    /// <summary>累计位移 → MAUI SwipeDirection（主轴判定，Android SwipeGestureHandler.OnFling 同款）</summary>
    internal static SwipeDirection MapSwipeDirection(double totalX, double totalY)
        => Math.Abs(totalX) >= Math.Abs(totalY)
            ? (totalX >= 0 ? SwipeDirection.Right : SwipeDirection.Left)
            : (totalY >= 0 ? SwipeDirection.Down : SwipeDirection.Up);

    // ───────────────────────── Pointer（同一触摸通道） ─────────────────────────

    private void RaisePointerPressed(GraphicsPoint position)
    {
        var posFunc = GetPositionFunc(position);
        foreach (var recognizer in _pointers)
        {
            MauiGestureBridge.SendPointerEntered(recognizer, _view, posFunc);
            MauiGestureBridge.SendPointerPressed(recognizer, _view, posFunc);
        }
    }

    private void RaisePointerMoved(GraphicsPoint position)
    {
        var posFunc = GetPositionFunc(position);
        foreach (var recognizer in _pointers)
            MauiGestureBridge.SendPointerMoved(recognizer, _view, posFunc);
    }

    private void RaisePointerReleased(GraphicsPoint position)
    {
        var posFunc = GetPositionFunc(position);
        foreach (var recognizer in _pointers)
            MauiGestureBridge.SendPointerReleased(recognizer, _view, posFunc);
    }

    /// <summary>GetPosition 语义：相对被点击视图本身的坐标可得；相对其它元素暂不换算（返回 null）</summary>
    private Func<IElement?, GraphicsPoint?> GetPositionFunc(GraphicsPoint position)
        => element => element is null || ReferenceEquals(element, _view)
            ? position
            : null;

    /// <summary>官方 EnumerableExtensions.GetGesturesFor 的本地替代（该类 internal）；显式循环免 LINQ 分配</summary>
    private static List<T> GesturesFor<T>(IEnumerable<IGestureRecognizer>? gestures, Func<T, bool>? predicate = null)
        where T : GestureRecognizer
    {
        var result = new List<T>();
        if (gestures is not null)
        {
            foreach (var g in gestures)
                if (g is T t && (predicate is null || predicate(t)))
                    result.Add(t);
        }
        return result;
    }

    /// <summary>官方 EnumerableExtensions.GetChildGesturesFor 的本地替代（该类 internal）</summary>
    private static List<T> ChildGesturesFor<T>(IEnumerable<GestureElement>? elements, Func<T, bool>? predicate = null)
        where T : GestureRecognizer
    {
        var result = new List<T>();
        if (elements is not null)
        {
            foreach (var element in elements)
                foreach (var g in element.GestureRecognizers)
                    if (g is T t && (predicate is null || predicate(t)))
                        result.Add(t);
        }
        return result;
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}
