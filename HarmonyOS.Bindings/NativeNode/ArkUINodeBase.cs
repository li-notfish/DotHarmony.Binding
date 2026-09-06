#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

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

    /// <summary>四边外边距（NODE_MARGIN，单位 vp）</summary>
    public void SetMarginEdges(float top, float right, float bottom, float left)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_MARGIN,
            ArkUIValue.F(top), ArkUIValue.F(right), ArkUIValue.F(bottom), ArkUIValue.F(left));
    }

    /// <summary>是否可见（NODE_VISIBILITY：0 = Visible）</summary>
    public bool Visible
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_VISIBILITY,
            ArkUIValue.I(value ? 0 : 1));
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
