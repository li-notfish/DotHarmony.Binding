#if HARMONYOS
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;

namespace HarmonyOS.Interop;

/// <summary>
/// napi_ref 的延迟释放队列。
/// napi_delete_reference 只能在持有 napi_env 的 JS 线程执行（env 为 ThreadStatic，见 <see cref="NapiEnv"/>）；
/// 终结器线程或其他线程到达 <see cref="NapiReference.Dispose()"/> 时不得触碰原生，
/// 改为入队，由 JS 线程上的热点（节点事件分发等）批量回收。
/// 队列只在事件洪流中截流：JS 线程每次事件分发 Drain 一次，空队列零成本。
/// </summary>
internal static class NapiFinalizationQueue
{
    private readonly record struct PendingReference(IntPtr Env, IntPtr Reference);

    private static readonly ConcurrentQueue<PendingReference> Pending = new();

    /// <summary>入队待回收引用（终结器安全：仅触及托管数据结构）。</summary>
    internal static void Enqueue(IntPtr env, IntPtr reference) => Pending.Enqueue(new(env, reference));

    /// <summary>在 JS 线程批量回收（调用方必须处于已注入 env 的线程）。</summary>
    internal static void Drain()
    {
        try
        {
            var currentEnv = NapiEnv.Current;

            // 只取本 env 的条目：非本 env 的原样留在队列里，等拥有它的线程来 drain。
            // 原实现把其它 env 的条目取出再重新入队，等于每轮 drain 都把整个队列搬运一遍，
            // 双 env 交替 drain 时会互相把对方的条目翻来翻去（活锁式重排，永不前进）。
            // ConcurrentQueue 没有"条件出队"，用本地暂存 + 计数上限保证单轮有界。
            List<PendingReference>? foreign = null;
            var visited = 0;
            var total = Pending.Count;

            while (visited < total && Pending.TryDequeue(out var pending))
            {
                visited++;

                if (pending.Env != currentEnv)
                {
                    (foreign ??= new List<PendingReference>()).Add(pending);
                    continue;
                }

                try
                {
                    NativeNodeApi.napi_delete_reference(currentEnv, pending.Reference).ThrowIfFailed();
                }
                catch (Exception ex)
                {
                    HiLog.Error("NapiFinalizationQueue",
                        $"delete failed, retained: {ex.GetType().Name}: {ex.Message}");
                    (foreign ??= new List<PendingReference>()).Add(pending);
                }
            }

            if (foreign is not null)
            {
                foreach (var item in foreign)
                    Pending.Enqueue(item);
            }
        }
        catch (Exception ex)
        {
            HiLog.Error("NapiFinalizationQueue", $"drain failed: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
#endif
