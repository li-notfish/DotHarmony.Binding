# ArkTsBinding / DotHarmony.Binding

一个**模仿 .NET MAUI 平台绑定层逻辑**（Mono.Android / Microsoft.iOS 的思路）的鸿蒙试验项目：
用 .NET (NativeAOT) 绑定 HarmonyOS (ArkUI/ArkTS)，并让 .NET MAUI 控件经 Handler 机制渲染为 ArkUI 原生节点。

**当前状态：核心链路已在模拟器端到端验证** —— 运行时初始化 → 原生节点树上屏 → 属性/事件闭环 →
`@ohos.*` 服务调用 → **XAML 声明式 UI → 鸿蒙原生渲染**。**21 个 MAUI 控件 Handler** 已适配并统一为官方 handler 风格。距离可用于生产的绑定库还有明确距离，见文末已知限制与 [ROADMAP.md](ROADMAP.md)。

## 这是什么

本项目在逻辑上模仿 Mono.Android / Microsoft.iOS 的分层思路（平台绑定库 + Handler 适配 + 工具链），
**并非其量级的实现**——那两者背后是微软与三星的专职团队。本项目的对应物：

| 官方生态 | 本项目 |
|---|---|
| Mono.Android（Java API 绑定） | `HarmonyOS.Bindings`（ArkUI C API / napi 绑定，生成器产出） |
| 各平台 Handler（Android/iOS/...） | `HarmonyOS.Maui`（MAUI 控件 → ArkUI 原生节点，不 fork dotnet/maui） |
| Android/iOS head 工程 | `HarmonyHost`（ArkTS 宿主 + C shim，固定模板） |
| workload / msbuild 集成 | `scripts/`（远程 NativeAOT + hvigor + hdc 一键脚本）

```
[MAUI 应用] XAML / C# 控件树（Microsoft.Maui.Controls VirtualView）
    ↓
[HarmonyOS.Maui]  HarmonyButtonHandler / HarmonyLabelHandler / HarmonyLayoutHandler /
                  HarmonyContentPageHandler（ViewHandler<,> + PropertyMapper/CommandMapper 协议）
    ↓ PlatformView
[HarmonyOS.Bindings]
    ├─ Nodes/       ArkUINodeBase : { ArkUI_NodeHandle } 包装类（解析器自动生成）
    ├─ NativeNode/  ArkUI NDK C API 互操作（ArkUI_NativeNodeAPI_1 函数表镜像、事件总线）
    ├─ Runtime/     napi 互操作（非 UI 的 @ohos.* 服务调用；env 注入、hilog 桥）
    └─ Hosting/     Host 入口（libapp.so 导出 HarmonyInit / HarmonyBuildUI）
    │ P/Invoke / dlopen
[HarmonyHost 宿主]  ArkTS 页面 (ContentSlot) + C shim (libentry.so) + libapp.so (NativeAOT)
```

## 关键架构决策

1. **UI 走 ArkUI NDK C API，不走 napi 调 ArkTS 对象。**
   ArkUI 声明式组件由 ArkTS 编译器在 build() 内构建，napi 创建的裸对象没有挂进 UI 树的路径；
   命令式 UI 的官方原生路径是 `ArkUI_NativeNodeAPI_1`（createNode/setAttribute/setEventListener），
   经 NodeContent/ContentSlot 挂载。`ArkUI_NodeHandle` 是稳定句柄（对齐 UIKit 指针语义）。
   早期白皮书（ArkTsBinding.md，文末附决策记录）的「napi 直调 ArkTS 影子对象」方案已被此路线取代。
2. **非 UI 的 @ohos.\* 服务**（传感器/定位等）继续走 napi，env 由宿主 shim 在入口注入（`NapiEnv.Initialize`）。
3. **运行时**：鸿蒙禁 JIT，唯一路线是 NativeAOT（linux-musl）产出 libapp.so，由 C shim dlopen。
   Windows 不支持 cross-OS NativeAOT，交叉编译在远程 Linux（WSL2）完成。
4. **入口程序集导出**：ILC 只导出入口程序集内的 `[UnmanagedCallersOnly]`，
   因此每个 libapp.so 应用需要薄转发层（见 HelloApp 的 `NativeExports`）。

## 快速开始

环境：Node.js + npm（解析器）、Python 3（头文件提取）、.NET 10 SDK（绑定库）、
DevEco Studio（内置 HarmonyOS SDK/NDK/hvigor）、可 SSH 的 Linux（NativeAOT 交叉编译）。

```bash
# 1. 解析器构建 + 测试（45 用例）
npm install && npm test

# 2. 从 NDK 头文件生成 C# 枚举（ArkUINodeTypes.g.cs + .json 元数据）
python src/nativeBinding/extract_arkui_types.py --dump-json HarmonyOS.Bindings/NativeNode/ArkUINodeTypes.json

# 3. 从 SDK 组件 .d.ts 生成 NodeHandle 包装类 → HarmonyOS.Bindings/Nodes/
npx ts-node src/parser/index.ts --native

# 4. Windows 上构建绑定库 + 样例
dotnet build ArkTsBinding.slnx

# 5. 交叉编译 libapp.so（双架构）。LOCAL=true 走本地 WSL（上传式构建），默认经 SSH 远程
bash scripts/remote-build.sh

# 6. 打 HAP（hvigor；DevEco 路径自动探测，或用 DEVECO_HOME 指定）
cmd //c scripts\build-hap.cmd

# 7. 部署到模拟器/真机并抓取日志（hdc 自动探测；PowerShell 版为 deploy-hap.ps1）
bash scripts/deploy-hap.sh
```

## 脚本工具链（scripts/）

| 脚本 | 用途 | 说明 |
|---|---|---|
| `remote-build.ps1` / `remote-build.sh` | 交叉编译 libapp.so（arm64 + x64 双架构） | `LOCAL=true`：本地 WSL 构建——**上传式**（打包 → 解压到 WSL 原生文件系统 → 构建 → 取回），勿在 `/mnt/*` 上直接构建（9p I/O 慢一个数量级）；默认经 SSH 远程构建（别名 `wsl_auzrelinux`，构建机 IP 漂移先跑 `resolve-remote.ps1`） |
| `build-hap.cmd` | hvigor 打 HAP | DevEco Studio 路径自动探测，`DEVECO_HOME` 可覆盖；签名需在 DevEco 中配置一次自动签名 |
| `deploy-hap.sh` / `deploy-hap.ps1` | 重装 HAP → 启动 → 抓取 HarmonyHost 日志 | hdc 自动探测；无设备 / 缺 HAP / 安装失败即报错停止；启动前 `aa force-stop` 防 install 竞争 |
| `build-files.txt` | remote-build 打包清单（含 excludes） | ps1/sh 共用的唯一来源，改一处即可 |
| `build-libapp.sh` | 构建机内部的 NativeAOT 发布 | 由 remote-build 调用，不必手动跑；musl.cc gcc（arm64）+ zig cc（x64）wrapper 幂等生成 |
| `resolve-remote.ps1` | 定位 SSH 构建机并回写 `~/.ssh/config` | 仅 SSH 远程模式需要 |
| `smoke-aot.sh` | NativeAOT + zig cc 工具链冒烟探针 | 工具链问题排查用 |
| `gen-module-sample.ts` | @ohos.* 模块绑定样例生成 | napi 路线（ROADMAP 2.3） |

环境变量（全部可选，脚本内置默认探测链）：

| 变量 | 作用 | 探测顺序 |
|---|---|---|
| `OHOS_SDK_BASE` | OpenHarmony SDK 根目录（含 `26.0.0/toolchains`） | → `OHSDK_HOME` → `D:\Harmony\OpenHarmony\Sdk` → DevEco 内置 sdk |
| `DEVECO_HOME` | DevEco Studio 安装目录 | → `D:\Program Files\Huawei\DevEco Studio` → C 盘同名 |
| `LOCAL` | `true` 时 remote-build 在本地 WSL 构建 | 否则走 SSH 远程 |
| `REMOTE` / `BUILD` | SSH 别名 / 构建目录 | `wsl_auzrelinux` / `/tmp/arktsbinding` |

> 约定：`.gitattributes` 强制 `*.sh` 为 LF（WSL bash 无法执行 CRLF 脚本）、`*.cmd/*.ps1` 为 CRLF；新增脚本请沿用"路径自动探测 + 前置检查失败即停"的风格。

## 类型映射（Native 模式）

| ArkTS (.d.ts) | C# (Nodes/) | 封送 |
|---|---|---|
| 组件 `TextInterface`/`TextAttribute` | `class Text : ArkUINodeBase` | `ARKUI_NODE_TEXT` |
| 构造参数 `Text(content)` | `string Content` | `SetStringAttribute(NODE_TEXT_CONTENT)` |
| `textAlign(...)` | `ArkUI_TextAlignment TextAlign` | i32 单值 |
| `fontColor(...)` | `void SetFontColor(r,g,b,a=255)` | u32 `0xAARRGGBB` |
| `onClick(...)` | `event Action<ArkUINodeEvent>? Click` | `NODE_ON_CLICK` + 事件总线 |
| 点击参数 | `ev.ClickX/.ClickY/.ClickDevice/...` | `NodeComponentEvent.data[]` 直读 |
| `Promise<T>` | `IntPtr`（napi promise 句柄） | async 待 TSFN 层 |
| 未映射属性 | —— | 记入 `Nodes/native-gaps.json` |

生成器不猜属性形态：只有登记在 `nativeCodeGenerator.ts` shape 表中的属性才生成代码，
其余进入 **gap 清单**（当前 3851 条），这是 C API 覆盖度的实时地图，也是扩展组件的待办清单。

## 运行时工具链要点（踩坑记录）

- **musl 交叉**（方案借鉴 [PublishAotCross](https://github.com/MichalStrehovsky/PublishAotCross)，见文末致谢）：arm64 用 musl.cc gcc（`naot-driver` wrapper 过滤 clang 风格 `--target`）；
  x64 用 zig cc（wrapper 需过滤 `-Wl,--gc-sections`——它会收割 ILC 的 `__modules` section，
  导致 dlopen 时 `__start___modules` 重定位失败）。
- **符号分离**：arm64 用工具链 objcopy；x64 用 `zig objcopy` 的 GNU 兼容 wrapper（`objcopy-gnu`）。
- **引导参数**：shim 在 dlopen 前 setenv `DOTNET_GCHeapHardLimit`（默认 256G 虚拟预留超限）与 `ICU_DATA`。
- **调试**：C# 经 `HiLog` 直写 hilog（含托管堆栈）；`hilog -x | grep HarmonyHost`。

## 项目结构

```
src/parser/                  解析器（TS Compiler API）
  ├─ astParser.ts            AST → 组件/API 中间模型
  ├─ nativeCodeGenerator.ts  C API 目标生成器（shape 表 + gap 登记）
  ├─ codeGenerator.ts        napi 目标生成器（@ohos.* 服务层）
  └─ typeMapper.ts           类型映射
src/nativeBinding/
  └─ extract_arkui_types.py  NDK 头文件 → C# 枚举 + JSON 元数据
HarmonyOS.Bindings/          绑定库（net10.0, AOT/trim 友好）
  ├─ NativeNode/             ArkUI C API 互操作 + ArkUINodeBase + 事件总线
  ├─ Nodes/                  生成的组件包装类 + native-gaps.json
  ├─ Runtime/                napi 互操作（env 注入、INapiRecord、HiLog）
  └─ Hosting/Host.cs         libapp.so 导出入口
src/HarmonyOS.Maui/          MAUI Handler 包（Button/Label/StackLayout/ContentPage → ArkUI 节点）
samples/HarmonyHost/         鸿蒙宿主工程（ArkTS + C shim + CMake + ohosImports.ets 模块登记）
samples/dotnet/HelloApp/     .NET 样例应用（XAML + NativeAOT → libapp.so）
scripts/                     remote-build / build-hap / deploy-hap 一键工具链
tests/                       jest（解析器/生成器 45 用例）
```

## 已知限制（当前真实状态）

- **布局语义**：ArkUI flex 托管布局——MAUI 的 measure/arrange 引擎未实现（Fill/Spacing/Margin/WidthRequest/HeightRequest 已对齐；Grid/AbsoluteLayout 未支持）
- **画刷**：SolidColorBrush/LinearGradientBrush/RadialGradientBrush 全支持；ImageBrush 为 MAUI internal 类型无法声明（节点层 SetBackgroundImage 原语已就位）
- **导航**：轻量 Page 栈（HarmonyNavigation.Push/Pop，节点保留式切换已实测）；Shell/NavigationPage 官方类型与页面动画未支持
- **异步 API**：`Promise<T>`→`Task<T>` 已接通（TypeMapper + 生成器 + `CallMethodAsync`/`CallMethodAsyncVoid`）；AsyncCallback 风格 `(result: T, err?: Error) => void` → `Task<T>` 已支持（parser AST 检测）；TSFN 完整生命周期三路径已封装（`ThreadSafeFunction.cs` + `HarmonySynchronizationContext.cs`）
- **控件覆盖**：21 个 Handler（Button/Label/ContentPage/StackLayout/Grid/AbsoluteLayout + Entry/Editor/Switch/CheckBox/RadioButton/Slider/ProgressBar/Image/ScrollView/Frame/RefreshView/Picker/DatePicker/TimePicker + CollectionView/CarouselView M1 物化版），代码风格已统一为官方 handler 模式；新控件适配指南见 [HANDLERS.md](HANDLERS.md)
- **仅模拟器（x86_64）验证**：真机 arm64 待验证（工具链已就绪）
- **napi handle scope 未系统化**：当前依赖宿主线程已有的 scope，规范做法待补
- **权限模型未接**：需要权限的 @ohos.* 模块（位置/相机等）未生成 `module.json5` 联动

## 路线图

详细的后续路线、实现方案与难点分析见 **[ROADMAP.md](ROADMAP.md)**：
- M1 尾巴（完成）：~~Brush 助手~~、~~WidthRequest/HeightRequest~~、~~轻量导航~~、~~Grid/AbsoluteLayout（MAUI 托管布局）~~；剩真机验证
- M2：~~TSFN 异步层（2.1 完成）~~、~~codeGenerator 缺陷修复（2.2 完成）~~、@ohos.* 批量绑定
- M3：NuGet 打包、单项目体验、CI

## 致谢 / Acknowledgements

本项目的交叉编译方案与运行时移植实践，建立在以下开源工作的基础上：
- **[PublishAotCross](https://github.com/MichalStrehovsky/PublishAotCross)**（Michal Strehovsky）——
  用 zig cc 作为 NativeAOT 自定义链接驱动以实现 linux-musl 交叉编译的开创性方案。
  本项目 x64 目标的链接驱动即此思路的手写实现（针对鸿蒙场景增加了
  `-Wl,--gc-sections` 过滤等适配），未直接引用其 NuGet 包，特此声明并致谢。
- **[musl.cc](https://musl.cc/)** —— aarch64-linux-musl 交叉工具链（arm64 构建使用）。
- **[OpenHarmony.Avalonia](https://github.com/CeSun/OpenHarmony.Avalonia)**（CeSun）——
  .NET 运行时鸿蒙移植的先行实践，本项目采用的 GC 堆上限与 ICU 引导参数配方源自其公开的移植记录。
- **OpenHarmony / HarmonyOS** —— ArkUI NDK（ArkUI_NativeNodeAPI_1）与 Node-API 的官方能力支撑。
