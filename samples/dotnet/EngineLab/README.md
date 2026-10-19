# EngineLab（实验）

ArkTS 命令总线引擎样例：.NET 侧构建 `VirtualNode` 场景，指令经 TSFN/初始化通道下发到
ArkTS 引擎，事件/量测/文本回流到 .NET。用于验证建树、属性写、事件回流、量测回流、
tick 自动冲刷与增量 diff。

## 运行

```powershell
dotnet build samples/dotnet/EngineLab -t:HarmonyRun
```

## 说明

- 依赖 `HarmonyOS.Bindings.Experimental`，**属于实验层，不是稳定公共 API**；
  不经过 MAUI Handler 层，也不引用 `HarmonyOS.Maui`。
- 本地 `Host/` overlay 会覆盖暂存宿主的 `entry/src/main/**`（`HarmonyStageEngineHost`）。
- 设计背景见 [MIGRATION_ARKTS_ENGINE.md](../../../MIGRATION_ARKTS_ENGINE.md)。
- 生产应用请以 `HelloApp`/`ControlsSampleApp` 的 MAUI 路径为准。
