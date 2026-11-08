# P2 MAUI 资产迁移清单（按库分类）

依据 `docs/WORKLOAD-PLAN.md` 第三节 P2-1 ~ P2-7 整理，作为后续迁移计划。

- 官方源码基线：`D:\Harmony\maui`（commit `699f16e4`，仅参照，不改 agreement）
- 本仓库源码基线：`D:\Harmony\DotHarmony.Binding`（Phase 1 已落地，P1-8 除外）
- TFM 约定：`net10.0-harmonyos26.0`，平台标识 `harmonyos`，编译常量 `HARMONYOS`，平台文件后缀 `.HarmonyOS.cs`，平台目录 `Platform/HarmonyOS/`
- 图例：✅ 可直接迁移（改命名空间/类名/partial 接线即可）｜⚠️ 适配迁移（结构需对齐官方后可迁）｜❌ 不可迁移（留在本仓库，附推荐方案）
- 源文件编码说明：本仓库 `.cs` 为 UTF-8 无 BOM（终端显示乱码只是控制台代码页问题，文件本身正常），迁移时保持 UTF-8。

---

## 0. 迁移总原则（对照 Tizen 的官方落地模式）

官方仓库对 Tizen 的支持由四层组成，HarmonyOS 逐层对照：

| 层 | Tizen 现状（官方仓库） | HarmonyOS 对应动作 |
|---|---|---|
| TFM 开关 | `Directory.Build.props:96-97` `_MauiTargetPlatformIsTizen`；`:114-121` Samsung workload 检测；`:132-133` `IncludeTizenTargetFrameworks=false`（当前禁用） | 新增 HarmonyOS 同构块（见 §1.1） |
| TFM 注入 | `Directory.Build.props:202` `MauiPlatforms` 追加 `net10.0-tizen7.0` | 追加 `net10.0-harmonyos26.0`（见 §1.1） |
| 平台文件过滤 | `src/MultiTargeting.targets:62-66` 非 Tizen 时 `Compile Remove **\**\*.Tizen.cs`；`:104-106` Tizen 时加 `TIZEN` 常量 | 新增 `*.HarmonyOS.cs` 过滤 + `HARMONYOS` 常量（见 §1.1） |
| 工作负载 | `WorkloadManifest.in.json`：`maui-mobile` extends 含 `maui-tizen`；`maui-tizen` 只 extends `maui-blazor` | 新增 `maui-harmonyos`（见 §1.1） |

依赖约束（同 Tizen 的 `Tizen.UIExtensions.NUI` 模式，`src/Core/src/Core.csproj:75-76`）：**`HarmonyOS.Bindings` / `HarmonyOS.ArkUI` / `HarmonyOS.Interop` 绑定程序集不进官方仓库**，改为 NuGet 包（本仓库 `artifacts/local-feed` 已有本地包产物），官方工程按 TFM 条件引用（见 §1.2 代码）。

---

## 1. Microsoft.Maui.Sdk / Workload（官方仓库构建系统）— P2-1 / P2-2

### 1.1 ✅ 可直接迁移（官方仓库侧改动，全部给出代码）

**(a) `Directory.Build.props`：HarmonyOS 检测开关**（仿 ：96-133 Tizen 块）

```xml
<!-- HarmonyOS -->
<_MauiTargetPlatformIsHarmonyOS>false</_MauiTargetPlatformIsHarmonyOS>
<_MauiTargetPlatformIsHarmonyOS Condition="'$(_MauiTargetPlatformIdentifier)' == 'harmonyos'">True</_MauiTargetPlatformIsHarmonyOS>

<DotNetHarmonyOSWorkloadInstalledVersion Condition="Exists('$(DotNetWorkloadInstallLocation)microsoft.net.sdk.harmonyos\WorkloadManifest.json')">$([System.Text.RegularExpressions.Regex]::Match($([System.IO.File]::ReadAllText('$(DotNetWorkloadInstallLocation)\microsoft.net.sdk.harmonyos\WorkloadManifest.json')), $(DotNetWorkloadVersionRegex)))</DotNetHarmonyOSWorkloadInstalledVersion>
<DotNetHarmonyOSWorkloadIsInstalled Condition="Exists('$(DotNetWorkloadPacksInstallLocation)Microsoft.NET.Sdk.HarmonyOS\$(DotNetHarmonyOSWorkloadInstalledVersion)\Sdk\AutoImport.props')">true</DotNetHarmonyOSWorkloadIsInstalled>
<IncludeHarmonyOSTargetFrameworks Condition="'$(DotNetHarmonyOSWorkloadIsInstalled)' == 'true'">true</IncludeHarmonyOSTargetFrameworks>

<HarmonyOSTargetFrameworkVersion>26.0</HarmonyOSTargetFrameworkVersion>
```

**(b) `Directory.Build.props:202` 附近：TFM 注入**

```xml
<MauiPlatforms Condition="'$(IncludeHarmonyOSTargetFrameworks)' == 'true'">net$(_MauiDotNetVersion)-harmonyos$(HarmonyOSTargetFrameworkVersion);$(MauiPlatforms)</MauiPlatforms>
```

**(c) `Directory.Build.targets:61-64` 附近：平台版本属性**

```xml
<PropertyGroup Condition="'$(_MauiTargetPlatformIsHarmonyOS)' == 'True'">
  <SupportedOSPlatformVersion>26.0</SupportedOSPlatformVersion>
  <TargetPlatformMinVersion>26.0</TargetPlatformMinVersion>
</PropertyGroup>
```

**(d) `src/MultiTargeting.targets`：平台文件过滤 + 编译常量**

```xml
<!-- HarmonyOS（非 HarmonyOS TFM 时移除平台文件） -->
<ItemGroup Condition=" '$(_MauiTargetPlatformIsHarmonyOS)' != 'True' ">
  <Compile Remove="**\**\*.HarmonyOS.cs" />
  <None Include="**\**\*.HarmonyOS.cs" Exclude="$(DefaultItemExcludes);$(DefaultExcludesInProjectFolder)" />
  <Compile Remove="**\HarmonyOS\**\*.cs" />
  <None Include="**\HarmonyOS\**\*.cs" Exclude="$(DefaultItemExcludes);$(DefaultExcludesInProjectFolder)" />
</ItemGroup>

<!-- HarmonyOS -->
<PropertyGroup Condition=" '$(_MauiTargetPlatformIsHarmonyOS)' == 'True' ">
  <DefineConstants>$(DefineConstants);HARMONYOS</DefineConstants>
</PropertyGroup>
```

**(e) `src/Workload/Microsoft.NET.Sdk.Maui.Manifest/WorkloadManifest.in.json`**：`maui-mobile.extends` 追加 `"maui-harmonyos"`，并新增条目（注意：**不 extends `harmonyos` 第三方 workload id**，与 `maui-tizen` 不 extends Samsung `tizen` 的做法保持一致，第三方 workload 只通过 §1.1(a) 的文件探测感知）：

```json
"maui-harmonyos": {
  "description": ".NET MAUI SDK for HarmonyOS",
  "extends": [
    "maui-blazor"
  ]
}
```

### 1.2 ❌ 不可迁移（留在 DotHarmony.Binding）

- workload 本体：`src/workloads/harmonyos/` 的 `WorkloadManifest.in.json` / `WorkloadManifest.targets` / `Microsoft.NET.Sdk.HarmonyOS` Sdk pack（P1 已落地）。官方 maui 仓库只做“已安装才启用”的探测，不承载 vendor workload。
- 推荐方案：维持现状。仅需在官方仓库 `src/Core/src/Core.csproj:75-76` 仿 Tizen 条件引用绑定包：

```xml
<ItemGroup Condition="$(TargetFramework.Contains('-harmonyos'))">
  <PackageReference Include="HarmonyOS.Bindings" />
</ItemGroup>
```


---

## 2. Microsoft.Maui.Core（`src/Core`）— P2-3 第一部分

### 2.1 ✅ 可直接迁移（首切片：MauiApplication、Handler 注册、Window、Button、Label、Dispatcher）

| 本仓库文件 | 官方目标位置 | 转换要点 |
|---|---|---|
| `src/HarmonyOS.Maui/Handlers/HarmonyButtonHandler.cs` | `src/Core/src/Handlers/Button/ButtonHandler.HarmonyOS.cs` | 类名改 `ButtonHandler`（partial），namespace `Microsoft.Maui.Handlers`；`Mapper` 里的 `MapXxx` 转为 partial 的 `public static void MapXxx(IButtonHandler, ...)`（对照 `ButtonHandler.Tizen.cs`） |
| `src/HarmonyOS.Maui/Handlers/HarmonyLabelHandler.cs` | `src/Core/src/Handlers/Label/LabelHandler.HarmonyOS.cs` | 同上，`ILabelHandler` |
| `src/HarmonyOS.Maui/Handlers/HarmonyViewHandler.cs` | `src/Core/src/Platform/HarmonyOS/HarmonyViewHandler.cs` | 泛型基类（继承官方 `ViewHandler<TVirtualView,TPlatformView>`），带手势/语义/视觉初始化 |
| `Handlers/HarmonyGestureManager.cs`、`MauiGestureBridge.cs`、`ButtonTouchRouter.cs` | `src/Core/src/Platform/HarmonyOS/` | 手势基建，namespace 改 `Microsoft.Maui.Platform` |
| `Handlers/HarmonyViewMapper.cs`、`HarmonyControlDefaults.cs`、`HarmonySemanticMapper.cs`、`FlexSemanticMapper.cs` | `src/Core/src/Platform/HarmonyOS/` | 默认值/语义基建 |
| `Handlers/BrushHelper.cs`、`ImageSourceResolver.cs`、`ImageEventRouter.cs`、`HarmonyImageSourcePartSetter.cs`、`NativeChildTracker.cs`、`PickerIndexTranslator.cs` | `src/Core/src/Platform/HarmonyOS/` | 工具类（对照 Tizen `ButtonExtensions.cs`/`ImageSourcePartExtensions.cs` 命名习惯，可顺手改名 `XxxExtensions`） |
| `Hosting/HarmonyDispatcher.cs` | `src/Core/src/Dispatching/Dispatcher.HarmonyOS.cs` | 官方 `Dispatcher` 平台分支（对照 `Dispatching` 下各平台文件的 `#if` 组织） |
| `Hosting/HarmonyApplication.cs` | `src/Core/src/Platform/HarmonyOS/MauiApplication.cs` | 对照 Tizen `Platform/Tizen/MauiApplication.cs` |
| `Hosting/HarmonyMauiContext.cs` | `src/Core/src/Platform/HarmonyOS/MauiContextExtensions.cs` | 对照 Tizen `MauiContextExtensions.cs` |
| `Hosting/MauiHarmonyHost.cs`、`HarmonyLifecycle.cs` | `src/Core/src/Platform/HarmonyOS/` | 宿主/生命周期（Window 生命周期部分拆到 `WindowHandler.HarmonyOS.cs`，见 ⚠️ 项） |
| `Hosting/HarmonyAppBuilderExtensions.cs`、`HarmonyMauiAppBuilder.cs` | `src/Core/src/Hosting/` 的 HarmonyOS 分支（或 `Platform/HarmonyOS/CoreAppExtensions.cs`） | `MauiAppBuilder` 扩展（对照 Tizen `CoreAppExtensions.cs`） |

**共享 partial 接线**（每个首切片 handler 必做）：`ButtonHandler.cs` / `LabelHandler.cs` 的平台别名块加分支：

```csharp
#elif HARMONYOS
using PlatformView = HarmonyOS.ArkUI.Flex;   // Button；Label 用 HarmonyOS.ArkUI.Text
```

**Handler 注册**：`src/Core/src/Hosting/HandlerMauiAppBuilderExtensions.cs` / Controls 的 `AppHostBuilderExtensions.cs`（见 §3.2）的 `#if` 链加 `|| HARMONYOS`。

**首切片后续波次**（同模式，非首日）：`HarmonyEntryHandler → EntryHandler.HarmonyOS.cs`、`HarmonyEditorHandler`、`HarmonySearchBarHandler`、`HarmonyCheckBoxHandler → CheckBoxHandler`、`HarmonySwitchHandler → SwitchHandler`、`HarmonySliderHandler`、`HarmonyStepperHandler`、`HarmonyDatePickerHandler`、`HarmonyTimePickerHandler`、`HarmonyPickerHandler`、`HarmonyProgressBarHandler → ProgressBarHandler`、`HarmonyRadioButtonHandler → RadioButtonHandler`、`HarmonyActivityIndicatorHandler → ActivityIndicatorHandler`、`HarmonyScrollViewHandler → ScrollViewHandler`、`HarmonyImageHandler → ImageHandler`、`HarmonyContentViewHandler`、`HarmonyRefreshViewHandler → RefreshViewHandler`（目标目录 `src/Core/src/Handlers/<Control>/`，官方均有同名目录）。

### 2.2 ⚠️ 适配迁移

- `HarmonyLayoutHandler.cs` / `HarmonyManagedLayoutHandler.cs` / `HarmonyFlexLayoutHandler.cs` → `src/Core/src/Handlers/Layout/LayoutHandler.HarmonyOS.cs`：官方 `LayoutHandler` 单类管理三种布局策略，需把三个 handler 的 ArkUI 布局物化逻辑收敛为 `ILayoutHandler` 一个 partial + 策略分支。
- `MauiHarmonyHost.cs` 中 Window 创建/激活/停止部分 → `src/Core/src/Handlers/Window/WindowHandler.HarmonyOS.cs`：官方有 `WindowHandler.Tizen.cs` 先例，需从宿主代码拆出 `IWindow` 映射。
- `Handlers/HarmonyCompatibilityViewHandlers.cs`：嵌入/兼容路径，按官方 `Embedding` 目录约定重排后再迁。

### 2.3 ❌ 不可迁移

- `Handlers/HarmonyXComponentHandler.cs`、`HarmonyXComponentView.cs`：XComponent（ArkUI 原生表面）是 HarmonyOS 特有概念，官方 `IView` 无对应抽象。
- 推荐方案：留在 `HarmonyOS.Bindings`/`HarmonyOS.Maui` 包内，作为包级扩展 handler，通过包内 `AddHarmonyOSHandlers` 扩展点注册（官方 handler 注册表支持第三方追加）；等官方接受平台扩展再评估上行。


---

## 3. Microsoft.Maui.Controls（`src/Controls`）— P2-3 第二部分

### 3.1 ✅ 可直接迁移

| 本仓库文件 | 官方目标位置 | 转换要点 |
|---|---|---|
| `HarmonyContentPageHandler.cs` | `src/Core/src/Handlers/Page/PageHandler.HarmonyOS.cs` + Controls 注册 | Page 在 Core，注册在 Controls |
| `HarmonyNavigationPageHandler.cs` | `src/Core/src/Handlers/NavigationPage/NavigationPageHandler.HarmonyOS.cs` | 对照 `NavigationPage` 官方目录 |
| `HarmonyTabbedPageHandler.cs` | `src/Core/src/Handlers/TabbedView/TabbedViewHandler.HarmonyOS.cs` | 官方目录名 `TabbedView` |
| `HarmonyShellHandler.cs`、`HarmonyShellTheme.cs`、`HarmonyShellNavigation.cs` | `src/Controls/src/Core/Handlers/Shell/ShellHandler.HarmonyOS.cs` + `ShellItemHandler.HarmonyOS.cs` + `ShellSectionHandler.HarmonyOS.cs` | 官方 Shell 目录三件套，Tizen 已有全部三个 `.Tizen.cs` 先例 |
| `HarmonyCollectionViewHandler.cs`、`ItemsViewMaterializer.cs`、`HarmonyCellHandlers.cs` | `src/Controls/src/Core/Handlers/Items/CollectionViewHandler.HarmonyOS.cs`（+ ItemsView 相关 partial） | 对照 `CollectionViewHandler.Tizen.cs` |
| `HarmonyCarouselViewHandler.cs` | `src/Controls/src/Core/Handlers/Items/CarouselViewHandler.HarmonyOS.cs` | 同上 |
| `HarmonyMenuHandlers.cs` | `src/Core/src/Handlers/MenuBar*`（`MenuBarHandler`/`MenuBarItemHandler`/`MenuFlyout*Handler.HarmonyOS.cs`） | 按官方 MenuBar 目录拆分 |
| `HarmonyFrameHandler.cs` | `src/Controls/src/Core/Compatibility/Handlers/HarmonyOS/FrameRenderer.cs` | 官方 Frame 注册到 `Handlers.Compatibility.FrameRenderer`（`AppHostBuilderExtensions.cs:195`），Tizen 有 `FrameRenderer.cs` 先例 |
| `HarmonyBoxViewHandler.cs` | `src/Core/src/Handlers/ShapeView/ShapeViewHandler.HarmonyOS.cs` | 官方 BoxView 走 ShapeViewHandler，需按 `IBoxView`→Shape 的属性映射改写 mapper 键 |

### 3.2 ✅ 注册接线（Controls）

`src/Controls/src/Core/Hosting/AppHostBuilderExtensions.cs`（现有 `#elif TIZEN` 在 ：24-26、:220）：

```csharp
#elif HARMONYOS
using Microsoft.Maui.Controls.Compatibility.Platform.HarmonyOS;
```

注册块（:167/:209/:226/:238 等处）把 `|| TIZEN` 链补成 `|| TIZEN || HARMONYOS`，并新增：

```csharp
#if HARMONYOS
            .AddHandler(typeof(Button), typeof(ButtonHandler))
            .AddHandler(typeof(Label), typeof(LabelHandler))
            .AddHandler(typeof(ContentPage), typeof(PageHandler))
            .AddHandler(typeof(Frame), typeof(Handlers.Compatibility.FrameRenderer))
            // ... 首切片控件
#endif
```

### 3.3 ❌ 不可迁移

- 无。Controls 侧全部可迁（差异只在目标目录选择）。


---

## 4. Microsoft.Maui.Essentials（`src/Essentials`）— P2-4

### 4.1 ✅ 可直接迁移（22 个服务，逐一转换）

官方约定：每服务一个 `<Service>.harmonyos.cs`，类为 `partial class <Service>Implementation : I<Service>`（对照 `Battery.tizen.cs` 的 `partial class BatteryImplementation : IBattery`）。静态接线零成本：`Battery.shared.cs:93-102` 的 `Default => new BatteryImplementation()` 在 `net10.0-harmonyos26.0` TFM 下自动解析到平台 partial，**无需任何注入代码**。

| 本仓库文件 | 官方目标（`src/Essentials/src/<目录>/`） | 官方命名空间 |
|---|---|---|
| `HarmonyDeviceInfo.cs` | `DeviceInfo/DeviceInfo.harmonyos.cs` | `Microsoft.Maui.Devices` |
| `HarmonyDeviceDisplay.cs` | `DeviceDisplay/DeviceDisplay.harmonyos.cs` | `Microsoft.Maui.Devices` |
| `HarmonyAppInfo.cs` | `AppInfo/AppInfo.harmonyos.cs` | `Microsoft.Maui.ApplicationModel` |
| `HarmonyClipboard.cs` | `Clipboard/Clipboard.harmonyos.cs` | `Microsoft.Maui.ApplicationModel.DataTransfer` |
| `HarmonyPreferences.cs` | `Preferences/Preferences.harmonyos.cs` | `Microsoft.Maui.Storage` |
| `HarmonyBattery.cs` | `Battery/Battery.harmonyos.cs` | `Microsoft.Maui.Devices` |
| `HarmonyVibration.cs` | `Vibration/Vibration.harmonyos.cs` | `Microsoft.Maui.Devices` |
| `HarmonyConnectivity.cs` | `Connectivity/Connectivity.harmonyos.cs` | `Microsoft.Maui.Networking` |
| `HarmonyFileSystem.cs` | `FileSystem/FileSystem.harmonyos.cs` | `Microsoft.Maui.Storage` |
| `HarmonyLauncher.cs` | `Launcher/Launcher.harmonyos.cs` | `Microsoft.Maui.ApplicationModel` |
| `HarmonyBrowser.cs` | `Browser/Browser.harmonyos.cs` | `Microsoft.Maui.ApplicationModel` |
| `HarmonyPhoneDialer.cs` | `PhoneDialer/PhoneDialer.harmonyos.cs` | `Microsoft.Maui.ApplicationModel.Communication` |
| `HarmonyShare.cs` | `Share/Share.harmonyos.cs` | `Microsoft.Maui.ApplicationModel.DataTransfer` |
| `HarmonyEmail.cs` | `Email/Email.harmonyos.cs` | `Microsoft.Maui.ApplicationModel.Communication` |
| `HarmonySecureStorage.cs` | `SecureStorage/SecureStorage.harmonyos.cs` | `Microsoft.Maui.Storage` |
| `HarmonySensors.cs`（Accelerometer） | `Accelerometer/Accelerometer.harmonyos.cs` | `Microsoft.Maui.Devices.Sensors` |
| `HarmonySensors.cs`（Magnetometer） | `Magnetometer/Magnetometer.harmonyos.cs` | 同上 |
| `HarmonySensors.cs`（Gyroscope） | `Gyroscope/Gyroscope.harmonyos.cs` | 同上 |
| `HarmonySensors.cs`（Compass） | `Compass/Compass.harmonyos.cs` | 同上 |
| `HarmonySensors.cs`（OrientationSensor） | `OrientationSensor/OrientationSensor.harmonyos.cs` | 同上 |
| `HarmonyGeolocation.cs` | `Geolocation/Geolocation.harmonyos.cs` | 同上 |
| `HarmonyMediaPicker.cs` | `MediaPicker/MediaPicker.harmonyos.cs` | `Microsoft.Maui.Media` |

**转换模板**（以 Battery 为例）：

```csharp
// src/Essentials/src/Battery/Battery.harmonyos.cs
using Microsoft.Maui.ApplicationModel;
using HBatteryInfo = HarmonyOS.Bindings.Api.BatteryInfo;

namespace Microsoft.Maui.Devices
{
    partial class BatteryImplementation : IBattery
    {
        public double ChargeLevel => !HBatteryInfo.IsBatteryPresent
            ? -1
            : Math.Clamp(HBatteryInfo.BatterySoc / 100.0, 0.0, 1.0);

        // State / PowerSource / EnergySaverStatus / 事件挂接：
        // 从本仓库 HarmonyBattery.cs 平移，把 HarmonyXxx 别名换成 HarmonyOS.Bindings 实际类型
    }
}
```

### 4.2 ❌ 不可迁移 + 推荐方案

**(a) `HarmonyEssentials.Install()` 反射注入机制（`SetImplementation` + `CreateDelegate`）**

不可迁移原因：官方多目标下 `new BatteryImplementation()` 由 TFM 静态解析，反射注入通道在官方代码里不存在且违反官方惯例（也过不了官方 Trim/AOT 分析门）。
推荐方案：整文件删除，改为官方 partial（§4.1）。迁移完成后本仓库 `MauiHarmonyHost.Run` 里的 `HarmonyEssentials.Install()` 调用一并移除。

**(b) 官方有、本仓库未实现的服务** —— 按 P2-4 约定出显式失败桩（新建 `<Service>.harmonyos.cs`）：

需桩：`AppActions`、`Contacts`、`FilePicker`、`Flashlight`、`Geocoding`、`Map`、`Screenshot`、`SemanticScreenReader`、`Sms`、`TextToSpeech`、`WebAuthenticator`。
需评估后决定：`MainThread`（本仓库 Dispatcher 已具备主线程能力，可能直接实现而非桩）、`HapticFeedback`（可复用 `HarmonyVibration` 的 `@ohos.vibrator` 通道）。
无需平台资产：`UnitConverters`、`VersionTracking`（纯 netstandard 实现）。

```csharp
// 桩模板（对照官方不支持平台惯例）
namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class ContactsImplementation : IContacts
    {
        public Task<Contact[]> GetAllAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Contacts is not supported on HarmonyOS.");
    }
}
```

**(c) `HarmonyPermissionGenerator` / `HarmonyPermissionMap.cs` / `HarmonyPermissionMappingDocument.cs`（source generator）**

不可迁移原因：官方 Permissions 用静态 partial + `EnsureDeclared` 模式，不接受运行时/构建期外部 sourcegen 注入；generator 是本仓库包体系的一部分。
推荐方案：`HarmonyPermissions.cs`/`HarmonyPermissionGate.cs` 的权限映射表内容（OHOS permission 字符串 → API 映射）迁成官方 partial：

```csharp
// src/Essentials/src/Permissions/Permissions.harmonyos.cs
namespace Microsoft.Maui.ApplicationModel
{
    partial class Permissions
    {
        internal static void EnsureDeclared(PermissionType type)
        {
            // 内容取自 HarmonyPermissionMap：type -> ohos.permission.XXX 常量
            // 未声明时抛 PermissionException（对照官方其它平台）
        }
    }
}
```

Generator 与 `HarmonyOS.Maui.Permissions.Abstractions` 属性留在本仓库（模板工程仍用它做编译期检查），后续视官方态度再评估上行。

**(d) `ConcurrentLazyCache.cs`**：仅当官方实现需要时迁入 `src/Essentials/src/Platform/HarmonyOS/`，否则留作包内部实现细节。

---

## 5. Microsoft.Maui.Graphics（`src/Graphics`）— P2-3 配套

### 5.1 ⚠️ 适配迁移

- `src/HarmonyOS.Maui/Handlers/HarmonyDrawingCanvas.cs`（`Microsoft.Maui.Graphics.ICanvas` on `OH_Drawing`）→ `src/Graphics/src/Graphics/Platforms/HarmonyOS/Canvas.harmonyos.cs`。
  官方 `Graphics/Platforms` 现有 `{Android,iOS,Mac,MaciOS,Windows}`，无 Tizen 先例，需走 §1.1(d) 的 `HarmonyOS\` 目录过滤（已覆盖）。依赖 `OHDrawingCanvas`/`OHDrawingPen`/`OHDrawingBrush`（HarmonyOS.Bindings.NativeNode）→ 包引用。
- `HarmonyDrawableViewHandlers.cs` + `HarmonyDrawingView` → `src/Core/src/Handlers/GraphicsView/GraphicsViewHandler.HarmonyOS.cs`（`ICanvas` 工厂指到上面的实现）。

### 5.2 当前实现缺口（迁移时保留 P2-4 式显式行为）

`HarmonyDrawingCanvas` 现状：`DrawString` no-op（排版未接）、`SetShadow` no-op、`SetFillPaint` 仅纯色。迁入官方前补齐或保留显式 `NotSupportedException`/文档说明，不得静默。


---

## 6. Microsoft.AspNetCore.Components.WebView.Maui（`src/BlazorWebView`）— P2-5

官方 Tizen 先例：`src/BlazorWebView/src/Maui/Tizen/`（`BlazorWebViewHandler.Tizen.cs`、`TizenWebViewManager.cs`、`TizenMauiAssetFileProvider.cs`）。**评估结论：可同仓交付，无需独立 fork。**

### 6.1 ✅ 逻辑可直接迁移

| 本仓库文件 | 官方目标（`src/BlazorWebView/src/Maui/HarmonyOS/`） | 对照 |
|---|---|---|
| `HarmonyBlazorWebViewManager.cs` | `HarmonyWebViewManager.cs` | `TizenWebViewManager.cs`（官方 `WebViewManager` 子类） |
| `HarmonyBlazorFileProvider.cs` | `HarmonyOSMauiAssetFileProvider.cs` | `TizenMauiAssetFileProvider.cs` |
| `HarmonyBlazorDispatcher.cs` | `HarmonyOSDispatcher.cs` | 官方 per-platform dispatcher |
| `HarmonyUrlLoading.cs`、`HarmonyIpcJsonOptions.cs`、`HarmonyIpcReceiver.cs` | 并入 manager/handler 对应位置 | 官方已有同概念，删本地重复 |
| `RootComponent.cs`、`RootComponentsCollection.cs` | **不迁**，官方 `RootComponentsCollection` 已存在 | 删本地重复类型 |
| `ServiceCollectionExtensions.cs` | `Extensions/ServiceCollectionExtensions.HarmonyOS.cs`（或并入官方 Extensions） | `AddHarmonyOSBlazorWebView()` |

### 6.2 ❌ 不能按现状直接迁移 + 推荐代码

`HarmonyBlazorWebView.cs`（自建虚控件）与 `HarmonyBlazorWebViewHandler.cs`（绑定自建类型）不可平移——官方 BlazorWebView 是固定虚控件 `BlazorWebView`，handler 必须是官方 partial。推荐重写骨架：

```csharp
// src/BlazorWebView/src/Maui/HarmonyOS/BlazorWebViewHandler.HarmonyOS.cs
using Microsoft.Extensions.DependencyInjection;
using ArkStack = HarmonyOS.ArkUI.Stack;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
    public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, ArkStack>
    {
        protected override ArkStack CreatePlatformView()
        {
            // 迁自 HarmonyBlazorWebViewHandler.CreatePlatformView：创建 ArkWeb surface（Stack + Web）
        }

        protected override void ConnectHandler(ArkStack platformView)
        {
            base.ConnectHandler(platformView);
            // manager = new HarmonyWebViewManager(...)
            // 对照 Tizen ConnectHandler 的组装顺序：HostPage -> RootComponents -> StartPath -> UrlLoading
        }

        public static void MapSource(IBlazorWebViewHandler handler, IBlazorWebView webView) { /* 首切片忽略 Source */ }
        public static void MapHostPage(IBlazorWebViewHandler handler, IBlazorWebView webView) { /* 迁自 MapHostPage */ }
        public static void MapRootComponents(IBlazorWebViewHandler handler, IBlazorWebView webView) { /* 迁自 MapRootComponents */ }
    }
}
```

```csharp
// src/BlazorWebView/src/Maui/HarmonyOS/HarmonyWebViewManager.cs（骨架）
public class HarmonyWebViewManager : WebViewManager
{
    public HarmonyWebViewManager(
        IServiceProvider provider, Dispatcher dispatcher, Uri appBaseUri,
        IFileProvider fileProvider, JsComponentConfigurationStore jsComponents)
        : base(provider, dispatcher, appBaseUri, fileProvider, jsComponents) { }

    protected override void NavigateCore(Uri absoluteUri)
        => _surface?.Navigate(absoluteUri.ToString());   // 迁自 HarmonyBlazorWebViewManager

    protected override void SendMessage(string message)
        => _surface?.PostWebMessage(message);            // 迁自 HarmonyIpcReceiver 对侧
}
```

关联项（P2-3 明确首切片**不含** WebView）：`src/HarmonyOS.Maui.Web/` 的 `HarmonyWebViewHandler.cs` → 第二波迁 `src/Core/src/Handlers/WebView/WebViewHandler.HarmonyOS.cs`（`IWebView` CommandMapper 已按官方接口写好，可平移），`HarmonyArkWebViewSurface`/`HarmonyWebRuntime` 等 ArkWeb 装载层留下或并入 Platform/HarmonyOS。

---

## 7. HAP 发布链 / 模板 / 验收 — P2-6 / P2-7

### 7.1 ❌ 全部不可迁移（留在 DotHarmony.Binding）+ 推荐代码

| 内容 | 不可迁移原因 | 推荐方案 |
|---|---|---|
| `src/HarmonyOS.Maui.Tasks/PatchOpenHarmonyNativeAot.cs` + `scripts/patch-openharmony-nativeaot.ps1` | arm64 `libapp.so` NUMA 二进制后处理是 HarmonyOS 专属（WORKLOAD-PLAN 范围约束：不进 runtime/官方源码） | 维持 MSBuild task；P2-6 把调用点从“publish 后 Exec”迁进 workload Sdk pack targets（见下） |
| `src/HarmonyOS.Maui/build/HarmonyOS.Maui.App.targets`（`HarmonyBuildLibApp`/`HarmonyBuildHap`/`HarmonyRun`） | HAP 签名/打包/宿主 stage 是 vendor 链路（对应 Samsung.Tizen.Sdk 的 TPK 职责，官方 maui 不含） | 改造为 publish 直通（下方骨架），目标文件随 workload pack 分发 |
| `src/HarmonyOS.Templates`（`dotnet new harmony-maui`） | 模板依赖 vendor 包与 workload | 留在本仓库；等官方 `maui-harmonyos` workload 就绪后再对齐模板工作区解耦（P1-3 已完成注册外迁） |
| `scripts/verify-workload.ps1` 等验收脚本 | 属本仓库流程 | P2-7 增补断言（见 7.2） |

**P2-6 推荐改造**（`src/workloads/harmonyos/Microsoft.NET.Sdk.HarmonyOS/Sdk/Sdk.targets`）——`dotnet publish` 直出 HAP，失败不留旧产物：

```xml
<Target Name="_HarmonyPublishLibApp"
        AfterTargets="Publish"
        Condition="$(TargetFramework.Contains('-harmonyos')) and '$(PublishAot)' == 'true'">
  <!-- Publish 已产出 $(PublishDir)libapp.so，此处只做 patch + staging，不再嵌套 dotnet publish -->
  <ItemGroup>
    <_HarmonyStaleLibApp Include="$(HarmonyHostDir)libs\**\libapp.so" />
  </ItemGroup>
  <Delete Files="@(_HarmonyStaleLibApp)" />

  <PatchOpenHarmonyNativeAot
      LibraryPath="$(PublishDir)libapp.so"
      RuntimeIdentifier="$(RuntimeIdentifier)"
      CheckOnly="false" />
  <PatchOpenHarmonyNativeAot
      LibraryPath="$(HarmonyHostDir)libs\$(HarmonyAbi)\libapp.so"
      RuntimeIdentifier="$(RuntimeIdentifier)"
      CheckOnly="true"
      Condition="'$(HarmonyAbi)' != ''" />

  <Error Condition="'$(RuntimeIdentifier)' == 'linux-musl-arm64' and !Exists('$(PublishDir)libapp.so')"
         Text="HarmonyOS publish：未找到 patch 后的 libapp.so，已中止 HAP 打包。" />
</Target>

<Target Name="_HarmonyPublishHap"
        AfterTargets="_HarmonyPublishLibApp"
        DependsOnTargets="HarmonyBuildHap">
  <Error Condition="!Exists('$(HarmonyHapOutput)')"
         Text="HarmonyOS publish：HAP 签名打包失败（$(HarmonyHapOutput) 不存在），旧 HAP 已清理。" />
</Target>
```

要点：`HarmonyBuildLibApp` 现有的“build 目标内嵌套 `dotnet publish` + `MSBuildEnableWorkloadResolver=false`”（WORKLOAD-PLAN 第一节自举缺口）随之消除——普通 `dotnet publish` 驱动一次发布。

### 7.2 P2-7 验收（流程，非迁移项）

推荐在 `scripts/verify-workload.ps1` 增补资产断言（官方构建产物就绪后执行）：
1. `project.assets.json` 的 `targets` 中 `Microsoft.Maui.Controls.Core/...` 的 compile/runtime 指向 `lib/net10.0-harmonyos26.0/`（不回落 `net10.0`）；
2. HelloApp 无任何 `HarmonyOS.Maui*` `PackageReference`；
3. 版本三元组（MAUI / .NET SDK / HarmonyOS SDK）记录进验收输出。

---

## 8. 后续执行顺序（依赖排序）

1. **§1 工程接线**（官方仓库 5 处小改动，全部照抄代码）——一切平台编译的前提，约半天。
2. **§2 Core 首切片**：`HarmonyViewHandler` 基座 + Button/Label + Dispatcher + MauiApplication；跑通 `net10.0-harmonyos26.0` 编译。
3. **§4 Essentials**：22 服务机械转换（风险最低、收益最快），桩与 Permissions partial 一并出。
4. **§3 Controls 注册 + Shell/Collection/Frame**。
5. **§6 BlazorWebView**：manager/file-provider 平移 + handler 骨架（上真机评估）。
6. **§7 发布链**（本仓库内）：`dotnet publish` 直通 HAP 改造。
7. **§5 Graphics**：canvas 落位（可与 4 并行）。

风险提示：
- 官方仓库 `MultiTargeting.targets`/`Directory.Build.props` 改动会影响全仓多目标矩阵——先开 `IncludeHarmonyOSTargetFrameworks` 探测开关验证 Tizen/其它平台行为不变，再放开 TFM（对应 P2-1/P2-2 验收）。
- 本仓库 handler 依赖 `HarmonyViewHandler` 基座的 Connect 顺序（visibility/opacity 初始化在 mapper 之前），迁入官方时保持该顺序，否则 ArkUI `-2` 布局失败会复现。
- Essentials 转换注意 `IBattery.ChargeLevel` 等接口语义（本仓库 `-1` 约定与官方一致，直接平移）。
