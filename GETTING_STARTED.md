# HarmonyOS MAUI 应用开发指南

> 面向两类读者：**从零创建**一个鸿蒙 MAUI 应用，或把鸿蒙平台**加入已有 MAUI 应用**。
> 架构一句话：你的 UI 代码编译成 NativeAOT 共享库（libapp.so），由 ArkTS 宿主 dlopen 后挂载，
> 所有控件经 HarmonyOS.Maui Handler 映射为 ArkUI 原生节点渲染。使用方式与 MAUI 单项目体验对齐：
> `dotnet build -t:HarmonyRun` 一键"编译 → 打 HAP → 部署运行"。

前置概念（详见 [README](README.md) / [HANDLERS](HANDLERS.md)）：

| 层 | 内容 | 你是否要写 |
|---|---|---|
| 应用工程 | Program.cs + XAML 页面 + Platforms/HarmonyOS 启动代码 | ✅ 本文的主角 |
| HarmonyOS.Maui | 28 个具体 Handler（30 个工厂分派形态）、手势、导航、托管布局 | ❌ 引用即可 |
| HarmonyOS.Essentials | MAUI Essentials 鸿蒙实现（22 服务） | ❌ 引用即可（MauiHarmonyHost.Run 自动安装） |
| HarmonyOS.Bindings | ArkUI NDK 原生节点 + 438 个 @ohos.* 模块绑定 | ❌ 引用即可（仅声明用到的 @ohos 模块） |
| HarmonyOS.Interop | napi 互操作核心独立装 | ❌ 引用即可 |
| HarmonyHost（ArkTS 宿主） | dlopen libapp.so 的壳工程模板 | ❌ 由 targets 自动生成按应用实例（见 §4） |

---

## 0. 前置条件

| 工具 | 用途 | 说明 |
|---|---|---|
| .NET 10 SDK（Windows） | 本机编译检查（C#/XAML 编译期验证） | `dotnet --version` ≥ 10 |
| PublishAotClang | Windows 本地 NativeAOT 交叉编译 libapp.so | 由 `Directory.Build.props` 自动引用；内置 Zig / clang / objcopy |
| DevEco Studio | hvigor 打 HAP、hdc 部署、模拟器 | 记住安装路径，脚本自动探测常见位置 |

以上工具链就绪后，仓库根目录：

```powershell
# 一键验证链路（HelloApp 示例：libapp.so 双架构 → HAP → 部署到模拟器）
dotnet publish samples/dotnet/HelloApp -c Release -r linux-musl-arm64
dotnet publish samples/dotnet/HelloApp -c Release -r linux-musl-x64
scripts\build-hap.cmd              # hvigor 打包
bash scripts/deploy-hap.sh         # hdc 安装 + 启动（多设备：HDC_TARGET=127.0.0.1:5555）
```

或直接在示例工程上：`dotnet build samples/dotnet/HelloApp -t:HarmonyRun`（等价上面三步）。

---

## 1. 路线 A：经 NuGet 模板从零创建（推荐，无需仓库工作副本）

前提：已产出或获得 `HarmonyOS.Maui` 等 nupkg（本仓库 `dotnet pack -c Release -o dist`，
或后续发布的远端源）。

```powershell
# 1. 安装应用模板（本地包）
dotnet new install <dist 目录>\HarmonyOS.Templates.1.0.0.nupkg

# 2. 实例化（目录与命名空间取项目名）
dotnet new harmony-maui -n MyApp

# 3. 本机 NuGet 源指向 dist（仓库外消费时）：
#    在解决方案目录建 nuget.config，packageSources 增加 <add key="local-dist" value="<dist 绝对路径>" />

# 4. 一键链路：stage 宿主 → NativeAOT → HAP → 部署
dotnet build MyApp -t:HarmonyStageHost   # 仅生成宿主实例（obj/harmony/host）
dotnet build MyApp -t:HarmonyRun         # 全链路
```

模板工程已含：`Platforms/HarmonyOS/HarmonyExports.cs`（NativeAOT 导出薄转发层）、
`Program.cs`（`MauiHarmonyHost.Run` 入口）、示例 `MainPage.xaml`。
宿主编排 targets 由 `HarmonyOS.Maui` 包经 buildTransitive 自动导入，工程内无需手写 Import。
release 签名通过属性簇配置（见 README「NuGet 包与模板」一节）。

## 2. 路线 B：仓库内参照开发（samples/dotnet/）

> 完整可运行的参照：[samples/dotnet/HelloApp](samples/dotnet/HelloApp)（XAML + 导航 + 手势 + 托管布局）与
> [samples/dotnet/ApiDemo](samples/dotnet/ApiDemo)（@ohos.* 服务调用）与
> [samples/dotnet/EssentialsApp](samples/dotnet/EssentialsApp)（DeviceInfo/Clipboard 等 Essentials 标准 API）。
> 此路线以 ProjectReference 直连源码，适用于本仓库自身的演进与调试。

### Step 1：创建 .NET 类库工程

`samples/dotnet/MyApp/MyApp.csproj`：

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- NativeAOT 共享库：宿主 dlopen 后调用导出函数 -->
    <TargetFramework>net10.0</TargetFramework>
    <RuntimeIdentifier>linux-musl-arm64</RuntimeIdentifier>
    <PublishAot>true</PublishAot>
    <NativeLib>Shared</NativeLib>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>

    <!-- 鸿蒙 musl 环境无 ICU 数据 -->
    <InvariantGlobalization>true</InvariantGlobalization>

    <!-- 事件跳板依赖完整根（UnmanagedCallersOnly），保守裁剪 -->
    <TrimMode>full</TrimMode>
    <!-- 完整栈元数据仅 Debug：Release 省体积，异常栈为十六进制帧（排障用 hilog 日志点） -->
    <IlcGenerateStackTraceData Condition="'$(Configuration)' == 'Debug'">true</IlcGenerateStackTraceData>

    <!-- 必须叫 app：发布产物 app.so 会被打包为 libs/<abi>/libapp.so（构建脚本约定） -->
    <OutputType>Library</OutputType>
    <AssemblyName>app</AssemblyName>

    <!-- 工程即入口：ModuleInitializer 在 dlopen 后注册根页面工厂，CA2255 不适用 -->
    <NoWarn>$(NoWarn);CA2255</NoWarn>

    <!-- XAML：SourceGen 编译期生成 InitializeComponent（NativeAOT 零反射） -->
    <EnableDefaultMauiItems>true</EnableDefaultMauiItems>
    <MauiXamlInflator>SourceGen</MauiXamlInflator>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\HarmonyOS.Bindings\HarmonyOS.Bindings.csproj" />
    <ProjectReference Include="..\..\src\HarmonyOS.Maui\HarmonyOS.Maui.csproj" />
  </ItemGroup>

  <!-- MAUI 风格一键编排：dotnet build -t:HarmonyRun（libapp.so → HAP → 部署） -->
  <Import Project="$(MSBuildThisFileDirectory)../../../src/HarmonyOS.Maui/build/HarmonyOS.Maui.App.targets" />

</Project>
```

### Step 2：应用入口（对应 MauiProgram.CreateMauiApp）

`Program.cs`：

```csharp
using Microsoft.Maui.Controls;
using HarmonyOS.Maui.Hosting;

namespace MyApp;

public static class Program
{
    // 根页面工厂在 UI 主线程被调用；根页用 NavigationPage 即可获得标准 PushAsync/PopAsync
    public static void Register() => MauiHarmonyHost.Run(() => new NavigationPage(new MainPage()));
}
```

支持的根页类型：`ContentPage` / `NavigationPage`（推荐，配套返回键/标题栏/模态）/
`Shell`（TabBar、Flyout 菜单、路由、query、模态、主题色、NavBar/TabBar 可见性）/ `TabbedPage`。

### Step 3：平台启动代码（必抄的薄转发层）

`Platforms/HarmonyOS/HarmonyExports.cs`（ILC 只导出**入口程序集**内的 `[UnmanagedCallersOnly]`，
所以每个应用都要这份文件——对齐 MAUI Platforms/Android/MainActivity 惯例）：

```csharp
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.Hosting;
using HarmonyOS.Maui.Hosting;

namespace MyApp.Platforms.HarmonyOS;

internal static class NativeExports
{
    [UnmanagedCallersOnly(EntryPoint = "HarmonyInit")]
    private static int HarmonyInit(nint env) => Host.InitializeCore(env);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyBuildUI")]
    private static int HarmonyBuildUI(nint env, nint nodeContentValue)
        => Host.BuildUICore(env, nodeContentValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyPopPage")]
    private static int HarmonyPopPage(nint env)
        => HarmonyNavigation.OnBackRequested() ? 1 : 0;   // 系统返回键 → MAUI 导航栈
}

internal static class Bootstrap
{
    [ModuleInitializer]
    internal static void Init() => Program.Register();   // dlopen 后、导出调用前执行
}
```

### Step 4：写页面（XAML 或 C#）

XAML 无需任何额外声明，`<MauiXamlInflator>SourceGen</MauiXamlInflator>` 已接入编译期膨胀：

```xml
<!-- MainPage.xaml -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.MainPage"
             Title="Home"
             BackgroundColor="White">
    <StackLayout Padding="20" Spacing="12">
        <Label Text="Hello HarmonyOS MAUI!" FontSize="20" />
        <Button Text="Push" Clicked="OnPushClicked" />
    </StackLayout>
</ContentPage>
```

```csharp
// MainPage.xaml.cs —— 导航/生命周期事件与官方 MAUI 完全同名。
// PushAsync 返回的 Task 须观察异常（吞掉会导致后续导航挂起且无日志）：
// 照抄 HelloApp 的 FireAndForgetNavigation 扩展（hilog 打点），不要用空 catch
private void OnPushClicked(object? sender, EventArgs e)
    => Navigation.PushAsync(new SecondPage()).FireAndForgetNavigation();
```

### Step 5：编译、运行

```bash
# Windows 本机先做编译期检查（秒级，不用等 AOT）
dotnet build samples/dotnet/MyApp

# 一键全链路（PublishAotClang AOT 双架构 → hvigor HAP → hdc 部署启动）
dotnet build samples/dotnet/MyApp -t:HarmonyRun
```

部署目标默认模拟器；多设备在线时 `HDC_TARGET=192.168.1.6:8710 dotnet build -t:HarmonyRun`（或逐段手动跑三个脚本，见 §4）。

---

## 3. 已有 MAUI 应用接入鸿蒙平台

原则：**不动既有平台工程**（Android/iOS/Catalyst 照旧），为鸿蒙建一个独立的"外壳"工程，复用你的共享 UI 层。

### Step 1：盘点可复用的代码

| 你的代码 | 可否直接复用 | 说明 |
|---|---|---|
| XAML 页面 / 控件树 | ✅ 直接复用 | XAML 编译期膨胀，平台无关；Shell 第一版可用 |
| MVVM（ViewModel/绑定/Command） | ✅ 直接复用 | 数据绑定走 MAUI 标准管线 |
| `Navigation.PushAsync/PopAsync/PushModalAsync` | ✅ 直接复用 | 经 IStackNavigation 协议转接到 ArkUI |
| 手势识别（Tap/Pan/Pinch/Swipe/Pointer） | ✅ 直接复用 | 注意 PanUpdated 单位为 vp（等价 iOS points，非 Android px） |
| Shell（tab/URI 路由/Flyout） | ✅ 支持 | TabBar、Flyout 菜单、绝对/相对路由、query、section 栈、模态、主题色；`FlyoutBehavior.Locked` 为覆盖式常驻（内容区不让宽，与 MAUI 并排布局有差异） |
| 自绘（Shape/GraphicsView） | ✅ 已支持 | ArkUI 自绘节点（ARKUI_NODE_CUSTOM）+ OH_Drawing；ICanvas 适配器 vp 语义 |
| CollectionView 大数据量 | ✅ 已支持 | NodeAdapter 虚拟化：按可见范围物化（实测 200 条仅物化 7 条，滚动按需推进/回滚） |
| **Essentials 标准 API**（`DeviceInfo.Current` / `Preferences.Set` / `Clipboard.SetTextAsync` / `Battery.Default` / `Connectivity.Current` / `FileSystem.Current` / `Launcher.Default` / `SecureStorage.Default` / 传感器 / `Geolocation` / `MediaPicker` 等） | ✅ 22 个服务 | 启动时经 `[DynamicDependency]+CreateDelegate` 桥经 SetCurrent/SetDefault 注入；MauiHarmonyHost.Run 自动安装。DeviceInfo/DeviceDisplay/AppInfo/Clipboard/Preferences/Battery/Vibration/Connectivity/FileSystem/Launcher/Browser/PhoneDialer/Share/Email/SecureStorage + Accelerometer/Magnetometer/Gyroscope/Compass/OrientationSensor/Geolocation/MediaPicker。IMainThread 暂缓（MAUI 10 无注入点）；IShare 文件分享需跨应用 URI 授权通道，留待立项；适配指南见 [ESSENTIALS.md](ESSENTIALS.md)。验证应用 samples/dotnet/EssentialsApp |
| 自定义 Handler / 平台服务 | ❌ 需移植 | 按 [HANDLERS.md](HANDLERS.md) 五步流程写鸿蒙侧 Handler |

### Step 2：建鸿蒙外壳工程

把路线 A 的 csproj 建在 `samples/dotnet/<你的应用名>/`，然后：

```xml
  <ItemGroup>
    <!-- 方式一：工程引用你的共享 UI 库（推荐） -->
    <ProjectReference Include="..\..\..\MySharedUi\MySharedUi.csproj" />
    <!-- 方式二：直接链接源文件/页面（试用阶段不想动共享库结构时） -->
    <Compile Include="..\..\..\MyMauiApp\Pages\**\*.xaml.cs" />
    <MauiXaml Include="..\..\..\MyMauiApp\Pages\**\*.xaml" />
  </ItemGroup>
```

`Program.cs` 里组装鸿蒙入口（替代你原 AppShell 的角色）：

```csharp
public static void Register()
    => MauiHarmonyHost.Run(() => new NavigationPage(new YourExistingMainPage()));
```

你的页面代码不用改——`INavigation`、生命周期事件、绑定全部照旧。

### Step 3：编译验证 + 真机/模拟器

同路线 A 的 Step 5。建议先用最小的两三个页面验证控件覆盖度（§3.1 的 ⚠️/❌ 项逐个过），
再全量引入。

### 3.1 移植注意（MAUI 官方语义已对齐的部分不再列出）

- `HorizontalOptions/VerticalOptions` 在 Grid/AbsoluteLayout 内为**单元格内对齐**（非 Fill 收缩偏移）；
  XAML 里没有 `HorizontalLayoutAlignment` 这个可写属性（MAUI 官方同样没有）
- 空字符串赋给 Text 类属性是合法的（基类已处理空串封送）
- 布局调试日志已 `const` 门控；需要布局级日志时改 `HarmonyManagedLayoutHandler.LogLayout`

---

## 4. 构建链路详解（一键背后的三步）

| 步骤 | 脚本 | 做什么 | 关键环境变量 |
|---|---|---|---|
| 1. AOT 编译 | PublishAotClang / MSBuild | Windows 本地直接 `dotnet publish -r linux-musl-arm64` / `-r linux-musl-x64`；arm64 云真机会自动应用 NativeAOT NUMA 探测补丁 | — |
| 2. 打 HAP | `scripts/build-hap.cmd` | hvigor assembleHap 生成 unsigned HAP（DevEco 路径自动探测） | — |
| 2b. 签名 | `scripts/sign-hap.ps1` | 提供完整 `HarmonySigning*` 属性时自动调用 `hap-sign-tool.jar`，输出 `entry-default-signed.hap` | — |
| 3. 部署 | `scripts/deploy-hap.sh` / `scripts/deploy-hap.ps1` | 优先安装 signed HAP，无签名产物时回退 unsigned；启动并跟踪 HarmonyHost 日志 | `HDC_TARGET` 设备选择 |

其它 targets：`HarmonyStageHost` / `HarmonyBuildLibApp` / `HarmonyBuildHap` / `HarmonyDeploy` 可单独执行。
如果只想手动打某个样例的 HAP，可以用：
```powershell
.\scripts\build-hap.cmd WeatherTwentyOne
```
该命令会自动使用 `samples/dotnet/WeatherTwentyOne/obj/harmony/host`。也可以传样例目录或 staged host 目录。
可用属性：`HarmonyHostRoot`（宿主模板目录）、`HarmonyGenerateHost`（默认 true）、`HarmonyBundleId`、
`HarmonyAppTitle`、`HarmonyHostDir`、`HarmonyPowerShell`（默认 `pwsh`，需 PowerShell 7+）。
签名属性：`HarmonySigningKeystore/KeystorePassword/CertAlias/CertPassword/CertPath/Profile`。
签名不写入 hvigor 的 `build-profile.json5`，因此不受 hvigor 对 `storePassword/keyPassword`
至少 32 字符的校验限制；signed HAP 位于应用 `obj/harmony/host/entry/build/default/outputs/default/`。

**宿主工程自动生成**：`HarmonyStageHost` 把共享模板（`samples/HarmonyHost`）实例化到应用工程的
`obj/harmony/host/`，并按应用重写 `bundleName`（默认 `com.arktsbinding.<工程名去符号小写>`）与应用
显示名——每个应用有独立的包名与 HAP，用户全程不需要在 DevEco 里建工程（hvigor 仅以 CLI 方式借用
DevEco 安装目录的 node/hvigor/SDK）。想用自备宿主：`-p:HarmonyGenerateHost=false -p:HarmonyHostRoot=<目录>`。

**权限自动推导**：构建时会用 Roslyn source generator 扫描当前应用工程里的 MAUI/Essentials 调用，
并生成 `obj/harmony/permissions.json`。`HarmonyStageHost` 再根据这份清单重写宿主的
`entry/src/main/module.json5` 与 `entry/src/main/resources/base/element/string.json`。
当前覆盖的高置信度映射包括：

- `Geolocation` / `Permissions.LocationWhenInUse` → `ohos.permission.LOCATION`
- `MediaPicker.Capture*` / `Permissions.Camera` → `ohos.permission.CAMERA`
- `MediaPicker.Pick*` / `Permissions.Photos` / `Permissions.Media` → `ohos.permission.READ_MEDIA`
- `Vibration` / `Permissions.Vibrate` → `ohos.permission.VIBRATE`
- `Accelerometer` / `Gyroscope` → `ohos.permission.ACCELEROMETER` / `ohos.permission.GYROSCOPE`
- `Permissions.Microphone` / `Permissions.Speech` → `ohos.permission.MICROPHONE`
- `Permissions.StorageRead` / `Permissions.StorageWrite` → `ohos.permission.READ_WRITE_DOCUMENTS_DIRECTORY`
- `Permissions.Bluetooth` → `ohos.permission.USE_BLUETOOTH`
- `Permissions.CalendarRead` / `Permissions.CalendarWrite` → `ohos.permission.READ_CALENDAR` / `ohos.permission.WRITE_CALENDAR`
- `Permissions.ContactsRead` / `Permissions.ContactsWrite` → `ohos.permission.READ_CONTACTS` / `ohos.permission.WRITE_CONTACTS`
- `Permissions.PostNotifications` → `ohos.permission.PUBLISH_AGENT_REMINDER`

权限默认会生成 `usedScene.when`：

- 前台定位、媒体、传感器、麦克风、存储、蓝牙、日历、联系人：`inuse`
- 震动、通知类权限：`always`

如果应用工程引用了其他也使用 `HarmonyOS.Maui` 的项目，`HarmonyGeneratePermissions`
会自动聚合这些 ProjectReference 生成的 `obj/harmony/permissions.json`。
同名权限会合并，`always` 优先于 `inuse`。

如果需要补充尚未映射的权限，在应用工程里显式声明即可，生成结果会与自动推导合并：

```xml
<ItemGroup>
  <HarmonyPermission Include="ohos.permission.MICROPHONE" When="inuse" />
</ItemGroup>
```

权限推导现在会同时扫描应用工程自身的 C# 源码和 XAML 事件处理器；引用类库内部调用仍通过
ProjectReference 聚合，运行时反射不参与推导。`HarmonyPermission` 仍然是权限清单的最终事实源，
自动推导只做高置信度补充。

如果内置映射不满足需求，可以在应用工程根目录添加 `harmony-permissions.custom.json`：

```json
{
  "version": 3,
  "mauiMethods": [
    {
      "containingType": "Microsoft.Maui.ApplicationModel.Communication.IContacts",
      "methodName": "GetAllAsync",
      "permission": "ohos.permission.READ_CONTACTS",
      "when": "inuse"
    }
  ]
}
```

自定义映射会与内置映射合并；若要覆盖已有映射，需要把对应条目的 `override` 设为 `true`。

构建后可单独查看权限报告：

```powershell
dotnet build -t:HarmonyPermissionReport
```

报告位于 `obj/harmony/permissions.report.md`，内容分为：

- 自动推导
- 显式声明
- 最终写入 `module.json5` 的合并结果

如果代码里调用了 `Permissions.RequestAsync<T>`，但 `T` 尚未映射到 HarmonyOS 权限，
source generator 会给出 `HMP001` warning。

权限推导还会输出以下 warning：

- `HMP002`：调用了已知需要权限的 MAUI API，但映射表尚未提供 HarmonyOS 权限；
  当前覆盖 `Contacts.GetAllAsync`、`Contacts.PickContactAsync`、`Screenshot.CaptureAsync`。
- `HMP003`：显式声明的 HarmonyOS 权限没有匹配到任何自动推导结果。
  显式清单来自 `obj/harmony/explicit-permissions.json`，由 MSBuild 的
  `HarmonyPermission` item 生成。
- `HMP004`：`Permissions.RequestAsync<T>` 的 `T` 存在多个可用 HarmonyOS 权限候选；
  当前用于 `StorageRead` / `StorageWrite`，需要开发者显式选择目标权限。
- `HMP005`：自定义映射与内置映射冲突，且没有显式声明 `override: true`。
- `HMP006`：`harmony-permissions.custom.json` 不是合法的映射文档。

如需校验 `permissions.json` 与 `module.json5` 是否一致，可运行：

```powershell
dotnet build -t:HarmonyPermissionCheck
```

发布包的本地消费验收可运行：

```powershell
./scripts/verify-package-consumer.ps1
```

该脚本会打包并从本地 NuGet feed 创建临时消费者工程，验证包还原、编译、
`HarmonyPermissionCheck`、XAML 事件来源、自定义映射，以及 `LOCATION` / `VIBRATE` 权限推导。

**用到了新的 @ohos.* 模块**？在 `samples/HarmonyHost/entry/src/main/ets/ohosImports.ets` 登记一行
re-export（`napi_load_module` 的平台铁律，详见 HANDLERS §4.4）——宿主模板层面的修改改模板即可，
暂存实例会在下次构建自动带上。

**真机**：arm64 云真机启动链路已验证；`HarmonyBuildLibApp` 会自动应用
`patch-openharmony-nativeaot.ps1`，规避云真机 seccomp 对 `get_mempolicy` 的拦截。
剩余待验证项是更广的真机矩阵与性能调参（ROADMAP 1.5）。

---

## 5. 排障速查

| 症状 | 去哪看 |
|---|---|
| 部署后白屏/闪退 | `hdc shell hilog \| grep HarmonyHost`；`.NET UI built successfully` 是否出现 |
| 导航静默失败、后续导航全挂 | FireAndForget 吞了异常——确认已按 §2 改造 `FireAndForgetNavigation` 打 hilog（HelloApp 版可照抄） |
| Windows 本地 AOT 失败 | 检查 `PublishAotClang` 包是否还原成功，以及 .NET 10 SDK 是否可用 |
| hdc 命令路径被 Git Bash 吃掉 | 前缀 `MSYS_NO_PATHCONV=1` |
| 更多坑 | [HANDLERS.md §7 速查表](HANDLERS.md) |

---

## 6. 当前能力边界（决定接入评估的细节）

- **控件**：28 个具体 Handler / 30 个工厂分派形态（基础控件 + Picker/RefreshView/BoxView + CollectionView 虚拟化/CarouselView + Shape/GraphicsView 自绘 + Shell 第一版）
- **手势**：Tap/Pan/Pinch/Swipe/Pointer/Drag&Drop；hover、鼠标按键区分待做
- **布局**：StackLayout（flex 托管）+ Grid/AbsoluteLayout（MAUI 托管，对齐/ZIndex/Auto 轨道自适应已对齐）
- **自绘**：Shape + GraphicsView（ArkUI 自绘节点 + OH_Drawing）
- **服务**：438 个 @ohos.* 模块绑定（Promise→Task、.NET 事件、ArrayBuffer/Map）+ **Essentials 22 服务**
- **证书/签名**：模拟器免签；真机需自行准备签名物料
- 路线图：[ROADMAP.md](ROADMAP.md)（M1 与 M2 已完成；M3 工程化核心已落地，
  最小 CI 门禁、性能基线脚本与 MAUI 权限自动推导已就绪）
