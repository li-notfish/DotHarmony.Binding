# Handler 适配指南

本文指导如何为剩余的 MAUI 控件适配 HarmonyOS Handler，以及如何新增 `@ohos.*` 系统模块绑定（deviceInfo 模式）。
适配前请先读 [README.md](README.md) 了解整体架构，[ROADMAP.md](ROADMAP.md) 了解里程碑状态。

---

## 1. 三层分工（改代码前先确认改哪层）

```
MAUI 控件 (VirtualView)                你要写的 Handler（本文重点）
      │ PropertyMapper / CommandMapper           │  属性/命令/事件翻译
      ▼                                          ▼
src/HarmonyOS.Maui/Handlers/*  ──────►  src/HarmonyOS.Bindings/Nodes/*（ArkUI 节点类）
                                         │  NODE_* 属性枚举 → SetAttribute
                                         ▼
                                libace_ndk.z.so（ArkUI C API，节点树渲染）

@ohos.* 系统模块（deviceInfo 等，与 UI 无关的服务 API）
      ▼
src/HarmonyOS.Bindings/Api/*  ──napi（经 HarmonyOS.Interop）──►  libace_napi.z.so（napi_load_module / call_function）
```

| 层 | 目录 | 职责 | 什么时候改 |
|---|---|---|---|
| 节点层 | `src/HarmonyOS.Bindings/Nodes/` | ArkUI 组件的 C# 包装（属性 setter、事件） | 目标属性/事件在节点类上不存在时 |
| Handler 层 | `src/HarmonyOS.Maui/Handlers/` | MAUI ↔ ArkUI 的属性/命令/事件翻译 | **适配新控件的主要工作** |
| API 层 | `src/HarmonyOS.Bindings/Api/` | `@ohos.*` 模块绑定（napi 通道） | 绑定系统服务 API 时 |
| 宿主 | `samples/HarmonyHost/` | ArkTS 壳（ContentSlot 挂载 + ohosImports.ets 模块登记） | 新增 `@ohos.*` 模块绑定时同步登记 |

**漏斗纪律（P0，机械闸强制）**：Handler/Hosting/Essentials 层（拆装后 FunnelDisciplineTests 扫双目录）只允许经包装类型触碰 ArkUI——节点包装类、
`ArkUINodeEvent`/`ArkUIPointerEvent` 载荷包装、手势包装类、事件枚举。禁止直连原生函数表类
（`ArkUINativeApi`/`ArkUIGestureApi`/`ArkUIAnimateApi`）、napi P/Invoke（`NativeNodeApi`/`OH_ArkUI_*`/
`napi_*`）、原始事件/属性结构体（`GetNodeComponentEvent`/`ArkUI_AttributeItem`）与自行
`DllImport/LibraryImport`。需要新原生能力时**先在节点层加包装方法**。
这道闸由 `tests/dotnet/HarmonyGestureTests/FunnelDisciplineTests`（源码扫描）强制执行——它是
渲染后端可替换（ArkTS 引擎迁移预案）的前提，违规测试即红。详见 [MIGRATION_ARKTS_ENGINE.md](MIGRATION_ARKTS_ENGINE.md) §1。

---

## 2. 适配一个新 Handler 的流程（五步）

以 **Slider** 为例走完全程。

### Step 1：确认 ArkUI 节点类是否够用

节点类在 `src/HarmonyOS.Bindings/Nodes/`，目前由 `tools/api-generator/nativeCodeGenerator.ts` 从 SDK `.d.ts` 自动产出（已有 button/checkbox/column/date_picker/flex/grid/image/list/progress/radio/refresh/row/scroll/slider/span/stack/swiper/text/text_area/text_input/text_picker/time_picker/toggle/xcomponent 共 24 个）。查两个地方：

- `src/HarmonyOS.Bindings/NativeNode/ArkUINodeTypes.g.cs` 里 `ARKUI_NODE_*` 枚举 —— 确认目标组件类型存在（如 `ARKUI_NODE_SLIDER`）；
- `Nodes/native-gaps.json` —— 生成器登记的"属性存在但 shape 未注册"缺口。

上述 24 个组件的节点类已由生成器自动产出（含属性、事件、构造参数），**无需手写**。对于不在 SDK `.d.ts` 中的自定义组件或生成器未覆盖的属性，可手写补充节点类：

```csharp
// src/HarmonyOS.Bindings/Nodes/slider.cs —— 手写节点类模板
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

/// <summary>Slider 组件（ARKUI_NODE_SLIDER）</summary>
public unsafe class Slider : ArkUINodeBase
{
    public Slider() : base(ArkUI_NodeType.ARKUI_NODE_SLIDER) { }

    /// <summary>取值（NODE_SLIDER_VALUE）</summary>
    public float Value
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_SLIDER_VALUE, ArkUIValue.F(value));
    }

    /// <summary>最小值（NODE_SLIDER_MIN_VALUE）</summary>
    public float MinValue
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_SLIDER_MIN_VALUE, ArkUIValue.F(value));
    }

    /// <summary>最大值（NODE_SLIDER_MAX_VALUE）</summary>
    public float MaxValue
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_SLIDER_MAX_VALUE, ArkUIValue.F(value));
    }

    /// <summary>onChange 事件（NODE_SLIDER_EVENT_ON_CHANGE：data[0].f32=当前值 data[1].i32=触发状态）</summary>
    public event Action<ArkUINodeEvent>? ValueChanged
    {
        add => On(ArkUI_NodeEventType.NODE_SLIDER_EVENT_ON_CHANGE, value!);
        remove => Off(ArkUI_NodeEventType.NODE_SLIDER_EVENT_ON_CHANGE);
    }
}
```

要点：

- 属性枚举名在 SDK 头文件 `native_node.h` 的 `ArkUI_NodeAttributeType` 里查（SDK 路径见 §5），注释里写明了每个属性的 `ArkUI_NumberValue[]` 布局（几个值、每个是 f32/i32/u32）；
- 颜色统一 `0xAARRGGBB` 的 u32（见基类 `SetBackgroundColor` 的写法）；
- **事件必须在节点类包装成 `event`**：`On()/Off()` 是 protected，Handler 无法直接调用（这是有意设计——事件注册要走基类的事件总线登记，绕过会收不到回调）。

### Step 2：写 Handler

模板（对照 `HarmonySliderHandler.cs`，当前代码风格）：

```csharp
// src/HarmonyOS.Maui/Handlers/HarmonySliderHandler.cs
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkSlider = HarmonyOS.ArkUI.Slider;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Slider 的 HarmonyOS Handler（ArkUI Slider 节点）。</summary>
public class HarmonySliderHandler : ViewHandler<Microsoft.Maui.Controls.Slider, ArkSlider>
{
    public static PropertyMapper<Microsoft.Maui.Controls.Slider, HarmonySliderHandler> Mapper =
        new(ViewMapper)
        {
            [nameof(Microsoft.Maui.Controls.Slider.Minimum)] = MapMinimum,
            [nameof(Microsoft.Maui.Controls.Slider.Maximum)] = MapMaximum,
            [nameof(Microsoft.Maui.Controls.Slider.Value)] = MapValue,
        };

    public HarmonySliderHandler() : base(Mapper) { }

    protected override ArkSlider CreatePlatformView() => new();

    protected override void ConnectHandler(ArkSlider platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ValueChanged += OnValueChanged;
    }

    protected override void DisconnectHandler(ArkSlider platformView)
    {
        platformView.ValueChanged -= OnValueChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapMinimum(HarmonySliderHandler h, Microsoft.Maui.Controls.Slider v)
    {
        h.PlatformView.MinValue = (float)v.Minimum;
    }

    public static void MapMaximum(HarmonySliderHandler h, Microsoft.Maui.Controls.Slider v)
    {
        h.PlatformView.MaxValue = (float)v.Maximum;
    }

    public static void MapValue(HarmonySliderHandler h, Microsoft.Maui.Controls.Slider v)
    {
        var clamped = Math.Clamp(v.Value, v.Minimum, v.Maximum);
        h.PlatformView.Value = (float)clamped;
    }

    private void OnValueChanged(ArkUINodeEvent e)
    {
        // data[0].f32 = current value（见 NODE_SLIDER_EVENT_ON_CHANGE 注释）
        var value = (double)e.ComponentData(0).f32;
        if (VirtualView.Value == value) return;
        VirtualView.Value = value;
    }
}
```

**三条规则：**

1. **属性进 `PropertyMapper`**：key 用 `nameof(控件属性)`，值是命名静态 `MapXxx` 方法（**不要**内联 lambda）；
2. **子树增删进 `CommandMapper`**：只有"容器型"控件需要（见 §3 布局三分法）；
3. **事件在 `ConnectHandler` 订阅、在 `DisconnectHandler` 取消订阅**：ArkUI 事件 → 调 MAUI 控件的命令方法（如直接修改 `VirtualView.Value` 触发属性变更）。

**命名 Map 方法 vs 内联 lambda 的选择：**

- 当前代码统一使用**命名静态 Map 方法**（`MapText`、`MapValue` 等），便于单元测试和日志追踪；
- 如果属性只是简单一行赋值，也可写内联 lambda，但请保持与现有代码风格一致。

### Step 3：注册到工厂

两条路径：

**A. 内置控件（改本仓库）**：`src/HarmonyOS.Maui/Handlers/HarmonyHandlerFactory.cs` 的 switch 里加一行。**注意顺序：子类在前**（`Grid` 在 `Layout` 前，否则永远命中基类分支）：

```csharp
Microsoft.Maui.Controls.Slider => new HarmonySliderHandler(),
```

**B. 应用/第三方库自定义 Handler（不改本仓库）**：开放注册表，优先于内置分派，
沿类型继承链向上找最近注册（鸿蒙宿主不走官方 `MauiAppBuilder`，此为 `ConfigureMauiHandlers` 的最小等价物）：

```csharp
// 应用启动时（MauiHarmonyHost.Run/RunApplication 之前）
HarmonyHandlerFactory.Register<MyCustomView>(() => new MyCustomViewHandler());

// 第三方库需要的托管服务（Handler.MauiContext.Services 解析口）：
HarmonyMauiContext.RegisterService<IMyService>(new MyService());
```

**C. 最小官方 `MauiAppBuilder` 等价引导（HarmonyMauiAppBuilder）**：API 形状对齐 MAUI 模板——
`ConfigureMauiHandlers`（`AddHandler<TView, THandler>`，落到上面的开放注册表）+ `ConfigureServices`
（标准 `IServiceCollection`，Build 后作为 `HarmonyMauiContext` 链式解析末段：
内置 AnimationManager → RegisterService 注册表 → DI 容器）：

```csharp
var builder = HarmonyMauiAppBuilder.CreateBuilder();
builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<MyView, MyHandler>());
builder.ConfigureServices(services => services.AddSingleton<IMyService, MyService>());
builder.Build();
MauiHarmonyHost.RunApplication(() => new App());
```

**CommunityToolkit 兼容性**（实测 CommunityToolkit.Maui 15.0.1，net10.0 + NativeAOT）：
平台无关组件（Converters / Behaviors / `CommunityToolkit.Mvvm`）开箱即用，无需任何注册
（见 HelloApp GridProbe 页的 `InvertedBoolConverter` 实机验证）；带平台 Handler 的控件
（DrawingView/Popup/CameraView/MediaElement 等）在 net10.0 基础包中只有 stub，
会抛 `PlatformNotSupportedException`——需按本指南 B/C 路径逐个编写鸿蒙 Handler 后才可用。
MCT 的 `UseMauiCommunityToolkit()` 扩展绑定官方 `MauiAppBuilder` 类型，不能直接用于
`HarmonyMauiAppBuilder`；其内部注册逻辑可手工展开为 `ConfigureMauiHandlers` 委托。

### Step 4：属性映射核对清单

> **基座收口（HarmonyViewMapper.Base）**：所有 Handler 的 `Mapper` 一律 `new(HarmonyViewMapper.Base)`，
> 不要在各 handler 重复映射 `WidthRequest/HeightRequest/Background/Padding`——基座已统一落地
> （显式值 SetWidth/SetHeight，-1 复位 Auto；Background 走 BrushHelper；Padding 非零才下发）。
> 需要特化（如 ContentView/Frame 的水平栈 auto 宽）时在自身 Mapper 写同名键覆盖即可（链式语义：外层优先）。
> 视觉属性（Visibility/IsEnabled/Opacity/Transform/Anchor/InputTransparent）由 `HarmonyViewHandler.UpdateValue`
> 拦截，同样不要进 mapper。
>
> **一致性门槛**：`dotnet run --project tools/HandlerAudit` 会比对工厂每个分派类型的接口继承链
> 属性全集与 handler 实际映射键，未在 `tools/HandlerAudit/baseline.json` 登记原因的缺口使 CI 失败；
> 同时静态检查工厂 switch 臂的继承拓扑（派生类必须排在基类臂之前）。报告见 `docs/handler-coverage.md`。

| MAUI 概念 | ArkUI 翻译 | 备注 |
|---|---|---|
| `Color` / `Brush` | `(byte)r,(byte)g,(byte)b,(byte)a` → `0xAARRGGBB` | 纯色走 `BrushHelper.TryGetColor`；背景画刷走 `BrushHelper.ApplyBackground`（LinearGradient→NODE_LINEAR_GRADIENT、RadialGradient→NODE_RADIAL_GRADIENT；ImageBrush 为 MAUI internal 类型不映射） |
| `BackgroundColor`（XAML 属性） | 直接读 `v.BackgroundColor` 纯色路径 | **勿**在 BackgroundColor 映射里读 `v.Background`——XAML 只设 `VisualElement.BackgroundColor`，与 `Background`（Brush）不互通，读到 null 静默丢色 |
| `FontSize` | `NODE_FONT_SIZE`（f32，vp） | |
| `WidthRequest`/`HeightRequest` | `SetWidth/SetHeight`（vp） | 负值 = 未设置，跳过 |
| 水平 Fill | `SetWidthPercent(1.0f)` | 见 `HarmonyLayoutHandler.AttachChild` |
| `TextAlignment` | `NODE_TEXT_ALIGN` + 枚举 `ArkUI_TextAlignment` | **别用** `ArkUI_Alignment`（那是九宫格对齐，曾报 401） |
| `IsVisible` | `Visible`（NODE_VISIBILITY） | |
| `CornerRadius` | `NODE_BORDER_RADIUS`（四值 f32） | |
| 事件 `Clicked` | `NODE_ON_CLICK` → `VirtualView.SendClicked()` | |

### Step 5：验证链路

```bash
# 0. 一键（宿主自动 stage → libapp.so → HAP → 部署，见 GETTING_STARTED §3）
dotnet build samples/dotnet/HelloApp/HelloApp.csproj -t:HarmonyRun

# 或分步：
# 1. 编译（Windows 本地即可验证 C#/XAML）
dotnet build samples/dotnet/HelloApp/HelloApp.csproj
# 2. NativeAOT → 双架构 libapp.so（PublishAotClang，Windows 本地）
dotnet publish samples/dotnet/HelloApp/HelloApp.csproj -c Release -r linux-musl-arm64
dotnet publish samples/dotnet/HelloApp/HelloApp.csproj -c Release -r linux-musl-x64
# 3. 打包 HAP
cmd //c "scripts\build-hap.cmd"
# 4. 部署模拟器（按需）
bash scripts/deploy-hap.sh
```

在 `samples/dotnet/HelloApp/` 加一个演示页（参照 `LayoutDemoPage.xaml` + MainPage 入口按钮），确认：属性渲染、事件回流、导航 Push/Pop 状态恢复。

---

## 3. 布局类 Handler：先选模型

| 模型 | 适用 | 平台视图 | 参照 |
|---|---|---|---|
| **flex 托管** | 子节点线性排列语义能对上 ArkUI 容器 | `Column`/`Row` | `HarmonyLayoutHandler`（StackLayout） |
| **MAUI 托管** | 轨道/绝对定位语义无法映射 flex | `Stack` + `NODE_POSITION` 绝对定位 | `HarmonyManagedLayoutHandler`（Grid/AbsoluteLayout） |
| **复合内容** | 单 Content 子节点 | 容器 + MapContent 装配 | `HarmonyContentPageHandler` |

容器型 Handler 必须实现 **ILayoutHandler 子树协议**（CommandMapper 字符串命令，Controls 侧 Children 变化时派发）：

```csharp
public static CommandMapper<MLAYOUT, HarmonyLayoutHandler> LayoutCommandMapper = new(ViewCommandMapper)
{
    [nameof(ILayoutHandler.Add)] = MapAdd,
    [nameof(ILayoutHandler.Remove)] = MapRemove,
    [nameof(ILayoutHandler.Clear)] = MapClear,
    [nameof(ILayoutHandler.Insert)] = MapInsert,
    [nameof(ILayoutHandler.Update)] = MapUpdate,
    [nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
};
```

另两个易漏点（都踩过）：

- `ConnectHandler` 里**全量同步已存在的 Children** —— Handler 连接之前加进 Controls 树的子节点不会补发 Add 命令；
- MAUI 托管布局需要容器尺寸：订阅 `NODE_ON_SIZE_CHANGE`（vp 值在 `e.SizeChangeWidth/Height`），不要用 px 的 `MeasuredSize` 直接当 vp。

MAUI 托管布局的对齐/ZIndex 约定（2026-09-12 落地）：

- **单元格内对齐**：读 `view.HorizontalLayoutAlignment/VerticalLayoutAlignment`（`IView` 接口属性，
  Controls 侧自动从 `HorizontalOptions/VerticalOptions` 的 Alignment 映射），非 Fill 调
  `HarmonyManagedLayoutHandler.ApplyAlignment` 收缩偏移（internal static，纯逻辑已单测）；
  Fill/内容未量测/内容≥frame 保持充满。XAML 写 `HorizontalOptions` 而非 `HorizontalLayoutAlignment`
  （后者在 Controls 上无 BindableProperty，MAUIX2002）
- **ZIndex**：`ArkUINodeBase.SetZIndex(float)`（NODE_Z_INDEX=21）；flex 托管与 MAUI 托管两套布局
  均接 `MapUpdateZIndex` 命令 + AttachChild 初始同步；managed 侧每次 Arrange 全量下发
- **Auto 轨道自适应**：AttachChild 时订阅子节点 `NODE_EVENT_ON_AREA_CHANGE` → Arrange
  （`_arrangeQueued` 同帧合并）；Remove/Clear 注销；Arrange 用 `_lastApplied` 快照做幂等跳过
  （AREA_CHANGE 风暴下不空转）

---

## 4. `@ohos.*` API 绑定（生成器产出，M2 完成）

UI 之外的系统服务（通知、振动、网络、设置项……）走 napi 通道。**模块绑定全部由生成器产出**（`src/HarmonyOS.Bindings/Api/`，438 个模块 / 438 个全量转正编译；`GRAYSCALE_MODULES` 当前为空），不要手写——下面的铁律是生成器与运行时已经实现的约束，排查问题时读。

### 4.1 生成与转正流程

```bash
# 全量生成（449 个 d.ts → 438 模块）；不带 --all 只处理 PILOT_MODULES 白名单
npx ts-node tools/api-generator/index.ts --sdk "<SDK 路径>" --all

# 转正策略：--all 模式默认全部转正，GRAYSCALE_MODULES（tools/api-generator/index.ts）里的回灰；
# 修复某模块的类型映射缺口后把它从黑名单移除即可
dotnet build ArkTsBinding.slnx
```

产物形态：`static unsafe partial class DeviceInfo`（模块类，Module 句柄懒加载）+ 嵌套接口/类的包装类（`JsObject` 派生）与入参 record（`INapiRecord`）。

### 4.2 封送能力（TypeMapper 单一事实源）

| ArkTS | C# | 说明 |
|---|---|---|
| `Promise<T>` | `Task<T>` | PromiseTaskBridge；reject → `ArkTSException`；**续体在 JS 线程内联恢复** |
| 仅 callback 形式 `AsyncCallback<T>` | `Task<T>` | CallbackTaskBridge（err-first 回调作末参传入） |
| `on/off/once(type, cb)` | .NET `event` + 类型化 `On/Off/Once` | EventListenerRegistry 按函数实例配对 |
| `ArrayBuffer`/TypedArray | `byte[]`（拷贝） | 内容逐字节往返已实测 |
| `bigint` / `Map<K,V>` | `JsBigInt` / `JsMap<K,V>` 活视图 | 超出 int64/uint64 抛异常 |
| 跨模块 `import type` | `IntPtr` 句柄（降级） | 强类型跨模块解析未实现 |

### 4.3 铁律（生成器已实现，手写扩展时必须遵守）

1. **库名**：napi P/Invoke 全部走 `libace_napi.z.so`（`libnapi.so` 不存在）；
2. **模块加载格式**：先试 `"=@ohos.xxx"`（系统模块前缀），失败回退不带 `=`；
3. **异常清理**：napi 调用失败后**必须 `napi_get_and_clear_last_exception`**，否则 pending 异常传回宿主执行流直接闪退；
4. **线程亲和**：`NapiEnv.Current` 是 `[ThreadStatic]`——napi 调用必须在 JS（宿主主）线程；后台线程经 TSFN 通道（`ThreadSafeFunction`，finalize 延迟释放防 UAF）；
5. **字符串读取**：先取 length，缓冲区 **`length + 1`** 字节（两段式），否则 UTF8 截尾（HUAWEI→HUAWE）；
6. **async void 兜底**：事件处理器必须 `catch (Exception)`——async void 中未处理异常直接杀进程。

### 4.4 宿主模块登记（自动）

生成器自动维护 `samples/HarmonyHost/entry/src/main/ets/ohosImports.ets`（re-export 所有已生成模块），并同时写 `module.json5` 的 `requestPermissions` 与 `string.json` 的权限 reason 资源。手动登记仅在调试新模块时需要。

---

## 5. 代码风格速查（与官方 Handler 对齐）

| 项 | 规范 | 反例 |
|---|---|---|
| `ViewHandler` 泛型 | 核心接口优先（`ISlider`/`IEntry`）；**接口缺属性时回退具体类型**（`Label.HorizontalTextAlignment` 不在 `ILabel` 上） | `ViewHandler<Microsoft.Maui.Controls.Button, ...>`（冗长） |
| PropertyMapper | `new(ViewHandler.ViewMapper)` | `new(ViewMapper)`（本仓当前未使用该写法） |
| Mapper key | `[nameof(Button.Text)]` | `[("Text")]`（字符串硬编码） |
| Mapper value | 命名静态方法 `MapText` | 内联 lambda `(h, v) => ...`（不利于测试和堆栈） |
| Map 方法签名 | `MapText(HarmonyButtonHandler h, Button v)` | `MapText(IButtonHandler h, IButton v)`（接口不存在的属性访问不到） |
| 颜色转换 | `(byte)(c.Red * 255), ...` | `new Color(r, g, b)`（与 ArkUI 值域不一致） |
| Background（Brush） | `BrushHelper.ApplyBackground(h.PlatformView, v.Background)` | 只处理 Brush 忘了 BackgroundColor |
| **BackgroundColor（Color）** | **直接读 `v.BackgroundColor` 走纯色路径**——XAML `BackgroundColor="X"` 只设置 `VisualElement.BackgroundColor`，**不会**同步到 `Background`（两者是独立 BindableProperty，读 `v.Background` 会拿到 null 静默丢色） | `BrushHelper.ApplyBackground(h.PlatformView, v.Background)`（BackgroundColor 映射里读 Brush） |
| 日志 | `HiLog.Debug/Warn("Tag", ...)`（模拟器 hilog 可见） | `System.Diagnostics.Debug.WriteLine`（release 上不可见）；常规路径用 Info 级刷屏 |
| 事件订阅 | `platformView.Click += OnClick` | `platformView.Click += _ => ...`（匿名委托无法取消订阅） |
| 事件取消 | `platformView.Click -= OnClick`（同名方法） | `platformView.Click -= _ => { }`（lambda 不是同一个委托，取消无效） |
| 事件命令 | `VirtualView.SendClicked()` / `VirtualView.SendCompleted()` | `VirtualView.Clicked()`（事件不能当方法调用） |
| 无固有尺寸的控件 | Toggle/CheckBox/Radio 等在 Row 中无约束会异常放大，`CreatePlatformView` 里显式 `SetWidth/SetHeight`（Switch 50×26、CheckBox/Radio 24×24） | 依赖 flex 自然约束（ArkUI 不给默认尺寸） |

---

## 6. 剩余 Handler 清单

ArkUI 节点类型枚举已全部生成（`ArkUINodeTypes.g.cs`），缺的只是 `Nodes/` 下的组件类和 Handler。按"价值/难度"排序：

| 优先 | MAUI 控件 | ArkUI 节点 | 难点 | 状态 |
|---|---|---|---|---|
| ★★★ | `Entry` | `ARKUI_NODE_TEXT_INPUT` | TextChange/焦点/键盘弹出 | ✅ 已完成 |
| ★★★ | `Image` | `ARKUI_NODE_IMAGE` | 资源管道 | ✅ 已完成 |
| ★★★ | `ScrollView` | `ARKUI_NODE_SCROLL` | 单 Content 子节点 | ✅ 已完成 |
| ★★☆ | `Switch` | `ARKUI_NODE_TOGGLE` | CHANGE 事件回流 | ✅ 已完成 |
| ★★☆ | `CheckBox` | `ARKUI_NODE_CHECKBOX` | CHANGE 事件回流 | ✅ 已完成 |
| ★★☆ | `RadioButton` | `ARKUI_NODE_RADIO` | SDK 无 NODE_RADIO_CONTENT：平台视图为 Row（圆点+Text）呈现 Content | ✅ 已完成 |
| ★★☆ | `ProgressBar` | `ARKUI_NODE_PROGRESS` | 直线进度 | ✅ 已完成 |
| ★★☆ | `Slider` | `ARKUI_NODE_SLIDER` | — | ✅ 已完成 |
| ★★☆ | `Editor` | `ARKUI_NODE_TEXT_AREA` | 同 Entry | ✅ 已完成 |
| ★★☆ | `Border`（含废弃的 `Frame`） | `ARKUI_NODE_STACK` | flex 托管；工厂只注册 `Border`（Frame 已废弃，XAML 用 Border） | ✅ 已完成 |
| ★☆☆ | `CollectionView` | `ARKUI_NODE_LIST` + NodeAdapter | **虚拟化**：条目按可见范围物化（ON_ADD/ON_REMOVE 事件回调，ItemTemplate 经 CreateContent，AOT 安全）；ItemsSource 变更走 SetTotalCount+ReloadAllItems；2026-09-17 模拟器实测（200 条目仅物化可见 7 条，滚动按需前进/回滚复现） | ✅ 已完成 |
| ★☆☆ | `CarouselView` | `ARKUI_NODE_SWIPER` | 全量物化子视图；需显式高度（HeightRequest）；Loop/位置回传暂略 | ✅ M1 已完成 |
| ★☆☆ | `Picker`/`DatePicker`/`TimePicker` | `ARKUI_NODE_TEXT_PICKER`/`DATE_PICKER`/`TIME_PICKER` | ArkUI 是内嵌滚轮非弹窗，视觉有差异；节点类已入解析器生成管线（NODE_TYPE_NAME_FIXES + CONSTRUCTOR_OPTION_PROPS，PickerManual/TextPickerManual.cs 已删除）；MAUI 10 的 Date/Time 为可空类型 | ✅ 已完成 |
| ★☆☆ | `RefreshView` | `ARKUI_NODE_REFRESH` | 下拉经 NODE_REFRESH_ON_REFRESH 置 IsRefreshing；刷新态 setter 已入生成管线（CONSTRUCTOR_OPTION_PROPS，RefreshManual.cs 已删除） | ✅ 已完成 |
| ★☆☆ | `BoxView` | `ARKUI_NODE_STACK` | 纯色矩形（Color/BackgroundColor → 背景色） | ✅ 已完成 |
| ★☆☆ | `ContentView` / `ContentPresenter` | `ARKUI_NODE_COLUMN` | 模板宿主（PresentedContent 槽）；空 Content 的槽位 HIT_TEST_MODE_NONE 防输入黑洞；Padding 已映射 NODE_PADDING | ✅ 已完成 |
| ★☆☆ | `Shell` | 根 `ARKUI_NODE_STACK`（主列 + 遮罩 + Flyout 面板） | 平台 fragment 机制自建（HarmonyShellNavigation：条目/路由/推送栈/query/模态转发）；Flyout 菜单、主题色、FlyoutIsPresented/FlyoutBehavior/NavBarIsVisible/TabBarIsVisible 已覆盖（Locked 为并排常驻：主列右移让宽，无遮罩） | ✅ 第一版完成 |
| ☆ | `Shape`/自绘 | `ARKUI_NODE_CUSTOM` + `NODE_ON_DRAW` | MAUI Graphics → OH_Drawing 适配；文本/渐变/位图/测量路径仍有部分高级能力待补 | ✅ 已完成（高级能力继续补） |
| ☆ | GestureRecognizers（Tap/Pan/Pinch/Swipe/Pointer） | NDK `ArkUI_NativeGestureAPI_1` + `NODE_TOUCH_EVENT` | 手势→MAUI Send* 协议回送；Tap/Pointer 走反射桥 | ✅ 已完成（见 §6.1） |

**查询属性枚举值**：SDK 头文件
`%LOCALAPPDATA%/OpenHarmony/Sdk/<ver>/native/sysroot/usr/include/arkui/native_node.h`
的 `ArkUI_NodeAttributeType` 注释——每个枚举项写明了参数个数与类型（`ArkUI_NumberValue[]` 布局），这是写节点类 setter 的唯一权威来源。

### 6.1 手势识别（GestureRecognizers）适配要点

MAUI 的手势平台管线在 netstandard Controls 产物中是 internal 空实现，平台侧由本仓库自建。给**新 Handler 启用手势**只需一件事：基类从 `ViewHandler<TVirtualView, TPlatformView>` 换成 `HarmonyViewHandler<TVirtualView, TPlatformView>`（`ConnectHandler` 内自动挂 `HarmonyGestureManager`）。注意：

- 仅 `Microsoft.Maui.Controls.View` 的子类有 `GestureRecognizers`；Page Handler（ContentPage/NavigationPage）保持直接继承 ViewHandler；
- `ConnectHandler` 里如果自己还要订阅节点事件，放在 `base.ConnectHandler(platformView)` 之后即可，与手势管理器互不干扰；
- `HarmonyGestureManager` 监听 `CompositeGestureRecognizers` 集合变化与 `IsEnabled/InputTransparent`，全量重建原生手势——不要在 Handler 里手工管理手势生命周期。

分层文件：NDK 函数表 `src/HarmonyOS.Bindings/NativeNode/ArkUIGestureApi.cs`（镜像自 native_gesture.h，C bool 按字节用 `byte` 表达）、公共指针包装 `ArkUIPointerEvent.cs`（经 `ArkUINodeEvent.InputEvent` → `ArkUIPointerEvent.From(IntPtr)`，坐标 px）、托管包装 `src/HarmonyOS.Bindings/Nodes/Gestures/`、翻译层 `src/HarmonyOS.Maui/Handlers/HarmonyGestureManager.cs`、反射桥 `MauiGestureBridge.cs`（`TapGestureRecognizer.SendTapped` / `PointerGestureRecognizer.SendPointer*` 为 MAUI internal，`[DynamicDependency]` 收根 + `CreateDelegate` 缓存，**不得**改为逐次 `MethodInfo.Invoke`）。

| MAUI 识别器 | 原生通道 | 说明 |
|---|---|---|
| TapGestureRecognizer | `ArkTapGesture(NumberOfTapsRequired)` | Accept 动作触发；`GetChildElements` 命中的子元素识别器（Label Span）优先；**同视图按连击数分组共享一个原生手势**（RaiseTap 广播，勿按识别器逐个挂——会 N² 重复触发） |
| PanGestureRecognizer | `NODE_TOUCH_EVENT` 触摸流 | 实测 pan 原生手势 offset 在 END 为 0、UPDATE/END 输入位置不可靠；按下记起点、移动累计、抬起收尾；`TotalX/Y` 单位 **vp**（等价 iOS points），Completed 事件本身不带坐标（MAUI 官方语义） |
| SwipeGestureRecognizer | 同上触摸流 | 累计位移喂 `SendSwipe`，抬起时 `MapSwipeDirection` → `DetectSwipe`（阈值判定在识别器内部） |
| PinchGestureRecognizer | `ArkPinchGesture(2)` | `GetScale` 为累计系数，直接透传 |
| PointerGestureRecognizer | 同一触摸流 | Down→Entered+Pressed、Move→Moved、Up→Released；hover/mouse 通道待补 |
| Drag/Drop 识别器 | `NODE_ON_DRAG_*` / `NODE_ON_DROP` + `libudmf.so` | 文本载荷可拖拽；UDMF 指针须持有至 `NODE_ON_DRAG_END` 再销毁 |

**原生 recognizer 生命周期铁律**：不得在事件分发回调内 `dispose()` recognizer——dispose 后原生管线仍派发事件（SIGSEGV UAF，实测）。重建场景 Detach + 入池复用（`HarmonyGestureManager.Rebuild`），dispose 仅在 Handler 断连时统一执行。

单测：`tests/dotnet/HarmonyGestureTests`（xunit，`dotnet test` 跑）——反射桥全链路、Swipe/Drag&Drop 映射、Grid/ZIndex/ScrollView/Shell 路由、Essentials 映射与 Preferences 编解码等，共 120 用例（含漏斗纪律源码扫描闸）；另有 `HarmonyEngineTests` 14 用例。

### 6.2 Essentials 注入适配要点

MAUI 的 Essentials 静态类（`DeviceInfo.Current`/`DeviceDisplay`/`AppInfo`/`Clipboard`）在 netstandard 产物中默认实现全部抛异常，平台侧经 internal `SetCurrent`/`SetDefault` 注入点装载。本仓库实现：`HarmonyEssentials.Install()`（反射桥模式，同手势桥：`[DynamicDependency]` 收根 + `CreateDelegate` 缓存）+ `Harmony*` 实现（`src/HarmonyOS.Essentials/`——Essentials 已独立成装）。注意：

- **装载时机是铁律**：`Install()` 必须在 `MauiHarmonyHost.Run` 的 `RootBuilder` lambda 内调用（UI 线程首次构建时，napi env 已就绪）——放 ModuleInitializer（dlopen 时）会因 napi 未初始化直接闪退（实测 DfxFaultLogger 崩在 HarmonyInit+16）；
- 实现类构造时即创建底层包装对象（如 `HarmonyClipboard` 构造时取 SystemPasteboard）——构造发生在 Install 时（napi 就绪），但**包装句柄跨时长持有会失效**（见 §7 PinnedValue 行）；
- 剪贴板读权限（API 26 起 `READ_PASTEBOARD` 为 user_grant）完整链路见 `HarmonyClipboard`：读前 `getSelfPermissionStatus` 主动查状态 → 未授权经 host 导出的 `globalThis.abilityContext` 发 `requestPermissionsFromUser` 弹窗 → 重试读。**宿主模板 EntryAbility.ets 已导出 `globalThis.abilityContext`**（自建宿主缺这行则剪贴板读永远拿不到授权弹窗）；同步 getter（`HasText`）无法弹窗，回退到最近已知状态。

---

## 7. 速查：已踩过的坑

| 症状 | 根因 | 修复 |
|---|---|---|
| 固定高兄弟节点被挤出可视区（如 Shell 底部 TabBar 不渲染） | ArkUI NDK `NODE_FLEX_GROW` 在 Column 内不按"剩余空间"收缩，内容区拿满整列高 | 列 SIZE_CHANGE 时显式回填内容区高度（列高 - 顶栏 - TabBar），见 HarmonyShellHandler.RelayoutContent |
| 隐藏容器仍占位顶歪布局 | `NODE_VISIBILITY` Hidden(1) 隐藏但占位 | 需要"不占位"用 `SetVisibility(ARKUI_VISIBILITY_NONE)` |
| 浅色主题 Flyout 面板/顶栏背景近乎透明（#01010101） | `Color.FromRgba(1, 1, 1, 1)` 命中 **int 重载**（0-255 域），1/255 ≈ 全透明 | 用 `FromRgba(255, 255, 255, 255)` 或 `FromArgb("#FFFFFF")`；0-1 域写法必须带小数点 |
| 事件注册了没反应 | `On()` 漏 `NodeEventBus.Register`；或 receiver 未注册 | 基类 `On()` 已内置；新事件类型走基类，勿绕过 |
| `SetAttribute` 返回 401 | 枚举用错（如对齐用了 `ArkUI_Alignment`） | 对照 native_node.h 注释选枚举 |
| `SetStringAttribute` 401（设空串时） | 空串 `GetBytes` 返回 0 长数组，`fixed` 得空指针传给 `item.@string` | 基类已修（空串转 NUL 结尾空 C 串）；绕过基类的手写封送注意同样问题 |
| 字符串少最后几个字符 | napi 字符串读取缓冲区没 +1 | 用 `NativeValue.ToString`（已处理） |
| 模块加载闪退（jscrash Cannot find module） | 宿主没 import 该 `@ohos.*` 模块 | `ohosImports.ets` 登记 re-export |
| NativeAOT 后导出符号缺失 | ILC 只导出入口程序集的 `[UnmanagedCallersOnly]` | 导出放 `HelloApp.NativeExports` 薄转发 |
| Handler 连接前的 Children 不显示 | Controls 侧不发补发 Add 命令 | `ConnectHandler` 全量同步 |
| 类名冲突编译错 | `Stack`/`AbsoluteLayout` 与 BCL/MAUI 类型撞名 | using 别名（`ArkStack`/`MAbsolute` 模式） |
| MapRange 改 VirtualView.Value 导致无限循环 | Map 方法里直接改 VirtualView 会触发 PropertyChanged，又回 Map | **不要在 Map 方法里改 VirtualView**，只读属性取 clamp 值 |
| 事件取消订阅无效 | `Click -= _ => { }` lambda 不等于 `Click += _ => { }` 的 lambda | 用**命名方法**订阅和取消订阅 |
| `view.Date.ToString("格式")` 编译错 CS1501 | MAUI 10 的 DatePicker.Date/TimePicker.Time 是**可空类型**（`DateTime?`/`TimeSpan?`） | 先 `?? 默认值` 再取字段插值（`$"{d.Year:d4}-..."`） |
| 核心接口缺成员（GroupName/Refreshing 等） | MAUI 核心接口（IRadioButton/IRefreshView）比 Controls 类型瘦 | 按官方风格回退 Controls 具体类型，虚拟视图泛型直接用 Controls 类（Picker 先例） |
| 生成器重跑覆盖手写节点类 | 手写节点类（如 RefreshManual.cs）被生成器输出覆盖 | 手写文件不带 `<auto-generated>` 标记，生成器只覆盖带标记的文件；`CLASS_NAME_FIXES` / `ATTR_ALIASES` / `DEFAULT_SHAPES` 保证生成类名与 handler 别名一致 |
| 连续 uitest 手势后 hdc 挂死 | 模拟器 UI/uitest 过载 | `hdc kill` → `tconn 127.0.0.1:5555` → 重试；快照用 `timeout` 包裹 |
| Windows 本地 AOT 失败 | 检查 `PublishAotClang` 包是否还原成功，以及 .NET 10 SDK 是否可用 |
| PushAsync/PopAsync 静默失败（无日志无崩溃，后续导航全挂起） | 工厂抛 `NotSupportedException` 等异常被 `FireAndForget` 吞掉；MAUI 的 SendHandlerUpdateAsync 信号量不释放，后续导航永久排队 | `FireAndForget` 兜底必须打 hilog（HelloApp 的 FireAndForgetNavigation 已改）；推入新控件页面前先确认工厂注册 |
| 嵌套 Grid/AbsoluteLayout 撑爆父容器（后续兄弟节点被推出屏幕） | 托管布局容器无条件 `SetHeightPercent(1.0)`——只对页面根布局正确；MAUI 语义里 StackLayout 主轴 Fill = 自然高度 | `ConnectHandler` 按 `HeightRequest > 0 → 显式高；Parent 是 Layout → 自然高；否则 100%` |
| Essentials 注入后启动闪退（DfxFaultLogger 崩在 HarmonyInit） | `HarmonyEssentials.Install()`（或任何 napi 调用）放进了 ModuleInitializer——dlopen 时 napi env 尚未初始化 | 注入必须发生在 `RootBuilder` lambda 内（UI 线程首次构建时）；见 §6.2 |
| 包装对象隔一段时间后方法调用全挂（`napi_function_expected`） | JsObject 实例方法曾用创建时的裸 `Handle`——句柄范围关闭 + ArkTS GC 后失效 | 基类已修：实例调用统一走 `PinnedValue`（napi 强引用 `NapiReference` 重取）；自写包装别直接用 `Handle` |
| JS 抛错只见 `napi_pending_exception` 状态码，错误文本全无 | `ThrowIfFailed` 只抛状态不取异常对象 | 已修：pending_exception 时 `get_and_clear` 并读 `.message` 带进 NapiException 消息 |
| `requestPermissionsFromUser` 等 JS API 报 "must be Array"（401） | 字符串字面量联合别名（如 `Permissions`）跨模块不被 astParser 解析，元素退回 IntPtr 发射出无法封送的 `IntPtr[]` 签名 | 已修（生成器源上）：入口扫描全部 d.ts 顶层字面量联合别名登记为 `string` + `NativeValue.From(string[])` 封送（napi_create_array_with_length）；运行时临时绕过可用 `NodeApi.CreateInstance(global, "Array"u8, item)` |
| user_grant 权限被拒读剪贴板"成功"但 recordCount=0 | API 26 被拒时系统返回空 PasteData 壳而非抛错，读请求根本不落到剪贴板服务 | 读前 `getSelfPermissionStatus` 主动查状态 → 未授权走授权弹窗 → 重试（HarmonyClipboard 完整实现） |
| 宿主模板改了 EntryAbility.ets 但 HAP 里还是旧代码 | stage-host 的内容戳只比对 app.json5 的 mtime | 已修：脚本对全模板取最新 mtime 比对；改模板后任一文件都会触发重导出 |
