#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace HarmonyOS.Interop;

/// <summary>
/// JS 回调 data 槽的托管对象登记表：把"回调携带的 IntPtr"从 GCHandle 指针换成
/// 单调递增的 token。
///
/// 为什么不用 GCHandle：
/// 取消路径必须能立刻释放句柄（否则 JS 永不 settle 时永久泄漏），但释放之后
/// 迟到的 settle 仍会带着旧指针回来。GCHandle.FromIntPtr(已释放指针) 是未定义行为——
/// 槽位可能已被别的 GCHandle.Alloc 复用，于是迟到回调会读到**另一个对象**（ABA），
/// 轻则静默错乱、重则跨桥操作别人的 Task。
///
/// token 方案：token 单调递增、永不复用；解析不到即"该桥已结束"，直接忽略。
/// 代价是一次字典查找（只在每次 settle 时发生，不在热循环里）。
/// </summary>
internal static class BridgeSlotRegistry
{
    private static readonly ConcurrentDictionary<long, object> Slots = new();
    private static long _next;

    /// <summary>登记一个桥状态，返回其 token（永不为 0）。</summary>
    internal static IntPtr Register(object state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var token = Interlocked.Increment(ref _next);
        Slots[token] = state;
        // IntPtr 在 32 位平台上只有 32 位；本仓库只出 arm64/x64，token 不会溢出。
        return (IntPtr)token;
    }

    /// <summary>按 token 取回状态；桥已释放或 token 非法时返回 null。</summary>
    internal static object? Resolve(IntPtr token)
    {
        var raw = (long)token;
        if (raw <= 0)
            return null;
        return Slots.TryGetValue(raw, out var state) ? state : null;
    }

    /// <summary>注销并返回状态；幂等，重复调用返回 null。</summary>
    internal static object? Unregister(IntPtr token)
    {
        var raw = (long)token;
        if (raw <= 0)
            return null;
        return Slots.TryRemove(raw, out var state) ? state : null;
    }

    /// <summary>当前登记数（诊断/断言用）。</summary>
    internal static int Count => Slots.Count;
}
