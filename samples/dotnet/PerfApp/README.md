# PerfApp

NativeAOT、TSFN、回调与事件总线的压力/性能样例（`InteropStress` 等探针），
配合 `scripts/perf/collect-baseline.ps1` 与 `docs/PERF_BASELINE.md` 记录基线。

## 运行

```powershell
dotnet build samples/dotnet/PerfApp -t:HarmonyRun
```

## 说明

- 这是性能/稳定性探针，不是功能教学页。
- 基线采集：`pwsh scripts/perf/collect-baseline.ps1 -Project samples/dotnet/PerfApp`；
  指标定义见 [docs/PERF_BASELINE.md](../../../docs/PERF_BASELINE.md)。
