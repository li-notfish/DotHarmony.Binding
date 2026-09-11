// TSFN 最小实验（ROADMAP 2.1 前置验证，M2）。
// 验证主张：.NET 后台线程 → napi_call_threadsafe_function → CallJsTrampoline
// 在 JS（宿主主）线程执行，且回调内 napi_env 可用。
// 实验方法：UI 线程创建 TSFN → 起一个真正的 .NET Thread 睡 300ms → Call() →
// 回调里做一次 NAPI 字符串往返（证明 env 可用）+ 线程号比对（证明同线程），
// 结果经 report 委托回 UI（report 在 JS 线程触发，可直接更新控件）。
// 生命周期：回调末尾 Release()（完成路径）。
using System;
using System.Diagnostics;
using System.Threading;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>TSFN 跨线程封送最小实验（实验代码，验证通过后收编进正式通道）</summary>
public static class TsfnExperiment
{
    /// <summary>
    /// 必须在宿主主线程（JS 线程）调用。<paramref name="report"/> 在 JS 线程触发一次。
    /// </summary>
    public static void Run(Action<string> report)
    {
        var jsThreadId = Environment.CurrentManagedThreadId;
        var stopwatch = Stopwatch.StartNew();
        var workerThreadId = 0L;

        var tsfn = ThreadSafeFunction.Create();
        tsfn.OnCallJs = _ =>
        {
            var callbackThreadId = Environment.CurrentManagedThreadId;

            // 证明回调内 napi_env 可用：走一次真实的 NAPI 字符串创建 + 读取往返
            string roundTrip;
            try
            {
                _ = NapiEnv.Current;
                var value = NativeValue.From("tsfn-ok");
                roundTrip = NativeValue.ToString(value);
            }
            catch (Exception ex)
            {
                roundTrip = $"FAILED: {ex.Message}";
            }

            var elapsed = stopwatch.ElapsedMilliseconds;
            report($"TSFN OK | cb_tid={callbackThreadId} js_tid={jsThreadId} " +
                   $"worker_tid={workerThreadId} same_thread={callbackThreadId == jsThreadId} | " +
                   $"env_roundtrip=\"{roundTrip}\" | latency={elapsed}ms (worker slept 300ms)");

            // 生命周期完成路径：用完即释放
            tsfn.Release();
        };

        var worker = new Thread(() =>
        {
            workerThreadId = Environment.CurrentManagedThreadId;
            Thread.Sleep(300); // 模拟后台工作
            tsfn.Call(IntPtr.Zero);
        })
        {
            IsBackground = true,
            Name = "tsfn-experiment",
        };
        worker.Start();
    }
}
