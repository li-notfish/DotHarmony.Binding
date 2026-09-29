using System.Diagnostics;
using HarmonyOS.Interop;

namespace PerfApp;

public static class PerfClock
{
    private static readonly Stopwatch Stopwatch = new();
    private static bool _uiReady;

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
        Stopwatch.Stop();
        HiLog.Info("Perf", $"PERF_RUNTIME_INIT_MS={Stopwatch.ElapsedMilliseconds}");
    }
}
