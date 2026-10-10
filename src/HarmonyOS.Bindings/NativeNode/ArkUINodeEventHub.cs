#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>
/// 同一节点同一事件类型的托管订阅聚合器。
/// ArkUI 原生层对同一事件只注册一个 receiver；本类在其上提供多播语义。
/// </summary>
internal sealed class ArkUINodeEventHub
{
    private readonly List<Action<ArkUINodeEvent>> _handlers = new();
    private readonly Lock _gate = new();

    public bool IsEmpty
    {
        get
        {
            lock (_gate)
                return _handlers.Count == 0;
        }
    }

    public void Add(Action<ArkUINodeEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        lock (_gate)
            _handlers.Add(handler);
    }

    public bool Remove(Action<ArkUINodeEvent> handler)
    {
        if (handler is null)
            return false;

        lock (_gate)
            return _handlers.Remove(handler);
    }

    public void Clear()
    {
        lock (_gate)
            _handlers.Clear();
    }

    public void Invoke(ArkUINodeEvent @event)
    {
        Action<ArkUINodeEvent>[] snapshot;
        lock (_gate)
            snapshot = _handlers.ToArray();

        foreach (var handler in snapshot)
            handler(@event);
    }
}
