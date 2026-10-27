# DotHarmony.Binding 全量 Code Review 指南

> 更新基线：2026-10-25。本文是**评审执行指南**，不是已经完成的全量审计报告。
> 项目仍在演进，模块数量、覆盖率和已知限制应以当前代码、`README.md` 与 `ROADMAP.md` 为准。

这是一个跨平台基础架构项目：在 .NET 10 NativeAOT 上同时接入 ArkUI NDK、`@ohos.*` 系统模块、MAUI Handler 与 ArkTS 宿主。全量 Review 不能只按单文件或普通 C# 业务项目检查，而应结合**分层审查、端到端追踪、生成物漂移检查和真机验证**。

## 第一阶段：先建立正确的架构模型

### 1. 两条主要互操作通道，不要混为一条链路

当前项目不是所有调用都经过 `Essentials -> NAPI -> Api Binding`。UI 与系统服务使用不同的原生通道：

```text
MAUI 应用（XAML / C# / MVVM）
├── HarmonyOS.Maui：Handler / 布局 / 导航 / 手势
│   └── HarmonyOS.Bindings：NativeNode / Nodes
│       └── ArkUI NDK C API（libace_ndk.z.so，函数表镜像）
│
├── HarmonyOS.Essentials：设备能力适配
│   └── HarmonyOS.Bindings：Api（生成的 @ohos.* 绑定）
│       └── HarmonyOS.Interop：Node-API / NAPI
│           └── ArkTS 系统模块
│
└── HarmonyOS.Maui.Web / Blazor（实验）
    └── NDK + NAPI + ArkWeb/ArkTS IPC 混合链路

samples/HarmonyHost（共享宿主模板）
└── 按应用 stage 后加载 NativeAOT libapp.so
```

评审时应分别关注：

- **NDK/C ABI 通道**：函数表 ABI、结构体布局、原生指针生命周期、事件接收器、UAF。
- **NAPI 通道**：`napi_env` / `napi_value` / `napi_ref`、handle scope、TSFN、异常清理、取消和线程模型。
- **上层 MAUI 语义**：Handler 映射、控件默认值、布局与导航、生命周期和 Essentials 契约。
- **宿主与交付链路**：NativeAOT 导出、宿主 staging、权限生成、HAP 构建签名和部署。

### 2. 以仓库中的维护者契约为审查基线

开始模块审查前，至少阅读：

- `README.md`：当前架构、样例矩阵、构建方式和已知限制。
- `docs/interop-rules.md`：NAPI 生命周期、异步取消、事件注册与节点事件不变量。
- `ROADMAP.md`：实测结论、平台铁律、工程化现状和遗留风险。
- `HANDLERS.md` / `ESSENTIALS.md`：Handler 与 Essentials 的扩展约定。
- `docs/PERMISSIONS.md` / `docs/PERF_BASELINE.md`：权限推导和性能验证口径。

不要把文档中的“已修复”“已覆盖”直接当成 Review 结论；每项结论都应能回到具体代码、测试或设备证据。

## 第二阶段：按模块分块审查

### 1. `src/HarmonyOS.Interop`：NAPI 互操作核心

重点检查：

- `NapiReference` 是否绑定所属 environment、恰好释放一次；跨线程释放是否经过 `NapiFinalizationQueue`。
- 所有 `napi_*` 调用是否检查 `napi_status`，失败时是否保留原始状态并尽可能带出 JS 错误文本。
- handle scope 是否统一使用 `NapiHandleScope`；裸 `napi_value` 是否被错误地带出 scope 或存入异常。
- Promise/callback 桥在完成、取消、异常和重复回调下，是否幂等释放 `GCHandle` 与 `CancellationTokenRegistration`。
- 已取消 token、注册期间同步回调等竞态是否会漏释放或留下永不完成的 Task。
- 原生回调边界是否可能泄漏托管异常；TSFN release 失败后句柄是否立即作废。
- 字符串、ArrayBuffer、TypedArray、Map、bigint 等封送是否有额外分配、截断或所有权错误。

线程模型不能按通用 .NET 经验误判：当前 Promise/callback 续体有意在 JS 线程内联恢复。应检查同步 getter 是否阻塞等待 Promise、长耗时续体是否应 `Task.Run`，而不是机械地要求所有 TCS 使用同一种续体选项。

已有机械闸：`tests/dotnet/HarmonyGestureTests/InteropInvariantTests.cs`。它只能证明部分源码不变量，不能替代泄漏、竞态和设备压力测试。

### 2. `src/HarmonyOS.Bindings`：NDK 与生成式 API 绑定

#### 2.1 `NativeNode` 与 `Nodes`

- 枚举、结构体、calling convention、函数表顺序是否与目标 NDK 头文件一致，是否发生 ABI 漂移。
- 新增 NDK 函数是否只追加到函数表配置末尾，避免后续 P/Invoke 顺序错位。
- 节点、事件参数、手势 recognizer、UDMF 数据等原生对象是否存在泄漏、重复释放或 UAF。
- recognizer 是否在事件分发回调内被 dispose；拖拽数据是否持有到 `NODE_ON_DRAG_END`。
- 节点事件是否为多播；首个订阅/最后退订与原生注册/注销是否一致，失败时是否正确回滚。
- 属性热路径是否引入不必要的数组、字符串或委托分配。

#### 2.2 `Api`

- `.d.ts` 到 C# 的类型映射、重载、枚举宽度、可选值和 NAPI `null` 转换是否正确。
- 值类型 optional 可映射为 `Nullable<T>`；引用类型应结合可空引用类型与实际运行时语义判断，不能一概使用 `Nullable<T>`。
- `Promise<T>`、异步 callback 与 `on/off/once` 是否正确映射为 Task 和 .NET 事件，异常与取消语义是否完整。
- `IntPtr` 降级项是否已记录并有补映射计划；复杂 options bag 不应仅凭“能编译”判定正确。
- 生成文件是否具有 SDK/生成器溯源、正确 using，且生成结果可重复、可审查。

不要把“438 个模块已生成并编译”等覆盖数字等同于运行时行为已验证。

### 3. `src/HarmonyOS.Maui`：Handler、布局与生命周期

- 以 `ViewHandler<,>`、`PropertyMapper` / `CommandMapper` 为基线，不再泛化为旧式 Renderer 或统一的 `Component/XComponent` 映射。
- 检查 `ConnectHandler` / `DisconnectHandler` 是否精确配对退订；共享节点时不得用无参退订清空其他 Handler 的监听。
- Handler 层是否遵守漏斗纪律，不直接调用函数表、NAPI 或原始结构体。
- 核对 MAUI 与 ArkUI 默认值、测量、布局、颜色/画刷、字体、命中测试和手势仲裁语义。
- 对 NavigationPage、模态、Shell、Window/Ability 生命周期分别验证，不以单页 Button 冒烟代表全部完成。
- NativeAOT 下检查反射收根、`[DynamicDependency]` 与入口程序集 `[UnmanagedCallersOnly]` 导出是否完整。

控件不能只看“有 Handler”：例如 Button 当前是 `Flex + Text` 组合，并同时有 `NODE_ON_CLICK` 和触摸兜底路径，需要检查重复点击、取消、Scroll 手势竞争与断连。

### 4. `src/HarmonyOS.Essentials`：平台服务契约

- 按服务逐项对照 MAUI 接口语义、异常类型、null/空值、权限拒绝、功能未启用和取消行为。
- 重点回归 `IPreferences.Set(key, null)` 删除键，以及 Geolocation 在定位关闭和权限拒绝时的契约。
- 检查事件订阅/退订、重复事件去重、后台线程回调与 UI 线程投递。
- 传感器、Geolocation、MediaPicker 等不能只用桌面契约测试判定完成，应补模拟器/真机矩阵。
- 未实现服务（以当前 `README.md` 清单为准）应明确报告为覆盖缺口，而不是用“100% 对齐”掩盖。

### 5. 权限生成器：`HarmonyOS.Maui.Permissions*`

- 增量生成是否具备确定性、缓存正确性和多 ProjectReference 聚合能力。
- XAML 事件、显式 `HarmonyPermission`、自定义映射和 `override` 冲突是否处理正确。
- `HMP001`–`HMP006` 是否覆盖未映射、缺映射、未使用、歧义、冲突和格式错误。
- 推导来源是否能追溯到文件和行号，生成清单与实际 `module.json5` 是否一致。
- 权限自动推导不能替代运行时申请、拒绝和撤销场景测试。

### 6. `HarmonyOS.Maui.Web` / `HarmonyOS.Maui.Blazor`

这是实验面，应单列审查，不能只归入普通 Handler：

- Web controller 与 Web 组件关联时序、多实例 webTag 路由和 TSFN 投递是否正确。
- 节点绑定晚于 Handler attach 时是否能可靠重试；混排布局、尺寸同步和点击是否相互干扰。
- JS Interop、URL loading、静态资源流、InputFile 和原生历史导航是否有字节完整性与生命周期保障。
- 回调中的普通 C 字符串与 `napi_value` 是否严格区分。
- Blazor 公开 API 形状、Dispatcher/作用域服务、首帧时序及异常路径是否符合预期。
- 不把 Hot Reload 缺失当作普通缺陷：NativeAOT 架构本身不支持基于 IL MetadataUpdate 的热重载。

### 7. 工具链、宿主与交付

审查范围应包括 `tools/`、`scripts/`、`src/HarmonyOS.Maui/build/`、`samples/HarmonyHost/` 和 `HarmonyOS.Templates/`，而不仅是“Scripts”。

- `tools/api-generator`：`.d.ts` 解析、类型映射、生成确定性和 Jest fixtures。
- `tools/arkui-bindgen`：libclang 解析、`semantics.yaml`、快照与 `check` 漂移门禁。
- `tools/HandlerAudit`：覆盖矩阵、工厂分派拓扑和自测试。
- 宿主编排：stage 是否幂等、按应用隔离，bundleName、SDK、权限和签名属性是否正确注入。
- `samples/HarmonyHost` 是共享宿主模板，不应直接部署；部署应走具体应用的 `HarmonyRun`，或使用该应用 `obj/harmony/host` 暂存目录。
- NativeAOT：arm64/x64 构建、`--gc-sections` 兼容、入口导出符号、arm64 NUMA/seccomp 补丁幂等与 `-CheckOnly`。
- NuGet 与模板：`buildTransitive` 是否让仓库外消费者获得完整脚本、宿主和 targets，双 TFM 是否都能构建。
- 当前生成器只生成 C# 绑定与元数据，宿主 C/CMake 仍手写维护；不要再按“生成 C/C++ 宿主代码”审查。

## 第三阶段：做端到端追踪，而不是只看模块内部

至少选择以下三条链路：

### 1. Button 点击：NDK 原生事件上行

```text
ArkUI NODE_ON_CLICK / NODE_TOUCH_EVENT
  -> NodeEventBus / ArkUINodeBase 多播事件 hub
  -> HarmonyButtonHandler.OnClick / 触摸兜底
  -> MAUI Button.SendClicked()
  -> 应用事件或 Command
```

检查正常点击与兜底是否重复触发、Cancel 是否误触发、精确退订是否完整，以及 Scroll 手势竞争和点击热路径分配。

### 2. Essentials 服务：NAPI 系统调用

以 DeviceInfo、Preferences 或 Accelerometer 为例：

```text
MAUI Essentials 静态入口
  -> HarmonyEssentials.Install 注入的平台实现
  -> HarmonyOS.Bindings.Api
  -> NapiModuleLoader / NodeApi / Promise 或 callback 桥
  -> @ohos.* 系统模块
```

同时追踪宿主 `ohosImports.ets` 登记、权限推导、异常映射和真机结果。不要从 Button 追踪到 Essentials，两者不是串行关系。

### 3. Web/Blazor：混合链路

从 MAUI 布局容器追踪到 ArkTS `BuilderNode`、Web 节点绑定、IPC/JS Interop 和静态资源响应，覆盖多实例与断连销毁。

## 第四阶段：验证与自动化门禁

### 当前仓库已有验证入口

在仓库根目录执行：

```powershell
dotnet build ArkTsBinding.slnx
npx jest --runInBand
python -m pytest tools/arkui-bindgen/tests -q

dotnet test tests/dotnet/HarmonyGestureTests/HarmonyGestureTests.csproj
dotnet test tests/dotnet/HarmonyEngineTests/HarmonyEngineTests.csproj
dotnet test tests/dotnet/HarmonyPermissionGeneratorTests/HarmonyPermissionGeneratorTests.csproj

dotnet build samples/dotnet/EssentialsApp/EssentialsApp.csproj -t:HarmonyPermissionCheck -c Release
dotnet run --project tools/HandlerAudit --selftest
dotnet run --project tools/HandlerAudit
```

有本机 OpenHarmony SDK 头文件时，额外执行：

```powershell
$env:PYTHONPATH = "tools/arkui-bindgen"
python -m arkui_bindgen check
```

设备侧构建与冒烟：

```powershell
dotnet build samples/dotnet/HelloApp -t:HarmonyRun
./scripts/perf/collect-baseline.ps1
```

当前 CI 已覆盖生成器单测、最小 arm64 NativeAOT 构建、双 TFM Bindings 构建、运行时补丁、部署/stage 脚本、权限报告与 HandlerAudit；但全量 xUnit、`arkui_bindgen check` 和 HAP 全链冒烟并非完整阻塞门禁。Review 报告必须区分：

1. **静态证据**：代码路径、机械扫描、编译或生成物 diff。
2. **桌面测试证据**：xUnit/Jest/pytest 的行为或契约验证。
3. **设备证据**：模拟器/真机 HAP、交互、权限、生命周期和长时间压力结果。

界面结构检查优先使用 `uitest dumpLayout`，不能用截图推断布局。布局 JSON、日志和临时产物统一放仓库 `tmp/` 并在任务结束后清理。

### 编译严格性与内存/性能验证

- `Nullable` 已在主要项目中广泛启用，不应再描述为“全仓尚未开启”；根 `Directory.Build.props` 当前主要集中管理 LangVersion、TFM 和包元数据。
- 全仓尚未统一配置 `TreatWarningsAsErrors`、`WarningsAsErrors` 或 `AnalysisLevel`。建议按项目分阶段引入，先处理生成代码、双 TFM 和现有 `NoWarn`，避免一次性升级掩盖真实告警。
- 不把 `dotnet-gcdump` 当作 NativeAOT 互操作的通用单测方案。托管桥应结合分配/句柄计数与压力回归；原生 C shim 或原生库在可用环境中使用 ASan；设备侧结合 hilog、启动/首帧/TSFN 基线和长时间事件测试。
- 性能基线当前只记录、不告警；若要把性能回归纳入门禁，需先定义设备矩阵、阈值和波动容忍度。

## Review 输出要求

每个问题至少记录：

- **严重级别**：Blocker / Critical / Major / Minor / Suggestion。
- **位置与证据**：文件、符号、代码路径，必要时附生成物或日志。
- **影响**：崩溃、UAF、泄漏、死锁、ABI 不兼容、MAUI 语义偏差、构建/部署失败或性能回退。
- **触发条件与复现**：包含线程时序、SDK/API 版本、TFM、架构和设备环境。
- **修复建议与验证**：对应单测、机械闸、生成物 diff 或真机场景。
- **置信度**：明确区分“代码可证实”“测试已验证”“仅疑似，需设备确认”。

可复用的模块评审 Prompt：

> 请基于当前仓库审查指定模块，不套用通用 Android/iOS 假设。先阅读 `docs/interop-rules.md` 与 `ROADMAP.md` 中相关约束，再区分 ArkUI NDK、NAPI、MAUI Handler、Web/Blazor 和宿主编排链路。每个发现必须给出代码证据、触发条件、影响、最小修复和可执行验证；生成成功或桌面测试通过不等于真机行为正确。不要修改生成文件来掩盖生成器问题，也不要把已记录的已知限制重复包装成新发现。
