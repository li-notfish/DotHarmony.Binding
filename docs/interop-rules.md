# 互操作不变量

本文记录修改 `HarmonyOS.Interop` 与生成式 API 绑定时必须保持的约束。

## NAPI 生命周期

- `NapiReference` 必须记录创建它的 environment，并恰好释放一次原生引用。
- 跨线程释放必须经 `NapiFinalizationQueue` 延迟；只有当前 environment 与引用所有者一致时才可直删。
- 回调桥与 promise 桥不得把异常抛过原生回调边界；完成是原子的，每个句柄只释放一次。
- 任何 `napi_*` 调用都必须检查返回值（`.ThrowIfFailed()` 或显式比较 `napi_status`），不得静默忽略。
- 非 `napi_ok` 状态必须尽力带出 JS 侧错误文本（挂起异常 / `error` 包装 / 字符串常量）；
  读不到时退化为状态码语义提示，且附加信息读取失败不得覆盖原始 status。
- handle scope 只能经 `NapiHandleScope`（RAII）使用，不得手写 open/close 配对。
- handle scope 内产生的裸 `napi_value` 不得带出该 scope：需要跨帧/跨线程持有必须先转 `NapiReference`。
- 异常类型不得携带 `IntPtr`/`napi_value`——异常会跨回调帧存活，句柄在 scope 关闭后即失效。
- TSFN `release` 返回非 ok 时句柄必须作废（置零 + 标记 disposed），不得复用。

## 异步取消

- Promise 桥与 callback void 桥必须提供 `CancellationToken` 重载，取消时以 `TaskStatus.Canceled` 结束。
- `CancellationTokenRegistration` 在取消、正常完成与异常三条路径上都要释放。
- 回调桥一旦完成，后续原生回调必须被忽略，不得重复完成任务。
- 两条桥的取消/完成/拒绝三路径都必须释放 `GCHandle` 与 `CancellationTokenRegistration`，
  统一经幂等出口（重复调用无副作用）。
- 注册取消回调后必须检查 `IsCancellationRequested` 并补一次取消处理：token 在注册前已取消时
  `Register` 会同步回调，此时 `Registration` 尚未赋值，只靠回调会漏释放注册项。

## 事件注册

- 托管事件注册在原生注册失败时必须回滚。
- 原生注销失败时，托管监听必须保持注册状态，不得静默丢弃，且失败原因必须记日志
  （调用方只看得到 `false`，需能区分「本来没订阅」与「off 抛错」）。

## 机械闸

- 上述不变量中可静态判定的部分由 `tests/dotnet/HarmonyGestureTests/InteropInvariantTests.cs` 强制：
  逐调用点 status 检查、handle scope 只走 RAII、无无调用方的 `napi_*` 声明、
  两条桥的三路径释放、`TaskCompletionSource` 续体模型白名单、异常类型不带原生句柄。
- `TaskCompletionSource` 的续体线程是**有意选择**：桥内联恢复（续体需继续持有 `napi_env`），
  `ArkUINodeBase.AnimateAsync` 异步恢复（宿主未安装 UI 线程 `SynchronizationContext`）。
  新增 TCS 必须先确定属于哪一类并登记到白名单。

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
