# ArkTsBinding / DotHarmony.Binding

一个**模仿 .NET MAUI 平台绑定层逻辑**（Mono.Android / Microsoft.iOS 的思路）的鸿蒙试验项目：
用 .NET (NativeAOT) 绑定 HarmonyOS (ArkUI/ArkTS)，并让 .NET MAUI 控件经 Handler 机制渲染为 ArkUI 原生节点。

**当前状态：M1（MAUI 基本面）与 M2（服务层）已完成，模拟器与 arm64 云真机均已端到端验证；M3 工程化核心已落地，最小 CI 门禁、性能基线脚本与 MAUI 权限自动推导已就绪** —— XAML 声明式 UI → 鸿蒙原生渲染、28 个具体 MAUI Handler（30 个工厂分派形态，含 Shell 第一版）、手势识别（Tap/Pan/Pinch/Swipe/Pointer/Drag&Drop）、Shape/GraphicsView 自绘（OH_Drawing）、438 个 @ohos.\* 模块绑定（全量编译，Promise→Task / .NET 事件 / ArrayBuffer/Map 封送全链路实测）。
距离可用于生产的绑定库还有明确距离，见文末已知限制与 [ROADMAP.md](ROADMAP.md)。

近期里程碑：**NDK 头文件生成器**（tools/arkui-bindgen，libclang 解析，枚举/结构体/函数表镜像全部由配置驱动生成，
漂移检查 `--check` 供 CI 门禁）与 **NuGet 打包**（四库 + 模板包，buildTransitive 宿主编排，应用工程无需仓库工作副本）已落地；
最小 CI 门禁与性能基线脚本已落地，详见 ROADMAP M3 与 `docs/PERF_BASELINE.md`。

**上手**：从零创建鸿蒙 MAUI 应用 / 给已有 MAUI 应用加鸿蒙平台，见 **[GETTING_STARTED.md](GETTING_STARTED.md)**。
**权限自动推导**：MAUI 侧调用会自动生成 `module.json5` 的 `requestPermissions` 和 `usedScene`，
并输出 `obj/harmony/permissions.report.md`；支持 ProjectReference 聚合、XAML 事件来源定位、
自定义映射与显式 `When` 覆盖，未映射或冲突场景会给出 `HMP001`–`HMP006` warning。
**平台服务**：适配一个新的 Essentials 服务（注入点取证/五步流程/坑表），见 **[ESSENTIALS.md](ESSENTIALS.md)**。
**风险预案**：C 原生节点 API 退出假设下的 ArkTS 引擎迁移计划见 **[MIGRATION_ARKTS_ENGINE.md](MIGRATION_ARKTS_ENGINE.md)**。

## 这是什么

本项目在逻辑上模仿 Mono.Android / Microsoft.iOS 的分层思路（平台绑定库 + Handler 适配 + 工具链），
**并非其量级的实现**——那两者背后是微软与三星的专职团队。本项目的对应物：

| 官方生态 | 本项目 |
|---|---|
| Java.Interop（通用 JNI 互操作核心） | `HarmonyOS.Interop`（napi 互操作核心独立装：env/封送/回调/TSFN/hilog） |
| Mono.Android（Java API 绑定 + Android.Runtime 胶水） | `HarmonyOS.Bindings`（ArkUI C API 节点 + @ohos.* 绑定，生成器产出） |
| Microsoft.Maui.Essentials | `HarmonyOS.Essentials`（MAUI Essentials 鸿蒙实现独立装，22 服务） |
| 各平台 Handler（Android/iOS/...） | `HarmonyOS.Maui`（MAUI 控件 → ArkUI 原生节点，不 fork dotnet/maui） |
| Android/iOS head 工程 | `HarmonyHost`（ArkTS 宿主 + C shim 模板；targets 按应用生成实例） |
| workload / msbuild 集成 | `scripts/`（PublishAotClang（Windows 本地 NativeAOT）+ hvigor + hdc 一键脚本）

```
[MAUI 应用] XAML / C# 控件树（Microsoft.Maui.Controls VirtualView）
    ↓
[HarmonyOS.Maui]  HarmonyButtonHandler / HarmonyLabelHandler / HarmonyLayoutHandler /
                  HarmonyContentPageHandler（ViewHandler<,> + PropertyMapper/CommandMapper 协议）
    ↓ PlatformView
[HarmonyOS.Bindings]
    ├─ Nodes/       ArkUINodeBase : { ArkUI_NodeHandle } 包装类（解析器自动生成）
    ├─ NativeNode/  ArkUI NDK C API 互操作（ArkUI_NativeNodeAPI_1 函数表镜像、事件总线）
    └─ Hosting/     Host 入口（libapp.so 导出 HarmonyInit / HarmonyBuildUI）
[HarmonyOS.Interop] napi 互操作核心（独立装，对齐 Java.Interop 分层；env 注入、值封送、回调/TSFN、hilog 桥）
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
   Windows 侧通过 PublishAotClang + Zig 直接交叉编译，不再依赖 WSL 或远程 Linux。
4. **入口程序集导出**：ILC 只导出入口程序集内的 `[UnmanagedCallersOnly]`，
   因此每个 libapp.so 应用需要薄转发层（见 HelloApp 的 `NativeExports`）。

## 快速开始

环境：Node.js + npm（解析器）、Python 3（头文件提取）、.NET 10 SDK（绑定库）、
DevEco Studio（内置 HarmonyOS SDK/NDK/hvigor）、PublishAotClang（Windows 本地 NativeAOT）。

```bash
# 1. 解析器构建 + 测试（79 用例）
npm install && npm test

# 2. 从 NDK 头文件生成 C# 枚举（ArkUINodeTypes.g.cs + .json 元数据；SDK 探测顺序 --sdk → OHOS_SDK_BASE → OHSDK_HOME → 默认路径，找不到报错退出）
#    （生成器：tools/arkui-bindgen；首次运行需 python -m pip install -r tools/arkui-bindgen/requirements.txt）
$env:PYTHONPATH = "tools/arkui-bindgen"
python -m arkui_bindgen gen --config tools/arkui-bindgen/config/semantics.yaml --out-dir src/HarmonyOS.Bindings/NativeNode
#    漂移检查（本地门禁，需本机 OpenHarmony SDK）：python -m arkui_bindgen check （同参数）

# 3. 从 SDK 组件 .d.ts 生成 NodeHandle 包装类 → src/HarmonyOS.Bindings/Nodes/
npx ts-node tools/api-generator/index.ts --native

# 4. Windows 上构建绑定库 + 样例
dotnet build ArkTsBinding.slnx

# 5. 交叉编译 libapp.so（PublishAotClang，双架构）
dotnet publish samples/dotnet/HelloApp -c Release -r linux-musl-arm64
dotnet publish samples/dotnet/HelloApp -c Release -r linux-musl-x64
# （或用 MAUI 风格一键：dotnet build samples/dotnet/HelloApp -t:HarmonyRun —— 自动 stage 宿主 → AOT → HAP → 部署）

# 6. 打 HAP（hvigor；DevEco 路径自动探测，或用 DEVECO_HOME 指定）
cmd //c scripts\build-hap.cmd

# 7. 部署到模拟器/真机并抓取日志（hdc 自动探测；PowerShell 版为 deploy-hap.ps1）
bash scripts/deploy-hap.sh
```

> 云调试/真机安装注意：`normal` 应用只会声明 `normal` 等级权限；生成器会按 SDK 的
> `PermissionDefinitions.json` 自动过滤。调试设备请使用 **debug profile**，release profile
> 只用于发布渠道，直接安装到云调试手机通常会失败。

## 脚本工具链（scripts/）

| 脚本 | 用途 | 说明 |
|---|---|---|
| `stage-host.ps1` | 宿主工程生成：模板 → 按应用实例（重写 bundleName/应用名） | 由 targets 的 HarmonyStageHost 调用（内容戳增量）；`HarmonyGenerateHost=false` 可跳过 |
| `patch-openharmony-nativeaot.ps1` | 修补 arm64 NativeAOT runtime 的 NUMA 探测调用 | 由 `HarmonyBuildLibApp` 自动执行；避免云真机 seccomp 拦截 `get_mempolicy` |
| `build-hap.cmd` | hvigor 打 HAP | DevEco Studio 路径自动探测，`DEVECO_HOME` 可覆盖；`HOST_DIR` 指向按应用暂存宿主（targets 自动设置） |
| `build-hap.cmd <样例名>` | hvigor 打指定样例的 HAP | 支持 `WeatherTwentyOne`、样例目录或 staged host 目录；省略参数时仍默认 `samples/HarmonyHost` |
| `sign-hap.ps1` | 用 OpenHarmony `hap-sign-tool.jar` 签名 unsigned HAP | 由 `HarmonyBuildHap` 在提供签名属性时自动调用；绕开 hvigor 对明文口令的 32 字符校验 |
| `deploy-hap.sh` / `deploy-hap.ps1` | 重装 HAP → 启动 → 抓取 HarmonyHost 日志 | hdc 自动探测；无设备 / 缺 HAP / 安装失败即报错停止；启动前 `aa force-stop` 防 install 竞争 |
| `gen-module-sample.ts` | @ohos.* 模块绑定样例生成 | napi 路线（ROADMAP 2.3） |

环境变量（全部可选，脚本内置默认探测链）：

| 变量 | 作用 | 探测顺序 |
|---|---|---|
| `OHOS_SDK_BASE` | OpenHarmony SDK 根目录（含 `26.0.0/toolchains`） | → `OHSDK_HOME` → `D:\Harmony\OpenHarmony\Sdk` → DevEco 内置 sdk |
| `DEVECO_HOME` | DevEco Studio 安装目录 | → `D:\Program Files\Huawei\DevEco Studio` → C 盘同名 |
| `HOST_DIR` | 宿主目录覆盖（三脚本通用；targets 生成模式自动指向 `obj/harmony/host`） | `samples/HarmonyHost` |

> 约定：`.gitattributes` 强制 `*.sh` 为 LF、`*.cmd/*.ps1` 为 CRLF；新增脚本请沿用"路径自动探测 + 前置检查失败即停"的风格。

## NuGet 包与模板

五个包经 `dotnet pack -c Release -o dist` 产出（版本统一由根 `Directory.Build.props` 的 `PackageVersion` 供给）：

| 包 | 内容 |
|---|---|
| `HarmonyOS.Interop` | napi 互操作核心（env 注入/封送/TSFN/HiLog） |
| `HarmonyOS.Bindings` | ArkUI/@ohos.* 绑定（依赖 Interop） |
| `HarmonyOS.Essentials` | Essentials 鸿蒙实现（依赖 Bindings） |
| `HarmonyOS.Maui` | MAUI 渲染层 + **buildTransitive 宿主编排**（targets + scripts + 宿主模板随包分发；应用工程无需仓库工作副本） |
| `HarmonyOS.Templates` | `dotnet new harmony-maui` 应用模板（含 `Platforms/HarmonyOS` 启动桩） |

消费方体验对齐 maui-android 单项目：`<PackageReference Include="HarmonyOS.Maui" Version="..." />` 后
`dotnet build -t:HarmonyStageHost / -t:HarmonyRun` 即可（targets 由包内 `buildTransitive/HarmonyOS.Maui.targets` 自动导入）。
签名属性簇（release 用）：`HarmonySigningKeystore/KeystorePassword/CertAlias/CertPassword/CertPath/Profile`，
缺项时 stage 会给出清晰报错。提供完整签名属性时，构建先生成 unsigned HAP，再由
`sign-hap.ps1` 输出 `entry-default-signed.hap`；全不配置则只产出 unsigned HAP。

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

- **Windows 本地 NativeAOT 交叉编译**：由 [PublishAotClang](https://github.com/xljiulang/PublishAotClang) 提供
  Zig / Clang / objcopy 工具链，`linux-musl-arm64` 与 `linux-musl-x64` 均可在 Windows 直接发布；
  该包会过滤 `-Wl,--gc-sections` 等与 ILC 不兼容的链接参数，避免 `__modules` 被误收割。
- **符号分离**：由 `PublishAotClang` 内置的 objcopy 统一处理，不再需要手写 wrapper。
- **引导参数**：shim 在 dlopen 前 setenv `DOTNET_GCHeapHardLimit`（默认 256G 虚拟预留超限）与 `ICU_DATA`。
- **OpenHarmony 云真机 arm64 兼容补丁**：官方 NativeAOT runtime 的 NUMA 探测会在启动时调用
  `get_mempolicy`（arm64 syscall 236），部分云真机的 seccomp 策略会拦截该 syscall 并导致进程被杀。
  `scripts/patch-openharmony-nativeaot.ps1` 会在 `HarmonyBuildLibApp` 中自动对 arm64 `app.so`
  做等价的编译期规避：把该探测调用短路为 `ENOSYS`。脚本是幂等的，并按指令特征定位而非固定偏移；
  升级 .NET 后如果 runtime 指令布局变化，脚本会显式失败，需要更新签名。
- **调试**：C# 经 `HiLog` 直写 hilog（含托管堆栈）；`hilog -x | grep HarmonyHost`。

## 项目结构

```
tools/api-generator/         解析器（TS Compiler API）
  ├─ astParser.ts            AST → 组件/API 中间模型
  ├─ nativeCodeGenerator.ts  C API 目标生成器（shape 表 + gap 登记）
  ├─ codeGenerator.ts        napi 目标生成器（@ohos.* 服务层）
  └─ typeMapper.ts           类型映射
tools/arkui-bindgen/           NDK 头文件 → C# 生成器（libclang；枚举/结构体/函数表镜像，semantics.yaml 语义配置）
src/HarmonyOS.Interop/       napi 互操作核心（独立装：env 注入、值封送、回调/TSFN、HiLog）
src/HarmonyOS.Bindings/      绑定库（net10.0, AOT/trim 友好；Api/ 438 个 @ohos.* 模块绑定）
  ├─ NativeNode/             ArkUI C API 互操作 + ArkUINodeBase + 事件总线
  ├─ Nodes/                  生成的组件包装类 + native-gaps.json
  └─ Hosting/Host.cs         libapp.so 导出入口
src/HarmonyOS.Essentials/    MAUI Essentials 鸿蒙实现独立装（DeviceInfo/剪贴板/Preferences/五类传感器/定位/选图等 22 服务）
src/HarmonyOS.Maui/          MAUI Handler 包（Button/Label/StackLayout/ContentPage → ArkUI 节点）
samples/HarmonyHost/         鸿蒙宿主模板（ArkTS + C shim + CMake + ohosImports.ets 模块登记；targets 按应用 stage 到 obj/harmony/host）
samples/dotnet/HelloApp/     M1 控件 demo（XAML + NativeAOT → libapp.so）
samples/dotnet/ApiDemo/      M2 API 绑定 demo（模块验证/Promise→Task/TSFN；DEMO_APP=ApiDemo 切换）
samples/dotnet/EssentialsApp/ M2.4 Essentials 验证（22 服务：信息栏 + 剪贴板授权回环 + Preferences 持久化 + Battery/Vibration/Connectivity + SecureStorage 回环 + Browser/Share/Email + 传感器/定位/选图）
                             两者的 Platforms/HarmonyOS/ 放平台启动代码（NativeExports 薄转发层，
                             对齐 MAUI Platforms/Android/MainActivity 惯例）；一键编排 targets
                             由 src/HarmonyOS.Maui/build/HarmonyOS.Maui.App.targets 提供
scripts/                     stage-host / build-hap / deploy-hap 一键工具链（+ verify-*-uitest 行为回归）
tests/                       jest（解析器/生成器 79 用例）
```

## 已知限制（当前真实状态）

- **布局语义**：ArkUI flex 托管（StackLayout→Column/Row）+ Grid/AbsoluteLayout MAUI 托管（HarmonyManagedLayoutHandler 绝对定位）。已对齐：WidthRequest/HeightRequest、Margin、StackLayout.Spacing、HorizontalOptions/VerticalOptions 对齐（flex 交叉轴经 NODE_ALIGN_SELF；Grid 单元格内 Start/Center/End 收缩偏移，Fill 充满）、ZIndex（两套布局均接通）、Auto 轨道随子内容变化自适应重排（AREA_CHANGE 驱动 + 幂等快照）；说明：Stack 主轴方向 Options 与 MAUI 官方一致（官方布局管理器即忽略）
- **画刷**：SolidColorBrush/LinearGradientBrush/RadialGradientBrush 全支持；ImageBrush 为 MAUI internal 类型无法声明（节点层 SetBackgroundImage 原语已就位）
- **导航**：根页用 `new NavigationPage(...)` 即可走 MAUI 标准 `Navigation.PushAsync/PopAsync`（HarmonyNavigationPageHandler 转接 IStackNavigation 协议，已实测）；另有轻量 Page 栈与系统返回键（优先级：模态 → NavigationPage 内栈 → 轻量栈）。**模态** `PushModalAsync/PopModalAsync` 标准可用（ArkStack 覆盖 + RootNavigationAdapter 转接）；**生命周期** Appearing/Disappearing 已透传（宿主建最小 Window/Application 逻辑链放行 MAUI 的 SendAppearing 守卫）；页面推入有 250ms 淡入、返回/模态关闭有 250ms 淡出（animateTo，完成后才摘除释放旧页）；NavigationPage 自带标题栏（返回键 + 页 Title，`HasNavigationBar=false` 隐藏）。**Shell 第一版已支持**（HarmonyShellHandler + HarmonyShellNavigation 自持协议）：TabBar 条目切换、绝对/相对路由与 `..` 返回、注册路由推送、query parameters（IQueryAttributable + QueryProperty）、页内 PushAsync 入 section 栈、模态转发根栈、Appearing/Disappearing 透传。未覆盖：Flyout 菜单视觉、Shell 主题色（当前走静态兜底）
- **异步 API（M2 完成）**：`Promise<T>`→`Task<T>`；仅 callback 形式 API→`Task<T>`（CallbackTaskBridge，err-first）；`.NET event` 事件模型（真实触发已实测）；TSFN 生命周期三路径封装 + finalize 延迟释放防 UAF。**续体在 JS 线程内联恢复**（`NapiEnv` 线程亲和性），长耗时工作需自行 `Task.Run`
- **零分配调用路径**：`params ReadOnlySpan<object?>`（C# 13）、trampoline/argv 栈分配、生成 record 的 u8 名字常量缓存、`SetNumericAttribute(params ReadOnlySpan<...>)`（C# 14 first-class span conversions，属性写热路径）、HiLog 格式串 u8 缓存 + 运行时开关；剩余分配源：基元装箱（object 转换点）、字符串结果物化、事件适配器闭包
- **控件覆盖**：28 个具体 Handler / 30 个工厂分派形态（Button/Label/ContentPage/StackLayout/Grid/AbsoluteLayout + Entry/Editor/Switch/CheckBox/RadioButton/Slider/ProgressBar/Image/ScrollView/Frame/Border(BoxView)/RefreshView/Picker/DatePicker/TimePicker + CollectionView（NodeAdapter 虚拟化）/CarouselView + Shape/GraphicsView 自绘 + Shell 第一版），代码风格已统一为官方 handler 模式；新控件适配指南见 [HANDLERS.md](HANDLERS.md)
- **控件完成度矩阵**：

  | 能力 | 状态 | 说明 |
  | --- | --- | --- |
  | ContentPage / Grid / StackLayout / AbsoluteLayout | 已支持 | 基础布局与对齐已接通 |
  | Label / Button / BoxView / Frame(Border) / Image | 已支持 | 常规属性已接通 |
  | Entry / Editor / Switch / CheckBox / Slider / ProgressBar | 已支持 | 交互与值映射已接通 |
  | ScrollView / CollectionView / CarouselView | 已支持 | 横竖滚动与虚拟化已接通 |
  | Picker / DatePicker / TimePicker / RefreshView | 已支持 | 基础展示已接通 |
  | Shape / GraphicsView | 部分支持 | 文本、渐变、位图与测量路径仍在补 |
  | Button 字体族 | 已支持 | 通过 `NODE_FONT_FAMILY` 和子 Text 双写 |
  | RadioButton 文本 | 已支持 | 以 Radio + Text 组合承载 |
  | RefreshView 颜色 | 部分支持 | 原生侧暂无进度球颜色属性 |
  | FontImageSource / DrawingCanvas 高级能力 | 部分支持 | 资源与绘制通道仍未完整对齐 |
  | Shell | 已支持（第一版） | TabBar/路由/模态/query 已接通；Flyout 视觉与主题后续子阶段 |
- **手势识别**：TapGestureRecognizer/PanGestureRecognizer/PinchGestureRecognizer/SwipeGestureRecognizer/PointerGestureRecognizer 全支持（`HarmonyViewHandler` 基类统一挂载；Tap/Pinch 走 NDK 原生手势，Pan/Swipe/Pointer 走触摸流——pan 原生手势事件数据不可靠，实测沉淀；Tap/Pointer 经 AOT 安全的反射桥触发 internal SendTapped/SendPointer*）；Drag/Drop 识别器（长按起拖 + UDMF 载荷，DRAG_END 销毁）已支持；PanUpdated 单位 vp（等价 iOS points）；未支持：鼠标 ButtonsMask 区分、hover 通道、Pinch 真机多点触控专项
- **模拟器与云真机验证**：x86_64 模拟器与 arm64 云真机均已通过端到端启动验证；
  arm64 云真机需要上述 NativeAOT NUMA 探测补丁，当前由工具链自动完成。
- **手势注入**：Metro Hub 场景此前现象为"注入/触摸滑动点击均无响应"，今已确认为 ContentPresenter 空槽命中测试黑洞所致（非模拟器注入限制），随 ContentPresenter/ContentView 宿主修复一并解决（`verify-zindex-probe.ps1` 的 `uitest uiInput click` 逐步走查在模拟器实测可用）；剩余限制：Pinch 多点触控注入通道，真机验收仍以实际触摸为准
- **napi handle scope 未系统化**：当前依赖宿主线程已有的 scope，规范做法待补
- **跨模块类型导入降级 IntPtr**：`@ohos.*` 模块间 `import type` 的类型（Want/NetAddress 等）不生成强类型（立项待做）；438/438 模块已全量转正编译，无灰度清单
- **基元装箱**：`object?` 参数转换点存在装箱；完全零装箱需要 union struct 参数设计（后续立项）

## 路线图

详细的后续路线、实现方案与难点分析见 **[ROADMAP.md](ROADMAP.md)**：
- M1（完成）：MAUI 基本面——布局对齐、导航（NavigationPage 转接）、手势识别、CollectionView 虚拟化、 .NET 10 / C# 14 优化批次；剩真机验证
- M2（完成）：服务层——TSFN 异步层、@ohos.\* 全量生成（438/438）、Promise→Task/事件/ArrayBuffer/Map 封送、Essentials 22 服务、端到端模拟器验证
- M3（核心完成，收尾中）：装配分层、NDK 头文件生成器（`tools/arkui-bindgen`，镜像已全部切换为生成式）、NuGet 打包与 `dotnet new harmony-maui` 模板已完成；最小 CI 门禁、性能基线脚本与 MAUI 权限自动推导已就绪

## 致谢 / Acknowledgements

本项目的交叉编译方案与运行时移植实践，建立在以下开源工作的基础上：
- **[PublishAotClang](https://github.com/xljiulang/PublishAotClang)**（xljiulang）——
  当前 Windows 本地 NativeAOT 交叉编译使用的 Zig/Clang 工具链包装，本项目已直接引用其 NuGet 包。
- **[PublishAotCross](https://github.com/MichalStrehovsky/PublishAotCross)**（Michal Strehovsky）——
  用 zig cc 作为 NativeAOT 自定义链接驱动以实现 linux-musl 交叉编译的开创性方案。
  `PublishAotClang` 也源自该项目；早期版本中 x64 链接驱动曾是其思路的手写实现。
- **[musl.cc](https://musl.cc/)** —— 历史上用于 arm64 构建的交叉工具链；当前主链路已切换为
  `PublishAotClang`，保留致谢与迁移记录。
- **[OpenHarmony.Avalonia](https://github.com/CeSun/OpenHarmony.Avalonia)**（CeSun）——
  .NET 运行时鸿蒙移植的先行实践，本项目采用的 GC 堆上限与 ICU 引导参数配方源自其公开的移植记录。
- **[OpenHarmony-NET/runtime](https://github.com/OpenHarmony-NET/runtime)** ——
  其 OpenHarmony 目标移植明确指出了 `NUMASupport` 对 `get_mempolicy` 的依赖，并在
  [60c85dca](https://github.com/OpenHarmony-NET/runtime/commit/60c85dca771c686d81d5c1802ae8abbbbc522acc)
  与 [435caa71](https://github.com/OpenHarmony-NET/runtime/commit/435caa71f6ea4ee5e7d13c9611d32730a4d4dcfa)
  中通过 `TARGET_OPENHARMONY` 在编译期跳过该路径。本项目的后链接二进制补丁采用了同一规避思路，
  使官方 NativeAOT runtime 产物也能在云真机上启动。
- **OpenHarmony / HarmonyOS** —— ArkUI NDK（ArkUI_NativeNodeAPI_1）与 Node-API 的官方能力支撑。
