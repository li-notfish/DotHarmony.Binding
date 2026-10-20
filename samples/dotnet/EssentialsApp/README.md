# EssentialsApp

MAUI Essentials 标准 API 的鸿蒙实现验证（DeviceInfo、Clipboard、Preferences、Battery、
Connectivity、Vibration、SecureStorage、FileSystem、Launcher/Browser/Share/Email、
传感器、Geolocation、MediaPicker 等）。

## 运行

```powershell
dotnet build samples/dotnet/EssentialsApp -t:HarmonyRun
```

## 说明

- 页面是诊断按钮网格：每个按钮走一次真实服务调用并显示结果/异常。
- 权限由构建期 source generator 自动推导（见 `obj/harmony/permissions.report.md`）；
  也可显式 `<HarmonyPermission>` 补充。权限被拒时按钮会显示 `PermissionException`。
- 定位/传感器/选图属于需要真机或模拟器能力项；失败原因会显示在按钮下方。
- 这不是 UI 教学示例；纯 UI 请看 `ControlsSampleApp`。
