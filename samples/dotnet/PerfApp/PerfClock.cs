using System.Diagnostics;
using HarmonyOS.Interop;

namespace PerfApp;

public static class PerfClock
{
    private static readonly Stopwatch Stopwatch = new();
    private static volatile bool _uiReady;
    private static volatile bool _firstFrame;

    public static void Start()
    {
        if (!Stopwatch.IsRunning)
            Stopwatch.Restart();
    }

    public static void MarkUiReady()
    {
        if (_uiReady)
            return;

        _uiReady = true;
        HiLog.Info("Perf", $"PERF_RUNTIME_INIT_MS={Stopwatch.ElapsedMilliseconds}");
    }

    /// <summary>首页完成首次布局（首帧近似点）。</summary>
    public static void MarkFirstFrame()
    {
        if (_firstFrame)
            return;

        _firstFrame = true;
        Stopwatch.Stop();
        HiLog.Info("Perf", $"PERF_FIRST_FRAME_MS={Stopwatch.ElapsedMilliseconds}");
    }
}
