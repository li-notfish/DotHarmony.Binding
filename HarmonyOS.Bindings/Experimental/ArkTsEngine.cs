using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Bindings.Experimental;

/// <summary>
/// ArkTS 命令总线引擎（MIGRATION_ARKTS_ENGINE.md §2 方案 A 的最小实验实现）。
///
/// 职责：
///   1. 指令队列：VirtualNode 的指令入队，<see cref="Flush"/> 经一次 napi 调用批量发给
///      ArkTS 引擎桥的 applyCommands（同帧属性写合并为单次跨语言调用的实验验证）。
///   2. 引擎桥挂接：C shim initEngine → HarmonyEngineInit 导出 → <see cref="InitializeCore"/>
///      存桥引用、把 onEvent 回调交给引擎、冲刷积压指令。
///   3. 回流分发：引擎经 onEvent(id, kind, payload) 回推事件/量测；量测写入影子缓存，
///      VirtualNode.MeasuredSize 读快照（替代同步 P/Invoke）。
///
/// 线程约定：全部调用发生在 JS/UI 线程（HarmonyInit 与 napi 回调同线程亲和）。
/// </summary>
public static class ArkTsEngine
{
    private const string Tag = "ArkTsEngine";

    private static readonly List<ArkTsCommand> _queue = new();
    private static readonly Dictionary<int, VirtualNode> _nodes = new();
    private static readonly Dictionary<int, (double W, double H, double Density)> _measures = new();
    private static int _nextId = 1;
    private static GCHandle _onEventHandle;

#if HARMONYOS
    private static NativeNodeApi.napi_ref _bridgeRef;
#endif

    /// <summary>引擎桥是否已挂接（挂接前 Flush 仅保留队列）</summary>
    public static bool IsAttached
    {
        get
        {
#if HARMONYOS
            return _bridgeRef != default;
#else
            return false;
#endif
        }
    }

    /// <summary>VirtualNode 默认指令出口</summary>
    internal static IArkTsCommandSink Sink => QueueSink.Instance;

    /// <summary>当前积压指令数（测试观测用）</summary>
    internal static int PendingCommandCount => _queue.Count;

    /// <summary>分配引擎侧节点句柄（进程内单调递增）</summary>
    public static int AllocateId() => _nextId++;

    internal static void Register(VirtualNode node) => _nodes[node.Id] = node;

    internal static void Unregister(int id) => _nodes.Remove(id);

    internal static VirtualNode? GetNode(int id) => _nodes.TryGetValue(id, out var node) ? node : null;

    /// <summary>设定引擎根节点（引擎 SceneGraph 的渲染入口）</summary>
    public static void SetRoot(VirtualNode root)
        => QueueSink.Instance.Enqueue(ArkTsCommand.SetRoot(root.Id));

    /// <summary>量测影子快照（引擎 onAreaChange 回流填充）</summary>
    public static ArkTsSize GetMeasuredSize(int id)
        => _measures.TryGetValue(id, out var m) ? new ArkTsSize((int)m.W, (int)m.H) : ArkTsSize.Empty;

    /// <summary>
    /// 冲刷指令队列：队列非空且桥已挂接时，把全部指令封送为 JS 对象数组，
    /// 经单次 napi 调用交给引擎 applyCommands。桥未挂接时保留队列（引导期积压语义）。
    /// </summary>
    public static void Flush()
    {
#if HARMONYOS
        if (_bridgeRef == default || _queue.Count == 0) return;

        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_reference_value(env, _bridgeRef, out var bridge).ThrowIfFailed();
        NativeNodeApi.napi_get_named_property(env, bridge, "applyCommands"u8.ToArray(), out var fn).ThrowIfFailed();

        NativeNodeApi.napi_create_array_with_length(env, _queue.Count, out var arr).ThrowIfFailed();
        int count = _queue.Count;
        for (int i = 0; i < count; i++)
            NativeNodeApi.napi_set_element(env, arr, (uint)i, MarshalCommand(env, _queue[i])).ThrowIfFailed();

        NativeNodeApi.napi_call_function(env, bridge, fn, 1, new IntPtr[] { arr }, out _).ThrowIfFailed();
        HiLog.Info(Tag, $"flushed {count} commands");
        _queue.Clear();
#endif
    }

#if HARMONYOS
    private static NativeNodeApi.napi_value MarshalCommand(NativeNodeApi.napi_env env, ArkTsCommand cmd)
    {
        NativeNodeApi.napi_create_object(env, out var obj).ThrowIfFailed();
        NativeNodeApi.napi_set_named_property(env, obj, "op"u8, NativeValue.From(OpName(cmd.Op))).ThrowIfFailed();
        NativeNodeApi.napi_set_named_property(env, obj, "id"u8, NativeValue.From(cmd.NodeId)).ThrowIfFailed();

        switch (cmd.Op)
        {
            case ArkTsCommandOp.Create:
                NativeNodeApi.napi_set_named_property(env, obj, "type"u8, NativeValue.From(cmd.NodeType)).ThrowIfFailed();
                break;

            case ArkTsCommandOp.SetAttrs:
                // 属性对以 [ [name, value], ... ] 数组下发（ArkTS 严格类型下避免动态对象属性遍历）
                NativeNodeApi.napi_create_array_with_length(env, cmd.Attrs!.Count, out var attrArr).ThrowIfFailed();
                for (int i = 0; i < cmd.Attrs.Count; i++)
                {
                    NativeNodeApi.napi_create_array_with_length(env, 2, out var pair).ThrowIfFailed();
                    NativeNodeApi.napi_set_element(env, pair, 0, NativeValue.From(cmd.Attrs[i].Key)).ThrowIfFailed();
                    NativeNodeApi.napi_set_element(env, pair, 1, NativeValue.From(cmd.Attrs[i].Value)).ThrowIfFailed();
                    NativeNodeApi.napi_set_element(env, attrArr, (uint)i, pair).ThrowIfFailed();
                }
                NativeNodeApi.napi_set_named_property(env, obj, "attrs"u8, attrArr).ThrowIfFailed();
                break;

            case ArkTsCommandOp.SetChildren:
                NativeNodeApi.napi_create_array_with_length(env, cmd.Children!.Count, out var children).ThrowIfFailed();
                for (int i = 0; i < cmd.Children.Count; i++)
                    NativeNodeApi.napi_set_element(env, children, (uint)i, NativeValue.From(cmd.Children[i])).ThrowIfFailed();
                NativeNodeApi.napi_set_named_property(env, obj, "children"u8, children).ThrowIfFailed();
                break;

            case ArkTsCommandOp.SetEvents:
                NativeNodeApi.napi_create_array_with_length(env, cmd.Events!.Count, out var events).ThrowIfFailed();
                for (int i = 0; i < cmd.Events.Count; i++)
                    NativeNodeApi.napi_set_element(env, events, (uint)i, NativeValue.From(cmd.Events[i])).ThrowIfFailed();
                NativeNodeApi.napi_set_named_property(env, obj, "events"u8, events).ThrowIfFailed();
                break;
        }
        return obj;
    }

    private static string OpName(ArkTsCommandOp op) => op switch
    {
        ArkTsCommandOp.Create => "create",
        ArkTsCommandOp.SetAttrs => "setAttrs",
        ArkTsCommandOp.SetChildren => "setChildren",
        ArkTsCommandOp.SetEvents => "setEvents",
        ArkTsCommandOp.Delete => "delete",
        ArkTsCommandOp.SetRoot => "setRoot",
        _ => throw new ArgumentOutOfRangeException(nameof(op)),
    };
#endif

    /// <summary>
    /// 引擎桥挂接入口（应用的导出转发层从 C shim initEngine 转发到这里；
    /// 符号 HarmonyEngineInit，仿 HarmonyBuildUI 的 dlsym 模式）。
    /// 同步完成：存桥引用 → init(onEvent) → 冲刷积压指令 → ArkTS 渲染首帧。
    /// </summary>
    public static int InitializeCore(IntPtr env, IntPtr bridgeValue)
    {
        try
        {
            NapiEnv.Initialize(env);
#if HARMONYOS
            NativeNodeApi.napi_create_reference(env, bridgeValue, 1, out _bridgeRef).ThrowIfFailed();

            // 把回流回调交给引擎（GCHandle 由进程生命周期持有）
            var (jsFunc, handle) = NodeApi.CreateCallbackFunction((Action<IntPtr[]>)OnEventFromJs);
            _onEventHandle = handle;
            NodeApi.CallMethodVoid(bridgeValue, "init", new object?[] { jsFunc });

            int pending = _queue.Count;
            Flush();
            HiLog.Info(Tag, $"bridge attached, dispatched {pending} pending commands");
#endif
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error(Tag, $"InitializeCore: {ex.GetType().Name}: {ex.Message}");
            return -1;
        }
    }

    /// <summary>引擎回流回调（ArgsTrampoline 适配：args = [id, kind, payload]）</summary>
    private static void OnEventFromJs(IntPtr[] args)
    {
        try
        {
            if (args.Length < 2 || args[0] == IntPtr.Zero) return;
            int id = NativeValue.ToInt(args[0]);
            string kind = NativeValue.ToString(args[1]) ?? "";

            double w = 0, h = 0, density = 0;
            if (kind == "area" && args.Length > 2 && args[2] != IntPtr.Zero)
            {
                w = ReadPayloadDouble(args[2], "width");
                h = ReadPayloadDouble(args[2], "height");
                density = ReadPayloadDouble(args[2], "density");
                _measures[id] = (w, h, density);
                HiLog.Debug(Tag, $"measure id={id}: {w:0}x{h:0}px @{density:0.00}");
            }
            GetNode(id)?.DispatchEvent(new ArkTsEventArgs { Kind = kind, WidthPx = w, HeightPx = h, Density = density });
        }
        catch (Exception ex)
        {
            HiLog.Error(Tag, $"OnEvent: {ex.GetType().Name}: {ex.Message}");
        }
    }

#if HARMONYOS
    private static double ReadPayloadDouble(IntPtr payload, string name)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_named_property(env, payload, System.Text.Encoding.UTF8.GetBytes(name), out var value).ThrowIfFailed();
        NativeNodeApi.napi_typeof(env, value, out var type).ThrowIfFailed();
        return type == NativeNodeApi.napi_valuetype.napi_number ? NativeValue.ToDouble(value) : 0;
    }
#endif

    /// <summary>指令出口默认实现：入队 + 显式 Flush（实验版批次策略；布局 pass 钩子属 P2 完整版）</summary>
    private sealed class QueueSink : IArkTsCommandSink
    {
        internal static readonly QueueSink Instance = new();
        public void Enqueue(ArkTsCommand command) => _queue.Add(command);
    }
}
