using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PerfMetric 枚举
/// </summary>
public enum PerfMetric
{
    Duration = 0,
    CpuLoad = 1,
    CpuUsage = 2,
    MemoryRss = 3,
    MemoryPss = 4,
    AppStartResponseTime = 5,
    AppStartCompleteTime = 6,
    PageSwitchCompleteTime = 7,
    ListSwipeFps = 8
}