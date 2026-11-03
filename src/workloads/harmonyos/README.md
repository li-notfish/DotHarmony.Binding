# microsoft.net.sdk.harmonyos workload

Phase 1（`docs/WORKLOAD-PLAN.md`）的 workload 骨架：平台识别、资产导入与 arm64
后处理注册，不引入 MAUI 的 HarmonyOS 平台 DLL。

## 布局

| 路径 | 作用 |
|---|---|
| `WorkloadManifest.in.json` | manifest 模板；`eng/Workloads/build-workload.ps1` 替换版本占位符后产出 `WorkloadManifest.json` |
| `WorkloadManifest.targets` | workload 安装后在 targets 阶段导入：平台注册 + Known* 条目 + SDK pack 挂载 |
| `Microsoft.NET.Sdk.HarmonyOS/Sdk/*` | SDK pack 内容（kind=Sdk）：`AutoImport.props` props 阶段自动导入，`Sdk.props` / `Sdk.targets` 经 `Import Sdk=` 接入 |
| `eng/Workloads/build-workload.ps1` | 版本戳定 + 产出 workload 安装布局（manifests / packs / nupkgs） |

## workload 设计

- `harmonyos`（必装集）：只含 `Microsoft.NET.Sdk.HarmonyOS` SDK pack。
  不声明依赖 `maui` / `maui-mobile`（P1-1 不做什么）。
- `harmonyos-nativeaot`（可选集）：`Microsoft.HarmonyOS.App.Ref`（托管 ref 程序集 +
  RuntimeIdentifierGraph）、`Microsoft.HarmonyOS.App.Runtime.NativeAOT.linux-musl-arm64`
  / `.linux-musl-x64`（实现程序集）、`Microsoft.HarmonyOS.ILCompiler`（交叉链，经
  `alias-to` 指向官方 `runtime.<hostRid>.Microsoft.DotNet.ILCompiler` pack 文件夹，
  复用官方 ilc 内容，不 fork）。作为可选 workload，`dotnet workload install harmonyos`
  不会要求这些 pack；NativeAOT 发布链当前仍由 SDK 默认 `KnownILCompilerPack` +
  `PublishAotClang` 解析（P1-5 验收的 "ILCompiler pack 解析成功" 即此路径）。

## 安装（隔离 SDK，不改全局）

```powershell
pwsh -NoProfile -File eng/Workloads/build-workload.ps1
pwsh -NoProfile -File scripts/verify-workload.ps1
```

`verify-workload.ps1` 会准备隔离 SDK、放置 manifest（`sdk-manifests\<band>\microsoft.net.sdk.harmonyos\<version>\`）
与 SDK pack（`packs\Microsoft.NET.Sdk.HarmonyOS\<version>\Sdk\`），然后验证
`dotnet workload list` 可见 `harmonyos`、模板工程 restore/build 通过。

## RID 语义

产物 RID 保持 `linux-musl-arm64` / `linux-musl-x64`（PublishAotClang + zig 交叉链）。
HarmonyOS 设备 ABI 映射关系记录在 `Microsoft.HarmonyOS.App.Ref` pack 内的
`RuntimeIdentifierGraph.json`：`harmonyos-arm64 -> linux-musl-arm64`、
`harmonyos-x64 -> linux-musl-x64`（Phase 1 为文档化映射，不参与 SDK 解析）。
