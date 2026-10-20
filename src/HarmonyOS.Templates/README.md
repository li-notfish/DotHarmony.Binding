# HarmonyOS MAUI 应用模板

本包提供两个项目模板。两者都生成 NativeAOT 共享库形式的 MAUI 应用：stage 共享的 HarmonyOS ArkTS 宿主、
编译 `libapp.so`、打包 HAP 并部署到已连接的设备。

- `harmony-maui`：标准应用模板，引用 `HarmonyOS.Maui`，不含 WebView 桥接文件。
- `harmony-maui-web`：带 WebView 的应用模板，引用 `HarmonyOS.Maui.Web`，包含 WebView 导出薄转发层，
  并声明 `ohos.permission.INTERNET`。

安装模板包后创建应用：

```powershell
dotnet new harmony-maui -n MyApp
dotnet new harmony-maui-web -n MyWebApp
```

两个模板都面向 `net10.0-harmonyos`，由 `HarmonyOS.Maui` 提供 `HarmonyStageHost`、`HarmonyBuildLibApp`、
`HarmonyBuildHap`、`HarmonyDeploy`、`HarmonyRun` 等目标。

宿主核心保持共享：`HarmonyStageHost` 把 `samples/HarmonyHost`（或包内副本）实例化到应用工程的
`obj/harmony/host`；引用 `HarmonyOS.Maui.Web` 时，其 target 会把 `host/Index.ets` 作为 WebView
覆盖层注入该暂存宿主。

包名默认由构建目标按工程名推导（`com.arktsbinding.<name-lower>`）；上架或云调试需要登记包名时显式指定：
`dotnet new harmony-maui -n MyApp --bundleId com.example.myapp`。
