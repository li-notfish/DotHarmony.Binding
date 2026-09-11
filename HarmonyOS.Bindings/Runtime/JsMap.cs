using System;
using System.Collections.Generic;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// JS Map 的 C# 活视图包装。方法经句柄调用底层 map.get/map.set/map.has/map.delete/map.entries，
/// 读取/写入直接作用于 JS 侧 Map 对象（不是快照）。TKey/TValue 支持
/// 基元类型、string、IntPtr、JsBigInt、byte[] 及生成的 JsObject 派生包装类。
/// </summary>
public class JsMap<TKey, TValue> : JsObject where TKey : notnull
{
    private readonly Func<IntPtr, TValue>? _valueFactory;

    /// <summary>包装既有 JS Map；valueFactory 供包装类值类型使用（如 new Geofence(h)），null 时按基元转换。</summary>
    public JsMap(IntPtr handle, Func<IntPtr, TValue>? valueFactory = null) : base(handle)
    {
        _valueFactory = valueFactory;
    }

    /// <summary>创建新的空 JS Map（经 globalThis.Map 构造）。</summary>
    public static JsMap<TKey, TValue> Create(Func<IntPtr, TValue>? valueFactory = null)
    {
#if HARMONYOS
        var global = NodeApi.GetGlobal();
        var handle = NodeApi.CreateInstance(global, "Map"u8);
        return new JsMap<TKey, TValue>(handle, valueFactory);
#else
        throw new PlatformNotSupportedException("JsMap requires HarmonyOS runtime");
#endif
    }

    /// <summary>map.size</summary>
    public int Count
    {
        get
        {
#if HARMONYOS
            return (int)NativeValue.ToDouble(GetPropertyRaw(_size));
#else
            throw new PlatformNotSupportedException("JsMap requires HarmonyOS runtime");
#endif
        }
    }

    /// <summary>map.has(key)</summary>
    public bool ContainsKey(TKey key)
    {
#if HARMONYOS
        return CallMethod<bool>(_has, NativeValue.From(key!));
#else
        throw new PlatformNotSupportedException("JsMap requires HarmonyOS runtime");
#endif
    }

    /// <summary>map.get(key)。键是否存在请先用 <see cref="ContainsKey"/> 判断（undefined 会转换为 default）。</summary>
    public TValue? Get(TKey key)
    {
#if HARMONYOS
        return ConvertValueBack(NodeApi.CallMethod<IntPtr>(Handle, _get, NativeValue.From(key!)));
#else
        throw new PlatformNotSupportedException("JsMap requires HarmonyOS runtime");
#endif
    }

    /// <summary>map.has(key) + map.get(key) 组合。</summary>
    public bool TryGet(TKey key, out TValue value)
    {
#if HARMONYOS
        if (!ContainsKey(key))
        {
            value = default!;
            return false;
        }
        value = ConvertValueBack(NodeApi.CallMethod<IntPtr>(Handle, _get, NativeValue.From(key!)))!;
        return true;
#else
        throw new PlatformNotSupportedException("JsMap requires HarmonyOS runtime");
#endif
    }

    /// <summary>map.set(key, value)（链式语义返回自身）。</summary>
    public JsMap<TKey, TValue> Set(TKey key, TValue value)
    {
#if HARMONYOS
        NodeApi.CallMethodVoid(Handle, _set, NativeValue.From(key!), NativeValue.From(value!));
        return this;
#else
        throw new PlatformNotSupportedException("JsMap requires HarmonyOS runtime");
#endif
    }

    /// <summary>map.delete(key)</summary>
    public bool Remove(TKey key)
    {
#if HARMONYOS
        return NodeApi.CallMethod<bool>(Handle, _delete, NativeValue.From(key!));
#else
        throw new PlatformNotSupportedException("JsMap requires HarmonyOS runtime");
#endif
    }

    /// <summary>map.clear()</summary>
    public void Clear()
    {
#if HARMONYOS
        NodeApi.CallMethodVoid(Handle, _clear);
#else
        throw new PlatformNotSupportedException("JsMap requires HarmonyOS runtime");
#endif
    }

    /// <summary>经 map.entries() 迭代器协议展开（键值对拷贝；TValue 为包装类时逐个建实例）。</summary>
    public KeyValuePair<TKey, TValue>[] Entries()
    {
#if HARMONYOS
        var iterator = NodeApi.CallMethod<IntPtr>(Handle, _entries);
        var result = new List<KeyValuePair<TKey, TValue>>();
        while (true)
        {
            var step = NodeApi.CallMethod<IntPtr>(iterator, _next);
            if (NativeValue.ToBool(NodeApi.GetProperty(step, _done)))
                break;
            var pair = NodeApi.GetProperty(step, _value);
            var key = NodeApi.GetElement(pair, 0);
            var val = NodeApi.GetElement(pair, 1);
            result.Add(new KeyValuePair<TKey, TValue>(ValueConverter.Convert<TKey>(key)!, ConvertValueBack(val)!));
        }
        return result.ToArray();
#else
        throw new PlatformNotSupportedException("JsMap requires HarmonyOS runtime");
#endif
    }

    /// <summary>Entries() 的字典形态（活读取的拷贝）。</summary>
    public Dictionary<TKey, TValue> ToDictionary()
    {
        var dict = new Dictionary<TKey, TValue>();
        foreach (var kvp in Entries())
            dict[kvp.Key] = kvp.Value;
        return dict;
    }

    private TValue ConvertValueBack(IntPtr value)
        => _valueFactory != null ? _valueFactory(value) : ValueConverter.Convert<TValue>(value)!;

    private static ReadOnlySpan<byte> _size => "size"u8;
    private static ReadOnlySpan<byte> _has => "has"u8;
    private static ReadOnlySpan<byte> _get => "get"u8;
    private static ReadOnlySpan<byte> _set => "set"u8;
    private static ReadOnlySpan<byte> _delete => "delete"u8;
    private static ReadOnlySpan<byte> _clear => "clear"u8;
    private static ReadOnlySpan<byte> _entries => "entries"u8;
    private static ReadOnlySpan<byte> _next => "next"u8;
    private static ReadOnlySpan<byte> _done => "done"u8;
    private static ReadOnlySpan<byte> _value => "value"u8;
}
