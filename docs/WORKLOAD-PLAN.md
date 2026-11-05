# MAUI Workload 化路线图

Samsung Tizen 能做到 `netX.0-tizen`，靠的是两层同时成立：三星维护厂商 workload SDK（`Samsung.Tizen.Sdk`
提供 `KnownFrameworkReference` / `KnownRuntimePack` / TPK 打包签名），MAUI 上游在官方源码里加入 Tizen 平台实现，
编进同一份 `Microsoft.Maui.*` 包。HarmonyOS 可以借鉴这个组织方式，但不能照搬 Tizen 的 framework-dependent
运行时模型——HarmonyOS 侧是 NativeAOT 产出 `libapp.so`，再由 ArkTS 宿主加载，运行时分发、交叉编译与 HAP 打包
都属于另一条链。

范围约束：不 clone、不修改 `dotnet/runtime`；arm64 `libapp.so` 的 NUMA 补丁属于二进制后处理，不进入 runtime 源码。
状态（2026-11-04）：Phase 1 的 P1-1 ~ P1-7 已落地并通过 `scripts/verify-workload.ps1` 冷启动验收，P1-8（CI）未做；
落地记录见第五节。第一节“现状”列保留落地前基线，供对照。现状事实见 `docs/TFM-MATRIX.md`。

## 一、现状 / 缺口 / 验收

| 层 | 现状（文件:行） | 缺口 | 验收 |
|---|---|---|---|
| SDK targets 注入 | 库侧用 `HarmonyOS.Maui` 的 `buildTransitive` 下发 targets；示例需显式 `PackageReference` | workload 没有 `microsoft.net.sdk.harmonyos` import 链，app 不引用包就拿不到 Harmony 构建 targets | 空 `PackageReference` 的应用仍可见 `-t:HarmonyBuildLibApp` / `HarmonyBuildHap`；pack 的 `Sdk/Sdk.props`、`Sdk/Sdk.targets`、`AutoImport.props` 被正确导入，`KnownFrameworkReference` / `KnownILCompilerPack` 在 SDK 默认项之后追加 |
| MAUI 运行时资产 | 官方 `Microsoft.Maui.Controls*` 只有 `net10.0` 与 Android/iOS 等平台资产；本仓库用 `HarmonyOS.Maui` handler 层补平台行为（`docs/TFM-MATRIX.md:49-57`） | 没有 `lib/net10.0-harmonyos26.0/` 的 `Microsoft.Maui.Controls.Core` / `Core` / `Essentials` / `Graphics` / `BlazorWebView` | `project.assets.json` 的 compile/runtime 明确选中 `lib/net10.0-harmonyos26.0/`，不静默回落到 `net10.0`；应用不引用外部平台包也能编译 |
| NativeAOT 交叉链 | 根 props `:28` 全仓引用 `PublishAotClang`；`HarmonyOS.Maui.App.targets:152` `HarmonyBuildLibApp` 在 Windows 上交叉发布 `linux-musl-arm64` / `x64` | 没有 workload 的 `KnownILCompilerPack`、RID 图、sysroot / linker 参数组；arm64 NUMA 补丁只靠 `Exec` 调 PowerShell，无增量与产物复查 | 交叉 pack 的 archives 可全部解析并参与链接；arm64 产物 NUMA 签名 0 次、补丁签名 1 次；`x64` 不受影响；无设备环境可完整跑完 |
| `libapp.so` 后处理 | `HarmonyOS.Maui.App.targets:165` 在 arm64 publish 后调 `scripts/patch-openharmony-nativeaot.ps1`（`-CheckOnly` 可验签） | 只靠 PowerShell `Exec`；重复扫描；不校验 HAP staging 后的同一份 so；失败时可能留下陈旧 `libs/arm64-v8a/libapp.so` | MSBuild task + 跨平台脚本；stamp / `Inputs` `Outputs` 跳过重复；patch 后 `-CheckOnly`，复制后对宿主产物再 `-CheckOnly` 一次；构建失败不残留旧 so |
| RID 图 | 两份产物都是 `linux-musl-arm64` / `linux-musl-x64`，与 HarmonyOS 设备/ABI 概念不一一对应 | 平台 RID 语义、`net10.0-harmonyos26.0` 下的默认 RID 选择、Android 等相邻平台外溢 | 文档化的 RID 映射表；显式指定与省略 `RuntimeIdentifier` 得到同一组产物；不会误拉 Android / 鸿蒙之外的 pack |
| HAP 打包 | `HarmonyOS.Maui.App.targets:172-175` 拷到宿主 `libs/arm64-v8a`、`libs/x86_64`；`:180` `HarmonyBuildHap`；`:242` `HarmonyRun` | 没有 workload 内的正式 pack；签名、失败持久化、清理策略分散在脚本 | 已签名 HAP 校验成功；app 内 `libapp.so` 与 publish 产物逐字节一致且已 patch；`dotnet publish` 直接出 HAP；`HarmonyRun` 设备安装并启动 |
| 项目模板 | `HarmonyApp1.csproj:37-49`、`HarmonyWebApp1.csproj:37-49` 内联平台注册片段与包引用 | 模板依赖工作区 `Directory.Build.targets` 和显式 `PackageReference`，外部用户复制后不可用 | `dotnet new harmony-maui` 默认工程不含内联片段，`restore` / `build` 通过；可完全移出仓库目录构建 |
| 构建链自举 | `HarmonyOS.Maui.App.targets:159-162` 在 build target 内嵌套 `dotnet publish` 并 `MSBuildEnableWorkloadResolver=false` | build 目标与 workload 解析互相绕开，无法证明“普通 `dotnet build` 就工作” | 最终链路由普通 MSBuild 图驱动一次发布（或正式拆成两段文档化命令）；无 `MSBuildEnableWorkloadResolver=false` 类绕过；锁定清单 `RestoreLockedMode` 可复现 |
| 第三方包 | `CommunityToolkit.Maui`、`Microsoft.AspNetCore.Components.*` 只能选 `net10.0` 资产（`docs/TFM-MATRIX.md:49-57`） | 第三方 Controls / Essentials 适配没有统一判定 | 分阶段判定表：可兼容（纯 `net10.0`、走 handler 扩展）、需上游平台资产、明确不支持 |

必须同时成立的两个前置条件，缺一个都只会得到“能 restore 但跑不起来”：

- `HARMONYOS` 不能继续在桌面 `net10.0` 全量定义（`ROADMAP.md:286-287`）；改为按 TFM 生成/条件定义，桌面桩体化。
- NativeAOT 的 arm64 结果不只受指令集影响：libc/syscall 探测、GC、TLS、平台探测都在同一份 `app.so` 里，必须做成
  可验证的最小切片 + 测试矩阵，而不是“编译过了就算”。

## 二、Phase 1：真 workload（不 fork runtime）

目标：**只实现平台识别、资产导入与 arm64 后处理，不引入 MAUI 的 HarmonyOS 平台 DLL。** 官方 MAUI 包仍选
`net10.0` 资产，应用的成功标准是“workload 装好即可构建 HAP”，MAUI 平台实现留到 Phase 2。

| 优先级 | 文件 / 生成物 | 改动 | 不做什么 | 验收 | 需上机 |
|---|---|---|---|---|---|
| P1-1 | `src/workloads/harmonyos/WorkloadManifest.in.json`、`Microsoft.NET.Sdk.HarmonyOS/Sdk/*`、`eng/Workloads/` | 建立 `microsoft.net.sdk.harmonyos` manifest 与 SDK pack 骨架，安装 band 对齐所在 SDK 的 feature band（如 10.0.403 → 10.0.400，按 SDK 版本推导） | 不声明依赖 `maui` / `maui-mobile`，避免安装时顺带重装官方 MAUI | `WorkloadManifest.json` 进入 `sdk-manifests\<SDK feature band>\microsoft.net.sdk.harmonyos\`；`dotnet workload list` 可见 | 否 |
| P1-2 | `Microsoft.NET.Sdk.HarmonyOS/Sdk/Sdk.props`、`Sdk.targets`、`AutoImport.props` | `WorkloadManifest.targets` 用 `Import Sdk=` 接 `AutoImport.props`；`KnownFrameworkReference` / `KnownILCompilerPack` 只在 `.targets` 追加，晚于 SDK 默认项 | 不在 props 里注册 framework reference（会被默认项覆盖或错过时机） | `MSBuildDebug` 属性求值可看到追加后的 item；workload 卸载后这些 item 消失 | 否 |
| P1-3 | 同上 + `Directory.Build.targets:18-27` + `HarmonyApp1.csproj:37-49`、`HarmonyWebApp1.csproj:37-49` | 把平台注册片段从仓库 targets 与两份模板内联中迁入 workload；模板删除片段并去掉平台包显式引用（保留其它必需引用） | 不删除 `PublishAotClang` 等与 workload 判定无关的现有引用 | `dotnet new harmony-maui` 在仓库外 `restore` / `build` 通过；`TargetFramework`、`TargetPlatformVersion=26.0`、`HARMONYOS` 常量符合预期 | 否 |
| P1-4 | HarmonyOS-owned packs（建议 `Microsoft.HarmonyOS.App.Ref`、`Microsoft.HarmonyOS.App.Runtime.NativeAOT.<RID>`、`Microsoft.HarmonyOS.ILCompiler`） | 托管 ref 程序集、`libapp.so` 拼装所需 native archives / 头文件、`RuntimeIdentifierGraph`；先用 OSC 自有 pack 名，不抢 `Microsoft.NETCore.App.*` | 不把 Tizen 式“设备自带运行时” runtime pack 当模板；不在 Phase 1 伪造 MAUI 平台 DLL | `RuntimeIdentifierGraph` 包含目标 RID；cross pack 的 archives 与头文件均被引用；`pack` 产物可被独立工程 restore | 否 |
| P1-5 | `KnownILCompilerPack` / ILCompiler cross pack 条目 | 把 `linux-musl-arm64` / `x64` 交叉链注册为 ILCompiler 可选包；链接参数组内保留 `--start-group` / `--end-group` | 不声称可在 NativeAOT link 阶段注入 `app.so`；当前 `zig objcopy` 不支持 ELF→object，把 `app.so` 当输入只会得到 1744/1824-byte stub | ILCompiler pack 解析成功；`PublishAotClang` + workload 组合可发布 | 否 |
| P1-6 | `scripts/patch-openharmony-nativeaot.ps1` + 新增 MSBuild task | 抽成 task + 跨平台脚本；按 `linux-musl-arm64` 精确匹配；stamp + `Inputs` / `Outputs`；patch 后立刻 `-CheckOnly`；HAP staging 后再 `-CheckOnly` 一次；不生成 `.bak`；失败清理陈旧 `libs/arm64-v8a/libapp.so` | 不改 runtime 源码；不对 x64 打补丁；不在 patch 失败时继续 `HarmonyBuildHap` | 原始 so 签名命中 0 次；patch 后命中 1 次；重复构建不重复扫描；删除 so 触发重算；产物清单中无 `.bak` | 否 |
| P1-7 | `scripts/verify-tfm-matrix.ps1` + 新 `scripts/verify-workload.ps1` | 在现有 pack / ZIP 资产检查之外，加隔离 SDK 或隔离 hive：安装 workload → `dotnet workload list` → 新模板工程 `restore` → 无设备 build；manifest 版本固定 | 不改 `%ProgramFiles%\dotnet` 下的用户全局 SDK；不复用上一轮 hive | 冷启动脚本从头成功；重复执行幂等；失败输出 hive 路径与清单快照；本机 `dotnet workload list` 因 CLI 环境问题崩溃时，以 `_HarmonyOSWorkloadInstalled` / `HarmonyOSWorkloadActive` MSBuild 属性探针为硬校验 | 否 |
| P1-8 | `.github/workflows/ci.yml` + `docs/WORKLOAD-PLAN.md` | CI 保留非设备 gate（lock 检查、matrix、workload 安装、构建到 HAP 产出为止）；真机安装/启动单独成设备 gate | 不让设备 gate 阻塞日常 PR；不把“NuGet restore 通过”写成“workload 可用” | 非设备 gate 稳定；设备 gate 至少一次真机或模拟器记录通过 | 是（仅设备 gate） |

可交付的第一步 = **P1-1 + P1-2 + P1-3**：workload 可安装、manifest 生效、仓库与模板不再内联片段。
`P1-4` ~ `P1-6` 才是让 NativeAOT / arm64 补丁进入正式发布链的部分；`P1-7`、`P1-8` 是证明它不会被本地状态“骗过”。

## 三、Phase 2：MAUI 平台资产（需构建真平台 DLL）

目标：让应用**无需外部 HarmonyOS 平台包**，就能以 `net10.0-harmonyos26.0` 使用 MAUI 的 Controls / Essentials，
并形成 `dotnet publish` 直出 HAP 的完整发布链。本地 MAUI 源码在 `D:\Harmony\maui`（参照用，不改 agreement）。

| 优先级 | 产物 / 文件 | 改动 | 不做什么 | 验收 | 需上机 |
|---|---|---|---|---|---|
| P2-1 | `D:\Harmony\maui` 构建基线 | 建立 MAUI 源码本地构建与平台测试基线：先保证现状 `net10.0` 等平台产物全绿，再评估 HarmonyOS TFM 注入点（`Directory.Build.props:132` `IncludeTizenTargetFrameworks` 的同类开关） | 不一次性打开全部平台 TFM；不改公共 API 约定 | 基线构建与现有测试通过并留记录 | 否 |
| P2-2 | `Microsoft.Maui.Sdk`（MAUI 内） | 增加 HarmonyOS workload / SDK 接入检测，按“已安装才启用”模式挂 TFM；`WorkloadManifest` 的 `maui-mobile` 扩展关系与官方顺序对齐 | 不把 HarmonyOS 设成无条件 extends，不让未安装用户 restore 失败 | 未安装 workload 时 MAUI 工程行为不变；安装后 TFM 自动可用 | 否 |
| P2-3 | `Microsoft.Maui.Controls` / `Core` 首切片 | 产出 `lib/net10.0-harmonyos26.0/`：`MauiApplication`、Handler 注册、`Window`、`Button`、`Label`、`Dispatcher` | 首切片不包含 Blazor / WebView / 全量控件 | 消费工程资产表选中平台目录；纯 MAUI 页面构建成功 | 部分（构建否，运行是） |
| P2-4 | `Microsoft.Maui.Essentials` | 按已实现且有设备验证的服务逐个出平台资产；未实现 API 显式 `NotSupportedException`（或平台特定异常），文档列清单 | 不宣称“全部支持”；不静默回落到桌面实现 | 每个服务有对应设备用例；不支持项可断言失败 | 是 |
| P2-5 | `Microsoft.AspNetCore.Components.WebView.Maui` | 独立评估能否同仓交付 BlazorWebView 平台资产；不能则拆独立 Fork，排在 Phase 2 后段 | 不在 P2-3 首切片里塞 Blazor / WebView | Blazor Hybrid 模板可构建并可加载页面 | 是 |
| P2-6 | HAP 发布链 | `PublishAot` + 共享库输出 + arm64 patch（复用 P1-6）+ 签名/打包任务化；pack 失败时清理旧产物；`dotnet publish` 直出 HAP | 不再依赖 `HarmonyBuildLibApp` 内部嵌套 `dotnet publish` | 一次 `dotnet publish` 得到已签名 HAP；失败不留旧 HAP / 旧 so | 部分（签名与安装） |
| P2-7 | Phase 2 验收 | 无外部 `HarmonyOS.Maui*` `PackageReference` 的 HelloApp 全链构建；检查 `project.assets.json` 的 compile / runtime 资产指向 `lib/net10.0-harmonyos26.0/`；设备安装启动、点击按钮、Label 更新 | 不复用缓存快照验收；MAUI 升版后重跑同一 gate | 上述全部通过并记录版本三元组（MAUI / .NET SDK / HarmonyOS SDK） | 是 |

## 四、判定线

- **两阶段都不做**：clone 或修改 `dotnet/runtime`；把普通 `net10.0` 第三方程序集伪装成 HarmonyOS 资产；把 NuGet restore 通过等同于 workload 可用。
- **Phase 1 成功**：消费工程无需内联平台片段；官方 .NET SDK 清单上下文中可构建；arm64 `libapp.so` 已 patch 且 HAP 内复核；workload 可安装、可复原、可卸载。
- **Phase 1 不足**：官方 `Microsoft.Maui.*` DLL 仍是 `net10.0`，平台行为仍由外部 `HarmonyOS.Maui` handler 层提供 → 进入 Phase 2。
- **Phase 2 成功**：Controls（首切片）/ Essentials 提供 `net10.0-harmonyos26.0` 资产，真机启动，`dotnet publish` 直出 HAP。
- **Phase 2 不宣称**：全量 MAUI 控件、Blazor / WebView 一步到位；任何第三方包自动具备 HarmonyOS 资产。

## 五、P1-1 ~ P1-7 落地记录（2026-11-04）

Phase 1 除 P1-8 外均已落地，验收入口为 `scripts/verify-workload.ps1`（隔离 SDK，不改全局安装）：

- workload 骨架：`src/workloads/harmonyos/` 提供 `WorkloadManifest.in.json`、`WorkloadManifest.targets` 与
  `Microsoft.NET.Sdk.HarmonyOS` Sdk pack（`Sdk.props` / `Sdk.targets` / `AutoImport.props`）；
  `eng/Workloads/build-workload.ps1` 产出 manifest、SDK pack、`Microsoft.HarmonyOS.App.Ref` /
  `Microsoft.HarmonyOS.App.Runtime.NativeAOT.linux-musl-arm64|x64` runtime pack（含 ILCompiler alias pack）与模板包 nupkg。
- manifest 安装 band 按目标 SDK 自身 feature band 推导（10.0.403 → `sdk-manifests\10.0.400\`），
  不再假设固定 10.0.100 band（`scripts/verify-workload.ps1:124-132`）。
- 平台注册双路径：workload 已安装时由 manifest targets 注册，`AutoImport.props` 置 `_HarmonyOSWorkloadInstalled=true`；
  未安装时 `Directory.Build.targets:23-30` 以同名条件兜底，仓库内工程不装 workload 也能构建
  （`dotnet build src/HarmonyOS.Essentials -c Release` 回归验证通过）。
- 模板：`harmony-maui` / `harmony-maui-web` 工程内不再内联平台注册片段，注册改由 workload 提供。
- NativeAOT arm64 补丁抽成 `src/HarmonyOS.Maui.Tasks/PatchOpenHarmonyNativeAot.cs` MSBuild task，
  接入 `HarmonyOS.Maui.App.targets:158-227`（publish 后 patch、HAP staging 后复核）。
- 本机 `dotnet workload list` 因 CLI 环境问题（`InstallerBase` 崩溃）不可用时，verify 脚本自动降级为
  `_HarmonyOSWorkloadInstalled` / `HarmonyOSWorkloadActive` MSBuild 属性探针硬校验，二者均须为 `true`。
- 待做：P1-8（`.github/workflows/ci.yml` 非设备 gate / 设备 gate）。
