#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using HarmonyOS.Interop;
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
    private readonly HashSet<ArkUI_NodeEventType> _handlers = new();
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

    /// <summary>背景色（u32 重载）——MAUI Color.ToUint() 即 0xAARRGGBB，Handler 层直传</summary>
    public void SetBackgroundColor(uint argb)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_BACKGROUND_COLOR, ArkUIValue.U(argb));
    }

    /// <summary>字重（NODE_FONT_WEIGHT，ArkUI_FontWeight）</summary>
    public void SetFontWeight(ArkUI_FontWeight weight)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_FONT_WEIGHT, ArkUIValue.I((int)weight));

    /// <summary>字体样式（NODE_FONT_STYLE，ArkUI_FontStyle：NORMAL/ITALIC）</summary>
    public void SetFontStyle(ArkUI_FontStyle style)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_FONT_STYLE, ArkUIValue.I((int)style));

    /// <summary>行高倍数（NODE_TEXT_LINE_HEIGHT_MULTIPLE，f32；MAUI LineHeight 语义即倍数）</summary>
    public void SetLineHeightMultiple(float multiple)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TEXT_LINE_HEIGHT_MULTIPLE, ArkUIValue.F(multiple));

    /// <summary>边框色（NODE_BORDER_COLOR，四边同值，0xAARRGGBB）</summary>
    public void SetBorderColor(uint argb)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_BORDER_COLOR,
            ArkUIValue.U(argb), ArkUIValue.U(argb), ArkUIValue.U(argb), ArkUIValue.U(argb));
    }

    /// <summary>边框宽（NODE_BORDER_WIDTH，四边同值，vp）</summary>
    public void SetBorderWidth(float widthVp)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_BORDER_WIDTH,
            ArkUIValue.F(widthVp), ArkUIValue.F(widthVp), ArkUIValue.F(widthVp), ArkUIValue.F(widthVp));
    }

    /// <summary>边框圆角（NODE_BORDER_RADIUS，四角 vp，顺序 TL/TR/BL/BR 与 MAUI CornerRadius 一致）。
    /// 同时开启 NODE_CLIP 让内容随圆角裁剪。</summary>
    public void SetBorderRadius(float topLeft, float topRight, float bottomLeft, float bottomRight)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_BORDER_RADIUS,
            ArkUIValue.F(topLeft), ArkUIValue.F(topRight), ArkUIValue.F(bottomLeft), ArkUIValue.F(bottomRight));
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_CLIP, ArkUIValue.I(1));
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
            NodeEventBus.Register(_gradientSizeChangeTargetId, ArkUI_NodeEventType.NODE_ON_SIZE_CHANGE,
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

    /// <summary>恢复宽度自适应：同时清掉固定宽和百分比宽，供 ScrollView 等容器切换主轴时复位。</summary>
    public void SetWidthAuto()
    {
        ResetAttribute(ArkUI_NodeAttributeType.NODE_WIDTH);
        ResetAttribute(ArkUI_NodeAttributeType.NODE_WIDTH_PERCENT);
    }

    /// <summary>恢复高度自适应：同时清掉固定高和百分比高，供 ScrollView 等容器切换主轴时复位。</summary>
    public void SetHeightAuto()
    {
        ResetAttribute(ArkUI_NodeAttributeType.NODE_HEIGHT);
        ResetAttribute(ArkUI_NodeAttributeType.NODE_HEIGHT_PERCENT);
    }

    /// <summary>字体族（NODE_FONT_FAMILY）。空字符串走平台默认字体。</summary>
    public void SetFontFamily(string family)
    {
        SetStringAttribute(ArkUI_NodeAttributeType.NODE_FONT_FAMILY, family ?? string.Empty);
    }

    /// <summary>命中测试行为（NODE_HIT_TEST_BEHAVIOR）。展示用子节点（如 Button 内建 label
    /// 的替代 Text）须设为 TRANSPARENT 透传点击，否则会吃掉父节点的 CLICK。</summary>
    public void SetHitTestBehavior(ArkUI_HitTestMode mode)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_HIT_TEST_BEHAVIOR, ArkUIValue.I((int)mode));

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

    /// <summary>主轴剩余空间分配权重（NODE_FLEX_GROW，默认 0）—— flex 容器内"占满剩余空间"</summary>
    public void SetFlexGrow(float grow)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_FLEX_GROW, ArkUIValue.F(grow));
    }

    /// <summary>是否可见（NODE_VISIBILITY：0 = Visible）</summary>
    public bool Visible
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_VISIBILITY,
            ArkUIValue.I(value ? 0 : 1));
    }

    /// <summary>三态可见性（NODE_VISIBILITY）：Visible / Hidden（隐藏但占位）/ None（不占位）。
    /// MAUI Visibility.Visible/Hidden/Collapsed 一一对应。</summary>
    public void SetVisibility(ArkUI_Visibility visibility)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_VISIBILITY, ArkUIValue.I((int)visibility));

    /// <summary>可交互（NODE_ENABLED）：false 时节点呈禁用态且不响应触摸</summary>
    public bool Enabled
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_ENABLED, ArkUIValue.I(value ? 1 : 0));
    }

    /// <summary>不透明度 0.0~1.0（NODE_OPACITY）——页面切换淡入动画的目标属性</summary>
    public void SetOpacity(float opacity)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_OPACITY, ArkUIValue.F(opacity));
    }

    /// <summary>平移偏移（vp，NODE_TRANSLATE：value[0]=x、[1]=y、[2]=z，三值齐发——
    /// NDK 形状为 ARRAY_OF_3，缺 z 会被参数校验 401 拒绝）</summary>
    public void SetTranslate(float x, float y)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TRANSLATE,
            ArkUIValue.F(x), ArkUIValue.F(y), ArkUIValue.F(0));

    /// <summary>缩放比例（NODE_SCALE：value[0]=x、[1]=y，1.0 = 原始尺寸）</summary>
    public void SetScale(float x, float y)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_SCALE,
            ArkUIValue.F(x), ArkUIValue.F(y));

    /// <summary>
    /// 旋转（NODE_ROTATE：轴向量 x/y/z + 角度 + 视距，单次写入单轴语义）。
    /// MAUI 的 Rotation(X/Y) 三角独立：按非零优先级选主轴（z > x > y）；
    /// 全零写 z 轴 0 度复位。
    /// </summary>
    public void SetRotation(float xAngle, float yAngle, float zAngle)
    {
        (float ax, float ay, float az, float angle) =
            MathF.Abs(zAngle) > 0.001f ? (0, 0, 1, zAngle) :
            MathF.Abs(xAngle) > 0.001f ? (1, 0, 0, xAngle) :
            MathF.Abs(yAngle) > 0.001f ? (0, 1, 0, yAngle) :
            (0, 0, 1, 0);
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_ROTATE,
            ArkUIValue.F(ax), ArkUIValue.F(ay), ArkUIValue.F(az),
            ArkUIValue.F(angle), ArkUIValue.F(0));
    }

    /// <summary>变换中心（NODE_TRANSFORM_CENTER）：前 3 值为 vp 绝对坐标、后 3 值为
    /// 百分比数字（0.5 = 50%）；MAUI AnchorX/Y 为 0~1 分数，走百分比槽位。</summary>
    public void SetPivot(float x, float y)
        => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TRANSFORM_CENTER,
            ArkUIValue.F(0), ArkUIValue.F(0), ArkUIValue.F(0),
            ArkUIValue.F(x), ArkUIValue.F(y), ArkUIValue.F(0));

    /// <summary>层级（NODE_Z_INDEX，值大者在上，默认 0）——MAUI IView.ZIndex 的翻译；
    /// 生成器未注册该属性 shape（native-gaps.json），按手写节点层约定补充</summary>
    public void SetZIndex(float zIndex)
    {
        SetNumericAttribute(ArkUI_NodeAttributeType.NODE_Z_INDEX, ArkUIValue.F(zIndex));
    }

    private int _areaChangeObserverTargetId;
    private Action? _areaChangeObserver;

    /// <summary>
    /// 区域变化观察（NODE_EVENT_ON_AREA_CHANGE，独立 targetId，不占用 On()/SubscribeEvent
    /// 的覆盖式订阅槽；与渐变的 NODE_ON_SIZE_CHANGE 专用槽分属不同事件类型互不冲突）。
    /// 位置或尺寸变化即回调（无载荷），供托管布局监听子节点自量测/内容变化触发重排。
    /// </summary>
    public void SetAreaChangeObserver(Action? observer)
    {
        ThrowIfDisposed();
        _areaChangeObserver = observer;
        if (observer is null)
        {
            if (_areaChangeObserverTargetId != 0)
            {
                NodeEventBus.Unregister(_areaChangeObserverTargetId, ArkUI_NodeEventType.NODE_EVENT_ON_AREA_CHANGE);
                ArkUINativeApi.UnregisterNodeEvent(_handle, ArkUI_NodeEventType.NODE_EVENT_ON_AREA_CHANGE);
                _areaChangeObserverTargetId = 0;
            }
            return;
        }
        if (_areaChangeObserverTargetId == 0)
        {
            _areaChangeObserverTargetId = NodeEventBus.NextTargetId();
            NodeEventBus.Register(_areaChangeObserverTargetId, ArkUI_NodeEventType.NODE_EVENT_ON_AREA_CHANGE,
                _ => _areaChangeObserver?.Invoke());
            ArkUINativeApi.RegisterNodeEvent(
                _handle, ArkUI_NodeEventType.NODE_EVENT_ON_AREA_CHANGE, _areaChangeObserverTargetId, null);
        }
    }

    /// <summary>
    /// 显式动画（animateTo）：updates 闭包内的属性变更按 durationMs 插值过渡。
    /// completed 在动画完成回调（UI 线程）内联执行——用于必须在 UI 线程收尾的导航过渡，
    /// 避免 Task 续体漂移到线程池（宿主未安装 UI 线程 SynchronizationContext）。
    /// </summary>
    public void Animate(Action updates, Action? completed, int durationMs = 250)
    {
        ThrowIfDisposed();
        var state = new AnimState { Updates = updates, Completed = completed };
        RunAnimateChecked(state, durationMs);
    }

    /// <summary>
    /// 显式动画（animateTo）：updates 闭包内的属性变更按 durationMs 插值过渡。
    /// 返回的 Task 在动画完成回调时结束；闭包由 ArkUI 在回调时机执行，节点须已挂树。
    /// </summary>
    public Task AnimateAsync(Action updates, int durationMs = 250)
    {
        ThrowIfDisposed();
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var state = new AnimState { Updates = updates };
        state.Completed = () =>
        {
            // update 闭包在原生回调内抛出的异常经 trampoline 捕获暂存（Failure），
            // 在此向 await 方传播，避免"静默跳过属性更新却返回成功"
            if (state.Failure is { } failure)
                tcs.TrySetException(failure);
            else
                tcs.TrySetResult();
        };
        RunAnimateChecked(state, durationMs);
        return tcs.Task;
    }

    private void RunAnimateChecked(AnimState state, int durationMs)
    {
        var context = ArkUINativeApi.OH_ArkUI_GetContextByNode(_handle);
        if (context == IntPtr.Zero)
        {
            // 无 UIContext（节点未挂树）：同步直跑，异常照常向调用方传播
            state.Updates();
            state.Completed?.Invoke();
            return;
        }
        RunAnimate(state, context, durationMs);
    }

    private void RunAnimate(AnimState state, IntPtr context, int durationMs)
    {
        var handle = GCHandle.Alloc(state);
        var update = new ArkUI_ContextCallback
        {
            UserData = (void*)GCHandle.ToIntPtr(handle),
            Callback = &AnimUpdateTrampoline,
        };
        var complete = new ArkUI_AnimateCompleteCallback
        {
            Type = ArkUI_FinishCallbackType.ARKUI_FINISH_CALLBACK_LOGICALLY,
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
    }

    private sealed class AnimState
    {
        public Action Updates = default!;
        public Action? Completed;
        /// <summary>update 闭包在原生回调中抛出的异常（由完成回调转交 await 方）</summary>
        public Exception? Failure;
        public bool Done;
    }

    [UnmanagedCallersOnly]
    private static void AnimUpdateTrampoline(void* userData)
    {
        var state = (AnimState?)GCHandle.FromIntPtr((IntPtr)userData).Target;
        if (state is null)
            return;
        try
        {
            state.Updates();
        }
        catch (Exception ex)
        {
            // 与 complete 跳板同纪律：异常不得穿透原生帧（会终止进程）；
            // 暂存后由完成回调交给同步/await 调用方
            state.Failure = ex;
            HiLog.Error("HarmonyAnim", $"animate update-closure error: {ex.GetType().Name}: {ex.Message}");
        }
    }

    [UnmanagedCallersOnly]
    private static void AnimCompleteTrampoline(void* userData)
    {
        var handle = GCHandle.FromIntPtr((IntPtr)userData);
        if (handle.Target is AnimState state && !state.Done)
        {
            state.Done = true;
            try
            {
                state.Completed?.Invoke();
            }
            catch (Exception ex)
            {
                // 不得穿透原生帧（进程会随之终止）
                HiLog.Error("HarmonyAnim", $"animate completed-callback error: {ex.GetType().Name}: {ex.Message}");
            }
        }
        handle.Free();
    }

    internal ArkUI_NodeHandle Handle => _handle;
    internal int TargetId => _targetId;

    // ───────────────────────── 属性 ─────────────────────────

    /// <summary>设置数值型属性（单值或多值数组）。C# 14 params span：调用点零数组分配，
    /// 编译器对 ≤N 元素的实参栈分配（全仓最热的原生 API 路径）。</summary>
    protected void SetNumericAttribute(ArkUI_NodeAttributeType attribute, params ReadOnlySpan<ArkUI_NumberValue> values)
    {
        EnsureHandle();
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
        EnsureHandle();
        // 原生侧按 NUL 结尾 C 串读取：GetBytes 不补终止符，长度恰好时会越界读
        // （短文本侥幸正常、长文本被截断/吃掉）——显式补 0，空串即 "\0"
        var utf8 = new byte[Encoding.UTF8.GetByteCount(value) + 1];
        Encoding.UTF8.GetBytes(value.AsSpan(), utf8.AsSpan());
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
        EnsureHandle();
        var item = new ArkUI_AttributeItem { @object = objectPtr };
        var status = ArkUINativeApi.SetAttribute(_handle, attribute, &item);
        if (status != 0)
            throw new InvalidOperationException($"SetAttribute({attribute}) failed: {status}");
    }

    /// <summary>复位属性到默认值</summary>
    protected void ResetAttribute(ArkUI_NodeAttributeType attribute)
    {
        EnsureHandle();
        var status = ArkUINativeApi.ResetAttribute(_handle, attribute);
        if (status != 0)
            throw new InvalidOperationException($"ResetAttribute({attribute}) failed: {status}");
    }

    // ───────────────────────── 事件 ─────────────────────────

    /// <summary>注册节点事件处理器（同类型事件覆盖式注册，符合 ArkUI 语义）</summary>
    protected void On(ArkUI_NodeEventType eventType, Action<ArkUINodeEvent> handler)
    {
        EnsureHandle();
        _handlers.Add(eventType);
        // 注册进全局分发总线（含首次时的原生 receiver 注册），再向节点注册事件
        NodeEventBus.Register(_targetId, eventType, handler);
        ArkUINativeApi.RegisterNodeEvent(_handle, eventType, _targetId, null);
        HiLog.Debug("HarmonyHost",
            $"[Event] register node=0x{_handle.Handle:X} type={eventType} targetId={_targetId}");
    }

    /// <summary>注销节点事件处理器</summary>
    protected void Off(ArkUI_NodeEventType eventType)
    {
        EnsureHandle();
        if (_handlers.Remove(eventType))
        {
            NodeEventBus.Unregister(_targetId, eventType);
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
        EnsureHandle();
        child.EnsureHandle();
        CheckAlive(child);
        var status = ArkUINativeApi.AddChild(_handle, child._handle);
        if (status != 0)
            throw new InvalidOperationException($"AddChild failed: {status}");
    }

    /// <summary>移除子节点</summary>
    public void RemoveChild(ArkUINodeBase child)
    {
        EnsureHandle();
        child.EnsureHandle();
        CheckAlive(child);
        var status = ArkUINativeApi.RemoveChild(_handle, child._handle);
        if (status != 0)
            throw new InvalidOperationException($"RemoveChild failed: {status}");
    }

    /// <summary>移除全部子节点</summary>
    public void RemoveAllChildren()
    {
        EnsureHandle();
        var status = ArkUINativeApi.RemoveAllChildren(_handle);
        if (status != 0)
            throw new InvalidOperationException($"RemoveAllChildren failed: {status}");
    }

    /// <summary>在指定兄弟节点后插入子节点</summary>
    public void InsertChildAfter(ArkUINodeBase child, ArkUINodeBase? sibling)
    {
        EnsureHandle();
        child.EnsureHandle();
        sibling?.EnsureHandle();
        CheckAlive(child);
        var status = ArkUINativeApi.InsertChildAfter(
            _handle, child._handle, sibling?._handle ?? default);
        if (status != 0)
            throw new InvalidOperationException($"InsertChildAfter failed: {status}");
    }

    /// <summary>在指定位置插入子节点</summary>
    public void InsertChildAt(ArkUINodeBase child, int position)
    {
        EnsureHandle();
        child.EnsureHandle();
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

    /// <summary>可拖拽（OH_ArkUI_SetNodeDraggable；返回 0 成功）</summary>
    public int SetDraggable(bool enabled)
    {
        EnsureHandle();
        return ArkUINativeApi.SetNodeDraggable(_handle, enabled);
    }

    /// <summary>放侧放行任意拖拽数据类型（OH_ArkUI_AllowNodeAllDropDataTypes；返回 0 成功）</summary>
    public int AllowAllDropDataTypes()
    {
        EnsureHandle();
        return ArkUINativeApi.AllowNodeAllDropDataTypes(_handle);
    }

    /// <summary>挂载虚拟化 adapter（NODE_LIST_NODE_ADAPTER，item.@object；返回 0 成功）</summary>
    public int SetNodeAdapter(IntPtr adapterHandle)
    {
        EnsureHandle();
        var item = new ArkUI_AttributeItem { @object = (void*)adapterHandle };
        var status = ArkUINativeApi.SetAttribute(
            _handle, ArkUI_NodeAttributeType.NODE_LIST_NODE_ADAPTER, &item);
        if (status != 0)
            throw new InvalidOperationException($"SetNodeAdapter failed: {status}");
        return status;
    }

    /// <summary>摘除虚拟化 adapter（NODE_LIST_NODE_ADAPTER 复位默认；dispose adapter 前必须先摘）</summary>
    public void ResetNodeAdapter()
    {
        EnsureHandle();
        var status = ArkUINativeApi.ResetAttribute(
            _handle, ArkUI_NodeAttributeType.NODE_LIST_NODE_ADAPTER);
        if (status != 0)
            throw new InvalidOperationException($"ResetNodeAdapter failed: {status}");
    }

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
        EnsureHandle();

        if (!_handle.IsNull)
        {
            if (_gradientSizeChangeTargetId != 0)
                NodeEventBus.Unregister(_gradientSizeChangeTargetId, ArkUI_NodeEventType.NODE_ON_SIZE_CHANGE);
            if (_areaChangeObserverTargetId != 0)
                NodeEventBus.Unregister(_areaChangeObserverTargetId, ArkUI_NodeEventType.NODE_EVENT_ON_AREA_CHANGE);
            foreach (var eventType in _handlers)
                NodeEventBus.Unregister(_targetId, eventType);
            ArkUINativeApi.DisposeNode(_handle);
            _handle = default;
        }
        _disposed = true;
    }

    ~ArkUINodeBase()
    {
        // 终结器路径无法安全触达原生 UI 线程（ArkUI C API 有主线程亲和
        // 且无安全的跨线程回收通道），节点必须在 UI 线程显式 Dispose。
        // 此处不做任何处理，也不提供跨作用域的原生句柄诊断。
    }

    private void ThrowIfDisposed()
    {
        EnsureHandle();
    }

    private void EnsureHandle()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
        NativeMainThread.Ensure();
    }

    private static void CheckAlive(ArkUINodeBase? node)
    {
        if (node == null || node._disposed || node._handle.IsNull)
            throw new ArgumentException("Node is disposed or invalid", nameof(node));
    }
}
