# BlazorHybridApp（实验）

Blazor Hybrid 在 HarmonyOS 上的最小验证：Router 起始路由、首次渲染、
WebView 原生点击触发的 Blazor 事件分发，以及 IJSRuntime 双向互操作。

## 运行

```powershell
dotnet build samples/dotnet/BlazorHybridApp -t:HarmonyRun
```

## 覆盖的能力

- **启动时序**：宿主 `Web onPageEnd` → `native.notifyWebPageEnded` → `Blazor.start()`，
  `__bwv` 握手自确认（不再使用固定延迟）。
- **API 面**（对齐官方 `BlazorWebView`）：BindableProperty 的 `HostPage`/`StartPath`、
  `RootComponents`、`AddRootComponentAsync`/`RemoveRootComponentAsync`、
  `UrlLoading` + `HarmonyUrlLoadingStrategy`（`OpenExternally`/`OpenInWebView`/`CancelLoad`）、
  `BlazorWebViewInitializing`/`Initialized`、`WebResourceRequested`，以及 `IHarmonyBlazorWebView` 接口。
- **内嵌布局**：`BlazorApp` 把 WebView 放进 `Grid` 的 star 行、原生 `Label`/`Button` 放 Auto 行，
  验证 Web 与原生控件同页混排、各自独立响应点击（官方 `BlazorWebView` 的常规用法）。
- **JS Interop**：`JsInterop.razor` 演示 .NET → JS（`IJSRuntime.InvokeAsync`）与
  JS → .NET（`DotNetObjectReference` + `[JSInvokable]`）双向调用。

## 说明

- 使用 `HarmonyOS.Maui.Blazor` + NDK ArkWeb scheme handler。
- **多实例**：`MultiInstancePage` 演示同页并排两个 `HarmonyBlazorWebView`（不同 `WebTag`）。
  宿主按需为未知 tag 创建 BuilderNode，两个实例各自渲染并可独立交互。
- **InputFile**：`Upload.razor` 演示文件上传；分块走标准 JSInterop，实测 250 KB PDF 字节无损。
- 内嵌模式要求 `WebviewController` 与 Web 组件完成关联（`onControllerAttached`）后才能调用其 API，
  宿主已按该事件驱动，不再使用延时。
- 继承 `Microsoft.Maui.Controls.WebView`，因此 `Navigating`/`Navigated`、`CanGoBack`/`GoBack`/`GoForward`、
  `UserAgent`/`Source` 等既有能力照常可用（历史前进/后退走 ArkWeb 原生历史，不用脚本）。
- 属于实验功能，稳定 API 以 MAUI/Controls 示例为准。
