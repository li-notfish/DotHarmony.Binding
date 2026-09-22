#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace HarmonyOS.Bindings.NativeNode;

/// <summary>
/// UI 主线程守卫。
/// ArkUI 原生节点 API 必须在主线程调用（官方约束）；
/// 宿主入口（napi init / C shim）即主线程，此处记录并断言。
/// </summary>
internal static class NativeMainThread
{
    private static long _mainThreadId;

    /// <summary>由宿主入口调用，将当前线程登记为 UI 主线程</summary>
    internal static void Capture()
    {
        // 跨线程发布的守卫字段：用 Interlocked 保证其他线程读到一致值
        Interlocked.Exchange(ref _mainThreadId, CurrentManagedThreadId64());
    }

    /// <summary>断言当前线程为 UI 主线程</summary>
    internal static void Ensure()
    {
        var main = Interlocked.Read(ref _mainThreadId);
        if (main == 0)
        {
            // 入口尚未登记（例如单元测试环境）——宽松放行，方便离线测试
            return;
        }
        if (main != CurrentManagedThreadId64())
        {
            throw new InvalidOperationException(
                "ArkUI native node APIs must be called on the main thread. " +
                "Use a ThreadSafeFunction or marshal to the UI thread first.");
        }
    }

    private static long CurrentManagedThreadId64()
        => Environment.CurrentManagedThreadId;
}
