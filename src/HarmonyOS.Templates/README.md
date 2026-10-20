# HarmonyOS MAUI App Templates

This package contains two project templates. Both create a NativeAOT shared-library MAUI app, stage the shared HarmonyOS ArkTS host, build `libapp.so`, package a HAP, and deploy it to a connected device.

- `harmony-maui` creates the standard app template. It references `HarmonyOS.Maui` and contains no WebView bridge files.
- `harmony-maui-web` creates a WebView-enabled app template. It references `HarmonyOS.Maui.Web`, includes the WebView native export shim, and declares `ohos.permission.INTERNET`.

After installing the template package, create an app with:

```powershell
dotnet new harmony-maui -n MyApp
dotnet new harmony-maui-web -n MyWebApp
```

Both generated projects target `net10.0-harmonyos` and support the normal HarmonyOS targets provided by `HarmonyOS.Maui`, including `HarmonyStageHost`, `HarmonyBuildLibApp`, `HarmonyBuildHap`, and `HarmonyDeploy`.

The host core remains shared. `HarmonyStageHost` stages `samples/HarmonyHost` (or its packaged copy); when `HarmonyOS.Maui.Web` is present, its target then injects `host/Index.ets` as the WebView-capable overlay for that staged host.
