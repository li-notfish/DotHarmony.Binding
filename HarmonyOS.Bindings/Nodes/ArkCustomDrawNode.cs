#nullable enable
using System;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.ArkUI;

/// <summary>
/// 自绘节点（ARKUI_NODE_CUSTOM）：经 NODE_ON_DRAW 自定义绘制。
/// 回调拿到 OHDrawingCanvas + vp 尺寸；画布矩阵为恒等 px 空间（DrawContext 无预缩放），
/// 回调以 vp 作图前须 Scale(density)（节点层代做，见 OnDrawEvent）。
/// 实测语义（API 26）：ON_MEASURE 的 LayoutConstraint 最大宽/高为 int(px)，上报约束最大值即
/// Fill（显式 NODE_WIDTH/HEIGHT 由 ArkUI 收紧约束后落在约束内）；ON_DRAW 的 DrawContext
/// 尺寸为 px；密度 = 绘制区 px ÷ NODE_ON_SIZE_CHANGE 上报的 vp 尺寸。
/// </summary>
public sealed unsafe class ArkCustomDrawNode : ArkUINodeBase
{
    private Action<OHDrawingCanvas, float, float>? _drawCallback;
    private readonly int _customTargetId;
    private float _vpWidth;
    private float _vpHeight;
    private bool _firstDrawLogged;

    public ArkCustomDrawNode() : base(ArkUI_NodeType.ARKUI_NODE_CUSTOM)
    {
        _customTargetId = NodeCustomEventBus.NextTargetId();
        NodeCustomEventBus.Register(
            _customTargetId, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_MEASURE, OnMeasureEvent);
        ArkUINativeApi.RegisterNodeCustomEvent(
            Handle, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_MEASURE, _customTargetId, null);
        // vp 尺寸跟踪（NODE_ON_SIZE_CHANGE 的 data[2]/data[3]，经 SizeChangeWidth/Height 访问器）
        On(ArkUI_NodeEventType.NODE_ON_SIZE_CHANGE, e =>
        {
            _vpWidth = e.SizeChangeWidth;
            _vpHeight = e.SizeChangeHeight;
        });
    }

    /// <summary>
    /// 注册自绘回调（覆盖式注册；传 null 注销）。回调在 UI 线程每帧执行，禁止分配重对象。
    /// </summary>
    public void SetDrawCallback(Action<OHDrawingCanvas, float, float>? callback)
    {
        EnsureAlive();
        _drawCallback = callback;
        if (callback != null)
        {
            NodeCustomEventBus.Register(
                _customTargetId, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_DRAW, OnDrawEvent);
            ArkUINativeApi.RegisterNodeCustomEvent(
                Handle, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_DRAW, _customTargetId, null);
        }
        else
        {
            NodeCustomEventBus.Unregister(
                _customTargetId, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_DRAW);
            ArkUINativeApi.UnregisterNodeCustomEvent(Handle, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_DRAW);
        }
    }

    /// <summary>请求重绘（触发 ON_MEASURE/ON_LAYOUT/ON_DRAW）</summary>
    public void Invalidate()
    {
        EnsureAlive();
        ArkUINativeApi.MarkDirty(Handle, ArkUI_NodeDirtyFlag.NODE_NEED_MEASURE | ArkUI_NodeDirtyFlag.NODE_NEED_LAYOUT);
    }

    /// <summary>Dispose 后句柄置空，用作守卫</summary>
    private void EnsureAlive()
    {
        if (Handle.IsNull)
            throw new ObjectDisposedException(nameof(ArkCustomDrawNode));
    }

    /// <summary>测量事件：约束为 int(px)，Fill 语义上报约束中可到达的最大值</summary>
    private void OnMeasureEvent(ArkUICustomEvent ev)
    {
        var constraint = ev.ConstraintInMeasure;
        if (constraint == null)
            return;
        ArkUINativeApi.SetMeasuredSize(Handle,
            ArkUINativeApi.ConstraintMaxWidth(constraint),
            ArkUINativeApi.ConstraintMaxHeight(constraint));
    }

    private void OnDrawEvent(ArkUICustomEvent ev)
    {
        var (canvas, widthPx, heightPx) = ev.GetDrawContext();
        // 密度推算：宽高各自可算则互校（不一致时取宽），单边未知用另一边，皆未知按 px 直绘
        var densityW = _vpWidth > 0 ? widthPx / _vpWidth : 0f;
        var densityH = _vpHeight > 0 ? heightPx / _vpHeight : 0f;
        var density = densityW > 0 ? densityW : densityH > 0 ? densityH : 1f;
        var widthVp = widthPx / density;
        var heightVp = heightPx / density;
        if (!float.IsFinite(widthVp) || !float.IsFinite(heightVp) || density <= 0)
        {
            widthVp = widthPx; // 单位未知时按 px 直绘，保证可见
            heightVp = heightPx;
            density = 1f;
        }
        if (!_firstDrawLogged)
        {
            _firstDrawLogged = true;
            HiLog.Debug("CustomDraw",
                $"draw: ctxPx=({widthPx},{heightPx}) vp=({_vpWidth},{_vpHeight}) density={density} final=({widthVp},{heightVp})");
        }
        if (density != 1f)
            canvas.Scale(density, density); // px 空间画布 → 回调以 vp 作图
        _drawCallback?.Invoke(canvas, widthVp, heightVp);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Handle.IsNull)
        {
            // 先注销 native（停事件源），再清路由表 —— 反序会让在途事件静默丢弃
            ArkUINativeApi.UnregisterNodeCustomEvent(Handle, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_MEASURE);
            ArkUINativeApi.UnregisterNodeCustomEvent(Handle, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_DRAW);
            NodeCustomEventBus.Unregister(_customTargetId, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_MEASURE);
            NodeCustomEventBus.Unregister(_customTargetId, ArkUI_NodeCustomEventType.ARKUI_NODE_CUSTOM_EVENT_ON_DRAW);
        }
        base.Dispose(disposing);
    }
}
