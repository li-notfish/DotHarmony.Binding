# Performance baseline

This baseline records three metrics for the NativeAOT HarmonyOS host:

- `libapp.so` size for `arm64-v8a` and `x86_64`
- HAP package size
- Cold start time reported by `aa start -W`
- .NET runtime initialization time from `HarmonyInit` to the first page appearing
- First-frame time from `HarmonyInit` to the first page's initial layout (`OnSizeAllocated`)
- TSFN throughput, measured as queued background-thread calls processed on the JS thread

## Collect

Run from the repository root:

```powershell
./scripts/perf/collect-baseline.ps1
```

Useful options:

```powershell
./scripts/perf/collect-baseline.ps1 -Runs 5
./scripts/perf/collect-baseline.ps1 -Target <hdc-target>
./scripts/perf/collect-baseline.ps1 -OutputPath scripts/perf/baseline.json
```

The script:

1. Builds `samples/dotnet/PerfApp` with `HarmonyBuildLibApp`.
2. Builds and installs the resulting HAP.
3. Launches the app and waits for `PERF_DONE` in `hilog`.
4. Records the run data in `scripts/perf/baseline.json`.

## Output

`scripts/perf/baseline.json` contains:

- The Git commit and UTC collection time
- The target device or emulator
- Arm64 and x64 `libapp.so` sizes
- HAP size
- Per-run cold start, runtime initialization, first-frame, and TSFN throughput values
- Averaged cold start, runtime initialization, first-frame, and TSFN throughput values

The baseline is informational only; it does not currently fail a build or emit alerts.
