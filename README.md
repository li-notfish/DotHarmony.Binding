# DotHarmony.Binding

**HarmonyOS 的 .NET MAUI 平台层** —— 在 NativeAOT 上把 ArkUI 与 `@ohos.*` 系统 API 绑定给 .NET，
并让 MAUI 控件经 Handler 渲染为 ArkUI 原生节点。

应用仍是标准 MAUI 工程：XAML、MVVM、Shell、导航、手势、Essentials 都按官方写法使用，
`dotnet build -t:HarmonyRun` 一条命令完成「NativeAOT 编译 → 打 HAP → 安装启动」。

| 项 | 说明 |
|---|---|
| 平台 | HarmonyOS / OpenHarmony（arm64 真机、x86_64 模拟器） |
| 运行时 | .NET 10 + NativeAOT（linux-musl），C# 14 |
| 宿主 | ArkTS 壳工程 dlopen 应用产物 `libapp.so` |
| 包族 | `HarmonyOS.Interop` / `.Bindings` / `.Essentials` / `.Maui` / `.Maui.Web` / `.Maui.Blazor` / `.Maui.Permissions.Abstractions` / `.Templates` |

> 状态：可用于原型与内部应用；距离生产级绑定库仍有明确差距，见[已知限制](#已知限制)与 [ROADMAP.md](ROADMAP.md)。

## 特性

- **XAML → 原生渲染**：58 个具体 Handler（60 个工厂分派形态），覆盖布局、输入、列表、Picker 族、
  Shape/GraphicsView 自绘（OH_Drawing）与 Shell / TabbedPage / FlyoutPage / WebView。
- **布局与导航**：StackLayout 走 ArkUI flex 托管；Grid / AbsoluteLayout 走 MAUI 托管绝对定位
  （对齐、ZIndex、Auto 轨道自适应已接通）；NavigationPage、模态与 Shell 路由转接 MAUI 官方协议，业务代码零改动。
- **手势**：Tap / Pan / Pinch / Swipe / Pointer / Drag&Drop，经 `HarmonyViewHandler` 基类统一挂载。
- **系统 API**：438 个 `@ohos.*` 模块绑定；`Promise<T>`→`Task<T>`、异步回调→`Task<T>`、
  `on/off/once`→.NET 事件、ArrayBuffer / TypedArray / Map / bigint 封送。
- **Essentials**：22 个平台服务（DeviceInfo、Clipboard、Preferences、Battery、Connectivity、
  五类传感器、Geolocation、MediaPicker、SecureStorage 等），静态入口自动注入。
- **权限自动推导**：扫描 MAUI/Essentials 调用，生成 `module.json5` 的 `requestPermissions` 与
  `usedScene`，并输出 `obj/harmony/permissions.report.md` 审计报告；未映射与冲突场景给出 `HMP001`–`HMP006` 诊断。
- **Blazor Hybrid**（实验）：可参与 MAUI 布局的内嵌 WebView、多实例、流式静态资源、双向 JS Interop。

## 快速开始

### 前置条件

| 依赖 | 用途 |
|---|---|
| .NET 10 SDK | 编译与 NativeAOT 发布 |
| DevEco Studio 5.1+、HarmonyOS/OpenHarmony SDK、ohpm | hvigor 打包、NDK、`hdc` 部署 |
| PowerShell 7+（`pwsh`）与 Git Bash | staging / 签名 / 部署脚本（Windows 自带的 5.1 不够） |
| 模拟器或真机 | 部署与运行；真机另需签名材料（`.p12`/`.cer`/`.p7b`） |
| Node.js ≥ 18、Python 3 | 仅修改绑定生成器时需要 |

环境自检：

```powershell
pwsh scripts/check-harmony-env.ps1              # 工具链 / SDK / hdc
pwsh scripts/check-harmony-env.ps1 -CheckDevice # 追加设备检查（Bash: --check-device）
```

### 运行一个样例

```powershell
# 自动 stage 宿主 → 双架构 NativeAOT → HAP → 安装并启动
dotnet build samples/dotnet/HelloApp -t:HarmonyRun
```

需要分段排查时使用**该应用的暂存宿主**（`samples/HarmonyHost` 是模板，不可直接部署）：

```powershell
dotnet build samples/dotnet/HelloApp -t:HarmonyStageHost
dotnet build samples/dotnet/HelloApp -t:HarmonyBuildLibApp
$env:HOST_DIR = "samples/dotnet/HelloApp/obj/harmony/host"
.\scripts\build-hap.cmd
pwsh scripts/deploy-hap.ps1
```

多设备在线时用 `HDC_TARGET=<host:port>` 指定目标。`HarmonyRun` 顺序执行
设备预检 → 宿主 staging / 权限生成 → NativeAOT → HAP / 签名 → 部署启动，任一步失败即停止，
失败定位见 [排障速查](GETTING_STARTED.md#5-排障速查)。

### 创建自己的应用

见 **[GETTING_STARTED.md](GETTING_STARTED.md)**：路线 A 经 NuGet 模板从零创建（推荐），
路线 B 在仓库内参照开发，路线 C 把鸿蒙平台加入已有 MAUI 应用。

### 样例矩阵

| 场景 | 样例 | 覆盖重点 |
|---|---|---|
| 纯 UI 渲染 | `ControlsSampleApp`、`WeatherTwentyOne`、`HelloApp` | 控件/输入/列表/布局/媒体/Shell 多 tab、XAML/ViewModel、手势与导航 |
| 系统 API | `EssentialsApp`（主）、`ApiDemo`（补充） | Essentials 22 服务；ApiDemo 覆盖真实 HTTP、文件往返、权限申请、定位、拍照/选图 |
| 复杂互操作 | `EngineLab`、`PerfApp` | ArkTS 引擎节点回流（实验层）、NativeAOT/TSFN/回调压力与性能基线 |

## NuGet 包

`dotnet pack -c Release -o dist` 产出八个包，版本统一由根 `Directory.Build.props` 的 `PackageVersion` 供给：

| 包 | 内容 |
|---|---|
| `HarmonyOS.Interop` | napi 互操作核心（env 注入 / 封送 / 回调 / TSFN / HiLog） |
| `HarmonyOS.Bindings` | ArkUI 节点与 `@ohos.*` 绑定（依赖 Interop） |
| `HarmonyOS.Essentials` | Essentials 鸿蒙实现（依赖 Bindings） |
| `HarmonyOS.Maui` | MAUI 渲染层 + buildTransitive 宿主编排 + `MauiAppBuilder` 桥 |
| `HarmonyOS.Maui.Web` | opt-in WebView surface 与 WebView Handler |
| `HarmonyOS.Maui.Blazor` | Blazor Hybrid 集成（基于 `.Maui.Web`） |
| `HarmonyOS.Maui.Permissions.Abstractions` | 权限推导 source generator 的公共抽象 |
| `HarmonyOS.Templates` | `dotnet new harmony-maui` / `harmony-maui-web` 应用模板 |

消费方体验对齐 maui-android 单项目：引用 `HarmonyOS.Maui` 后
`dotnet build -t:HarmonyStageHost` / `-t:HarmonyRun` 即可，targets 由包内
`buildTransitive/HarmonyOS.Maui.targets` 自动导入，应用工程无需仓库工作副本。

Release 签名通过属性簇配置：`HarmonySigningKeystore` / `KeystorePassword` / `CertAlias` /
`CertPassword` / `CertPath` / `Profile`，缺项时 staging 直接报错。口令可由
`HarmonySigningKeystorePasswordFile` / `HarmonySigningCertPasswordFile` 指定文件读取
（`hap-sign-tool` 只接受 argv，文件方式仅缩小泄露面）。提供完整签名属性时输出
`entry-default-signed.hap`，否则只产出 unsigned HAP。

## 工具链

| 脚本 | 用途 |
|---|---|
| `scripts/stage-host.ps1` | 按应用实例化宿主模板（重写 bundleName / 应用名 / 目标 SDK），内容戳增量 |
| `scripts/check-harmony-env.ps1` / `.sh` | 环境自检（.NET / Node / Python / DevEco / SDK / hdc / ohpm，`-CheckDevice` 可选） |
| `scripts/build-hap.cmd` | hvigor 打 HAP；`HOST_DIR` 必须指向暂存宿主，缺失时直接报错 |
| `scripts/sign-hap.ps1` | 用 `hap-sign-tool.jar` 签名 unsigned HAP |
| `scripts/deploy-hap.sh` / `.ps1` | 安装 HAP → 启动 → 抓取 HarmonyHost 日志 |
| `scripts/patch-openharmony-nativeaot.ps1` | 修补 arm64 NativeAOT runtime 的 NUMA 探测（云真机 seccomp 兼容） |

常用环境变量（全部可选，脚本内置默认探测链）：

| 变量 | 作用 |
|---|---|
| `OHOS_SDK_BASE` | SDK 版本目录或集合根目录（→ `OHOS_SDK_HOME` → `OHSDK_HOME` → `DEVECO_SDK_HOME` → DevEco 内置） |
| `DEVECO_HOME` | DevEco Studio 安装目录 |
| `HOST_DIR` | 暂存宿主目录覆盖（必须指向 `<sample>/obj/harmony/host`，targets 自动设置） |
| `HARMONY_COMPATIBLE_SDK_VERSION` / `HARMONY_RUNTIME_OS` | 交叉编译目标 SDK 版本 / 覆盖 `HarmonyOS`、`OpenHarmony` |
| `HDC_TARGET` | 多设备时的部署目标 |

> 约定：`.gitattributes` 强制 `*.sh` 为 LF、`*.cmd/*.ps1` 为 CRLF；新增脚本沿用
> 「路径自动探测 + 前置检查失败即停」的风格。

## 架构

分层与 Mono.Android / Microsoft.iOS 同构：

| 官方生态 | 本项目 |
|---|---|
| Java.Interop | `HarmonyOS.Interop`（napi 互操作核心） |
| Mono.Android | `HarmonyOS.Bindings`（ArkUI 节点 + `@ohos.*` 绑定，生成器产出） |
| Microsoft.Maui.Essentials | `HarmonyOS.Essentials`（22 服务） |
| 各平台 Handler | `HarmonyOS.Maui`（MAUI 控件 → ArkUI 节点，不 fork dotnet/maui） |
| Android/iOS head 工程 | `HarmonyHost`（ArkTS 宿主 + C shim 模板，按应用 stage 实例） |
| workload / msbuild 集成 | `scripts/`（PublishAotClang + hvigor + hdc 一键脚本） |

```
[MAUI 应用] XAML / C# 控件树（VirtualView）
    ↓
[HarmonyOS.Maui]  ViewHandler<,> + PropertyMapper/CommandMapper
    ↓ PlatformView
[HarmonyOS.Bindings]
    ├─ Nodes/       ArkUINodeBase 包装类（生成器产出）
    ├─ NativeNode/  ArkUI NDK C API 互操作（函数表镜像、事件总线）
    └─ Hosting/     libapp.so 导出入口（HarmonyInit / HarmonyBuildUI）
[HarmonyOS.Interop] napi 核心（env 注入、值封送、回调/TSFN、HiLog）
    │ P/Invoke
[HarmonyHost 宿主]  ArkTS 页面 (ContentSlot) + C shim (libentry.so) + libapp.so (NativeAOT)
```

设计取舍与实测记录（UI 为何走 ArkUI NDK C API、NativeAOT 交叉编译方案、平台约束等）见
[ROADMAP.md](ROADMAP.md) 与 [docs/archive/ArkTsBinding.md](docs/archive/ArkTsBinding.md)。

## 项目结构

```
tools/api-generator/         .d.ts 解析器与 @ohos.* / 节点绑定生成器（TS Compiler API）
tools/arkui-bindgen/         NDK 头文件 → C# 生成器（libclang，语义配置驱动）
tools/HandlerAudit/          Handler 覆盖矩阵与工厂分派拓扑审计
src/HarmonyOS.Interop/       napi 互操作核心（独立包）
src/HarmonyOS.Bindings/      绑定库（net10.0 + net10.0-harmonyos）
  ├─ NativeNode/             ArkUI C API 互操作 + ArkUINodeBase + 事件总线
  ├─ Nodes/                  生成的组件包装类 + native-gaps.json
  ├─ Api/                    438 个 @ohos.* 模块绑定
  ├─ Hosting/                libapp.so 导出入口
  └─ Experimental/           ArkTS 命令总线引擎实验层
src/HarmonyOS.Maui/          Handler 包 + 宿主编排 targets
src/HarmonyOS.Essentials/    Essentials 平台实现
src/HarmonyOS.Maui.Web/      WebView surface
src/HarmonyOS.Maui.Blazor/   Blazor Hybrid 集成
src/HarmonyOS.Maui.Permissions/              权限推导 source generator（打为 analyzer）
src/HarmonyOS.Maui.Permissions.Abstractions/ generator 的公共抽象
src/HarmonyOS.Templates/     dotnet new 模板包
samples/HarmonyHost/         共享 ArkTS 宿主核心（模板，由 targets 按应用 stage）
samples/dotnet/              示例应用（见上方样例矩阵）
tests/                       jest（生成器）+ tests/dotnet（xunit）
scripts/                     staging / 打包 / 签名 / 部署 / 验证脚本
docs/                        专项文档（权限、性能基线、覆盖矩阵、互操作契约）
```

## 文档

| 文档 | 内容 |
|---|---|
| [GETTING_STARTED.md](GETTING_STARTED.md) | 从零创建应用 / 接入已有 MAUI 应用 / 构建链路 / 排障 |
| [HANDLERS.md](HANDLERS.md) | 控件 Handler 与 `@ohos.*` 绑定的开发指南、踩坑速查 |
| [ESSENTIALS.md](ESSENTIALS.md) | 新增 Essentials 服务的五步流程与坑表 |
| [docs/PERMISSIONS.md](docs/PERMISSIONS.md) | 权限推导的三类来源与 capability map schema |
| [docs/handler-coverage.md](docs/handler-coverage.md) | Handler 覆盖矩阵（由 `tools/HandlerAudit` 生成） |
| [docs/control-defaults-audit.md](docs/control-defaults-audit.md) | MAUI 与 ArkUI 控件默认值差异审计 |
| [docs/interop-rules.md](docs/interop-rules.md) | 互操作不变量（维护者契约） |
| [docs/PERF_BASELINE.md](docs/PERF_BASELINE.md) | 性能基线指标与采集方式 |
| [MIGRATION_ARKTS_ENGINE.md](MIGRATION_ARKTS_ENGINE.md) | C 节点 API 退出假设下的 ArkTS 引擎迁移预案 |
| [ROADMAP.md](ROADMAP.md) | 设计取舍、实测结论与遗留项 |

## 已知限制

- **布局语义**：StackLayout 主轴方向的 `LayoutOptions` 与 MAUI 官方一致地被忽略；
  Span>1 的 Auto 轨道未做专项验证。Grid / AbsoluteLayout 由 MAUI 托管绝对定位（非 ArkUI 原生栅格）。
- **画刷**：SolidColorBrush、LinearGradientBrush、RadialGradientBrush 已支持；ImageBrush 在 MAUI 10 中仍是
  internal 类型，用户代码无法声明（节点层 `SetBackgroundImage` 原语已就位）。
- **导航**：NavigationPage、模态、Shell（TabBar / Flyout / 路由 / query / 主题色）均已接通。
- **自绘**：Shape / GraphicsView 的文本、渐变、位图与测量路径仍在补齐。
- **异步**：Promise 与 callback 桥的续体在 JS 线程内联恢复，长耗时工作需自行 `Task.Run`；
  逐次调用的 napi handle scope 尚未统一 RAII 封装。
- **类型映射**：未映射的复杂 options bag 仍退化为 `IntPtr`，清单见
  `artifacts/generator/api-intptr-report.json`；`object?` 参数转换点存在基元装箱。
- **手势**：鼠标按键区分、hover 通道、Pinch 多点触控注入通道未覆盖。
- **Essentials**：TextToSpeech / HapticFeedback / Flashlight / Map / FilePicker / Screenshot 尚未实现；
  传感器、Geolocation、MediaPicker 缺真机专项验证。
- **Blazor Hybrid** 属实验特性，公开 API 以 MAUI/Controls 示例为准。
- **平台 TFM**：`net10.0-harmonyos` 目前是无 workload 的最小注册片段，未做真正的 workload 化。

## 参与开发

```powershell
dotnet build ArkTsBinding.slnx          # 绑定库与样例
npx jest --runInBand                    # 生成器单测
python -m pytest tools/arkui-bindgen/tests -q   # NDK 生成器单测
dotnet test tests/dotnet/HarmonyGestureTests/HarmonyGestureTests.csproj   # 行为与契约测试
```

重新生成绑定时（需要 SDK 头文件，属维护者操作）：

```powershell
# NDK 头文件 → C# 枚举 / 结构体 / 函数表镜像
$env:PYTHONPATH = "tools/arkui-bindgen"
python -m arkui_bindgen gen --config tools/arkui-bindgen/config/semantics.yaml --out-dir src/HarmonyOS.Bindings/NativeNode
python -m arkui_bindgen check          # 漂移检查（需本机 OpenHarmony SDK）

# SDK .d.ts → 节点包装类与 @ohos.* 模块绑定
npx ts-node tools/api-generator/index.ts --native
npx ts-node tools/api-generator/index.ts --sdk "<SDK 路径>" --all
```

生成范围：仓库没有生成宿主 C 代码的链路——生成器只产出 C# 绑定与元数据，
`HarmonyHost` 的 C/CMake 仍按手写模板维护；ArkTS 模块登记（`ohosImports.ets`）与权限 JSON
由生成器/构建维护。

程序集内只有入口程序集的 `[UnmanagedCallersOnly]` 会进入 `app.so` 动态符号表，
因此每个应用都需要一份薄转发层（见 `samples/dotnet/HelloApp/Platforms/HarmonyOS/HarmonyExports.cs`）。

## 致谢

本项目的交叉编译方案与运行时移植实践，建立在以下开源工作之上：

- **[PublishAotClang](https://github.com/xljiulang/PublishAotClang)**（xljiulang）——
  当前 Windows 本地 NativeAOT 交叉编译使用的 Zig/Clang 工具链包装，本项目直接引用其 NuGet 包。
- **[PublishAotCross](https://github.com/MichalStrehovsky/PublishAotCross)**（Michal Strehovsky）——
  用 zig cc 作为 NativeAOT 自定义链接驱动实现 linux-musl 交叉编译的开创性方案，PublishAotClang 亦源自该项目。
- **[musl.cc](https://musl.cc/)** —— 历史上用于 arm64 构建的交叉工具链；当前主链路已切换为 PublishAotClang。
- **[OpenHarmony.Avalonia](https://github.com/CeSun/OpenHarmony.Avalonia)**（CeSun）——
  .NET 运行时鸿蒙移植的先行实践，本项目采用的 GC 堆上限与 ICU 引导参数配方源自其公开记录。
- **[OpenHarmony-NET/runtime](https://github.com/OpenHarmony-NET/runtime)** ——
  其 OpenHarmony 目标移植指出了 `NUMASupport` 对 `get_mempolicy` 的依赖，并通过
  `TARGET_OPENHARMONY` 在编译期跳过该路径。本项目的后链接二进制补丁采用同一规避思路。
- **OpenHarmony / HarmonyOS** —— ArkUI NDK（`ArkUI_NativeNodeAPI_1`）与 Node-API 的官方能力支撑。
