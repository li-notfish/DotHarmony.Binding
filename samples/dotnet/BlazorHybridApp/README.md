# BlazorHybridApp（实验）

Blazor Hybrid 在 HarmonyOS 上的最小验证：Router 起始路由、首次渲染、
WebView 原生点击触发的 Blazor 事件分发。

## 运行

```powershell
dotnet build samples/dotnet/BlazorHybridApp -t:HarmonyRun
```

## 说明

- 使用 `HarmonyOS.Maui.Blazor` + NDK ArkWeb scheme handler；一个应用仅支持一个 WebView 宿主。
- 页面目前只有 Counter/About，**没有 `IJSRuntime` 互操作示例**；不要把它当作完整
  Blazor 接入指南。架构要点见根 [README.md](../../../README.md) 的“已知限制”。
- 属于实验功能，稳定 API 以 MAUI/Controls 示例为准。
