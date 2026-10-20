# ApiDemo

`@ohos.*` 绑定与互操作演示：设备信息、电池、显示、传感器事件、Promise→Task、TSFN，
以及 **Hardware / Permissions** 页的真实 HTTP / 文件 / 权限 / 定位 / 相机流程。

## 运行

```powershell
dotnet build samples/dotnet/ApiDemo -t:HarmonyRun
```

## 页面

- `MainPage`：DeviceInfo 展示、Module Verify / Async Demo / TSFN 入口
- `ModuleVerifyPage`：真实调用 DeviceInfo / Battery / Display / Sensor；
  其余模块按钮是“模块可加载”探针（不要当作完整硬件示例）
- `AsyncDemoPage`：Promise/回调 → `Task<T>` 的各种返回类型与异常路径
- `HardwarePage`：HTTP GET（`@ohos.net.http` + `JsObject` 手工收口 response）、
  文件往返（`FileSystem` + `System.IO`）、定位权限申请、Geolocation、
  MediaPicker 选图/拍照

## 注意

- HTTP 需要模拟器/真机联网；失败会显示异常类型与消息。
- 相机在模拟器上可能不可用，按钮会显示失败原因。
- `HardwarePage` 的 `HttpResponseView` 是 `IntPtr` 降级点的正确处理示例；
  生成器的降级清单见 `artifacts/generator/api-intptr-report.json`。
