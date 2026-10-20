# 互操作不变量

本文记录修改 `HarmonyOS.Interop` 与生成式 API 绑定时必须保持的约束。

## NAPI 生命周期

- `NapiReference` 必须记录创建它的 environment，并恰好释放一次原生引用。
- 跨线程释放必须经 `NapiFinalizationQueue` 延迟；只有当前 environment 与引用所有者一致时才可直删。
- 回调桥与 promise 桥不得把异常抛过原生回调边界；完成是原子的，每个句柄只释放一次。

## 异步取消

- Promise 桥与 callback void 桥必须提供 `CancellationToken` 重载，取消时以 `TaskStatus.Canceled` 结束。
- `CancellationTokenRegistration` 在取消、正常完成与异常三条路径上都要释放。
- 回调桥一旦完成，后续原生回调必须被忽略，不得重复完成任务。

## 事件注册

- 托管事件注册在原生注册失败时必须回滚。
- 原生注销失败时，托管监听必须保持注册状态，不得静默丢弃。

## 生成的绑定

- record 属性写入必须经 `.ThrowIfFailed()` 暴露 `NapiException`。
- 生成文件必须按方法体实际需要推导 using，并带上含生成器与 SDK 版本的溯源头。
- 可选值必须先转成 C# `null` 再做 NAPI 值转换；ArrayBuffer 与 `byte[]` 的通道规则不得改动。

## MAUI 契约对齐（Essentials）

- `IPreferences.Set(key, null)`：删除该键（与 dotnet/maui 的 Android / Windows 实现一致，不抛异常）。
- `IGeolocation`：定位关闭时抛 `FeatureNotEnabledException`，权限被拒时抛 `PermissionException`，
  三条路径（`GetLastKnownLocationAsync`、`GetLocationAsync`、`StartListeningForegroundAsync`）一致
  （对齐 dotnet/maui 的 iOS 行为，StartListening 同时对齐 Android）。

## 节点事件（ArkUINodeBase）

- 事件订阅自 c94bc79 起为多播：`SubscribeEvent` 追加；首个托管订阅者出现时才向原生注册，
  hub 清空时注销。
- Handler 对同一节点实例至多订阅一次（通常在创建节点处），并用精确的
  `UnsubscribeEvent(eventType, handler)` 配对退订。无参 `UnsubscribeEvent(eventType)` 会清空整个 hub，
  共享该节点的 Handler 不得使用。
