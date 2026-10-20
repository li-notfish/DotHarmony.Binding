# WeatherTwentyOne (HarmonyOS)

This sample ports the core weather UI from the official **WeatherTwentyOne** MAUI sample to **HarmonyOS** through `HarmonyOS.Maui`.

It keeps the familiar three-part layout:

- current weather card
- hourly forecast strip
- seven-day forecast list

The data is static and intentionally local, so the sample runs without network access or API keys.

## Build and run

Run from this sample directory (the `.csproj` is the current folder):

```powershell
dotnet build -t:HarmonyRun
```

Prerequisites (PowerShell 7+, DevEco SDK, a connected emulator/device) and first-run
troubleshooting are documented in [GETTING_STARTED.md](../../../GETTING_STARTED.md).
Do not deploy `samples/HarmonyHost` directly: `-t:HarmonyRun` stages a per-app host into
`obj/harmony/host` automatically.

If you want to restore only:

```powershell
dotnet restore
```

## What this sample exercises

- MAUI XAML rendered by the HarmonyOS handlers
- `Grid`, `ScrollView`, `BindableLayout`, `CollectionView`, `Image`, and `Border`
- `ObservableCollection` bindings for the hourly and daily forecasts
- NativeAOT hosting through `HarmonyOS.Maui`
