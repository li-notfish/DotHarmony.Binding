# 性能基线

本基线记录 HarmonyOS NativeAOT 宿主的六项指标：

- `libapp.so` 体积（`arm64-v8a` 与 `x86_64`）
- HAP 包体积
- `aa start -W` 报告的冷启动时间
- .NET 运行时初始化时间（`HarmonyInit` → 首页出现）
- 首帧时间（`HarmonyInit` → 首页首次布局 `OnSizeAllocated`）
- TSFN 吞吐（后台线程投递、JS 线程处理的调用数）

## 采集

在仓库根目录执行：

```powershell
./scripts/perf/collect-baseline.ps1
```

常用参数：

```powershell
./scripts/perf/collect-baseline.ps1 -Runs 5
./scripts/perf/collect-baseline.ps1 -Target <hdc-target>
./scripts/perf/collect-baseline.ps1 -OutputPath scripts/perf/baseline.json
```

脚本流程：

1. 以 `HarmonyBuildLibApp` 构建 `samples/dotnet/PerfApp`；
2. 构建并安装产出的 HAP；
3. 启动应用并等待 hilog 中的 `PERF_DONE`；
4. 把本轮数据写入 `scripts/perf/baseline.json`。

多设备矩阵用 `scripts/perf/device-matrix.ps1`（参数同上，输出 `scripts/perf/device-matrix.json`）。

## 输出

`scripts/perf/baseline.json` 包含：

- Git 提交与 UTC 采集时间
- 目标设备或模拟器
- arm64 与 x64 的 `libapp.so` 体积
- HAP 体积
- 每轮的冷启动、运行时初始化、首帧与 TSFN 吞吐
- 上述四项的平均值

基线仅作记录，不阻塞构建，也不发出告警。
