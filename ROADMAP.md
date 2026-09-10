# ROADMAP —— 后续路线、实现方案与难点留档

> 本文档记录项目当前状态之后的开发路线。每个阶段给出：做什么、怎么做、难点在哪。
> 文中的"平台铁律"均为实测结论（模拟器 x86_64 / API 26），是后续实现的边界条件。

## 当前基线（已完成，详见 README）

- ✅ UI 通道：ArkUI NDK C API（`ArkUI_NativeNodeAPI_1`）→ `ArkUINodeBase` 稳定句柄
- ✅ 服务通道：napi（`napi_load_module("=@ohos.xxx")`）→ `@ohos.deviceInfo` 端到端
- ✅ MAUI Handler 包：21 个 Handler（16 基础 + RefreshView/Picker/DatePicker/TimePicker/CollectionView/CarouselView）+ XAML（XamlC/SourceGen 编译期，NativeAOT 零反射）；代码风格已统一为官方 handler 模式
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

**遗留限制（M1）**：
- Stack 主轴方向的 Options（如竖直 Stack 里的 VerticalOptions）不生效（MAUI 语义复杂，折衷忽略）
- Grid 单元格内非 Fill 对齐不生效（所有子节点按 Fill 充满单元格）；Span>1 的 Auto 轨道不参与实测
- Auto 轨道依赖子节点上一帧自量测（首帧有按控件类型的兜底估算，收敛需 1~2 帧）；内容自身变化不触发重排
- 两套布局的 ZIndex 均按 addChild 顺序，UpdateZIndex 忽略

### 1.3 Window / Navigation（✅ 已完成：轻量栈 + NavigationPage 转接层 / 中）

两级导航均已实测通过（HelloApp，模拟器）：

**① 轻量栈 `HarmonyNavigation.Push/Pop`**（宿主根容器 + 节点保留式切换）：Push→Pop（主页状态完整恢复）→再 Push（计数延续）。原"摘除-恢复"难点已实测排除。

**② NavigationPage 官方协议转接层 `HarmonyNavigationPageHandler`**：根页改为 `new NavigationPage(new MainPage())` 后，业务代码用 MAUI 标准 `Navigation.PushAsync/PopAsync` 即可。实测：PushAsync→visit #1、PopAsync→回主页、再 Push→visit #2（MainPage 实例跨 push/pop 保留）、系统返回键消费 NavigationPage 内栈而非退出应用。

**原理**（对 MAUI 10.0.11 源码取证）：
- 子页 `NavigationProxy.Inner` 由 `NavigableElement.OnParentSet()` 沿父链自动接到 NavigationPage 的 `MauiNavigationImpl`——业务代码零改动
- NavigationPage 把整包导航栈打包为 `NavigationRequest`，经 `Handler.Invoke(nameof(IStackNavigation.RequestNavigation))` 到达 handler
- handler 把 ArkUI 节点栈同步成请求的栈（仅栈顶挂入 ArkStack，低层摘除保留句柄），完成后**必须**回调 `IStackNavigation.NavigationFinished`，否则 `SendHandlerUpdateAsync` 内 await 永久挂起（PushAsync 不返回）
- 无 NavigationPage 时 MAUI 官方语义即抛 "PushAsync is not supported, please use a NavigationPage."（`Window.NavigationImpl`）——宿主返回键先消费 NavigationPage 内栈，再退到根级轻量栈

**剩余**：返回/弹出方向的过渡动画（当前仅新页淡入 250ms）、`Window.Toolbar`（NavigationPage 标题栏由宿主承担，暂无返回按钮 UI，依赖系统返回键/页面内返回按钮）。

**生命周期**：`Appearing/Disappearing` 已透传（含 NavigationPage 内部切换）。MAUI 的 `Page.SendAppearing` 有守卫（父链上须存在 `Parent` 非空的 `IWindow`），宿主以最小逻辑链放行：`Window.Parent = Application`（均为 public API；不设 `Window.Page`，其 setter 会用 `Window.NavigationImpl` 覆写页面的 NavigationProxy.Inner）。实测：主页 2A/1D 随模态开闭精确变化，NavigationPage 内推入页同样触发。

**模态**：`Navigation.PushModalAsync/PopModalAsync` 标准可用。宿主根容器为 ArkStack（后挂者覆盖），模态页覆盖页面之上；`RootNavigationAdapter`（`NavigationProxy` 子类）挂在根页 `NavigationProxy.Inner` 上承接——含 NavigationPage 路径（`MauiNavigationImpl` 未覆写模态调用，经基类转发到适配器）。实测：PushModalAsync→覆盖显示+生命周期正确、PopModalAsync→恢复、系统返回键优先关闭模态。

**切换动画**：绑定 `ArkUI_NativeAnimateAPI_1.animateTo`（`ArkUIAnimateApi.cs`）+ `ArkUINodeBase.AnimateAsync`；轻量栈与模态推入时新页淡入（NODE_OPACITY 0→1，250ms EASE_IN_OUT）。

**Shell 转接结论**：Shell 不在支持计划内（flyout/tab/URI 路由协议太重）。多平台 Shell 应用做鸿蒙适配时，入口改为 NavigationPage/TabbedPage 结构（`MauiHarmonyHost.Run(() => new NavigationPage(...))`）——这正是 NavigationPage 转接层存在的意义；TabbedPage（底部页签）为后续候选。

### 1.4 更多控件 Handler（✅ 已完成 21 个）

已完成 21 个 Handler（16 基础 + RefreshView/Picker/DatePicker/TimePicker + CollectionView/CarouselView M1 版；RadioButton 的 Content 经 Row+Text 包装呈现，GroupName 已接通）。代码风格已统一为官方 handler 模式（`ViewHandler<TVirtualView, TPlatformView>` + 命名 Map 方法）。注意 MAUI 10 核心/Controls 接口差异：DatePicker.Date 等为可空类型，GroupName 仅在 Controls 类型上——虚拟视图类型按 Picker 先例直接用 Controls 具体类型。剩余：Shape/自绘（需 MAUI Graphics 前端）与 CollectionView 虚拟化（NodeAdapter）按需插入。

### 1.5 真机 arm64 验证（小）

工具链已就绪（arm64 libapp.so 一直同步产出）。难点只有签名物料与真机性能观测（AOT 启动时间、GC 表现——`DOTNET_GCHeapHardLimit` 可能需要按真机内存调参）。

---

## M2 —— 服务层完备（异步是核心）

> **2026-09-10 进度**：2.1 已完成——
> - P/Invoke 声明：`napi_create/release/call_threadsafe_function` 已添加到 `NativeNodeApi.cs`
> - `ThreadSafeFunction.cs`：封装 TSFN 生命周期三路径（完成/取消/异常），Promise 回调注册
> - `HarmonySynchronizationContext.cs`：主线程调度器，支持 Post/Send 回到 UI 线程
> - 最小切片：`Promise<T>`→`Task<T>` 映射接通（TypeMapper + CodeGenerator + 47/47 测试）
> - AsyncCallback 支持：`(result: T, err?: Error) => void` → `Task<T>` parser + 生成器
> 2.2 已完成——using 生成、CallMethodVoid 重载、方法折叠、Promise→Task 映射。
> 端到端模拟器验证待手动执行。


### 2.1 TSFN 异步层（大，M2 的核心难点）

**做什么**：让 `@ohos.*` 的 `Promise<T>` / callback 风格 API 在 C# 里以 `Task<T>` 可用。当前全部映射为 `IntPtr` 占位。

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
