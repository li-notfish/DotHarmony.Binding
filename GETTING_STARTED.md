# HarmonyOS MAUI 应用开发指南

> 面向两类读者：**从零创建**一个鸿蒙 MAUI 应用，或把鸿蒙平台**加入已有 MAUI 应用**。
> 架构一句话：你的 UI 代码编译成 NativeAOT 共享库（libapp.so），由 ArkTS 宿主 dlopen 后挂载，
> 所有控件经 HarmonyOS.Maui Handler 映射为 ArkUI 原生节点渲染。使用方式与 MAUI 单项目体验对齐：
> `dotnet build -t:HarmonyRun` 一键"编译 → 打 HAP → 部署运行"。

前置概念（详见 [README](README.md) / [HANDLERS](HANDLERS.md)）：

| 层 | 内容 | 你是否要写 |
|---|---|---|
| 应用工程 | Program.cs + XAML 页面 + Platforms/HarmonyOS 启动代码 | ✅ 本文的主角 |
| HarmonyOS.Maui | 22 个控件 Handler、手势、导航、托管布局 | ❌ 引用即可 |
| HarmonyOS.Bindings | ArkUI NDK 原生节点 + 438 个 @ohos.* 模块绑定 | ❌ 引用即可（仅声明用到的 @ohos 模块） |
| HarmonyHost（ArkTS 宿主） | dlopen libapp.so 的壳工程模板 | ❌ 由 targets 自动生成按应用实例（见 §3） |

---

## 0. 前置条件

| 工具 | 用途 | 说明 |
|---|---|---|
| .NET 10 SDK（Windows） | 本机编译检查（C#/XAML 编译期验证） | `dotnet --version` ≥ 10 |
| WSL2（Ubuntu） | NativeAOT 交叉编译 libapp.so | 构建脚本走 WSL 原生文件系统 |
| .NET 10 SDK（WSL 内 `$HOME/.dotnet`） | ILC 编译 | |
| aarch64 musl 交叉工具链 | arm64 libapp.so | musl.cc gcc 解压到 `$HOME/aarch64-linux-musl-cross` |
| zig（`$HOME/zig`） | x64 libapp.so（zig cc / zig objcopy） | 模拟器是 x86_64，必需 |
| DevEco Studio | hvigor 打 HAP、hdc 部署、模拟器 | 记住安装路径，脚本自动探测常见位置 |

以上工具链就绪后，仓库根目录：

```powershell
# 一键验证链路（HelloApp 示例：libapp.so 双架构 → HAP → 部署到模拟器）
pwsh scripts/remote-build.ps1      # LOCAL=true 走本地 WSL；否则按 REMOTE/BUILD 变量走远程
scripts\build-hap.cmd              # hvigor 打包
bash scripts/deploy-hap.sh         # hdc 安装 + 启动（多设备：HDC_TARGET=127.0.0.1:5555）
```

或直接在示例工程上：`dotnet build samples/dotnet/HelloApp -t:HarmonyRun`（等价上面三步）。

---

## 1. 路线 A：从零创建鸿蒙 MAUI 应用

> 完整可运行的参照：[samples/dotnet/HelloApp](samples/dotnet/HelloApp)（XAML + 导航 + 手势 + 托管布局）与
> [samples/dotnet/ApiDemo](samples/dotnet/ApiDemo)（@ohos.* 服务调用）。
> **当前约束**：应用工程须放在本仓库 `samples/dotnet/<应用名>/` 下（打包清单与 libapp.so 取回路径按此约定；
> NuGet 化后解除，见 ROADMAP M3）。

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
    <ProjectReference Include="..\..\HarmonyOS.Bindings\HarmonyOS.Bindings.csproj" />
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

支持的根页类型：`ContentPage` / `NavigationPage`（推荐，配套返回键/标题栏/模态）/ `TabbedPage` 待支持。
**Shell 不在支持计划内**——多平台 Shell 应用请把鸿蒙入口改写为 NavigationPage 结构（见路线 B）。

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

# 一键全链路（WSL AOT 双架构 → hvigor HAP → hdc 部署启动）
dotnet build samples/dotnet/MyApp -t:HarmonyRun
```

部署目标默认模拟器；多设备在线时 `HDC_TARGET=192.168.1.6:8710 dotnet build -t:HarmonyRun`（或逐段手动跑三个脚本，见 §3）。

---

## 2. 路线 B：已有 MAUI 应用加入鸿蒙平台

原则：**不动既有平台工程**（Android/iOS/Catalyst 照旧），为鸿蒙建一个独立的"外壳"工程，复用你的共享 UI 层。

### Step 1：盘点可复用的代码

| 你的代码 | 可否直接复用 | 说明 |
|---|---|---|
| XAML 页面 / 控件树（不依赖 Shell） | ✅ 直接复用 | XAML 编译期膨胀，平台无关 |
| MVVM（ViewModel/绑定/Command） | ✅ 直接复用 | 数据绑定走 MAUI 标准管线 |
| `Navigation.PushAsync/PopAsync/PushModalAsync` | ✅ 直接复用 | 经 IStackNavigation 协议转接到 ArkUI |
| 手势识别（Tap/Pan/Pinch/Swipe/Pointer） | ✅ 直接复用 | 注意 PanUpdated 单位为 vp（等价 iOS points，非 Android px） |
| Shell（flyout/tab/URI 路由） | ❌ 需改造 | 入口改为 `new NavigationPage(...)`；TabbedPage 后续支持 |
| 自绘（Shape/GraphicsView） | ❌ 待支持 | 需 MAUI Graphics 前端（ROADMAP 1.4 剩余） |
| CollectionView 大数据量 | ⚠️ 可用 | 当前全量物化无虚拟化（NodeAdapter 后续做） |
| Essentials（DeviceInfo/Clipboard…） | ⚠️ 部分 | M2 服务层可用 `@ohos.*` 绑定替代；Essentials 接口实现未启动 |
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

同路线 A 的 Step 5。建议先用最小的两三个页面验证控件覆盖度（§2.1 的 ⚠️/❌ 项逐个过），
再全量引入。

### 2.1 移植注意（MAUI 官方语义已对齐的部分不再列出）

- `HorizontalOptions/VerticalOptions` 在 Grid/AbsoluteLayout 内为**单元格内对齐**（非 Fill 收缩偏移）；
  XAML 里没有 `HorizontalLayoutAlignment` 这个可写属性（MAUI 官方同样没有）
- 空字符串赋给 Text 类属性是合法的（基类已处理空串封送）
- 布局调试日志已 `const` 门控；需要布局级日志时改 `HarmonyManagedLayoutHandler.LogLayout`

---

## 3. 构建链路详解（一键背后的三步）

| 步骤 | 脚本 | 做什么 | 关键环境变量 |
|---|---|---|---|
| 1. AOT 编译 | `scripts/remote-build.ps1` | 打包源码 → WSL 原生 FS → ILC 双架构（arm64 musl.cc / x64 zig）→ 取回 libapp.so 放入 `samples/HarmonyHost/entry/libs/<abi>/` | `LOCAL=true` 本地 WSL；`REMOTE`/`BUILD` 远程；`DEMO_APP` 应用名 |
| 2. 打 HAP | `scripts/build-hap.cmd` | hvigor assembleHap（DevEco 路径自动探测） | — |
| 3. 部署 | `scripts/deploy-hap.sh` | hdc 安装 → 启动 → 跟踪 HarmonyHost 日志 | `HDC_TARGET` 设备选择 |

其它 targets：`HarmonyStageHost` / `HarmonyBuildLibApp` / `HarmonyBuildHap` / `HarmonyDeploy` 可单独执行。
可用属性：`HarmonyHostRoot`（宿主模板目录）、`HarmonyGenerateHost`（默认 true）、`HarmonyBundleId`、
`HarmonyAppTitle`、`HarmonyHostDir`、`HarmonyPowerShell`（默认 `pwsh`，需 PowerShell 7+）。

**宿主工程自动生成**：`HarmonyStageHost` 把共享模板（`samples/HarmonyHost`）实例化到应用工程的
`obj/harmony/host/`，并按应用重写 `bundleName`（默认 `com.arktsbinding.<工程名去符号小写>`）与应用
显示名——每个应用有独立的包名与 HAP，用户全程不需要在 DevEco 里建工程（hvigor 仅以 CLI 方式借用
DevEco 安装目录的 node/hvigor/SDK）。想用自备宿主：`-p:HarmonyGenerateHost=false -p:HarmonyHostRoot=<目录>`。

**用到了新的 @ohos.* 模块**？在 `samples/HarmonyHost/entry/src/main/ets/ohosImports.ets` 登记一行
re-export（`napi_load_module` 的平台铁律，详见 HANDLERS §4.4）——宿主模板层面的修改改模板即可，
暂存实例会在下次构建自动带上。

**真机**：工具链就绪（arm64 libapp.so 始终同步产出），当前未验证项是签名物料与真机性能调参（ROADMAP 1.5）。

---

## 4. 排障速查

| 症状 | 去哪看 |
|---|---|
| 部署后白屏/闪退 | `hdc shell hilog \| grep HarmonyHost`；`.NET UI built successfully` 是否出现 |
| 导航静默失败、后续导航全挂 | FireAndForget 吞了异常——确认已按 §1 改造 `FireAndForgetNavigation` 打 hilog（HelloApp 版可照抄） |
| Windows 编译过、WSL 编译不过 | 根目录新增共享文件（如 `Directory.Build.props`）要加进 `scripts/build-files.txt` 打包清单 |
| hdc 命令路径被 Git Bash 吃掉 | 前缀 `MSYS_NO_PATHCONV=1` |
| 更多坑 | [HANDLERS.md §7 速查表](HANDLERS.md) |

---

## 5. 当前能力边界（决定 §2 盘点结论的细节）

- **控件**：22 个 Handler（基础控件 + Picker/RefreshView/BoxView + CollectionView/CarouselView 物化版）
- **手势**：Tap/Pan/Pinch/Swipe/Pointer；Drag/Drop、hover、鼠标按键区分待做
- **布局**：StackLayout（flex 托管）+ Grid/AbsoluteLayout（MAUI 托管，对齐/ZIndex/Auto 轨道自适应已对齐）
- **服务**：438 个 @ohos.* 模块绑定（Promise→Task、.NET 事件、ArrayBuffer/Map）；Essentials 未启动
- **证书/签名**：模拟器免签；真机需自行准备签名物料
- 路线图：[ROADMAP.md](ROADMAP.md)（M1 控件/M2 服务已完成，M3 NuGet 打包等工程化远期）
