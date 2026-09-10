#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>
/// ArkUI 原生节点包装基类 —— HarmonyOS 平台视图层的根基类
/// （角色对等于 Mono.Android 的 Java.Lang.Object / xamarin-macios 的 NSObject）。
///
/// 句柄模型：持有稳定的 ArkUI_NodeHandle（与 UIKit 指针语义一致），
/// 不同于 napi_value（handle scope 内短命句柄），NodeHandle 可跨帧长期持有。
///
/// 线程约束：所有实例方法必须在 UI 主线程调用（NativeMainThread.Ensure）。
/// </summary>
public abstract unsafe class ArkUINodeBase : IDisposable
{
    private ArkUI_NodeHandle _handle;
    private readonly Dictionary<ArkUI_NodeEventType, Action<ArkUINodeEvent>> _handlers = new();
    private readonly int _targetId;
    private bool _disposed;

    protected ArkUINodeBase(ArkUI_NodeType nodeType)
    {
        NativeMainThread.Ensure();
        _handle = ArkUINativeApi.CreateNode(nodeType);
        if (_handle.IsNull)
            throw new InvalidOperationException($"Failed to create native node of type {nodeType}");
        // 尺寸单位统一为 vp（密度无关像素），与 ArkTS 默认体验一致；失败则维持默认单位
        ArkUINativeApi.SetLengthMetricUnit(_handle, ArkUI_LengthMetricUnit.ARKUI_LENGTH_METRIC_UNIT_VP);
        _targetId = NodeEventBus.NextTargetId();
    }

    // ───────────────────────── 通用属性（全部节点共享）─────────────────────────

    /// <summary>宽度（vp）</summary>
    public float Width
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_WIDTH, ArkUIValue.F(value));
    }

    /// <summary>高度（vp）</summary>
    public float Height
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_HEIGHT, ArkUIValue.F(value));
    }

    /// <summary>内边距（四边同值，vp）</summary>
    public float Padding
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_PADDING,
            ArkUIValue.F(value), ArkUIValue.F(value), ArkUIValue.F(value), ArkUIValue.F(value));
    }

    /// <summary>内边距（四边独立，vp）</summary>
    public void SetPaddingEdges(float top, float right, float bottom, float left)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_PADDING,
            ArkUIValue.F(top), ArkUIValue.F(right), ArkUIValue.F(bottom), ArkUIValue.F(left));
    }

    /// <summary>外边距（四边同值，vp）</summary>
    public float Margin
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_MARGIN,
            ArkUIValue.F(value), ArkUIValue.F(value), ArkUIValue.F(value), ArkUIValue.F(value));
    }

    /// <summary>背景色（NODE_BACKGROUND_COLOR，单值 u32，0xAARRGGBB 格式）</summary>
    public void SetBackgroundColor(byte r, byte g, byte b, byte a = 255)
    {
        var argb = (uint)((a << 24) | (r << 16) | (g << 8) | b);
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_BACKGROUND_COLOR, ArkUIValue.U(argb));
    }

    /// <summary>
    /// 线性渐变背景（NODE_LINEAR_GRADIENT）。
    /// 角度为 CSS 语义（0 = 向上，顺时针增大，默认 180 = 向下）；
    /// 方向固定 CUSTOM(9) 才能让 angle 生效。
    /// colors 为 0xAARRGGBB，stops 为 0~1 位置（两数组等长）。
    /// </summary>
    public void SetLinearGradient(float angleDeg, bool repeating, uint[] colors, float[] stops)
    {
        SetGradientAttribute(ArkUI_NodeAttributeType.NODE_LINEAR_GRADIENT,
            new[]
            {
                ArkUIValue.F(angleDeg),
                ArkUIValue.I(9), // ArkUI_LinearGradientDirection.CUSTOM
                ArkUIValue.I(repeating ? 1 : 0),
            },
            colors, stops);
    }

    /// <summary>
    /// 径向渐变背景（NODE_RADIAL_GRADIENT）。
    /// center 为组件相对坐标（0~1）；radius 相对半对角线（对齐 MAUI RadialGradientPaint 语义）。
    /// 属性设置时节点可能尚未布局（尺寸为 0），故经 NODE_ON_SIZE_CHANGE（独立 targetId，
    /// 不占用用户 SubscribeEvent 的覆盖式订阅槽）在尺寸变化时按实测尺寸重算。
    /// </summary>
    public void SetRadialGradient(float centerXFrac, float centerYFrac, float radiusFrac,
        bool repeating, uint[] colors, float[] stops)
    {
        ThrowIfDisposed();
        _radialGradient = (centerXFrac, centerYFrac, radiusFrac);
        _radialColors = colors;
        _radialStops = stops;
        _radialRepeating = repeating;

        if (_gradientSizeChangeTargetId == 0)
        {
            _gradientSizeChangeTargetId = NodeEventBus.NextTargetId();
            NodeEventBus.Register(_gradientSizeChangeTargetId,
                ev => ApplyRadialGradient(ev.SizeChangeWidth, ev.SizeChangeHeight));
            ArkUINativeApi.RegisterNodeEvent(
                _handle, ArkUI_NodeEventType.NODE_ON_SIZE_CHANGE, _gradientSizeChangeTargetId, null);
        }

        ApplyRadialGradient(0, 0);
    }

    private (float Cx, float Cy, float R)? _radialGradient;
    private uint[]? _radialColors;
    private float[]? _radialStops;
    private bool _radialRepeating;
    private int _gradientSizeChangeTargetId;

    private void ApplyRadialGradient(float widthVp, float heightVp)
    {
        var f = _radialGradient!.Value;
        var values = new[]
        {
            ArkUIValue.F(f.Cx * widthVp),
            ArkUIValue.F(f.Cy * heightVp),
            ArkUIValue.F(f.R * 0.5f * MathF.Sqrt(widthVp * widthVp + heightVp * heightVp)),
            ArkUIValue.I(_radialRepeating ? 1 : 0),
        };
        SetGradientAttribute(ArkUI_NodeAttributeType.NODE_RADIAL_GRADIENT, values, _radialColors!, _radialStops!);
    }

    /// <summary>背景图（NODE_BACKGROUND_IMAGE）。repeatMode 取 ArkUI_ImageRepeat（0 = 不重复）。</summary>
    public void SetBackgroundImage(string uri, int repeatMode = 0)
    {
        ThrowIfDisposed();
        var utf8 = Encoding.UTF8.GetBytes(uri);
        var values = new[] { ArkUIValue.I(repeatMode) };
        fixed (byte* ps = utf8)
        fixed (ArkUI_NumberValue* pv = values)
        {
            var item = new ArkUI_AttributeItem { @string = ps, value = pv, size = 1 };
            var status = ArkUINativeApi.SetAttribute(_handle, ArkUI_NodeAttributeType.NODE_BACKGROUND_IMAGE, &item);
            if (status != 0)
                throw new InvalidOperationException($"SetAttribute(NODE_BACKGROUND_IMAGE) failed: {status}");
        }
    }

    /// <summary>设置带色标对象的渐变属性（value 数组 + ArkUI_ColorStop 对象同时入参）</summary>
    private void SetGradientAttribute(ArkUI_NodeAttributeType attribute, ArkUI_NumberValue[] values, uint[] colors, float[] stops)
    {
        fixed (ArkUI_NumberValue* pv = values)
        fixed (uint* pc = colors)
        fixed (float* ps = stops)
        {
            var stop = new ArkUI_ColorStop { colors = pc, stops = ps, size = colors.Length };
            var item = new ArkUI_AttributeItem { value = pv, size = values.Length, @object = &stop };
            var status = ArkUINativeApi.SetAttribute(_handle, attribute, &item);
            if (status != 0)
                throw new InvalidOperationException($"SetAttribute({attribute}) failed: {status}");
        }
    }

    /// <summary>固定宽度（vp，NODE_WIDTH）——MAUI WidthRequest 的 ArkUI 翻译</summary>
    public void SetWidth(float vp)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_WIDTH, ArkUIValue.F(vp));
    }

    /// <summary>固定高度（vp，NODE_HEIGHT）——MAUI HeightRequest 的 ArkUI 翻译</summary>
    public void SetHeight(float vp)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_HEIGHT, ArkUIValue.F(vp));
    }

    /// <summary>宽度百分比（1.0 = 100%，NODE_WIDTH_PERCENT）——MAUI Fill 语义的 ArkUI 翻译</summary>
    public void SetWidthPercent(float fraction)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_WIDTH_PERCENT, ArkUIValue.F(fraction));
    }

    /// <summary>高度百分比（1.0 = 100%，NODE_HEIGHT_PERCENT）</summary>
    public void SetHeightPercent(float fraction)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_HEIGHT_PERCENT, ArkUIValue.F(fraction));
    }

    /// <summary>绝对定位（vp，NODE_POSITION）—— 相对父容器左上角，托管布局的定位原语</summary>
    public void SetPosition(float x, float y)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_POSITION, ArkUIValue.F(x), ArkUIValue.F(y));
    }

    /// <summary>四边外边距（NODE_MARGIN，单位 vp）</summary>
    public void SetMarginEdges(float top, float right, float bottom, float left)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_MARGIN,
            ArkUIValue.F(top), ArkUIValue.F(right), ArkUIValue.F(bottom), ArkUIValue.F(left));
    }

    /// <summary>交叉轴子项对齐（NODE_ALIGN_SELF，ArkUI_ItemAlignment）—— flex 容器内逐子项对齐，
    /// 用于 MAUI HorizontalOptions/VerticalOptions 的折衷映射（ArkUI alignItems 是容器级）</summary>
    public void SetAlignSelf(ArkUI_ItemAlignment alignment)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_ALIGN_SELF, ArkUIValue.I((int)alignment));
    }

    /// <summary>是否可见（NODE_VISIBILITY：0 = Visible）</summary>
    public bool Visible
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_VISIBILITY,
            ArkUIValue.I(value ? 0 : 1));
    }

    /// <summary>不透明度 0.0~1.0（NODE_OPACITY）——页面切换淡入动画的目标属性</summary>
    public void SetOpacity(float opacity)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_OPACITY, ArkUIValue.F(opacity));
    }

    /// <summary>
    /// 显式动画（animateTo）：updates 闭包内的属性变更按 durationMs 插值过渡。
    /// 返回的 Task 在动画完成回调时结束；闭包由 ArkUI 在回调时机执行，节点须已挂树。
    /// </summary>
    public Task AnimateAsync(Action updates, int durationMs = 250)
    {
        ThrowIfDisposed();
        var context = ArkUINativeApi.OH_ArkUI_GetContextByNode(_handle);
        if (context == IntPtr.Zero)
        {
            updates();
            return Task.CompletedTask;
        }

        var state = new AnimState { Updates = updates };
        var handle = GCHandle.Alloc(state);
        var update = new ArkUI_ContextCallback
        {
            UserData = (void*)GCHandle.ToIntPtr(handle),
            Callback = &AnimUpdateTrampoline,
        };
        var complete = new ArkUI_AnimateCompleteCallback
        {
            Type = ArkUI_FinishCallbackType.ARKUI_FINISH_CALLBACK_TYPE_LOGICAL,
            UserData = (void*)GCHandle.ToIntPtr(handle),
            Callback = &AnimCompleteTrampoline,
        };

        var option = ArkUINativeApi.OH_ArkUI_AnimateOption_Create();
        ArkUINativeApi.OH_ArkUI_AnimateOption_SetDuration(option, durationMs);
        ArkUINativeApi.OH_ArkUI_AnimateOption_SetCurve(option, ArkUI_AnimationCurve.ARKUI_CURVE_EASE_IN_OUT);
        var status = ArkUINativeApi.Animate->animateTo(context, option, &update, &complete);
        ArkUINativeApi.OH_ArkUI_AnimateOption_Dispose(option);

        if (status != 0)
        {
            handle.Free();
            throw new InvalidOperationException($"animateTo failed: {status}");
        }
        return state.Completion.Task;
    }

    private sealed class AnimState
    {
        public Action Updates = default!;
        public TaskCompletionSource Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool Done;
    }

    [UnmanagedCallersOnly]
    private static void AnimUpdateTrampoline(void* userData)
    {
        var state = (AnimState?)GCHandle.FromIntPtr((IntPtr)userData).Target;
        state?.Updates();
    }

    [UnmanagedCallersOnly]
    private static void AnimCompleteTrampoline(void* userData)
    {
        var handle = GCHandle.FromIntPtr((IntPtr)userData);
        if (handle.Target is AnimState state && !state.Done)
        {
            state.Done = true;
            state.Completion.SetResult();
        }
        handle.Free();
    }

    internal ArkUI_NodeHandle Handle => _handle;
    internal int TargetId => _targetId;

    // ───────────────────────── 属性 ─────────────────────────

    /// <summary>设置数值型属性（单值或多值数组）</summary>
    protected void SetNumericAttribute(ArkUI_NodeAttributeType attribute, params ArkUI_NumberValue[] values)
    {
        ThrowIfDisposed();
        fixed (ArkUI_NumberValue* p = values)
        {
            var item = new ArkUI_AttributeItem { value = p, size = values.Length };
            var status = ArkUINativeApi.SetAttribute(_handle, attribute, &item);
            if (status != 0)
                throw new InvalidOperationException($"SetAttribute({attribute}) failed: {status}");
        }
    }

    /// <summary>设置字符串型属性</summary>
    protected void SetStringAttribute(ArkUI_NodeAttributeType attribute, string value)
    {
        ThrowIfDisposed();
        var utf8 = Encoding.UTF8.GetBytes(value);
        fixed (byte* p = utf8)
        {
            var item = new ArkUI_AttributeItem { @string = p };
            var status = ArkUINativeApi.SetAttribute(_handle, attribute, &item);
            if (status != 0)
                throw new InvalidOperationException($"SetAttribute({attribute}) failed: {status}");
        }
    }

    /// <summary>设置对象型属性（如 ArkUI_TextStyle 等复杂结构）</summary>
    protected void SetObjectAttribute(ArkUI_NodeAttributeType attribute, void* objectPtr)
    {
        ThrowIfDisposed();
        var item = new ArkUI_AttributeItem { @object = objectPtr };
        var status = ArkUINativeApi.SetAttribute(_handle, attribute, &item);
        if (status != 0)
            throw new InvalidOperationException($"SetAttribute({attribute}) failed: {status}");
    }

    /// <summary>复位属性到默认值</summary>
    protected void ResetAttribute(ArkUI_NodeAttributeType attribute)
    {
        ThrowIfDisposed();
        var status = ArkUINativeApi.ResetAttribute(_handle, attribute);
        if (status != 0)
            throw new InvalidOperationException($"ResetAttribute({attribute}) failed: {status}");
    }

    // ───────────────────────── 事件 ─────────────────────────

    /// <summary>注册节点事件处理器（同类型事件覆盖式注册，符合 ArkUI 语义）</summary>
    protected void On(ArkUI_NodeEventType eventType, Action<ArkUINodeEvent> handler)
    {
        ThrowIfDisposed();
        _handlers[eventType] = handler;
        // 注册进全局分发总线（含首次时的原生 receiver 注册），再向节点注册事件
        NodeEventBus.Register(_targetId, handler);
        ArkUINativeApi.RegisterNodeEvent(_handle, eventType, _targetId, null);
        Runtime.HiLog.Debug("HarmonyHost",
            $"[Event] register node=0x{_handle.Handle:X} type={eventType} targetId={_targetId}");
    }

    /// <summary>注销节点事件处理器</summary>
    protected void Off(ArkUI_NodeEventType eventType)
    {
        ThrowIfDisposed();
        if (_handlers.Remove(eventType))
        {
            ArkUINativeApi.UnregisterNodeEvent(_handle, eventType);
        }
    }

    /// <summary>
    /// 通用事件订阅入口。组件生成类只包装了 .d.ts 声明的事件子集；
    /// ArkUI_NodeEventType 枚举为 NDK 头文件全量（含 NODE_EVENT_ON_APPEAR / NODE_EVENT_ON_AREA_CHANGE 等），
    /// 可经此直接使用。覆盖式注册语义与 On() 一致：同类型事件后注册者替换先注册者。
    /// </summary>
    public void SubscribeEvent(ArkUI_NodeEventType eventType, Action<ArkUINodeEvent> handler)
        => On(eventType, handler);

    /// <summary>注销通用订阅（同类型覆盖式注册语义，见 SubscribeEvent）</summary>
    public void UnsubscribeEvent(ArkUI_NodeEventType eventType)
        => Off(eventType);

    // ───────────────────────── 树操作 ─────────────────────────

    /// <summary>追加子节点</summary>
    public void AddChild(ArkUINodeBase child)
    {
        ThrowIfDisposed();
        CheckAlive(child);
        var status = ArkUINativeApi.AddChild(_handle, child._handle);
        if (status != 0)
            throw new InvalidOperationException($"AddChild failed: {status}");
    }

    /// <summary>移除子节点</summary>
    public void RemoveChild(ArkUINodeBase child)
    {
        ThrowIfDisposed();
        CheckAlive(child);
        var status = ArkUINativeApi.RemoveChild(_handle, child._handle);
        if (status != 0)
            throw new InvalidOperationException($"RemoveChild failed: {status}");
    }

    /// <summary>移除全部子节点</summary>
    public void RemoveAllChildren()
    {
        ThrowIfDisposed();
        var status = ArkUINativeApi.RemoveAllChildren(_handle);
        if (status != 0)
            throw new InvalidOperationException($"RemoveAllChildren failed: {status}");
    }

    /// <summary>在指定兄弟节点后插入子节点</summary>
    public void InsertChildAfter(ArkUINodeBase child, ArkUINodeBase? sibling)
    {
        ThrowIfDisposed();
        CheckAlive(child);
        var status = ArkUINativeApi.InsertChildAfter(
            _handle, child._handle, sibling?._handle ?? default);
        if (status != 0)
            throw new InvalidOperationException($"InsertChildAfter failed: {status}");
    }

    /// <summary>在指定位置插入子节点</summary>
    public void InsertChildAt(ArkUINodeBase child, int position)
    {
        ThrowIfDisposed();
        CheckAlive(child);
        var status = ArkUINativeApi.InsertChildAt(_handle, child._handle, position);
        if (status != 0)
            throw new InvalidOperationException($"InsertChildAt failed: {status}");
    }

    /// <summary>子节点数量</summary>
    public uint ChildCount
    {
        get
        {
            ThrowIfDisposed();
            return ArkUINativeApi.GetTotalChildCount(_handle);
        }
    }

    // ───────────────────────── 布局 ─────────────────────────

    /// <summary>节点实测尺寸（px）</summary>
    public ArkUI_IntSize MeasuredSize
    {
        get
        {
            ThrowIfDisposed();
            return ArkUINativeApi.GetMeasuredSize(_handle);
        }
    }

    /// <summary>节点布局位置（px）</summary>
    public ArkUI_IntOffset LayoutPosition
    {
        get
        {
            ThrowIfDisposed();
            return ArkUINativeApi.GetLayoutPosition(_handle);
        }
    }

    // ───────────────────────── 生命周期 ─────────────────────────

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (!_handle.IsNull)
        {
            if (_gradientSizeChangeTargetId != 0)
                NodeEventBus.Unregister(_gradientSizeChangeTargetId);
            NodeEventBus.Unregister(_targetId);
            ArkUINativeApi.DisposeNode(_handle);
            _handle = default;
        }
        _disposed = true;
    }

    ~ArkUINodeBase()
    {
        // 终结器路径无法安全触达原生 UI 线程，仅记录句柄泄漏；
        // 节点必须在 UI 线程显式 Dispose。
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    private static void CheckAlive(ArkUINodeBase? node)
    {
        if (node == null || node._disposed || node._handle.IsNull)
            throw new ArgumentException("Node is disposed or invalid", nameof(node));
    }
}
