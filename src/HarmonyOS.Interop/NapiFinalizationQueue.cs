#if HARMONYOS
using System.Collections.Concurrent;

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
    private static readonly ConcurrentQueue<NativeNodeApi.napi_ref> Pending = new();

    /// <summary>入队待回收引用（终结器安全：仅触及托管数据结构）。</summary>
    internal static void Enqueue(NativeNodeApi.napi_ref reference) => Pending.Enqueue(reference);

    /// <summary>在 JS 线程批量回收（调用方必须处于已注入 env 的线程）。</summary>
    internal static void Drain()
    {
        while (Pending.TryDequeue(out var reference))
            NativeNodeApi.napi_delete_reference(NapiEnv.Current, reference);
    }
}
#endif
