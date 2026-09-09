# Handler 适配指南

本文指导如何为剩余的 MAUI 控件适配 HarmonyOS Handler，以及如何新增 `@ohos.*` 系统模块绑定（deviceInfo 模式）。
适配前请先读 [README.md](README.md) 了解整体架构，[ROADMAP.md](ROADMAP.md) 了解里程碑状态。

---

## 1. 三层分工（改代码前先确认改哪层）

```
MAUI 控件 (VirtualView)                你要写的 Handler（本文重点）
      │ PropertyMapper / CommandMapper           │  属性/命令/事件翻译
      ▼                                          ▼
src/HarmonyOS.Maui/Handlers/*  ──────►  HarmonyOS.Bindings/Nodes/*（ArkUI 节点类）
                                         │  NODE_* 属性枚举 → SetAttribute
                                         ▼
                                libace_ndk.z.so（ArkUI C API，节点树渲染）

@ohos.* 系统模块（deviceInfo 等，与 UI 无关的服务 API）
      ▼
HarmonyOS.Bindings/Api/*  ──napi──►  libace_napi.z.so（napi_load_module / call_function）
```

| 层 | 目录 | 职责 | 什么时候改 |
|---|---|---|---|
| 节点层 | `HarmonyOS.Bindings/Nodes/` | ArkUI 组件的 C# 包装（属性 setter、事件） | 目标属性/事件在节点类上不存在时 |
| Handler 层 | `src/HarmonyOS.Maui/Handlers/` | MAUI ↔ ArkUI 的属性/命令/事件翻译 | **适配新控件的主要工作** |
| API 层 | `HarmonyOS.Bindings/Api/` | `@ohos.*` 模块绑定（napi 通道） | 绑定系统服务 API 时 |
| 宿主 | `samples/HarmonyHost/` | ArkTS 壳（ContentSlot 挂载 + ohosImports.ets 模块登记） | 新增 `@ohos.*` 模块绑定时同步登记 |

---

## 2. 适配一个新 Handler 的流程（五步）

以 **Slider** 为例走完全程。

### Step 1：确认 ArkUI 节点类是否够用

节点类在 `HarmonyOS.Bindings/Nodes/`，目前由 `src/parser/nativeCodeGenerator.ts` 从 SDK `.d.ts` 生成（已有 button/column/flex/row/stack/text 等）。查两个地方：

- `HarmonyOS.Bindings/NativeNode/ArkUINodeTypes.g.cs` 里 `ARKUI_NODE_*` 枚举 —— 确认目标组件类型存在（如 `ARKUI_NODE_SLIDER`）；
- `Nodes/native-gaps.json` —— 生成器登记的"属性存在但 shape 未注册"缺口。

如果目标组件类不存在或属性不够，**手写补一个节点类**（生成器是按需跑的，少量补充直接手写更快）：

```csharp
// HarmonyOS.Bindings/Nodes/slider.cs —— 手写节点类模板
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
    public event Action<ArkUINodeEvent>? ValueChange
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
        platformView.ValueChange += OnValueChange;
    }

    protected override void DisconnectHandler(ArkSlider platformView)
    {
        platformView.ValueChange -= OnValueChange;
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

    private void OnValueChange(ArkUINodeEvent e)
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

`src/HarmonyOS.Maui/Handlers/HarmonyHandlerFactory.cs` 的 switch 里加一行。**注意顺序：子类在前**（`Grid` 在 `Layout` 前，否则永远命中基类分支）：

```csharp
Microsoft.Maui.Controls.Slider => new HarmonySliderHandler(),
```

### Step 4：属性映射核对清单

| MAUI 概念 | ArkUI 翻译 | 备注 |
|---|---|---|
| `Color` / `Brush` | `(byte)r,(byte)g,(byte)b,(byte)a` → `0xAARRGGBB` | 纯色走 `BrushHelper.TryGetColor`；渐变 M1 静默降级透明 |
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
# 1. 编译（Windows 本地即可验证 C#/XAML）
dotnet build samples/dotnet/HelloApp/HelloApp.csproj
# 2. 远程 NativeAOT → 双架构 libapp.so
bash scripts/remote-build.sh
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

---

## 4. `@ohos.*` API 绑定（deviceInfo 模式）

UI 之外的系统服务（通知、振动、网络、设置项……）走 napi 通道。**只读常量型模块**照抄 `HarmonyOS.Bindings/Api/DeviceInfo.cs` 即可：

### 4.1 手写模板

```csharp
public static unsafe partial class Vibration
{
    private const string ModuleName = "@ohos.vibrator";

    private static NapiReference? _moduleRef;
    private static bool _loadAttempted;

    private static IntPtr Module
    {
        get
        {
            if (_moduleRef != null) return _moduleRef.Value;
            // …与 DeviceInfo.Module 完全相同的加载样板…
        }
    }
}
```

**五条铁律**（每条都有真实闪退/bug 对应，详见 ROADMAP）：

1. **库名**：napi P/Invoke 全部走 `libace_napi.z.so`（`libnapi.so` 不存在）；
2. **模块加载格式**：先试 `"=@ohos.xxx"`（系统模块前缀），失败回退不带 `=`；
3. **异常清理**：`napi_load_module` / 任何 napi 调用失败后**必须 `napi_get_and_clear_last_exception`**，否则 pending 异常传回宿主 ArkTS 执行流直接闪退；
4. **handle scope**：`napi_value` 只在 `napi_open_handle_scope` 内有效，加载成功后立即转 `NapiReference`（napi_ref）长期持有；
5. **字符串读取**：先取 length，缓冲区要 **`length + 1`** 字节（两段式），否则 UTF8 截尾（HUAWEI→HUAWE）。

### 4.2 宿主模块登记（容易漏！）

HarmonyOS 只允许加载**宿主 ArkTS 已 import** 的模块。每个新模块绑定必须在 `samples/HarmonyHost/entry/src/main/ets/ohosImports.ets` 追加一行 re-export：

```typescript
export { default as vibrator } from '@ohos.vibrator';
```

漏了这行的症状：jscrash `Cannot find module '@ohos.vibrator'`。（生成器后续会自动维护此文件，M2。）

### 4.3 带方法调用的模块（M2 方向，先了解）

deviceInfo 只有属性读取；带方法的模块（如 `vibrator.start()`）需要 `napi_get_named_property` 取函数 → `napi_call_function` 调用，参数用 `HarmonyOS.Bindings/Runtime/NativeValue.cs` 的 `From/To` 系列封送（`From(string/double/bool/Enum/object)`，字符串 `ToString` 已处理两段式）。异步回调（Promise → Task）依赖 TSFN 异步层，属于 M2 范畴——当前阶段先绑**同步方法**。

---

## 5. 代码风格速查（与官方 Handler 对齐）

| 项 | 规范 | 反例 |
|---|---|---|
| `ViewHandler` 泛型 | 核心接口优先（`ISlider`/`IEntry`）；**接口缺属性时回退具体类型**（`Label.HorizontalTextAlignment` 不在 `ILabel` 上） | `ViewHandler<Microsoft.Maui.Controls.Button, ...>`（冗长） |
| PropertyMapper | `new(ViewMapper)` | `new(ViewHandler.ViewMapper)`（过时写法） |
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
| ★☆☆ | `CollectionView`/`ListView` | `ARKUI_NODE_LIST` + `ARKUI_NODE_LIST_ITEM` | 虚拟化、复用、模板实例化、滚动定位 | ⏳ 待做 |
| ★☆☆ | `CarouselView` | `ARKUI_NODE_SWIPER` | | ⏳ 待做 |
| ★☆☆ | `Picker`/`DatePicker`/`TimePicker` | `ARKUI_NODE_TEXT_PICKER`/`DATE_PICKER`/`TIME_PICKER` | ArkUI 是内嵌节点非弹窗，视觉与 MAUI 弹窗 Picker 有差异 | ⏳ 待做 |
| ★☆☆ | `RefreshView` | `ARKUI_NODE_REFRESH` | 下拉经 NODE_REFRESH_ON_REFRESH 置 IsRefreshing；刷新态 setter 为手写 RefreshNode 子类（d.ts Evo 属性包构造参数，解析器未展开） | ✅ 已完成 |
| ☆ | `Shape`/自绘 | `ARKUI_NODE_CUSTOM` + `NODE_ON_DRAW` | 等价于 iOS `Draw`；需 MAUI Graphics 前端 | ⏳ 待做 |

**查询属性枚举值**：SDK 头文件
`%LOCALAPPDATA%/OpenHarmony/Sdk/<ver>/native/sysroot/usr/include/arkui/native_node.h`
的 `ArkUI_NodeAttributeType` 注释——每个枚举项写明了参数个数与类型（`ArkUI_NumberValue[]` 布局），这是写节点类 setter 的唯一权威来源。

---

## 7. 速查：已踩过的坑

| 症状 | 根因 | 修复 |
|---|---|---|
| 事件注册了没反应 | `On()` 漏 `NodeEventBus.Register`；或 receiver 未注册 | 基类 `On()` 已内置；新事件类型走基类，勿绕过 |
| `SetAttribute` 返回 401 | 枚举用错（如对齐用了 `ArkUI_Alignment`） | 对照 native_node.h 注释选枚举 |
| 字符串少最后几个字符 | napi 字符串读取缓冲区没 +1 | 用 `NativeValue.ToString`（已处理） |
| 模块加载闪退（jscrash Cannot find module） | 宿主没 import 该 `@ohos.*` 模块 | `ohosImports.ets` 登记 re-export |
| NativeAOT 后导出符号缺失 | ILC 只导出入口程序集的 `[UnmanagedCallersOnly]` | 导出放 `HelloApp.NativeExports` 薄转发 |
| Handler 连接前的 Children 不显示 | Controls 侧不发补发 Add 命令 | `ConnectHandler` 全量同步 |
| 类名冲突编译错 | `Stack`/`AbsoluteLayout` 与 BCL/MAUI 类型撞名 | using 别名（`ArkStack`/`MAbsolute` 模式） |
| MapRange 改 VirtualView.Value 导致无限循环 | Map 方法里直接改 VirtualView 会触发 PropertyChanged，又回 Map | **不要在 Map 方法里改 VirtualView**，只读属性取 clamp 值 |
| 事件取消订阅无效 | `Click -= _ => { }` lambda 不等于 `Click += _ => { }` 的 lambda | 用**命名方法**订阅和取消订阅 |
