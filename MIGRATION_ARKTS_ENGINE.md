# ArkTS 渲染引擎迁移计划（C 原生节点 API 的退出预案）

> 状态：**预案（未激活）**。本文回答一个问题：如果 HarmonyOS 未来移除 ArkUI NDK 的 C 原生节点 API
> （`ArkUI_NativeNodeAPI_*`、native_gesture.h 等），本项目如何迁移到纯 ArkTS 声明式 UI。
> 触发条件：Huawei 官方宣布 native_node.h 弃用时间表 / 新 SDK 移除头文件或运行时符号。

## 0. 结论摘要

1. **迁移是"换引擎"，不是"重写"**。最有价值的资产——Handler 语义层、托管布局、手势→MAUI 事件翻译、
   导航协议转接——全部与节点 API 无关，原样存活。死区只有 `ArkUINodeBase`/`Nodes/*`（命令式包装）、
   `ArkUIGestureApi`（NDK 函数表）和宿主 NodeContent 胶水三处，它们统一收敛在一个漏斗层里。
2. **方案 A（ArkTS 命令总线引擎）为主体**：C# 发指令流，ArkTS 侧声明式引擎持有真实节点树，
   覆盖全部运行时动态语义。**方案 B（编译期 .ets 生成）为静态层优化**：纯 XAML 常量子树在构建期
   直接生成 ArkTS 组件代码，运行时零指令流。两者互补分层，不是二选一；B 独立于 C 节点 API
   （今天就能做），但不能脱离 A 单独承担完整 MAUI 渲染。
3. **真正的硬点只有三个**：同步量测回读（Auto 轨道/手势坐标依赖 `MeasuredSize` 的同步 P/Invoke）、
   手势动态挂接、动画完成回调。三者的现有设计（AREA_CHANGE 驱动 + 幂等快照、触摸流翻译层、
   inline 完成回调）都预留了应对空间。

## 1. 风险面盘点（哪些代码在死区）

| 代码 | 绑定的 API 面 | 死区？ |
|---|---|---|
| `HarmonyOS.Bindings/NativeNode/ArkUINodeBase.cs` + `Nodes/*.cs` | `ArkUI_NativeNodeAPI_1`（createNode/setAttribute/addChild/事件注册） | ❌ 重写为 VirtualNode |
| `NativeNode/ArkUIGestureApi.cs` + `Nodes/Gestures/` | `ArkUI_NativeGestureAPI_1` | ❌ 重写为 ArkTS 声明式手势 |
| `NativeNode/ArkUIAnimateApi.cs` | `ArkUI_NativeAnimateAPI_1.animateTo` | ❌ 换 ArkTS animateTo |
| 宿主 `napi_init.c`（NodeContent 桥） | `OH_ArkUI_GetNodeContentFromWindow` + C 节点填充 | ❌ 改为引擎根组件 |
| `src/HarmonyOS.Maui/Handlers/*`（22 个 Handler + 布局 + 导航） | 只调包装方法 | ✅ 存活 |
| 手势翻译层（`HarmonyGestureManager` 的 Send* 协议回送） | 平台无关 | ✅ 存活（仅事件源更换） |
| `@ohos.*` 绑定、napi 运行时桥（TSFN/Promise） | Node-API（ArkTS 运行时稳定面） | ✅ 完全无关 |

**漏斗纪律（P0，✅ 已执行，2026-09-12）**：Handler 层禁止直接触碰 `ArkUI_NativeNodeAPI_*`。经审计，
当前 Handler/Hosting 层已无函数表直连（全部经包装类型）；纪律以
`tests/dotnet/HarmonyGestureTests/FunnelDisciplineTests`（源码扫描 11 类禁用符号：函数表类、
napi P/Invoke、原始事件/属性结构体、自行 DllImport）**机械强制**——新增直连需求必须先在节点层
加包装方法，违规即测试红。

---

## 2. 方案 A：ArkTS 命令总线引擎（主体方案）

### 2.1 架构总览

```
C# 侧（NativeAOT）                     ArkTS 侧（引擎）
────────────────────                   ──────────────────────────────
VirtualNode（保留 id 句柄）             SceneGraph：@State 节点树
  SetPosition/SetWidth/...   ──指令──→  CommandQueue → diff → 声明式组件参数
  AddChild/RemoveChild        批合并    递归 @Builder 自定义组件渲染
  RegisterEvent(type)        ←─事件──   组件事件/手势回调 → napi 回流（复用
  MeasuredSize (影子缓存)     ←─量测──   ArgsTrampoline/EventListenerRegistry 模式）
  Animate(updates, done)     ──动画──   animateTo + 完成回调回流
```

- **接口不变量**：`VirtualNode` 公开方法签名与今天的 `ArkUINodeBase` 保持一致（差异仅
  "同步原生回读 → 影子快照"），Handler 层迁移趋近于零。
- **指令批处理**：同 UI 帧内的属性写合并为一次 napi 调用（在布局 pass 边界或 16ms 定时冲刷）。
  性能生命线——托管布局一次 Arrange 可写几十个属性，逐条 napi 调用不可接受。

### 2.2 指令集草案

指令经一条批量化通道（`napi_call_function` 携带 record 数组；复用现有 `INapiRecord` 封送）：

| 指令 | 载荷 | 对应今天的调用 |
|---|---|---|
| `Create` | nodeId, nodeType, 初始属性表 | `new ArkXxx()` |
| `SetAttrs` | nodeId, 属性名→值字典（数值/字符串/颜色/边距/…) | 各 SetXxx |
| `SetChildren` | nodeId, 有序 childId 列表 | AddChild/RemoveChild/RemoveAll |
| `SetEvents` | nodeId, 事件类型表 + 回调注册号 | On/Off（NodeEventBus 复合键不变） |
| `AttachGesture` | nodeId, 手势描述（类型/参数/回调注册号） | ArkUIGestureRecognizer.Attach |
| `DetachGesture` | nodeId, 手势注册号 | Detach（池化语义改为指令撤销） |
| `Animate` | 属性快照表 + duration/curve + 完成回调注册号 | Animate(updates, completed) |
| `RequestMeasure` | nodeId | —（新增：主动触发引擎量测回推） |
| 事件回流（引擎→C#） | targetId, eventType, 载荷（坐标/量测值） | ArkUINodeEvent 分发 |
| 量测回流（引擎→C#） | nodeId, measuredW/H（px）+ 密度 | MeasuredSize 读取 |

### 2.3 ArkTS 引擎组件设计

- **SceneGraph**：`Map<number, NodeRecord>` 保留树；NodeRecord = { type, props, children, gestureIds }。
  指令先落 SceneGraph，再统一 diff 出"本轮实际变化"驱动 `@State`。
- **渲染**：单一递归自定义组件 `DynamicNode({ record })`，按 type 分派到具体 ArkUI 组件
  （`Text(record.props.textContent)` / `Stack()` / `Scroll()` ...），属性经一层映射表从记录名转
  ArkUI 参数。组件树由 `@State`/`@ObjectLink` 数组驱动重建——ArkUI 最小 diff 到变化的组件。
- **映射表**：属性映射（`position → .position({x,y})`、`widthPercent → .width('100%')` …）与
  节点类型映射是引擎仅有的两份"知识"，以声明式配置存在（`mapping.ets`），与 M3 NDK 生成器的
  产物可共用同一份源描述。
- **事件**：引擎为组件挂回调 → 统一转发 `emitEvent(targetId, eventType, payload)` 到 napi 侧。
- **手势**：`.gesture()` / `.parallelGesture()` 按指令挂接；Tap/Pan/Pinch/Swipe/LongPress 参数直映；
  保留两条实测铁律——按 (view, taps) 分组防 N² 触发；"不得在回调中销毁"语义由指令撤销替代。

### 2.4 三个硬点的解法

**① 同步量测回读 → 影子缓存**
今天 `node.MeasuredSize`/`GetX` 是同步 P/Invoke。改为：引擎在 ON_AREA_CHANGE / 触摸事件里把量测值
随事件载荷推给 C#，C# 维护每节点快照，`MeasuredSize` 读快照。
- 托管布局已是"AREA_CHANGE 驱动 + `_lastApplied` 幂等"设计，量测迟到不影响收敛语义；
- 首帧兜底估算（按控件类型）保留；收敛帧数可能 +1~2，以 uitest 断言容忍；
- 密度推导（px/vp）由引擎直接随量测下发，替代 MeasuredSize/SizeChange 比值推算。

**② 手势 → 声明式手势动态挂接**
C# 侧触摸流→`SendPan*` 翻译层不变，事件源从 NDK 回调换成引擎手势回调。Pan/Swipe 继续用
"按下记起点、移动累计、抬起收尾"的触摸流模型（引擎回传原始 touch 流即可）。

**③ 动画 → 引擎 animateTo**
`Animate(updates, completed)`：C# 先执行 updates 收集属性变化 → 打包 `Animate` 指令 → 引擎
animateTo → 完成回调回流。"完成回调必须 UI 线程内联"的约定改为"在 napi 回调帧内联"，语义等价。

### 2.5 宿主与挂载

宿主 `Index.ets` 改为：`EntryAbility` → 根组件 `EngineRoot()`，dlopen libapp.so 后经 napi 拉
"初始指令块"重建场景树。NodeContent/C shim 退役；`HarmonyInit`/`HarmonyBuildUI` 导出签名保持
（内部改为引擎通道），EntryAbility 无需感知变化。

---

## 3. 方案 B：编译期 .ets 生成（静态层优化）

### 3.1 它是什么、独立到什么程度

MAUI XAML 本来就是编译期膨胀的——在 SourceGen 阶段额外生成一份 ArkTS 声明代码（`@Component`
结构体），宿主直接加载静态页面树，运行时只对动态部分发指令。

- **相对于 C 节点 API：完全独立。** B 的运行时是 ArkTS 声明式引擎，一行 C 节点 API 都不碰，
  今天就能做（不依赖任何"未来 API"）。
- **相对于完整 MAUI 渲染：不能单独承担。** MAUI UI 本质是运行时动态的——绑定回写、集合增删、
  IsEnabled/属性变更、PushAsync/PopAsync、手势识别器动态增删。静态树无法表达"运行时改变"
  （除非 B 内嵌自己的命令通道——那就变成了 A）。纯 B 只能覆盖"一次性静态页面"。

### 3.2 生成管线

```
XAML ──SourceGen──→ 控件树 IR（编译期已有）
                        ├→ InitializeComponent()（现状，C# 侧）
                        └→ 【新增】静态/动态分区标记
                              ├─ 纯常量子树 → .ets @Component 代码
                              └─ 动态节点   → 动态槽（slot），运行时由引擎接管
.ets 随 stage-host 注入宿主工程 → hvigor 编译 → 首帧直接 renderComponent
```

### 3.3 静态/动态边界判定（B 的核心规则）

| 子树特征 | 处理 |
|---|---|
| 全部属性为 XAML 字面常量、无绑定/无 x:Name/无事件处理器（纯展示：Label 文本、色块、静态图标） | **静态生成** |
| 含 `{Binding}`、`x:Name`（代码访问）、事件处理器、`x:Reference`、模板化内容（CollectionView/CarouselView/ContentPresenter） | **动态槽** |
| 布局容器自身静态、子内容动态（StackLayout 包 ListView） | 容器静态生成 + 槽位占位 |
| 页面根（ContentPage） | 总是动态槽（导航协议需要运行时挂摘） |

边界判错（把动态节点当静态生成）的后果是"改了不生效"——因此默认策略宁滥勿缺：
**只有整棵子树可证明静态才生成**，判据由 SourceGen 从 IR 保守推导。

### 3.4 混合运行时

静态 `.ets` 组件与引擎动态槽共存于同一棵宿主组件树：
- 静态组件上的事件同样需要回流（点击处理器即使 XAML 常量也有 `Clicked`）→ 静态组件统一挂
  引擎事件回流（napi），等价于动态节点的事件路径；
- 导航推入新页 → 新页可能是"静态组件实例 + 动态槽"的组合，由导航转接层（存活资产）驱动；
- 引擎 SceneGraph 中静态子树表现为一个"叶子节点"（无属性指令可达），指令流天然绕过。

### 3.5 构建与调试的额外代价（B 的真实成本）

- **双工具链**：.ets 生成物进宿主工程参与 hvigor 编译，C# 增量构建与 hvigor 增量的失效联动需要编排；
  生成物放 `obj/harmony/host/ets-gen/`（stage-host 已有暂存机制可挂靠）。
- **调试割裂**：静态部分的运行时错误在 ArkTS 侧，无 .NET 栈——需要生成代码带 XAML 源位置注释，
  引擎日志打点对齐（Release 无栈元数据的现状加剧这一点）。
- **XAML 热路径受限**：任何运行时改属性的控件一旦被误判静态即出 bug，故 3.3 的保守判据不可放松。

### 3.6 B 的里程碑

| 阶段 | 内容 | 出口标准 |
|---|---|---|
| B1 | SourceGen 输出树 IR → 静态性分析器（纯函数，可单测） | 分析器单测覆盖边界规则全表 |
| B2 | .ets 代码发射（常量属性直映 + 事件回流挂钩） | 生成的 .ets 经 hvigor 编译通过 |
| B3 | 动态槽协议（槽位注册 → 引擎接管） | 静态页 + 动态混合页 uitest 通过 |
| B4 | 性能对照 | 首帧/静态页滚动 vs 纯引擎基线报告 |

---

## 4. A 与 B 的分工（决策表）

| | 方案 A（引擎） | 方案 B（编译期生成） |
|---|---|---|
| 动态 UI（绑定/集合/导航） | ✅ 全覆盖 | ❌ |
| 静态树运行时开销 | 指令流 + diff | **零**（原生声明式） |
| 首帧性能 | 需重建场景树 | **最好** |
| 工程复杂度 | 中 | 高（双工具链、调试割裂） |
| 实施顺序 | **先做**（语义基座） | 后做（性能优化） |

**执行顺序不可颠倒**：没有 A 的运行时语义（量测/事件/动画回流），B 的动态槽无从挂靠；
B 在没有 A 的情况下只能做演示级静态页面。B 的价值在 A 跑通后兑现：指令流规模从"全 UI"
降到"动态增量"，静态页面首帧与滚动开销归零。

## 5. 总体实施路线（激活时按此推进）

| 阶段 | 内容 | 出口标准 |
|---|---|---|
| **P0 对冲（✅ 已执行，持续保持）** | 漏斗纪律已落地并以 FunnelDisciplineTests 机械强制；uitest/单测保持后端无关；任何新节点能力先加包装 | 测试闸常绿（16 用例含漏斗扫描） |
| **P1 抽象** | 定义 `IPlatformNode` 接口（现 `ArkUINodeBase` 公开面的子集）；宿主挂载点抽象为 `IHostMount` | 全部 Handler 编译于接口之上 |
| **P2 引擎 MVP（A）** | ArkTS SceneGraph + 指令批合并 + 事件回流；Label/Button/StackLayout 跑通 | HelloApp 主页可跑，uitest 通过 |
| **P3 语义完备（A）** | 影子量测缓存、托管布局收敛、手势挂接、动画、导航/模态 | 15 单测 + verify-m1-uitest.sh 全绿 |
| **P4 静态层（B）** | §3.6 的 B1→B4 | 混合页 uitest + 性能对照报告 |
| **回归策略** | 每阶段以 `tests/dotnet`（纯逻辑）+ `verify-m1-uitest.sh`（行为）为闸——两者均不引用节点 API 细节 | — |

**回退预案**：P1 的接口抽象使 C 后端与 ArkTS 后端可并存（按工程属性选择），旧 API 真被移除前
始终有可用的 C 路径。

## 6. 概率判断

C NDK API 是 ArkUI-X 跨平台与游戏引擎接入的官方通道，整体移除概率低；**现实威胁是 API 演进**
（结构体加字段、函数表版本升级、属性枚举重组——gesture API 的 version 首成员已是这种痕迹）。
漏斗层使这类演进同样只改一处。P0 对冲已执行（机械闸测试）；P1~P4 在触发条件出现时再启动。
