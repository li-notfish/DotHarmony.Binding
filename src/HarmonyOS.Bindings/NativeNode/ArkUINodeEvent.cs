#nullable enable
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using HarmonyOS.Interop;
namespace HarmonyOS.Bindings.NativeNode;

/// <summary>
/// ArkUI 节点事件的托管包装（只读视图）
/// </summary>
public readonly unsafe struct ArkUINodeEvent
{
    private readonly ArkUI_NodeEvent* _ptr;

    internal ArkUINodeEvent(ArkUI_NodeEvent* ptr) => _ptr = ptr;

    /// <summary>事件类型</summary>
    public ArkUI_NodeEventType EventType => ArkUINativeApi.GetEventType(_ptr);

    /// <summary>注册事件时指定的 targetId</summary>
    public int TargetId => ArkUINativeApi.GetTargetId(_ptr);

    /// <summary>发出事件的节点句柄</summary>
    public ArkUI_NodeHandle NodeHandle => ArkUINativeApi.GetEventNodeHandle(_ptr);

    /// <summary>原生输入事件指针（触摸/按键），需要时经 ui_input_event.h 访问器解析</summary>
    public IntPtr InputEvent => ArkUINativeApi.GetInputEvent(_ptr);

    /// <summary>拖拽事件指针（NODE_ON_DRAG_* / NODE_ON_DROP），经 drag_and_drop.h 访问器解析</summary>
    public IntPtr DragEvent => ArkUINativeApi.GetDragEvent(_ptr);

    /// <summary>写入拖拽载荷（UDMF data 指针，NODE_ON_DRAG_START；status != 0 时抛出）</summary>
    public void SetDragData(IntPtr udmfData)
    {
        var status = ArkUINativeApi.DragEventSetData(DragEvent, udmfData);
        if (status != 0)
            throw new InvalidOperationException($"DragEventSetData failed: {status}");
    }

    /// <summary>读取拖拽 UDMF 载荷（NODE_ON_DROP / ON_DRAG_*；false = 无载荷）</summary>
    public bool TryGetUdmfData(IntPtr udmfData)
        => ArkUINativeApi.DragEventGetUdmfData(DragEvent, udmfData) == 0;

    /// <summary>读取 NodeComponentEvent 附加数值数组第 index 项</summary>
    public ArkUI_NumberValue GetNumber(int index) => ArkUINativeApi.GetEventNumber(_ptr, index);

    /// <summary>读取字符串事件（StringAsyncEvent）的 UTF8 字符串</summary>
    public string? GetString()
    {
        var p = ArkUINativeApi.GetStringAsyncEvent(_ptr);
        if (p == IntPtr.Zero) return null;
        var ev = *(ArkUI_StringAsyncEvent*)p;
        return Utf8ToString(ev.pStr);
    }

    // ───────────── 点击事件（NODE_ON_CLICK）data[] 布局访问器 ─────────────
    // 经 GetNodeComponentEvent 直接读 ArkUI_NodeComponentEvent.data[12]：
    // data[0..1]=组件内坐标(vp) data[2]=时间戳(μs) data[3]=设备(1鼠标2触摸4按键)
    // data[4..5]=窗口坐标 data[6..7]=屏幕坐标
    // （OH_ArkUI_NodeEvent_GetNumberValue 对 click 事件返回 106108，不可用）

    /// <summary>读取 NodeComponentEvent 附加数值数组第 index 项（各事件的 data 布局见 native_node.h 注释；index ∈ [0, 12)）</summary>
    public ref readonly ArkUI_NumberValue ComponentData(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        if (index >= 12)
            throw new ArgumentOutOfRangeException(nameof(index), index,
                "ArkUI_NodeComponentEvent data has exactly 12 slots (native_node.h)");
        var p = ArkUINativeApi.GetNodeComponentEvent(_ptr);
        if (p == IntPtr.Zero)
            throw new InvalidOperationException("event carries no NodeComponentEvent data");
        return ref (*(ArkUI_NodeComponentEvent*)p).data[index];
    }

    /// <summary>点击 X 坐标（相对组件左上角，vp）</summary>
    public float ClickX => ComponentData(0).f32;

    /// <summary>点击 Y 坐标（相对组件左上角，vp）</summary>
    public float ClickY => ComponentData(1).f32;

    /// <summary>事件时间戳（相对系统启动，微秒）。
    /// 经 f32 槽位承载（native_node.h data[] 为 NumberValue），绝对精度约 24 位尾数——
    /// 大量程下只可比较先后/求差，不可换算墙钟时间。</summary>
    public long ClickTimestamp => (long)ComponentData(2).f32;

    /// <summary>输入设备：1=鼠标，2=触摸屏，4=按键</summary>
    public int ClickDevice => ComponentData(3).i32;

    /// <summary>点击 X 坐标（相对应用窗口，vp）</summary>
    public float ClickWindowX => ComponentData(4).f32;

    /// <summary>点击 Y 坐标（相对应用窗口，vp）</summary>
    public float ClickWindowY => ComponentData(5).f32;

    /// <summary>点击 X 坐标（相对屏幕，vp）</summary>
    public float ClickScreenX => ComponentData(6).f32;

    /// <summary>点击 Y 坐标（相对屏幕，vp）</summary>
    public float ClickScreenY => ComponentData(7).f32;

    // ───────────── 尺寸变化事件（NODE_ON_SIZE_CHANGE）data[] 布局访问器 ─────────────
    // 原生头文件（native_node.h @since 21）：data[0]=旧宽 data[1]=旧高 data[2]=新宽 data[3]=新高

    /// <summary>尺寸变化后的新宽度（vp）</summary>
    public float SizeChangeWidth => ComponentData(2).f32;

    /// <summary>尺寸变化后的新高度（vp）</summary>
    public float SizeChangeHeight => ComponentData(3).f32;

    internal static string? Utf8ToString(byte* p)
    {
        if (p == null) return null;
        int len = 0;
        while (p[len] != 0) len++;
        return Encoding.UTF8.GetString(p, len);
    }
}

/// <summary>
/// 全局事件分发总线。
/// registerNodeEventReceiver 只能注册一个原生接收器，此处以 (targetId, eventType) 复合键
/// 路由到各节点——同节点可并存多种事件订阅（如 Button 的 Click 与手势通道的 Touch）。
/// </summary>
internal static unsafe class NodeEventBus
{
    private static readonly ConcurrentDictionary<(int TargetId, int EventType), Action<ArkUINodeEvent>> Handlers = new();
    private static delegate* unmanaged<ArkUI_NodeEvent*, void> _receiverPtr;
    private static int _nextTargetId;
    private static bool _registered;

    /// <summary>分配进程内唯一的 targetId</summary>
    internal static int NextTargetId() => Interlocked.Increment(ref _nextTargetId);

    internal static void Register(int targetId, ArkUI_NodeEventType eventType, Action<ArkUINodeEvent> handler)
    {
        EnsureRegistered();
        Handlers[(targetId, (int)eventType)] = handler;
    }

    internal static void Unregister(int targetId, ArkUI_NodeEventType eventType)
    {
        Handlers.TryRemove((targetId, (int)eventType), out _);
    }

    private static void EnsureRegistered()
    {
        if (_registered) return;
        _receiverPtr = &Dispatch;
        ArkUINativeApi.RegisterNodeEventReceiver(_receiverPtr);
        _registered = true;
        HiLog.Debug("HarmonyHost", $"[EventBus] receiver registered, fnPtr=0x{(long)_receiverPtr:X}");
    }

    [UnmanagedCallersOnly]
    private static void Dispatch(ArkUI_NodeEvent* eventPtr)
    {
        // JS 线程热点：顺带批量回收终结器线程暂存的 napi_ref（空队列零成本）
        NapiFinalizationQueue.Drain();
        try
        {
            var key = (ArkUINativeApi.GetTargetId(eventPtr), (int)ArkUINativeApi.GetEventType(eventPtr));
            if (Handlers.TryGetValue(key, out var handler))
            {
                handler(new ArkUINodeEvent(eventPtr));
            }
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyHost", $"[EventBus] dispatch error: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
