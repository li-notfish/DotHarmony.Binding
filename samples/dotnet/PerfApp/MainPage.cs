using System.Diagnostics;
using System.Runtime.InteropServices;
using HarmonyOS.Interop;
using Microsoft.Maui.Controls;

namespace PerfApp;

public partial class MainPage : ContentPage
{
    private const int TsfnIterations = 10_000;
    private readonly Label _status = new() { Text = "running" };
    private bool _started;

    public MainPage()
    {
        Content = new VerticalStackLayout
        {
            Padding = 24,
            Children =
            {
                new Label { Text = "Harmony performance baseline" },
                _status
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_started)
            return;

        _started = true;
        PerfClock.MarkUiReady();
        // 首帧近似：当前 UI 构建批次排空后的第一个 dispatcher 回调
        Dispatcher.Dispatch(PerfClock.MarkFirstFrame);
        _ = RunBaselineAsync();
    }

    private async Task RunBaselineAsync()
    {
        try
        {
            await InteropStress.RunAsync();
            await ResumeOnUiAsync();
            var throughput = await MeasureTsfThroughputAsync(TsfnIterations);
            HiLog.Info("Perf", $"PERF_TSFN_THROUGHPUT_OPS_PER_SEC={throughput:F2}");
            HiLog.Info("Perf", "PERF_DONE");
            _status.Text = $"done: {throughput:F2} ops/s";
        }
        catch (Exception ex)
        {
            HiLog.Error("Perf", $"PERF_FAILED: {ex.GetType().Name}: {ex.Message}");
            _status.Text = "failed";
        }
    }

    private static Task ResumeOnUiAsync()
    {
        var completion = new TaskCompletionSource();
        MainThreadDispatcher.Post(() => completion.TrySetResult());
        return completion.Task;
    }

    private static async Task<double> MeasureTsfThroughputAsync(int iterations)
    {
        using var tsfn = ThreadSafeFunction.Create();
        var completion = new TaskCompletionSource();
        var processed = 0;

        tsfn.OnCallJs = _ =>
        {
            var current = Interlocked.Increment(ref processed);
            if (current == iterations)
                completion.TrySetResult();
        };

        var stopwatch = Stopwatch.StartNew();
        var worker = new Thread(() =>
        {
            for (var i = 0; i < iterations; i++)
                tsfn.Call(IntPtr.Zero);
        })
        {
            IsBackground = true,
            Name = "perf-tsfn"
        };

        worker.Start();
        var finished = await Task.WhenAny(completion.Task, Task.Delay(TimeSpan.FromSeconds(30)));
        stopwatch.Stop();

        if (finished != completion.Task)
            throw new TimeoutException($"TSFN baseline timed out: {processed}/{iterations} calls processed.");

        return iterations / stopwatch.Elapsed.TotalSeconds;
    }
}
