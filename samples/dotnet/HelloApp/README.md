# HelloApp

最小组装示例，用来验证「XAML/导航/手势/布局 → NativeAOT → HarmonyOS 宿主」全链路。

## 运行

在仓库根目录：

```powershell
dotnet build samples/dotnet/HelloApp -t:HarmonyRun
```

`HarmonyRun` 会自动 staging 宿主到 `samples/dotnet/HelloApp/obj/harmony/host`，
再编译双架构 `libapp.so`、打 HAP、安装并启动。不要单独部署 `samples/HarmonyHost`。

## 默认入口

默认根页是 `NavigationPage(MainPage)`（`Program.UseShell = false`）。把
`samples/dotnet/HelloApp/Program.cs` 里的 `UseShell` 改为 `true` 可切到
`AppShell`（`VerificationShell.cs`，Shell 路由/query/模态回归宿主，供脚本使用）。
应用宿主为 `VerificationApp`（真实 `Application`，自带 Resources/UserAppTheme）。

## 覆盖内容

- `MainPage.xaml` / `SecondPage` / `ModalPage`：XAML、导航栈、模态、生命周期
- `LayoutDemoPage` / `GesturesDemoPage` / `ControlsDemoPage`：布局、手势、控件
- `ProbeZIndexPage` / `BindingProbePage`：专项探针（非教学页）

## 注意

- 页面推入统一用 `FireAndForgetNavigation()` 观察异常（不要空 catch，否则后续导航会挂起）。
- 需要 PowerShell 7+、DevEco SDK 与在线模拟器/真机；完整前置见
  [GETTING_STARTED.md](../../../GETTING_STARTED.md)。
