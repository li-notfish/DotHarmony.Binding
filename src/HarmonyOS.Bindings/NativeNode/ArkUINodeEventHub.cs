#nullable enable
using System;
using System.Threading;
using HarmonyOS.Interop;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>
/// 同一节点同一事件类型的托管订阅聚合器。
/// ArkUI 原生层对同一事件只注册一个 receiver；本类在其上提供多播语义。
/// </summary>
internal sealed class ArkUINodeEventHub
{
    // 写时复制数组：Invoke 只读一个字段引用（原子），分发路径免锁免分配；
    // NODE_TOUCH_EVENT 等热路径不应为每次分发付出 ToArray 拷贝
    private Action<ArkUINodeEvent>[] _handlers = Array.Empty<Action<ArkUINodeEvent>>();
    private readonly Lock _gate = new();

    public bool IsEmpty
    {
        get
        {
            lock (_gate)
                return _handlers.Length == 0;
        }
    }

    public void Add(Action<ArkUINodeEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        lock (_gate)
        {
            var old = _handlers;
            var next = new Action<ArkUINodeEvent>[old.Length + 1];
            Array.Copy(old, next, old.Length);
            next[old.Length] = handler;
            _handlers = next;
        }
    }

    public bool Remove(Action<ArkUINodeEvent> handler)
    {
        if (handler is null)
            return false;

        lock (_gate)
        {
            var old = _handlers;
            var index = Array.IndexOf(old, handler);
            if (index < 0)
                return false;
            var next = new Action<ArkUINodeEvent>[old.Length - 1];
            Array.Copy(old, 0, next, 0, index);
            Array.Copy(old, index + 1, next, index, old.Length - index - 1);
            _handlers = next;
            return true;
        }
    }

    public void Clear()
    {
        lock (_gate)
            _handlers = Array.Empty<Action<ArkUINodeEvent>>();
    }

    public void Invoke(ArkUINodeEvent @event)
    {
        var handlers = _handlers;

        foreach (var handler in handlers)
        {
            // 单订阅者异常不得中断其余订阅者（多播语义）；吞掉记日志，不穿透原生帧
            try
            {
                handler(@event);
            }
            catch (Exception ex)
            {
                HiLog.Error("HarmonyHost", $"[EventHub] handler error: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
