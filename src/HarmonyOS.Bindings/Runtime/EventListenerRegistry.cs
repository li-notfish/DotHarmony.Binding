using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// .NET 事件/回调订阅登记表：handler → (GCHandle, napi_ref) 配对管理。
/// add 时创建 JS 回调函数并调用 on；remove 时用同一 JS 函数调用 off（JS off 按函数实例匹配），
/// 然后释放 GCHandle 与 napi 强引用。线程安全。
/// </summary>
public sealed class EventListenerRegistry
{
    private readonly object _lock = new();
    private readonly Dictionary<object, ListenerEntry> _listeners = new();

    private readonly struct ListenerEntry
    {
        public ListenerEntry(GCHandle gch, NapiReference jsFuncRef)
        {
            Gch = gch;
            JsFuncRef = jsFuncRef;
        }
        public GCHandle Gch { get; }
        public NapiReference JsFuncRef { get; }
    }

    /// <param name="key">订阅键（如 (type, handler) 元组或 event 访问器的 handler 本身）</param>
    /// <param name="adapted">参数适配闭包：IntPtr[] → 逐参转换 → 调用用户委托</param>
    /// <param name="on">把 JS 回调函数接入目标（调用 on/once）</param>
    public void Add(object key, Action<IntPtr[]> adapted, Action<IntPtr> on)
    {
        lock (_lock)
        {
            if (_listeners.ContainsKey(key))
                return; // 幂等：重复订阅同一 handler 忽略
            var (jsFunc, gch) = NodeApi.CreateCallbackFunction(adapted);
            on(jsFunc);
            _listeners[key] = new ListenerEntry(gch, new NapiReference(jsFunc));
        }
    }

    /// <summary>解除订阅并释放资源；未找到订阅时返回 false。</summary>
    public bool Remove(object key, Action<IntPtr> off)
    {
        lock (_lock)
        {
            if (!_listeners.Remove(key, out var entry))
                return false;
            off(entry.JsFuncRef.Value);
            NodeApi.FreeEventHandle(entry.Gch);
            entry.JsFuncRef.Dispose();
            return true;
        }
    }
}
