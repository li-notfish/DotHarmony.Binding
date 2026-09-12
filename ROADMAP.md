# ROADMAP —— 后续路线、实现方案与难点留档

> 本文档记录项目当前状态之后的开发路线。每个阶段给出：做什么、怎么做、难点在哪。
> 文中的"平台铁律"均为实测结论（模拟器 x86_64 / API 26），是后续实现的边界条件。

## 当前基线（已完成，详见 README）

- ✅ UI 通道：ArkUI NDK C API（`ArkUI_NativeNodeAPI_1`）→ `ArkUINodeBase` 稳定句柄
- ✅ 服务通道：napi（`napi_load_module("=@ohos.xxx")`）→ `@ohos.deviceInfo` 端到端
- ✅ MAUI Handler 包：22 个 Handler（16 基础 + RefreshView/Picker/DatePicker/TimePicker/CollectionView/CarouselView）+ XAML（XamlC/SourceGen 编译期，NativeAOT 零反射）；代码风格已统一为官方 handler 模式
- ✅ 工具链：`remote-build.sh`（远程 NativeAOT）→ `build-hap.cmd`（hvigor）→ `deploy-hap.sh`（hdc）

---

## M1 尾巴 —— MAUI 基本面补齐

### 1.1 Brush→ArkUI 背景翻译（✅ 已完成，含渐变/图片画刷）

`BrushHelper` 已支持 SolidColorBrush 与全部公开的渐变画刷：`LinearGradientBrush`→`NODE_LINEAR_GRADIENT`（StartPoint/EndPoint 方向向量转 CSS 角度，方向固定 CUSTOM(9)）；`RadialGradientBrush`→`NODE_RADIAL_GRADIENT`（Center 相对坐标 × 实测尺寸，Radius 相对半对角线；节点未布局时经 `NODE_ON_SIZE_CHANGE` 以独立 targetId 重算，不占用用户订阅槽）。渐变色标经 `ArkUI_ColorStop` 对象入参（native_type.h @since 12），GradientStop Offset 全 0 时按 MAUI 语义均匀分布。已接通 ContentPage/Frame(Border)/Label/Layout 四类 Handler，ControlsDemoPage 含两种渐变验证项。

**ImageBrush 上游缺口**：MAUI 10 将 `ImageBrush` 保持为 internal 类型（用户代码无法构造/XAML 无法声明），属上游 API 限制；节点层 `SetBackgroundImage`（NODE_BACKGROUND_IMAGE + ArkUI_ImageRepeat）原语已就位，上游公开后即可在 BrushHelper 接线。

**历史说明**：一期只做纯色（Gradient/Image 静默透明）；渐变/图片画刷已于本阶段补齐。

### 1.2 布局对齐 —— 两套布局引擎的取舍（✅ 已完成，2026-09-11 模拟器实测）

**实现结果**：
- `WidthRequest/HeightRequest` → `NODE_WIDTH/HEIGHT`（显式请求优先于实测值，HarmonyLayoutHandler.AttachChild）
- `Margin` → `NODE_MARGIN` 四边：flex 子树经 `SetMarginEdges`（`StackLayout.Spacing` 以同侧外边距叠加）；
  Grid/Absolute 托管子树在 Arrange 中按轨道单元内缩（auto 维不缩，让内容自撑）
- `HorizontalOptions/VerticalOptions` → **per-child 对齐**：`NODE_ALIGN_SELF`（alignSelf）。
  竖直 Stack（Column）交叉轴=水平，映射 HorizontalOptions；水平 Stack（Row）交叉轴=垂直，映射
  VerticalOptions。Fill → 百分比宽（Column）/百分比高（Row，仅当 Row 高度受显式 HeightRequest
  约束——auto 高父容器的子项百分比会退化为 0）；Start/Center/End → alignSelf 直映
- `Grid/AbsoluteLayout` → **MAUI 托管**（`HarmonyManagedLayoutHandler`）：Stack +
  `NODE_POSITION/NODE_WIDTH/HEIGHT` 绝对定位；轨道解析支持 Absolute/Auto/Star（Star 按剩余空间
  权重分配）；Span 按起始轨道求和；AbsoluteLayout 支持 PositionProportional/SizeProportional
  （比例定位锚定扣除自身尺寸后的可放置区）；容器尺寸来自 `NODE_ON_SIZE_CHANGE`，
  px/vp 密度由 MeasuredSize/SizeChange 比值推导

**实测**：ControlsDemoPage 含 Align Start/Center/End、Margin、V-Center/V-End 用例，模拟器截图逐项确认。

**遗留限制（M1）**——~~2026-09-12 起 1.7 已逐项修复，见下~~：
- ~~Grid 单元格内非 Fill 对齐不生效~~ → 已修（1.7）
- ~~Auto 轨道……内容自身变化不触发重排~~ → 已修（1.7，AREA_CHANGE 驱动重排）
- ~~两套布局的 ZIndex 均按 addChild 顺序~~ → 已修（1.7）
- Stack 主轴方向的 Options：经 MAUI 10.0.11 源码取证（`StackLayoutManager`），官方布局管理器
  发出的 frame 即 `DesiredSize`（主轴对齐本就被忽略）——现行为与官方一致，**关单为对齐语义说明**
- Span>1 的 Auto 轨道不参与实测（维持原状，按需立项）

### 1.3 Window / Navigation（✅ 已完成：轻量栈 + NavigationPage 转接层 / 中）

两级导航均已实测通过（HelloApp，模拟器）：

**① 轻量栈 `HarmonyNavigation.Push/Pop`**（宿主根容器 + 节点保留式切换）：Push→Pop（主页状态完整恢复）→再 Push（计数延续）。原"摘除-恢复"难点已实测排除。

**② NavigationPage 官方协议转接层 `HarmonyNavigationPageHandler`**：根页改为 `new NavigationPage(new MainPage())` 后，业务代码用 MAUI 标准 `Navigation.PushAsync/PopAsync` 即可。实测：PushAsync→visit #1、PopAsync→回主页、再 Push→visit #2（MainPage 实例跨 push/pop 保留）、系统返回键消费 NavigationPage 内栈而非退出应用。

**原理**（对 MAUI 10.0.11 源码取证）：
- 子页 `NavigationProxy.Inner` 由 `NavigableElement.OnParentSet()` 沿父链自动接到 NavigationPage 的 `MauiNavigationImpl`——业务代码零改动
- NavigationPage 把整包导航栈打包为 `NavigationRequest`，经 `Handler.Invoke(nameof(IStackNavigation.RequestNavigation))` 到达 handler
- handler 把 ArkUI 节点栈同步成请求的栈（仅栈顶挂入 ArkStack，低层摘除保留句柄），完成后**必须**回调 `IStackNavigation.NavigationFinished`，否则 `SendHandlerUpdateAsync` 内 await 永久挂起（PushAsync 不返回）
- 无 NavigationPage 时 MAUI 官方语义即抛 "PushAsync is not supported, please use a NavigationPage."（`Window.NavigationImpl`）——宿主返回键先消费 NavigationPage 内栈，再退到根级轻量栈

**剩余**：~~返回/弹出方向的过渡动画~~、~~`Window.Toolbar`~~ → **2026-09-12 均已完成**（见下 1.7 前序批次与本次收尾）：
- 返回方向过渡：`HarmonyNavigation.Pop/PopModal` 与 NavigationPage 缩栈分支在 RemoveChild 前
  对旧栈顶做 1→0 淡出（250ms，inline 完成回调——无 UI 线程 SynchronizationContext，续体不得走 Task 调度），
  `_transitioning` 守卫防重入；PushAsync 返回延迟 ~250ms 属预期（NavigationFinished 在动画后回调）
- NavigationPage 标题栏：容器顶部 ArkRow（56vp）——"←"返回按钮（栈深>1 可见，点击走 `PopAsync`）+
  `CurrentPage.Title` 标题（PropertyChanged 跟踪刷新）；`HasNavigationBar=false` 隐藏；内容区 SetFlexGrow 占满

**生命周期**：`Appearing/Disappearing` 已透传（含 NavigationPage 内部切换）。MAUI 的 `Page.SendAppearing` 有守卫（父链上须存在 `Parent` 非空的 `IWindow`），宿主以最小逻辑链放行：`Window.Parent = Application`（均为 public API；不设 `Window.Page`，其 setter 会用 `Window.NavigationImpl` 覆写页面的 NavigationProxy.Inner）。实测：主页 2A/1D 随模态开闭精确变化，NavigationPage 内推入页同样触发。

**模态**：`Navigation.PushModalAsync/PopModalAsync` 标准可用。宿主根容器为 ArkStack（后挂者覆盖），模态页覆盖页面之上；`RootNavigationAdapter`（`NavigationProxy` 子类）挂在根页 `NavigationProxy.Inner` 上承接——含 NavigationPage 路径（`MauiNavigationImpl` 未覆写模态调用，经基类转发到适配器）。实测：PushModalAsync→覆盖显示+生命周期正确、PopModalAsync→恢复、系统返回键优先关闭模态。

**切换动画**：绑定 `ArkUI_NativeAnimateAPI_1.animateTo`（`ArkUIAnimateApi.cs`）+ `ArkUINodeBase.AnimateAsync`；轻量栈与模态推入时新页淡入（NODE_OPACITY 0→1，250ms EASE_IN_OUT）。

**Shell 转接结论**：Shell 不在支持计划内（flyout/tab/URI 路由协议太重）。多平台 Shell 应用做鸿蒙适配时，入口改为 NavigationPage/TabbedPage 结构（`MauiHarmonyHost.Run(() => new NavigationPage(...))`）——这正是 NavigationPage 转接层存在的意义；TabbedPage（底部页签）为后续候选。

### 1.4 更多控件 Handler（✅ 已完成 22 个）

已完成 22 个 Handler（16 基础 + RefreshView/Picker/DatePicker/TimePicker + BoxView（纯色矩形→Stack 背景色，2026-09-12 补）+ CollectionView/CarouselView M1 版；RadioButton 的 Content 经 Row+Text 包装呈现，GroupName 已接通）。代码风格已统一为官方 handler 模式（`ViewHandler<TVirtualView, TPlatformView>` + 命名 Map 方法）。注意 MAUI 10 核心/Controls 接口差异：DatePicker.Date 等为可空类型，GroupName 仅在 Controls 类型上——虚拟视图类型按 Picker 先例直接用 Controls 具体类型。剩余：Shape/自绘（需 MAUI Graphics 前端）与 CollectionView 虚拟化（NodeAdapter）按需插入。

### 1.5 真机 arm64 验证（小）

工具链已就绪（arm64 libapp.so 一直同步产出）。难点只有签名物料与真机性能观测（AOT 启动时间、GC 表现——`DOTNET_GCHeapHardLimit` 可能需要按真机内存调参）。

### 1.6 手势识别 GestureRecognizers（✅ 已完成，2026-09-12）

MAUI 手势平台管线在 netstandard Controls 产物中为空实现（`GesturePlatformManager.Standard.cs`，internal），须完全自建平台侧。原生侧接 ArkUI NDK `ArkUI_NativeGestureAPI_1`（native_gesture.h @since 12，经既有 `OH_ArkUI_QueryModuleInterfaceByName(ARKUI_NATIVE_GESTURE=2)` 通道，镜像 `ArkUIAnimateApi.cs` 模式——`ArkUIGestureApi.cs`）。

**分层**：
- **NDK 绑定**：`ArkUIGestureApi.cs`（函数表 version 字段为首成员；C bool 在 unmanaged fn ptr 中按 1 字节→显式 byte）+ `ArkUIPointerEvent.cs`（ArkUI_UIInputEvent 公共只读包装，坐标 px）
- **包装层**：`Nodes/Gestures/`（`ArkUIGestureRecognizer` 基类 + Tap/Pan/Pinch/Swipe/LongPress；`setGestureEventTarget` 回调走 GCHandle extraParams + 单一 `[UnmanagedCallersOnly]` 跳板，同 NodeEventBus 模式；Parallel + Normal mask 与内建 onClick 并行不互斥）
- **MAUI 层**：`HarmonyViewHandler<,>` 新基类（19 个 View Handler 全部迁移，Page Handler 除外）+ `HarmonyGestureManager`（监听 `CompositeGestureRecognizers` CollectionChanged 与 IsEnabled/InputTransparent，全量重建原生手势）

**各识别器通道**：
- Tap → `createTapGesture(NumberOfTapsRequired)`，Accept 动作触发；子元素（Label Span）优先消费；**同视图按连击数分组共享一个原生手势**（RaiseTap 广播给组内识别器，否则 N 个识别器 × N 个原生回调 = N² 次触发，实测已踩）
- Pan/Swipe → **NODE_TOUCH_EVENT 触摸流驱动**（Android touch listener 同款模型）：实测 pan 原生手势的 `GetOffsetX/Y` 在 END 返回 0、原始输入位置在 UPDATE/END 不可靠，不可依赖；按下记起点、移动累计、抬起收尾 → `SendPanStarted/SendPan/SendPanCompleted/SendPanCanceled`（公开接口）；Swipe 累计位移喂 `SendSwipe`，抬起时 `MapSwipeDirection(主轴判定)` → `DetectSwipe`（阈值判定在识别器内部）
- Pinch → `createPinchGesture(2)`；scale 累计系数直接透传 `SendPinch`
- Pointer → 同一触摸通道：Down→Entered+Pressed、Move→Moved、Up→Released

**坐标系（模拟器 API 26 实测沉淀）**：触摸通道 `GetX/Y` 为 **vp**（ui_input_event.h 标注 px 系笔误：(displayX−节点窗口偏移px)/密度 == GetX 逐位吻合），故 `PanUpdatedEventArgs.TotalX/Y` 为 vp（等价 iOS points 语义，跨平台代码注意与 Android px 差异）；手势事件（tap）位置为节点相对（原生窗口坐标系，与 dumpLayout/屏幕坐标差一个窗口原点偏移）。MAUI 10 的 `SendPanCompleted` 不携带坐标（`PanUpdatedEventArgs(Completed, gestureId)`），Completed 时 TotalX/Y 恒 0 是官方语义——累计值应从最后一次 Running 读取。

**原生 recognizer 生命周期铁律**：**不得在事件分发回调内 `dispose()` recognizer**——实测 dispose 后原生手势管线仍对已释放结构派发事件（SIGSEGV UAF，fault 栈 libace_ndk 调堆上野指针）。`HarmonyGestureManager.Rebuild` 改为 Detach + 入池按配置键复用，dispose 仅在 Handler 断连时统一执行。

**反射桥（唯一非公开通道）**：`TapGestureRecognizer.SendTapped` 与 `PointerGestureRecognizer.SendPointer*` 在 MAUI 10 为 internal（Tapped 是 event 无法外部 raise）。`MauiGestureBridge` 用 `[DynamicDependency(NonPublicMethods)]` 收根 + MethodInfo 查一次 `CreateDelegate` 缓存成强类型委托（NativeAOT 安全；与 XAML 零反射铁律不冲突——反射范围仅这两个类的指定方法）。单测覆盖桥全链路与 Swipe 方向映射（tests/dotnet/HarmonyGestureTests，9 用例）。

**单位约定**：PanUpdated 单位 vp（见上）；tap 位置为节点相对坐标。Tap 的 ButtonsMask 鼠标按键区分 NDK 不暴露，按 Primary 处理。Pointer 仅触摸通道（hover/mouse 待补）。DragGestureRecognizer/DropGestureRecognizer 未实现（longpress+跨视图状态机，后续立项）。

**模拟器实测通过**（uitest 全链路）：单击恰好 +1（含 pos 回传）、双击、pan 累计位移 198vp（700px 拖动 ÷ 密度 3.5 吻合）、swipe Right/Up 方向判定、pointer pressed/moved/released 流、动态增删识别器后恰好 +1（池化回归通过）、IsEnabled=false 静默、进程存活。Pinch 多点触控 uitest 无注入能力，代码路径 + 单测覆盖，留待真机专项。

### 1.7 布局遗留修复 + .NET 10 / C# 14 优化（✅ 已完成，2026-09-12）

1.2 遗留清单逐项处理结果（取证与实现记录）：

- **Grid 单元格内对齐**：`HarmonyManagedLayoutHandler.ApplyAlignment`（internal static，纯逻辑可单测）——
  Fill/内容未量测(≤0)/内容≥frame 保持原样；Start/Center/End 按实测尺寸收缩并在 frame 内偏移。
  首帧未量测退化 Fill，AREA_CHANGE 到来后重排自然生效
- **ZIndex**：`ArkUINodeBase.SetZIndex`（NODE_Z_INDEX=21，float）；`HarmonyLayoutHandler` 与
  managed 两套布局均接通 MapUpdateZIndex + AttachChild 初始同步；managed 侧 Arrange 全量下发
- **Auto 轨道内容变化重排**：AttachChild 订阅子节点 `NODE_EVENT_ON_AREA_CHANGE`（位置+尺寸变化均发）
  → Arrange（`_arrangeQueued` 同帧合并节流）；Remove/Clear 注销；首帧类型兜底估算保留
- **Stack 主轴对齐关单**：MAUI 10.0.11 `StackLayoutManager` 发出的 frame 即 DesiredSize，
  主轴 LayoutOptions 官方即忽略——现行为一致，不改代码（1.2 已标注）
- **重排幂等**：managed Arrange 增加 `_lastApplied` 快照，位置/尺寸未变化时跳过原生属性写入，
  AREA_CHANGE 风暴下不再空转；调试日志（FormatTracks/string.Join）`const bool` 门控

**模拟器全链路验证暴露并修复的三个问题**（2026-09-12）：
1. **空串 SetStringAttribute 401 闪退**：`Encoding.UTF8.GetBytes("")` 得 0 长数组，`fixed` 出空指针
   传给 `item.@string` → 原生 401 → 启动即崩（NavigationPage 标题栏初始空 Title 触发）。基类改为
   空串传 NUL 结尾空 C 串
2. **BoxView 无 Handler**：`PushAsync` 静默失败——工厂 `NotSupportedException` 被 FireAndForget 吞掉，
   且 MAUI SendHandlerUpdateAsync 信号量不释放，后续导航全部永久排队。新增 `HarmonyBoxViewHandler`
   （22 号 Handler）；`FireAndForgetNavigation` 改打 hilog
3. **嵌套 Grid 撑爆父容器**：托管布局容器无条件 `SetHeightPercent(1.0)` 只对页面根布局正确——
   嵌套在 StackLayout 内时把后续兄弟推出屏幕。改为 `HeightRequest 显式 / Parent 是 Layout 自然高 /
   根布局 100%`

**.NET 10 / C# 14 优化批次**（与 2.10 零分配改造同思路，控件层落点）：
- `ArkUINodeBase.SetNumericAttribute(params ReadOnlySpan<ArkUI_NumberValue>)`——C# 14 first-class
  span conversions，全仓最热属性映射路径调用点零数组分配
- `HarmonyGestureManager` 热路径去分配：Pointer 识别器 Rebuild 快照缓存（替代每触摸事件 OfType）、
  GesturesFor/ChildGesturesFor 普通循环、触摸状态 `double` 字段、识别器分类型入池
- `HarmonyUIExtensions`：C# 14 extension members（具名接收者 extension 块）收口 Color→ARGBCast
  12 处复制（SetBackgroundColor(Color)/SetFontColor/PlaceholderColor/Slider 色系等）
- C# 14 null-conditional assignment（`x?.Event -= h`）、`field` 关键字（ImageSourceResolver.TempDir）
- `HiLog`：格式串 `u8` 静态缓存 + `VerboseEnabled` 运行时开关；`PromiseTaskBridge` 桥名字节缓存、
  `IntPtr[]` → `ReadOnlySpan<IntPtr>` 调用
- 构建：`Directory.Build.props`（LangVersion=preview + HARMONYOS 常量收敛，**已加入打包清单**）、
  `IlcGenerateStackTraceData` 限 Debug（Release NativeAOT 体积优化；栈回溯退化为数字地址）
- 单测：tests/dotnet/HarmonyGestureTests 增至 **15 用例**（9 手势桥/方向映射 + 6 Grid 对齐偏移），全绿

---

## M2 —— 服务层完备（异步是核心）

> **2026-09-10 进度**：2.1 已完成——
> - P/Invoke 声明：`napi_create/release/call_threadsafe_function` 已添加到 `NativeNodeApi.cs`
> - `ThreadSafeFunction.cs`：封装 TSFN 生命周期三路径（完成/取消/异常），Promise 回调注册
> - `HarmonySynchronizationContext.cs`：主线程调度器，支持 Post/Send 回到 UI 线程
> - 最小切片：`Promise<T>`→`Task<T>` 映射接通（TypeMapper + CodeGenerator + 47/47 测试）
> - AsyncCallback 支持：`(result: T, err?: Error) => void` → `Task<T>` parser + 生成器
> 2.2 已完成——using 生成、CallMethodVoid 重载、方法折叠、Promise→Task 映射。
> ~~端到端模拟器验证待手动执行。~~ → **2026-09-12 已实测通过**（见 2.7）。


### 2.1 TSFN 异步层（大，M2 的核心难点）——✅ 最小实验已通过；✅ .NET 风格标准化已落地

**做什么**：让 `@ohos.*` 的 `Promise<T>` / callback 风格 API 在 C# 里以 `Task<T>` 可用。~~当前全部映射为 `IntPtr` 占位~~ → **2026-09-11 标准化完成**：`Promise<Array<Display>>` → `Task<DisplayObject[]>` 全链路打通，接口类型生成 JsObject 派生包装类（114 个）/ 纯数据入参生成 INapiRecord record（47 个），裸 IntPtr 返回从 103 降至 19。

**2.5 .NET 风格标准化（2026-09-11 完成）**：
- 命名规范化：`naming.ts` 缩写词归一（`getURI→GetUri`、`TYPE_DEFAULT→TypeDefault`），Task 方法自动 `Async` 后缀
- 类型映射收口 TypeMapper 单一事实源；修复 `mapGenericType` 贪婪正则对嵌套泛型的误切（`Promise<Array<T>>` 曾整体退化为 IntPtr 的潜在 bug）
- Runtime：`JsObject` 强引用包装基类、`ValueConverter` 统一转换（含枚举/数组/显式工厂委托）、`NodeApi.SetProperty/GetGlobal/CreateInstance/GetArrayElements`、PromiseTaskBridge 吸并 ThreadSafeFunction.FromPromise 重复实现
- 服务模块 `on*` 函数不再误判为组件事件；嵌套类构造函数支持（Picker 空壳修复）；无注解字面量常量类型推断（Pasteboard MIMETYPE_* 恢复）
- 明确不做（后续立项）：ArrayBuffer/BigInt 封送、Map/Set 容器映射、EventHandler/EventArgs 事件模型、模拟器端到端手动验证（TSFN abort 路径）

**2.6 封送补全与事件模型（2026-09-12 完成）**：
- ArrayBuffer/TypedArray ↔ `byte[]` 拷贝语义（typedarray_info 取字节切片）；bigint → `JsBigInt`（long 语义，经 `napi_create_bigint_int64` 通道与 JS number 区分；超出 int64/uint64 抛异常）
- `JsMap<TKey,TValue>` 活视图：map.get/set/has/delete + entries() 迭代器协议；`Map<number, Geofence>` → `Task<JsMap<double, Geofence>>`（PILOT_MODULES 增加 @ohos.geoLocationManager）
- 完整 .NET 事件模型：199 个事件访问器（`Display.Change += handler`），add/remove 经 `EventListenerRegistry` 配对 on/off（JS off 按函数实例匹配，GCHandle/napi_ref 生命周期托管）；类型化 `On(type, Action<T>)` 重载；单一共享 `ArgsTrampoline`（Action<IntPtr[]>）+ 生成器调用点适配器，任意回调形状 AOT 安全
- 明确不做：Set 容器（试点 0 使用）、DataView、Int32Array 等精确 TypedArray 类型（一律按字节拷贝，有损）、BigInt words 全精度、NativeCallbacks 反射兜底的替换
- ~~待手动验证：模拟器端到端（事件触发、ArrayBuffer 读写、Map 迭代）~~ → 见 2.7（事件订阅/退订回路已实测；ArrayBuffer 读写、Map 迭代仍待专项用例）

**2.10 M2 收尾与零分配改造（2026-09-12 完成）——M2 除 2.4 Essentials 外全部完成**：
- **封送专项实测通过**（模拟器）：ArrayBuffer byte[8] 往返逐字节一致；JsMap 读侧（Count/TryGet/Entries）与写侧（Create+Set → JS forEach 求和）全通；TSFN 加固后 worker(tid 9)→JS(tid 1) 回调、env 往返正常
- **TSFN 生命周期加固**：Release/Abort 与 Call 互斥（锁）防句柄竞态；GCHandle 延迟到 finalize 回调释放——abort 后已入队消息仍会派发，过早释放即 UAF；CallJsTrampoline 异常捕获（不得穿透原生帧）
- **全量生成（M2.3 终态）**：`--all` 生成 449 个 d.ts 中的 438 个模块，**375 转正编译、63 灰度**（黑名单 GRAYSCALE_MODULES）。规模暴露的生成器工程化缺陷全部修复：模块 className 唯一化（resourceManager/global.resourceManager、net/bluetooth 的 connection/socket 同名互覆盖）；跨模块 import 类型降级 IntPtr（含 default import）；枚举冲突按属主模块加前缀（废弃"全局去重丢弃"——首发射者被灰度会拖垮依赖者）；陈旧 Enums.cs 清理；灰度清单携带唯一化后 className；含 UTF-16（CRLF/代理对）转义与命名保留字处理
- **已知残留（立项待做）**：TS 声明合并类型（window.WindowRect 双定义）；跨模块强类型解析（现在降级 IntPtr）；63 灰度模块的类型映射缺口清单
- **零分配调用路径（.NET 10 / C# 13）**：`params ReadOnlySpan<object?>`（params collections，调用点零数组）；argv 栈分配（InvokeMethod/CreateInstance/trampolines，>8 参数回退堆）；P/Invoke 改 `ReadOnlySpan<IntPtr>`（LibraryImport 钉扎零拷贝）；生成 record 的 WriteTo 属性名 u8 常量缓存（替代每次 `Encoding.UTF8.GetBytes`）；NodeApi 胶水名字节静态缓存。**剩余分配源**：object 转换点基元装箱、字符串结果物化、事件适配器闭包——完全零装箱需 union struct 参数设计，后续立项

**2.9 事件触发路径实测 + 第四批扩展（2026-09-12 完成）**：
- **事件触发路径首次全链路实测通过**（模拟器）：`Sensor.Accelerometer += handler` 订阅 → JS 持续触发 → ArgsTrampoline → 类型化 `AccelerometerResponse` 载荷（X/Y/Z 属性读取，实测 y=9.80 标准重力值）→ handler 内第 5 次自动退订（off 按函数实例匹配）。示例：ModuleVerifyPage Sensor 按钮
- 批量扩展 46 → **74 全部转正编译**（+util 容器 9 个、events.emitter、commonEventManager、resourceManager、taskpool、worker、data.relationalStore/dataSharePredicates、file.hash/statvfs/securityLabel、multimedia.audio、graphics.displaySync/colorSpaceManager、screenLock、accounts.osAccount、formBindingData/formProvider、convertxml、zlib）
- 新暴露并修复的生成器缺陷：① 枚举撞手写 Nodes 类型名（relationalStore.Progress 撞 Nodes/progress.cs，CS0101）→ 从 Nodes/*.cs 扫描保留名集，撞名枚举加模块前缀；② 枚举值超 int32 → long 基底（audio 声道布局位掩码，CS0266）；③ 事件成员 extra 参数撞名 type/callback → 改名（CS0100）；④ 事件成员签名去重补齐 event 键类型维度并跳过普通成员已发射的签名（emitter Off(string) 重复，CS0111）
- 待续：后续批次同法御用（生成→全批转正→编译筛选→修复/回灰），向 143 模块目标推进

**2.8 AsyncCallback 正式通道 + 批量扩展（2026-09-12 完成）**：
- `CallbackTaskBridge`：仅 callback 形式 API（无 Promise 重载，如 `settings.registerKeyObserver`、`thermal.subscribeThermalLevel`、fs 实例方法）→ `Task<T>`。运行时 `napi_create_function` 创建 err-first JS 回调作末参传入，JS 触发时解析 (err, data)：成功 → SetResult(convert(data))，BusinessError → `ArkTSException`（读 err.code/err.message）。TCS 同步续体保持 NapiEnv 可用（同 PromiseTaskBridge 语义）
- 关键认知：**双形态 API 之前是"碰巧能工作"**——原生实现按末参是否为函数自适应返回 Promise，剥掉 callback 调用即进 Promise 模式；真正坏掉的只有 callback-only API（无回调调用会同步抛 401）。生成器双形态判定：同名且剥回调后参数一致的 Promise 重载存在 → Promise 通道；否则 bridge 通道
- 包装类 junk 重载清理：`void Show(IntPtr callback)` 之类手搓回调指针的产物折叠为单一 `ShowAsync()`（Window -380 行）
- 生成器修复（批量扩展暴露）：非标识符成员名过滤（url 的 `[Symbol.iterator]`）；服务模块顶层 `export interface/class` 解析为实例类型（intl.LocaleOptions 等被引用但从未生成）；可达性 BFS 补 wrapper 构造函数参数种子（输入位 record）；record WriteTo 中 `System.Text.Encoding` 完全限定（属性名撞名）
- 批量扩展：PILOT_MODULES 21 → **46 全部转正编译**（+thermal/power/wallpaper/wifiManager/telephony.radio/sms/usbManager/inputMethod/hilog/hiAppEvent/i18n/intl/mediaquery/font/measure/uri/url/matrix4/curves/net.webSocket/net.socket/data.dataShare/bundle.bundleManager/app.ability.appManager/app.ability.context/notification）
- 遗留：CS1737 修复（demoteOptionals 必须在事件回调强制必需之后，Sensor off 重载）；后续批次向 143 模块推进时同法筛选

**2.7 端到端模拟器验证（2026-09-12 完成，x86_64 模拟器 / API 26）**：
- 全链路：generator 重出 → WSL NativeAOT 双架构 libapp.so → hvigor HAP → hdc 部署 → 实机点击验证
- 实测通过：`napi_load_module` 各模块加载；同步属性（DeviceInfo Brand/Model/OsFullName=OpenHarmony-7.0.0.105）；Promise→Task 全类型桥（string/double/bool/int/uint/void/reject——reject 正确抛 `ArkTSException` 携带 reason）；wrapper 属性 await 后可读（`GetDefaultDisplayAsync()` → 1320x2856 @560dpi）；`.NET event` 订阅/退订回路（Display.Change +=/-=）
- **实测修复 1**：生成器注入 `$string:permission_XXX_reason` 但从不写 string.json 资源 → hvigor CompileResource 直接失败；`writeModuleJson5Permissions` 现同步写 32 条 reason 字符串（merge 保留既有条目）
- **实测修复 2（架构级）**：PromiseTaskBridge 原用 `RunContinuationsAsynchronously` + `ContinueWith` 把续体调度到线程池——wrapper 结果 await 后访问属性时 `NapiEnv.Current`（[ThreadStatic]）无 env 直接抛异常。改为 TCS 同步续体：Promise resolve 的 trampoline 在 JS 线程内联执行用户续体（与 JS await 微任务语义一致）；副作用：用户续体长耗时工作须自行 `Task.Run` 切走
- **实测修复 3**：`async void` 事件处理器仅 catch `ArkTSException`，其它异常（如调用不存在的 JS 函数）未处理直接杀进程；samples 兜底 `catch (Exception)`，宿主 EntryAbility 补 `globalThis.somePromiseApi/willFail` 测试函数

**最小实验结论**（2026-09-11，模拟器，HelloApp "TSFN test" 按钮 / `Runtime/TsfnExperiment.cs`）：
- `CallJsTrampoline` 已实装（原为空壳）：context 解析回 ThreadSafeFunction 实例，转发 `OnCallJs`
- 后台 .NET 线程 `Call()` → libuv 在 **JS（宿主主）线程**触发回调（managed thread id 与 UI 线程一致，实测吻合）
- 回调内 `NapiEnv.Current` 可用，NAPI 字符串创建+读取往返成功
- 封送开销 ~13ms（worker 睡 300ms，端到端 313ms）；回调内直接更新 ArkUI 控件无崩溃
- 生命周期完成路径（回调末尾 `Release()`）实测无泄漏/UAF；连点两次（两个实例并发）无竞态
- ⚠️ 未验证：abort 路径、release 后 call 的防御、跨 TSFN 实例 GC 压力——收编正式通道时补

**怎么做**：
1. `napi_create_threadsafe_function` 封装（`Runtime/ThreadSafeFunction.cs`）：
   - C# 发起调用 → 拿到 Promise → 注册 TSFN（经 `napi_resolve` 在 JS 线程桥接）
   - C# 侧返回 `TaskCompletionSource<napi_value>` 包装的 `Task<T>`
2. Promise 完成时（JS 线程）→ TSFN 回调 C#（线程安全排队）→ TCS.SetResult → `await` 继续
3. 生成器把 `Promise<T>` 映射从 `IntPtr` 改为 `Task<T>`（或 `Task<string>` 等具体类型，按 T 递归）

**难点**：
1. **TSFN 生命周期**：`napi_release_threadsafe_function` 的调用时机（完成/取消/异常三路径）错一处就泄漏或 UAF
2. **线程模型**：`napi_env` 线程绑定（已在 `NapiEnv` 文档化）；TSFN 回调在任意线程进入 C#，回调内**不得**直接调 ArkUI 节点 API（需回 UI 线程——需要为 .NET 侧建一个 UI 线程调度器，宿主主线程跑一个 .NET SynchronizationContext）
3. **AOT 下的泛型封送**：TSFN 的 callback data 用 `GCHandle`，泛型 T 的收尾需要具体类型跳板（每个 T 一个 `[UnmanagedCallersOnly]`，生成器产出或限于常用类型集）
4. **ArkTS 异常**：Promise reject → C# 侧应 `Task.FromException`，同样要清 pending exception（M0 铁律）

### 2.2 codeGenerator 缺陷修复（小，TSFN 的前置）

- `CallMethod<void>` 非法 C# → 无参/void 方法（白皮书"泛型 void 清洁重载"）
- using 生成缺失（产物缺 `using HarmonyOS.Bindings.Runtime`）
- 方法重载折叠保留全部签名
- `Promise<T>` 映射接 2.1 的 `Task<T>`

**难点**：老产物（Api/ 561 文件）当年就是这些缺陷的产物；修复后需小批量重出验证再全量。

### 2.3 @ohos.* 批量绑定（机械，但有两道闸）

**做什么**：`--sdk` 全量生成 143 模块。

**两道闸**（已验证的平台事实）：
1. `ohosImports.ets` 自动登记 ✅（已实现）；但**每个被用到的模块是否需要权限**（位置/相机/蓝牙等）需要生成器顺带产出 `module.json5` 的 `requestPermissions` 清单，否则运行时才爆
2. 全量产物先以"生成但不进编译"姿态评审（恢复 `Compile Remove` 灰度策略），按模块逐个转正

### 2.4 Essentials 平台实现（可选，价值高）

`Microsoft.Maui.Essentials` 的 `IDeviceInfo/IDisplay/IClipboard` 等接口做鸿蒙实现，让 MAUI 生态的标准 API 直接可用（M1 中已出现与 Essentials 的 DeviceInfo 撞名——说明生态对接是真实需求）。

---

## M3 —— 工程化（远期）

- **NDK 头文件生成器（native API 绑定自动化）**：解析 ArkUI NDK C 头文件（native_node.h / native_gesture.h / native_animate.h / native_type.h 等），自动生成函数表镜像、枚举、结构体与 P/Invoke 声明，替换 `ArkUIGestureApi.cs` / `ArkUIAnimateApi.cs` / 事件/属性枚举等全部手工维护的原生层代码。要点与难点：
  - C 解析需 libclang（或等价 C 前端），不能用正则——现有 `extract_arkui_types.py` 只抽枚举，函数表/结构体均为手写镜像
  - 语义修正规则须沉淀为声明式配置（生成器不猜）：C bool 在 unmanaged 函数指针中按 1 字节显式 `byte`；头文件注释单位不可信（`GetX` 标注 px 实测为 vp 的笔误）需人工勘误表覆盖；`version` 字段为首成员的函数表布局约定
  - 与 TS 侧生成器（`src/parser`）同仓共存：产物目录、命名归一、`--sdk` 输入路径复用
  - 验收：重出产物与手写版逐签名 diff 为零（除勘误表标注项），全量单测 + 模拟器手势/动画链路回归通过
- **NuGet 打包**：Bindings / HarmonyOS.Maui / 宿主工程模板三件套分发
- **单项目体验**：MSBuild targets 让用户工程 `dotnet build` 直出 HAP（自动跑远程 AOT 或本机 WSL）
- **CI**：Linux runner 出 libapp.so + 签名 + 模拟器回归
- **性能**：XamlC 产物 AOT 体积、启动时间、TSFN 吞吐基线

---

## 平台铁律速查（实测沉淀，实现前必读）

| # | 铁律 | 出处 |
|---|---|---|
| 1 | Node-API 实现库是 `libace_napi.z.so`，`libnapi.so` 不存在 | M0 |
| 2 | `napi_load_module("=@ohos.xxx")` 要求宿主 ArkTS 已 import 该模块（`ohosImports.ets` 登记） | M0 |
| 3 | 可能抛 ArkTS 异常的 napi 调用后必须 `napi_get_and_clear_last_exception`，否则宿主闪退 | M0 |
| 4 | 模块 `napi_value` 须在 handle scope 内转 `napi_ref` 再跨 P/Invoke 持有 | M0 |
| 5 | 两段式 `napi_get_value_string_utf8` 缓冲区必须 `length+1`，否则末字节被裁 | M1 |
| 6 | NODE_ON_CLICK 的参数经 `GetNodeComponentEvent().data[]` 直读，`GetNumberValue` 返回 106108 | M1 |
| 7 | Background 属性是 Brush 体系，与 Graphics.SolidPaint 平行不可混用 | M1 |
| 8 | `IViewHandler` 导出/生成只认入口程序集；`--gc-sections` 会收割 ILC 的 `__modules` section | M1 |
| 9 | Controls 与 Graphics 的颜色类型树平行（SolidColorBrush vs SolidPaint），转换需显式助手 | M1 |
