# TFM 与包资产矩阵

## 目标与边界

第一阶段把本仓库自有的**运行时平台包**统一为
`net10.0;net10.0-harmonyos26.0` 双目标，并保证 NuGet 包同时包含桌面与 HarmonyOS
资产。

这不等于要求所有第三方 NuGet 包都提供 `net10.0-harmonyos26.0` 资产。NuGet 仍会按兼容
规则为第三方包选择 `net10.0` 等通用资产；HarmonyOS 的实际平台行为由本仓库的运行时
平台包、Handler 和 Essentials 实现提供。

## 运行时平台包

以下项目属于应用运行时依赖，必须同时提供通用与 HarmonyOS 平台资产：

| 包 | 项目 | 包内目标 |
|---|---|---|
| `HarmonyOS.Interop` | `src/HarmonyOS.Interop/HarmonyOS.Interop.csproj` | `net10.0`、`net10.0-harmonyos26.0` |
| `HarmonyOS.Bindings` | `src/HarmonyOS.Bindings/HarmonyOS.Bindings.csproj` | `net10.0`、`net10.0-harmonyos26.0` |
| `HarmonyOS.Essentials` | `src/HarmonyOS.Essentials/HarmonyOS.Essentials.csproj` | `net10.0`、`net10.0-harmonyos26.0` |
| `HarmonyOS.Maui` | `src/HarmonyOS.Maui/HarmonyOS.Maui.csproj` | `net10.0`、`net10.0-harmonyos26.0` |
| `HarmonyOS.Maui.Web` | `src/HarmonyOS.Maui.Web/HarmonyOS.Maui.Web.csproj` | `net10.0`、`net10.0-harmonyos26.0` |
| `HarmonyOS.Maui.Blazor` | `src/HarmonyOS.Maui.Blazor/HarmonyOS.Maui.Blazor.csproj` | `net10.0`、`net10.0-harmonyos26.0` |

当前 HarmonyOS SDK 为 `26.0.0` / API 26，平台版本为 `26.0`，因此打包后的平台资产目录为
`lib/net10.0-harmonyos26.0/`。

平台注册现在有两条路径：

- **首选：harmonyos workload**（P1-1 ~ P1-3 已落地）。安装 `microsoft.net.sdk.harmonyos`
  workload 后，manifest 的 `WorkloadManifest.targets` 在 targets 求值早期注册
  `TargetPlatformSupported`、`TargetPlatformVersion=26.0` 与
  `SdkSupportedTargetPlatformVersion=26.0`，并由 `AutoImport.props` 置
  `_HarmonyOSWorkloadInstalled=true`。外部消费工程无需内联任何注册片段。
- **仓库内 fallback**：workload 未安装时，根 `Directory.Build.targets:23-30` 在
  `$(_HarmonyOSWorkloadInstalled) != 'true'` 条件下提供同一组注册，仓库内工程
  不装 workload 也能构建。此时外部 NuGet 消费工程首次 restore 前仍须把注册片段
  内联到工程或自己的 `Directory.Build.targets`，因为 NuGet `buildTransitive` 导入
  发生在 SDK 完成 TFM 推断之后。

## 平台无关与工具项目

以下项目不承载 HarmonyOS 平台行为，不应为追求“全平台化”而强行改成平台 TFM：

| 项目 | TFM | 原因 |
|---|---|---|
| `src/HarmonyOS.Maui.Permissions.Abstractions` | `netstandard2.0` | 公共标记与抽象，无平台调用 |
| `src/HarmonyOS.Maui.Permissions` | `netstandard2.0` | Roslyn source generator，以 analyzer 形式使用且不分发运行时包 |
| `src/HarmonyOS.Templates` | `net10.0` | `dotnet new` 内容与工具包 |
| `tools/HandlerAudit` | `net10.0` | 桌面审计工具 |
| `tests/dotnet/*` | `net10.0` | 桌面单元测试与契约测试 |

HarmonyOS 示例应用和模板内容工程使用 `net10.0-harmonyos26.0`。

## 第三方包策略

- `Microsoft.Maui.*`：官方包没有 `net10.0-harmonyos26.0` 资产，HarmonyOS 项目会按 NuGet
  兼容规则选择其 `net10.0` API 资产。平台 Handler 与 Essentials 实现由
  `HarmonyOS.Maui`、`HarmonyOS.Essentials` 提供。
- `Microsoft.AspNetCore.Components.*`：保留 `net10.0` 通用资产；
  `HarmonyOS.Maui.Web` 与 `HarmonyOS.Maui.Blazor` 提供 HarmonyOS WebView/Blazor 平台层。
- `CommunityToolkit.Maui`：保留 `net10.0` 通用资产，仅消费其平台无关 API；当前示例只
  使用 `InvertedBoolConverter`。

## 自动验收

运行：

```powershell
pwsh -NoProfile -File scripts/verify-tfm-matrix.ps1
```

脚本会：

1. 校验运行时平台包、平台无关包和工具项目的 TFM 分类；
2. 打包全部运行时平台包及权限抽象包；
3. 检查每个运行时包同时包含 `lib/net10.0/` 与
   `lib/net10.0-harmonyos26.0/` 资产；
4. 创建隔离的外部消费工程，从本地 feed 恢复六个运行时包，并确认
   `project.assets.json` 的 compile/runtime 资产全部来自
   `lib/net10.0-harmonyos26.0/`。

该验收只涉及 MSBuild、NuGet 打包和资产表检查，不需要上机。workload 安装与模板工程构建
另行由 `scripts/verify-workload.ps1` 在隔离 SDK 中验收；设备验证留到 NativeAOT 发布链、
runtime pack 或 HAP 运行行为发生变化时。

## 下一阶段

workload 骨架（manifest、SDK pack、App.Ref / NativeAOT runtime pack、ILCompiler alias、
模板包）已随 P1-1 ~ P1-7 落地，覆盖 `KnownFrameworkReference` / `KnownILCompilerPack`
注册与 RID 解析。剩余工作：P1-8 把 workload 安装与非设备构建纳入 CI gate；
Phase 2 提供官方 MAUI 的 `net10.0-harmonyos26.0` 平台资产后，需要重新进行设备与
模拟器验证。
