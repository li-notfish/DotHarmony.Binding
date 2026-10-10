# ROADMAP —— 路线、实现方案与难点记录

> 本文档记录项目各阶段的方案决策与实测结论。文中的"平台铁律"均为模拟器（x86_64 / API 26）
> 实测结论，是后续实现的边界条件。

## 当前基线

- UI 通道：ArkUI NDK C API（`ArkUI_NativeNodeAPI_1`）→ `ArkUINodeBase` 稳定句柄
- 服务通道：napi（`napi_load_module("=@ohos.xxx")`）→ `@ohos.*` 全量模块端到端
- MAUI Handler 包：28 个具体 Handler（30 个工厂分派形态，含 Shell 第一版）+ 六类手势识别器（Tap/Pan/Pinch/Swipe/Pointer/Drag&Drop）+ XAML（SourceGen 编译期，NativeAOT 零反射）；
  代码风格与 dotnet/maui 官方 handler 模式一致；漏斗纪律（Handler 层禁直连原生 API）由 `FunnelDisciplineTests` 机械强制
- 工具链：`HarmonyStageHost`（宿主工程按应用自动生成）→ `PublishAotClang`（Windows 本地 NativeAOT 双架构）
  → `build-hap.cmd`（hvigor）→ `deploy-hap`（hdc）；`dotnet build -t:HarmonyRun` 一键直达
- 质量基线：xunit 120+14 全绿、jest 79/79、pytest 11/11、`arkui_bindgen check` 零漂移、五面 ArkUI 镜像全部生成式产出

---

## M1 —— MAUI 基本面（已完成）

### 1.1 Brush → ArkUI 背景翻译

`BrushHelper` 支持 SolidColorBrush 与全部公开渐变画刷：`LinearGradientBrush`→`NODE_LINEAR_GRADIENT`
（StartPoint/EndPoint 方向向量转 CSS 角度，方向固定 CUSTOM(9)）；`RadialGradientBrush`→`NODE_RADIAL_GRADIENT`
（Center 相对坐标 × 实测尺寸，Radius 相对半对角线；节点未布局时经 `NODE_ON_SIZE_CHANGE` 以独立 targetId
重算，不占用用户订阅槽）。渐变色标经 `ArkUI_ColorStop` 对象入参（native_type.h @since 12），
GradientStop Offset 全 0 时按 MAUI 语义均匀分布。已接通 ContentPage/Frame(Border)/Label/Layout 四类 Handler。

**ImageBrush 上游缺口**：MAUI 10 将 `ImageBrush` 保持为 internal 类型（用户代码无法构造/XAML 无法声明），
属上游 API 限制；节点层 `SetBackgroundImage`（NODE_BACKGROUND_IMAGE + ArkUI_ImageRepeat）原语已就位，
上游公开后即可在 BrushHelper 接线。

### 1.2 布局对齐 —— 两套布局引擎的取舍

**实现**：
- `WidthRequest/HeightRequest` → `NODE_WIDTH/HEIGHT`（显式请求优先于实测值）
- `Margin` → `NODE_MARGIN` 四边：flex 子树经 `SetMarginEdges`（`StackLayout.Spacing` 以同侧外边距叠加）；
  Grid/Absolute 托管子树在 Arrange 中按轨道单元内缩（auto 维不缩，让内容自撑）
- `HorizontalOptions/VerticalOptions` → 逐子项对齐 `NODE_ALIGN_SELF`：竖直 Stack（Column）交叉轴=水平，
  映射 HorizontalOptions；水平 Stack（Row）交叉轴=垂直，映射 VerticalOptions；Fill → 百分比宽/高
  （Row 需父高有显式约束，否则 auto 高容器内百分比退化为 0）；Start/Center/End → alignSelf 直映
- `Grid/AbsoluteLayout` → MAUI 托管（`HarmonyManagedLayoutHandler`）：Stack + `NODE_POSITION/NODE_WIDTH/HEIGHT`
  绝对定位；轨道解析支持 Absolute/Auto/Star；Span 按起始轨道求和；AbsoluteLayout 支持
  PositionProportional/SizeProportional；容器尺寸来自 `NODE_ON_SIZE_CHANGE`，px/vp 密度由
  MeasuredSize/SizeChange 比值推导；Grid 单元格内非 Fill 对齐经 `ApplyAlignment` 收缩偏移；
  ZIndex 两套布局均接通（NODE_Z_INDEX）；Auto 轨道随子内容 AREA_CHANGE 驱动重排（同帧合并节流、幂等快照）

**已知边界**：Stack 主轴方向的 LayoutOptions —— MAUI 官方布局管理器（10.0.11 `StackLayoutManager`）
发出的 frame 即 DesiredSize，主轴对齐本就被官方忽略，本实现与官方一致，不按缺陷处理；
Span>1 的 Auto 轨道未做专项实测（按需立项）。

### 1.3 Window / Navigation

两级导航均已在模拟器实测通过（HelloApp）：

**① 轻量栈 `HarmonyNavigation.Push/Pop`**（宿主根容器 + 节点保留式切换）。
**② NavigationPage 官方协议转接层 `HarmonyNavigationPageHandler`**：根页为 `new NavigationPage(...)` 时，
业务代码使用 MAUI 标准 `Navigation.PushAsync/PopAsync` 即可。

**原理**（对 MAUI 10.0.11 源码取证）：
- 子页 `NavigationProxy.Inner` 由 `NavigableElement.OnParentSet()` 沿父链自动接到 NavigationPage 的
  `MauiNavigationImpl`，业务代码零改动
- NavigationPage 把整包导航栈打包为 `NavigationRequest`，经 `IStackNavigation.RequestNavigation`
  到达 handler；handler 同步节点栈后**必须**回调 `NavigationFinished`，否则
  `SendHandlerUpdateAsync` 内 await 永久挂起
- 返回方向过渡动画：缩栈前先对旧栈顶做 250ms 淡出（inline 完成回调——宿主无 UI 线程
  SynchronizationContext，续体不得走 Task 调度），`_transitioning` 守卫防重入
- NavigationPage 标题栏：容器顶部 ArkRow（56vp）——返回按钮（栈深>1 可见）+ `CurrentPage.Title`
  跟踪刷新；`HasNavigationBar=false` 隐藏
- 生命周期：MAUI 的 `Page.SendAppearing` 要求父链存在 `Parent` 非空的 `IWindow`，宿主以最小逻辑链放行
  （`Window.Parent = Application`）；模态 `PushModalAsync/PopModalAsync` 经 `RootNavigationAdapter` 承接，
  系统返回键优先关闭模态

**Shell 结论**：第一版已支持（TabBar 条目切换、绝对/相对路由与 `..` 返回、注册路由推送、
query parameters、section 栈 PushAsync、模态转发、生命周期透传）。未覆盖：Flyout 菜单视觉、
Shell 主题色（当前静态兜底）；TabbedPage 为后续候选。

### 1.4 控件 Handler

28 个具体 Handler / 30 个工厂分派形态（Button/Label/ContentPage/StackLayout/Grid/AbsoluteLayout、
Entry/Editor/Switch/CheckBox/RadioButton/Slider/ProgressBar/Image/ScrollView/Frame/Border(BoxView)/
RefreshView/Picker/DatePicker/TimePicker、CollectionView/CarouselView、Shape/GraphicsView 自绘、
ContentView/ContentPresenter、Shell 第一版）。新控件适配指南见
[HANDLERS.md](HANDLERS.md)。

**CollectionView 虚拟化**：平台视图为 `ARKUI_NODE_LIST` + NodeAdapter（`ArkUINodeAdapter` 包装类，
GCHandle + 单一 `[UnmanagedCallersOnly]` 跳板）；条目按可见范围物化，`ON_ADD_NODE_TO_ADAPTER`
创建（ItemTemplate 经 CreateContent，AOT 安全），`ON_REMOVE_NODE_FROM_ADAPTER` 处置
（Handler 断连 + 节点 Dispose）；ItemsSource 变更走 `SetTotalCount + ReloadAllItems` 全量重载。

### 1.5 真机 arm64 验证（云真机启动已通过）

工具链已就绪（arm64 libapp.so 随构建同步产出），并已在 arm64 云真机完成端到端启动验证。
云真机需启用 `patch-openharmony-nativeaot.ps1` 的 NUMA 探测规避，当前该步骤由
`HarmonyBuildLibApp` 自动执行。剩余待办为更广的真机矩阵与性能观测
（AOT 启动时间、GC 表现——`DOTNET_GCHeapHardLimit` 可能需按真机内存调参）。

### 1.6 手势识别 GestureRecognizers

MAUI 手势平台管线在 netstandard Controls 产物中为空实现（`GesturePlatformManager.Standard.cs`），
平台侧完全自建。原生侧接 `ArkUI_NativeGestureAPI_1`（native_gesture.h @since 12）。

**分层**：NDK 绑定（`ArkUIGestureApi.g.cs` 生成式函数表镜像）+ 包装层（`Nodes/Gestures/`）+
MAUI 层（`HarmonyViewHandler<,>` 基类 + `HarmonyGestureManager` 监听 CollectionChanged 全量重建）。

**各识别器通道**：
- Tap → `createTapGesture(NumberOfTapsRequired)`；**同视图按连击数分组共享一个原生手势**
  （否则 N 个识别器 × N 个原生回调 = N² 次触发）
- Pan/Swipe → NODE_TOUCH_EVENT 触摸流驱动（原生 pan 手势的位移在 UPDATE/END 数据不可靠，
  实测结论以触摸流为准）
- Pinch → `createPinchGesture(2)`，scale 累计系数直接透传
- Pointer → 同一触摸通道：Down→Entered+Pressed、Move→Moved、Up→Released

**坐标系（实测沉淀）**：触摸通道 `GetX/Y` 为 **vp**（ui_input_event.h 标注 px 系勘误，
(displayX−节点窗口偏移px)/密度 == GetX 逐位吻合）；手势事件位置为节点相对坐标。

**Recognizer 生命周期铁律**：不得在事件分发回调内 `dispose()` recognizer（原生管线仍可能对已释放
结构派发事件，UAF）。`HarmonyGestureManager.Rebuild` 采用 Detach + 按配置键入池复用，
dispose 仅在 Handler 断连时统一执行。

**反射桥（唯一非公开通道）**：`TapGestureRecognizer.SendTapped` 与 `PointerGestureRecognizer.SendPointer*`
在 MAUI 10 为 internal。`MauiGestureBridge` 用 `[DynamicDependency]` 收根 + MethodInfo 缓存为强类型委托
（NativeAOT 安全；与 XAML 零反射约束不冲突——反射范围仅限这两个类的指定方法）。

**Drag/Drop**：`NODE_ON_DRAG_*`/`NODE_ON_DROP` + `SetNodeDraggable`/`AllowNodeAllDropDataTypes`，
文本载荷经 `libudmf.so` 构造/签收。关键结论：`DragEvent.SetData` 的 UDMF 指针在回调栈之后才被
ACE 消费，托管侧须持有至 `NODE_ON_DRAG_END` 才能 `OH_UdmfData_Destroy`。

### 1.7 .NET 10 / C# 14 优化批次

- `SetNumericAttribute(params ReadOnlySpan<ArkUI_NumberValue>)`（first-class span conversions）——
  属性写热路径零数组分配
- `HarmonyGestureManager` 热路径去分配（Rebuild 快照缓存、普通循环、识别器分类型入池）
- `HarmonyUIExtensions`：extension members 收口 Color→ARGBCast 重复实现
- `HiLog` 格式串 `u8` 静态缓存 + 运行时开关；PromiseTaskBridge 桥名字节缓存
- 构建：`Directory.Build.props` 集中 LangVersion/常量/TFM；`IlcGenerateStackTraceData` 限 Debug

---

## M2 —— 服务层（异步是核心，已完成）

### 2.1 TSFN 异步层

**方案**：`napi_create_threadsafe_function` 封装（`ThreadSafeFunction.cs`，完成/取消/异常三路径）+
Promise/AsyncCallback 回调桥接（`PromiseTaskBridge` / `CallbackTaskBridge`）。

**关键实测结论**：
- TSFN 回调在 JS（宿主主）线程触发，续体在 JS 线程内联恢复（与 JS await 微任务语义一致）；
  用户长耗时工作须自行 `Task.Run` 切走——同步 getter 路径不允许 await promise，阻塞即死锁
- GCHandle 异常兜底：成功后桥创建失败、调用失败路径均取消 Task 并释放句柄，避免 await 悬垂
- 桥名字节经 u8 静态缓存；`MainThreadDispatcher`（`AttachUiThread`/`Post`）为通用 UI 投递通道，
  `HarmonySynchronizationContext` 保留为 .NET 生态兼容面

### 2.2 codeGenerator 缺陷修复

无参/void 方法非法 C# 重载生成、using 生成缺失、重载折叠、`Promise<T>`→`Task<T>` 映射——均已修复并回归。

### 2.3 @ohos.* 批量绑定

438/438 模块全量生成并全量转正编译。生成器在批量扩展中暴露并修复的缺陷类别：
枚举撞手写类型名（从 Nodes/*.cs 扫描保留名集避让）、枚举值超 int32 → long 基底、
事件成员参数撞名改名与签名去重、非标识符成员名过滤、跨模块同名别名解析等
（gap 清单 `native-gaps.json` 为 C API/类型覆盖度的实时底账）。

### 2.4 Essentials 平台实现（22 服务）

接线原理：MAUI 10 Essentials 静态入口在 netstandard 产物中缺省实现全部 throw，但留有 internal
`SetCurrent`/`SetDefault` 注入点。`HarmonyEssentials.Install()`（`MauiHarmonyHost.Run` 自动调用）经
`[DynamicDependency]` 收根 + `CreateDelegate` 缓存完成注入，MAUI 生态代码零改造可用。

22 个服务：DeviceInfo/DeviceDisplay/AppInfo/Clipboard/Preferences/Battery/Vibration/Connectivity/
FileSystem/Launcher/Browser/PhoneDialer/Share/Email/SecureStorage + Accelerometer/Magnetometer/
Gyroscope/Compass/OrientationSensor + Geolocation/MediaPicker。IMainThread 暂缓
（MAUI 10.0.11 无注入点，宿主代码本就运行在 UI 线程）。要点：
- IClipboard：API 26 起 `READ_PASTEBOARD` 为 user_grant，被拒返回空 PasteData 壳而非抛错——
  读前主动查权限状态，未授权弹窗后重试；每次读重查授权
- IPreferences：值用「类型标签:载荷」字符串编码规避 OHOS number 的 2^53 精度丢失
- 事件类服务经 commonEvent / 模块事件订阅，回调内重读 + 去重缓存
- 线程模型：JS 线程 == UI 线程 == Install 线程；`MainThreadDispatcher`/`HarmonySynchronizationContext`
  作为内部投递通道直接内联，不依赖 MAUI `MainThread` 静态入口

适配指南见 [ESSENTIALS.md](ESSENTIALS.md)；验证应用为 `samples/dotnet/EssentialsApp`。

**留白（立项待办）**：TextToSpeech/HapticFeedback/
Flashlight、Map/FilePicker/Screenshot；传感器族与 Geolocation/MediaPicker 的真机/模拟器专项验证；
电池/网络/SecureStorage 事件的模拟器触发验证。

---

## M3 —— 工程化（核心完成，收尾中）

### 已完成

- **装配分层**：`HarmonyOS.Interop`（napi 互操作核心，对齐 Java.Interop 分层）/
  `HarmonyOS.Bindings`（ArkUI 绑定 + @ohos.*）/`HarmonyOS.Essentials` 独立装；全产品收编 `src/`；
  TFM 与 CPM 集中（`Directory.Build.props` / `Directory.Packages.props`）
- **NDK 头文件生成器 `tools/arkui-bindgen`**（libclang）：枚举/结构体/函数表/PInvoke 全部由 NDK
  头文件声明式生成（`config/semantics.yaml` 为唯一语义配置基，生成器不猜）；Animate/Node/Gesture/
  CustomEvent/Adapter 五面镜像已全部切换为 `.g.cs` 产物；`--check` 同时取代原
  `scripts/check-abi-mirror.ps1`（逐字节漂移 + 快照逐签名双门禁）；手写偏差经 `member_overrides`/
  `per_fn` 勘误表显式声明。验收口径：生成物与手写快照逐签名 diff=0，dotnet 120+14、jest 79/79 与 pytest 11/11 全绿
- **NuGet 打包与分发**：`HarmonyOS.Interop` / `HarmonyOS.Bindings` / `HarmonyOS.Essentials` /
  `HarmonyOS.Maui` 四包；`HarmonyOS.Templates`（`dotnet new harmony-maui`）。版本集中于根
  `Directory.Build.props` 的 `PackageVersion`；`HarmonyOS.Maui` 经 buildTransitive 自动导入编排
  targets（脚本与宿主模板随包分发，消费方无需仓库工作副本）；签名属性簇
  （`HarmonySigningKeystore/KeystorePassword/CertAlias/CertPassword/CertPath/Profile`）注入 stage 配置
- **单项目体验**：`dotnet build -t:HarmonyStageHost / -t:HarmonyRun`（宿主按应用自动生成，
  内容戳增量）

### 待办

- **CI**：最小 PR 门禁已落地（最小 NativeAOT 样例构建、arm64 `app.so` 的
  `get_mempolicy` 短路补丁校验）；`arkui_bindgen check` 因依赖本机 OpenHarmony SDK
  保持本地门禁；全量单测与 HAP 全链冒烟暂不阻塞合入
- **性能基线**：`scripts/perf/collect-baseline.ps1` 与 `docs/PERF_BASELINE.md` 已落地，
  覆盖 libapp.so/HAP 体积、冷启动、TSFN 吞吐，持续记录不告警
- **权限自动推导**：Roslyn source generator 扫描应用侧 MAUI/Essentials 调用，自动生成
  `module.json5` 的 `requestPermissions`；显式 `HarmonyPermission` 仍可补充尚未映射的权限
  - v1.1 已开始：新增 `HarmonyPermissionReport` 与 `HMP001` 未映射权限诊断
  - v1.2 已开始：新增 `HarmonyPermissionCheck`，CI 上传 `permissions.report.md`
  - v2 已完成：映射表外置为 `permission-mapping.json`，扩展常用 Essentials 权限，
    支持 `usedScene`、显式 `When` 覆盖与 ProjectReference 聚合；`HMP002`–`HMP004`
    覆盖已知 API 缺映射、显式权限未使用、映射歧义三类诊断，并通过本地 NuGet 包消费者验收
  - v3 已完成：支持 `harmony-permissions.custom.json` 自定义映射、XAML 事件来源定位、
    `HMP005`/`HMP006` 冲突与格式诊断；包消费者验收覆盖 XAML + 自定义映射 + ProjectReference
- **平台 TFM 第一步（已完成）**：`net10.0-harmonyos` 落地——Interop/Bindings 双目标
  （`net10.0;net10.0-harmonyos`），`HARMONYOS` 常量改为 TFM 驱动（harmonyos TFM 由 SDK 按
  TargetPlatformIdentifier 自动定义；桌面 net10.0 目标在库级 props 过渡性全量定义，
  `#else` 死代码语义与桌面测试资产不变）；无 workload 的最小平台注册片段
  （`TargetPlatformSupported` + `SdkSupportedTargetPlatformVersion` + `TargetPlatformVersion=1.0`
  + CA1418 静默）固化于根 `Directory.Build.targets` 与模板工程（必须内联/Directory.Build.targets——
  首次还原时 NuGet buildTransitive 导入尚不存在，包内片段无法生效，实测 NETSDK1139）；
  生成器 `arkui-bindgen` 支持 per-TFM `#if` 包裹（`semantics.yaml` 顶层 `tfm_guard` 声明常量名，
  code_target 条目 `tfm_guard: true` 开启，默认不包裹、check/diff 零漂移）；
  模板工程 `harmony-maui` 已切平台 TFM，端到端验证（StageHost → PublishAotClang 双架构 →
  build-hap → 模拟器 HarmonyRun）通过。另修复两个包分发缺口：`PublishAotClang` 需在应用工程
  显式引用（传递依赖 exclude=Build 导致交叉 AOT 报错）、`patch-openharmony-nativeaot.ps1`
  补入包内 scripts。
- **远期**：真 workload 化（前置：`HARMONYOS` 桌面全量定义摘除（桌面桩体化，生成器 per-TFM
  包裹逐目标开启）、KnownFrameworkReference/RuntimePack 注册与 RID 图策略、
  workload manifest 广告链；需真实多 RID 需求支撑）

---

## 平台铁律速查（实测沉淀，实现前必读）

| # | 铁律 |
|---|---|
| 1 | Node-API 实现库是 `libace_napi.z.so`，`libnapi.so` 不存在 |
| 2 | `napi_load_module("=@ohos.xxx")` 要求宿主 ArkTS 已 import 该模块（`ohosImports.ets` 登记） |
| 3 | 可能抛 ArkTS 异常的 napi 调用后必须 `napi_get_and_clear_last_exception`，否则宿主闪退 |
| 4 | 模块 `napi_value` 须在 handle scope 内转 `napi_ref` 再跨 P/Invoke 持有 |
| 5 | 两段式 `napi_get_value_string_utf8` 缓冲区必须 `length+1`，否则末字节被裁 |
| 6 | NODE_ON_CLICK 的参数经 `GetNodeComponentEvent().data[]` 直读（`GetNumberValue` 不适用） |
| 7 | Background 属性是 Brush 体系，与 Graphics.SolidPaint 平行不可混用 |
| 8 | `IViewHandler`/`[UnmanagedCallersOnly]` 导出只认入口程序集；`--gc-sections` 会收割 ILC 的 `__modules` section |
| 9 | Controls 与 Graphics 的颜色类型树平行（SolidColorBrush vs SolidPaint），转换需显式助手 |
| 10 | 字符串属性空串封送：`GetBytes("")` + `fixed` 得空指针 → `SetAttribute` 401；基类统一传 NUL 结尾空 C 串 |
| 11 | Handler 层禁止直连原生 API（函数表/napi/原始结构体）——漏斗纪律，`FunnelDisciplineTests` 机械强制 |
| 12 | UDMF 拖拽载荷在拖拽回调栈之后才被消费，须持有至 `NODE_ON_DRAG_END` 再销毁 |
