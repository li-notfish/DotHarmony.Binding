# HarmonyOS MAUI App Template

This package contains the `dotnet new harmony-maui` project template. It creates a NativeAOT shared-library MAUI app and uses the `HarmonyOS.Maui` package to stage the HarmonyOS ArkTS host, build `libapp.so`, package a HAP, and deploy it to a connected device.

After installing the template package, create an app with:

```powershell
dotnet new harmony-maui -n MyApp
```

The generated project targets `net10.0-harmonyos` and supports the normal HarmonyOS targets provided by `HarmonyOS.Maui`, including `HarmonyStageHost`, `HarmonyBuildLibApp`, `HarmonyBuildHap`, and `HarmonyDeploy`.
