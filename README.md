# ArkTsBinding / DotHarmony.Binding

一个**模仿 .NET MAUI 平台绑定层逻辑**（Mono.Android / Microsoft.iOS 的思路）的鸿蒙试验项目：
用 .NET (NativeAOT) 绑定 HarmonyOS (ArkUI/ArkTS)，并让 .NET MAUI 控件经 Handler 机制渲染为 ArkUI 原生节点。

**当前状态：M1（MAUI 基本面）与 M2（服务层）均已完成并在模拟器端到端验证** —— XAML 声明式 UI → 鸿蒙原生渲染、
22 个 MAUI 控件 Handler、手势识别（Tap/Pan/Pinch/Swipe/Pointer → ArkUI 原生手势 NDK）、**438 个 @ohos.\* 模块绑定（375 转正编译，Promise→Task / .NET 事件 / ArrayBuffer/Map 封送全链路实测）**。
距离可用于生产的绑定库还有明确距离，见文末已知限制与 [ROADMAP.md](ROADMAP.md)。

**上手**：从零创建鸿蒙 MAUI 应用 / 给已有 MAUI 应用加鸿蒙平台，见 **[GETTING_STARTED.md](GETTING_STARTED.md)**。
**风险预案**：C 原生节点 API 退出假设下的 ArkTS 引擎迁移计划见 **[MIGRATION_ARKTS_ENGINE.md](MIGRATION_ARKTS_ENGINE.md)**。

## 这是什么

本项目在逻辑上模仿 Mono.Android / Microsoft.iOS 的分层思路（平台绑定库 + Handler 适配 + 工具链），
**并非其量级的实现**——那两者背后是微软与三星的专职团队。本项目的对应物：

| 官方生态 | 本项目 |
|---|---|
| Mono.Android（Java API 绑定） | `HarmonyOS.Bindings`（ArkUI C API / napi 绑定，生成器产出） |
| 各平台 Handler（Android/iOS/...） | `HarmonyOS.Maui`（MAUI 控件 → ArkUI 原生节点，不 fork dotnet/maui） |
| Android/iOS head 工程 | `HarmonyHost`（ArkTS 宿主 + C shim 模板；targets 按应用生成实例） |
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
# 1. 解析器构建 + 测试（76 用例）
npm install && npm test

# 2. 从 NDK 头文件生成 C# 枚举（ArkUINodeTypes.g.cs + .json 元数据；SDK 探测顺序 --sdk → OHOS_SDK_BASE → OHSDK_HOME → 默认路径，找不到报错退出）
python src/nativeBinding/extract_arkui_types.py --dump-json HarmonyOS.Bindings/NativeNode/ArkUINodeTypes.json

# 3. 从 SDK 组件 .d.ts 生成 NodeHandle 包装类 → HarmonyOS.Bindings/Nodes/
npx ts-node src/parser/index.ts --native

# 4. Windows 上构建绑定库 + 样例
dotnet build ArkTsBinding.slnx

# 5. 交叉编译 libapp.so（双架构）。LOCAL=true 走本地 WSL（上传式构建），默认经 SSH 远程
bash scripts/remote-build.sh
# （或用 MAUI 风格一键：dotnet build samples/dotnet/HelloApp -t:HarmonyRun —— 自动 stage 宿主 → AOT → HAP → 部署）

# 6. 打 HAP（hvigor；DevEco 路径自动探测，或用 DEVECO_HOME 指定）
cmd //c scripts\build-hap.cmd

# 7. 部署到模拟器/真机并抓取日志（hdc 自动探测；PowerShell 版为 deploy-hap.ps1）
bash scripts/deploy-hap.sh
```

## 脚本工具链（scripts/）

| 脚本 | 用途 | 说明 |
|---|---|---|
| `remote-build.ps1` / `remote-build.sh` | 交叉编译 libapp.so（arm64 + x64 双架构） | `LOCAL=true`：本地 WSL 构建——**上传式**（打包 → 解压到 WSL 原生文件系统 → 构建 → 取回），勿在 `/mnt/*` 上直接构建（9p I/O 慢一个数量级）；默认经 SSH 远程构建（别名 `wsl_auzrelinux`，构建机 IP 漂移先跑 `resolve-remote.ps1`） |
| `stage-host.ps1` | 宿主工程生成：模板 → 按应用实例（重写 bundleName/应用名） | 由 targets 的 HarmonyStageHost 调用（内容戳增量）；`HarmonyGenerateHost=false` 可跳过 |
| `build-hap.cmd` | hvigor 打 HAP | DevEco Studio 路径自动探测，`DEVECO_HOME` 可覆盖；`HOST_DIR` 指向按应用暂存宿主（targets 自动设置） |
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
| `DEMO_APP` | libapp.so 打包哪个 demo 工程（`HelloApp`=控件 demo / `ApiDemo`=API 绑定 demo） | `HelloApp` |
| `REMOTE` / `BUILD` | SSH 别名 / 构建目录 | `wsl_auzrelinux` / `/tmp/arktsbinding` |
| `HOST_DIR` | 宿主目录覆盖（三脚本通用；targets 生成模式自动指向 `obj/harmony/host`） | `samples/HarmonyHost` |

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
| `Promise<T>` / `AsyncCallback<T>` | `Task<T>` / `Task`（含 reject → `ArkTSException`） | PromiseTaskBridge / CallbackTaskBridge（续体在 JS 线程内联恢复） |
| 事件 `on/off/once(type, cb)` | .NET `event Action<T>` + 类型化 `On/Off/Once` | EventListenerRegistry 按函数实例配对 |
| `ArrayBuffer`/TypedArray | `byte[]`（拷贝语义） | `napi_get_typedarray_info`/`arraybuffer_info` |
| `bigint` / `Map<K,V>` | `JsBigInt`（long 语义）/ `JsMap<K,V>` 活视图 | int64 无损通道 / entries() 迭代协议 |
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
HarmonyOS.Bindings/          绑定库（net10.0, AOT/trim 友好；Api/ 438 个 @ohos.* 模块绑定）
  ├─ NativeNode/             ArkUI C API 互操作 + ArkUINodeBase + 事件总线
  ├─ Nodes/                  生成的组件包装类 + native-gaps.json
  ├─ Runtime/                napi 互操作（env 注入、INapiRecord、HiLog）
  └─ Hosting/Host.cs         libapp.so 导出入口
src/HarmonyOS.Maui/          MAUI Handler 包（Button/Label/StackLayout/ContentPage → ArkUI 节点）
samples/HarmonyHost/         鸿蒙宿主模板（ArkTS + C shim + CMake + ohosImports.ets 模块登记；targets 按应用 stage 到 obj/harmony/host）
samples/dotnet/HelloApp/     M1 控件 demo（XAML + NativeAOT → libapp.so）
samples/dotnet/ApiDemo/      M2 API 绑定 demo（模块验证/Promise→Task/TSFN；DEMO_APP=ApiDemo 切换）
                             两者的 Platforms/HarmonyOS/ 放平台启动代码（NativeExports 薄转发层，
                             对齐 MAUI Platforms/Android/MainActivity 惯例）；一键编排 targets
                             由 src/HarmonyOS.Maui/build/HarmonyOS.Maui.App.targets 提供
scripts/                     remote-build / stage-host / build-hap / deploy-hap 一键工具链（+ verify-m1-uitest 行为回归）
tests/                       jest（解析器/生成器 76 用例）
```

## 已知限制（当前真实状态）

- **布局语义**：ArkUI flex 托管（StackLayout→Column/Row）+ Grid/AbsoluteLayout MAUI 托管（HarmonyManagedLayoutHandler 绝对定位）。已对齐：WidthRequest/HeightRequest、Margin、StackLayout.Spacing、HorizontalOptions/VerticalOptions 对齐（flex 交叉轴经 NODE_ALIGN_SELF；Grid 单元格内 Start/Center/End 收缩偏移，Fill 充满）、ZIndex（两套布局均接通）、Auto 轨道随子内容变化自适应重排（AREA_CHANGE 驱动 + 幂等快照）；说明：Stack 主轴方向 Options 与 MAUI 官方一致（官方布局管理器即忽略）
- **画刷**：SolidColorBrush/LinearGradientBrush/RadialGradientBrush 全支持；ImageBrush 为 MAUI internal 类型无法声明（节点层 SetBackgroundImage 原语已就位）
- **导航**：根页用 `new NavigationPage(...)` 即可走 MAUI 标准 `Navigation.PushAsync/PopAsync`（HarmonyNavigationPageHandler 转接 IStackNavigation 协议，已实测）；另有轻量 Page 栈与系统返回键（优先级：模态 → NavigationPage 内栈 → 轻量栈）。**模态** `PushModalAsync/PopModalAsync` 标准可用（ArkStack 覆盖 + RootNavigationAdapter 转接）；**生命周期** Appearing/Disappearing 已透传（宿主建最小 Window/Application 逻辑链放行 MAUI 的 SendAppearing 守卫）；页面推入有 250ms 淡入、返回/模态关闭有 250ms 淡出（animateTo，完成后才摘除释放旧页）；NavigationPage 自带标题栏（返回键 + 页 Title，`HasNavigationBar=false` 隐藏）。未支持：Shell（多平台 Shell 应用请把入口改写为 NavigationPage 结构）
- **异步 API（M2 完成）**：`Promise<T>`→`Task<T>`；仅 callback 形式 API→`Task<T>`（CallbackTaskBridge，err-first）；`.NET event` 事件模型（真实触发已实测）；TSFN 生命周期三路径封装 + finalize 延迟释放防 UAF。**续体在 JS 线程内联恢复**（`NapiEnv` 线程亲和性），长耗时工作需自行 `Task.Run`
- **零分配调用路径**：`params ReadOnlySpan<object?>`（C# 13）、trampoline/argv 栈分配、生成 record 的 u8 名字常量缓存、`SetNumericAttribute(params ReadOnlySpan<...>)`（C# 14 first-class span conversions，属性写热路径）、HiLog 格式串 u8 缓存 + 运行时开关；剩余分配源：基元装箱（object 转换点）、字符串结果物化、事件适配器闭包
- **控件覆盖**：22 个 Handler（Button/Label/ContentPage/StackLayout/Grid/AbsoluteLayout + Entry/Editor/Switch/CheckBox/RadioButton/Slider/ProgressBar/Image/ScrollView/Frame/BoxView/RefreshView/Picker/DatePicker/TimePicker + CollectionView/CarouselView M1 物化版），代码风格已统一为官方 handler 模式；新控件适配指南见 [HANDLERS.md](HANDLERS.md)
- **手势识别**：TapGestureRecognizer/PanGestureRecognizer/PinchGestureRecognizer/SwipeGestureRecognizer/PointerGestureRecognizer 全支持（`HarmonyViewHandler` 基类统一挂载；Tap/Pinch 走 NDK 原生手势，Pan/Swipe/Pointer 走触摸流——pan 原生手势事件数据不可靠，实测沉淀；Tap/Pointer 经 AOT 安全的反射桥触发 internal SendTapped/SendPointer*）；PanUpdated 单位 vp（等价 iOS points）；未支持：Drag/Drop 识别器、鼠标 ButtonsMask 区分、hover 通道、Pinch 真机多点触控专项
- **仅模拟器（x86_64）验证**：真机 arm64 待验证（工具链已就绪）
- **napi handle scope 未系统化**：当前依赖宿主线程已有的 scope，规范做法待补
- **跨模块类型导入降级 IntPtr**：`@ohos.*` 模块间 `import type` 的类型（Want/NetAddress 等）不生成强类型（立项待做）；63 个含复杂缺口（TS 声明合并/深导入链）的模块保持灰度（`GRAYSCALE_MODULES`），清单见 `src/parser/index.ts`
- **基元装箱**：`object?` 参数转换点存在装箱；完全零装箱需要 union struct 参数设计（后续立项）

## 路线图

详细的后续路线、实现方案与难点分析见 **[ROADMAP.md](ROADMAP.md)**：
- M1 尾巴（完成）：~~Brush 助手~~、~~WidthRequest/HeightRequest~~、~~轻量导航~~、~~Grid/AbsoluteLayout（MAUI 托管布局）~~、~~布局遗留修复（Grid 对齐/ZIndex/Auto 重排）+ 返回动画 + NavigationPage 标题栏 + .NET 10 / C# 14 优化批次~~；剩真机验证
- M2（除 Essentials 外全部完成）：~~TSFN 异步层~~、~~codeGenerator 修复~~、~~@ohos.* 全量生成（438 模块/375 转正）~~、~~Promise→Task/AsyncCallback/.NET 事件/ArrayBuffer/Map~~、~~端到端模拟器验证~~、~~零分配调用路径~~；2.4 Essentials 平台实现未启动
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
