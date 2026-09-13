# ESSENTIALS — 适配一个新的 Essentials 服务

MAUI Essentials 的鸿蒙平台实现指南。视图 Handler 的适配见 [HANDLERS.md](HANDLERS.md)；
本文只讲 `Microsoft.Maui.Essentials` / `Microsoft.Maui.Devices` / `Microsoft.Maui.ApplicationModel` /
`Microsoft.Maui.Storage` 下的平台服务。

## 0. 原理（为什么可用）

MAUI 的 Essentials 静态入口（`DeviceInfo.Current` / `Preferences.Default` / `Clipboard.Default`…）
在 netstandard 产物里是**抛异常的缺省实现**，但每个静态类都留有 internal 注入点。
本仓做法（`src/HarmonyOS.Maui/Essentials/HarmonyEssentials.cs`）：

1. 实现层：每个服务写一个 `HarmonyXxx : IXxx`（`src/HarmonyOS.Maui/Essentials/`），
   方法体调用 `HarmonyOS.Bindings.Api` 的 `@ohos.*` 包装类型；
2. 注入层：`HarmonyEssentials.Install()` 经**反射桥**完成注入——
   `[DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods)]` 收根 +
   `CreateDelegate` 缓存强类型委托。**禁止逐次 `MethodInfo.Invoke`**（AOT/性能，
   与 MauiGestureBridge 同款铁律）；
3. 时机：`Install()` 在 `MauiHarmonyHost.Run` 的 **RootBuilder lambda 内**调用（UI 线程首次
   构建、napi env 就绪）。放进 ModuleInitializer（dlopen 阶段）会在任何 napi 调用上闪退——实测
   DfxFaultLogger 崩在 HarmonyInit+16。这是铁律。

## 1. 适配五步流程

### Step 1：取证注入点（MAUI 源码，勿猜）

注入点在 `D:\Harmony\maui\src\Essentials\src\<Service>\<Service>.shared.cs`
（MAUI 源码本地副本在 `D:\Harmony\maui`）。命名规律：

| 形态 | setter | 例 |
|---|---|---|
| `Xxx.Current` | `internal static void SetCurrent(IXxx? impl)` | DeviceInfo / DeviceDisplay / AppInfo / FileSystem / Geocoding / Connectivity / AppActions |
| `Xxx.Default` | `internal static void SetDefault(IXxx? impl)` | Clipboard / Preferences / SecureStorage / Vibration / Battery / Launcher / Share / Email / PhoneDialer / Flashlight / Map / MediaPicker / FilePicker / Screenshot / TextToSpeech / HapticFeedback / Browser / Geolocation |
| 例外 | `internal static void SetCustomImplementation(Func<bool> isMainThread, Action<Action> beginInvokeOnMainThread)` | MainThread（两个委托，非接口实现） |

现有四个实现可作为范式：同步属性型（HarmonyDeviceInfo）、事件转发型（HarmonyDeviceDisplay）、
异步 Promise 型（HarmonyClipboard）。

### Step 2：确认底层 @ohos 模块

- 生成产物存在：`HarmonyOS.Bindings/Api/<Module>.cs`（没有 → 生成器不支持该 d.ts，先补生成器）；
- **转正状态**：`src/parser/index.ts` 的 `GRAYSCALE_MODULES`。灰度模块生成器会在 csproj 发射
  `Compile Remove`——**转正必须改这份名单**（源上），手改 csproj 会被下次重生成还原
  （BundleManager 踩过：`610dfa0`）；
- 宿主登记：`ohosImports.ets` 由生成器自动维护，无需手改；
- 系统能力需要 ability 上下文的（权限弹窗、窗口、startAbility 类）：宿主模板
  `EntryAbility.ets` 已导出 `globalThis.abilityContext`，C# 侧
  `NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8)` 取用。

### Step 3：写 `src/HarmonyOS.Maui/Essentials/HarmonyXxx.cs`

参照范式选择通道，硬规则：

- **同步 getter（属性）里禁止 await promise**——promise 续体在 JS 线程内联恢复，同步阻塞即死锁。
  同步语义用同步 API（`getXxxSync` / 同步属性）；没有同步 API 的服务整体走异步方法；
- **user_grant 权限**：读前 `getSelfPermissionStatus` 主动查状态（同步、廉价，每次读都查），
  未授权经 `requestPermissionsFromUser` 弹窗后重试。**不要**依赖"被拒会抛异常"——
  API 26 大多返回空壳结果而非抛错（剪贴板实测）。弹窗每会话至多一次，别骚扰用户；
- **MAUI 事件**：底层事件处理器里**失效一切缓存**再转发（否则外部改写后 getter 按旧缓存回答）；
  JS 线程 == UI 线程目前成立，但属于隐含假设，注释里标明；
- 长生命周期包装对象的方法调用走 `PinnedValue`（基类已统一处理），自建包装别缓存裸 `Handle`；
- 所有 fallback 打 `HiLog`（tag 用 "Essentials"），禁止空 catch。

### Step 4：注册

`HarmonyEssentials.Install()` 里加一个 `SetImplementation` 调用：

```csharp
SetImplementation(typeof(global::Microsoft.Maui.Storage.Preferences), "SetDefault",
    new HarmonyPreferences(), static (m, impl) => m.CreateDelegate<Action<IPreferences>>()(impl));
```

服务构造在 Install 时发生（napi 就绪）——构造函数里只做廉价初始化；任何一个服务构造抛错会
中断整个 Install（暂无逐服务隔离，见 ROADMAP 2.4）。

### Step 5：示例 + 验证

- `samples/dotnet/EssentialsApp` 是**专门的 Essentials 验证应用**（不要往 HelloApp 加——
  Controls 示例与 Essentials 示例分离是既定约定）：给新服务加一栏信息 Label 或操作按钮；
- 一键验证：`dotnet build samples/dotnet/EssentialsApp -t:HarmonyRun`，结果在页面 Label 上
  （uitest dumpLayout 读取），排障用 `hilog | grep Essentials`；
- 涉及权限的服务用**卸载重装**重置授权状态再全流程走一遍（`bm uninstall -n <bundle>`）；
- 纯逻辑（单位换算、枚举映射、状态机）进 `tests/dotnet/HarmonyGestureTests/EssentialsMappingTests.cs`。

## 2. 已踩坑速查（Essentials 相关，完整表见 HANDLERS.md §7）

| 症状 | 根因 | 修复/规则 |
|---|---|---|
| 启动闪退（崩在 HarmonyInit） | Install/任何 napi 调用放进 ModuleInitializer | 只在 RootBuilder lambda 内装（§0 铁律） |
| 同步属性调用挂死 | getter 里 await promise（JS 线程内联续体） | 同步 getter 只用同步 API |
| 被拒却"成功"但结果是空（recordCount=0） | user_grant 被拒返回空壳不抛错 | 读前查状态 → 弹窗 → 重试（HarmonyClipboard 范式） |
| 用户在设置里授权了但 app 内仍拿不到 | 权限标志一次性闩死 | 每次读重查状态，弹窗才受"每会话一次"限制 |
| 其他 app 改写剪贴板后 HasText 仍旧值 | 缓存未失效 | 底层变更事件先失效缓存再转发 |
| 包装对象隔段时间方法全挂（napi_function_expected） | 裸 Handle 跨句柄范围 + GC 失效 | 基类已统一走 PinnedValue，别绕过 |
| JS 报 401/must be Array 但错误文本不可见 | 旧版 ThrowIfFailed 丢 pending exception 消息 | 已修；手写封送数组用 `NativeValue.From(string[])` 或 `NodeApi.CreateInstance(global, "Array"u8, item)` |
| 手改 csproj 转正被还原 | 转正状态归生成器所有 | 改 `src/parser/index.ts` 的 GRAYSCALE_MODULES |
| ability 上下文调用报 "The context is invalid" | 构造时缓存了 abilityContext 裸句柄，Install 到使用之间句柄失效 | 每次使用前重读 `NodeApi.GetProperty(global, "abilityContext"u8)`，勿缓存裸 napi 值（HarmonyPreferences 先例） |

## 3. 服务清单

注入点签名以 MAUI 源码为准（上表）；底层模块转正状态随生成器漂移，动手前重查 §Step 2。

| 服务 | 注入点 | 底层 @ohos | 状态 |
|---|---|---|---|
| IDeviceInfo | SetCurrent | deviceInfo（同步属性） | ✅ 2026-09-13 |
| IDeviceDisplay | SetCurrent | display（同步 + Change 事件） | ✅ 2026-09-13 |
| IAppInfo | SetCurrent | bundle.bundleManager（getBundleInfoForSelfSync） | ✅ 2026-09-13 |
| IClipboard | SetDefault | pasteboard SystemPasteboard（user_grant 授权闭环） | ✅ 2026-09-13 |
| IPreferences | SetDefault | data.preferences（值用类型标签字符串编码；getSync/putSync 经 NodeApi 直调——包装的 ValueType 签名被 distributedData 同名枚举污染） | ✅ 2026-09-13（43/44 测试含编解码 21 个） |
| IBattery | SetDefault | batteryInfo（纯同步属性）+ @ohos.power.getPowerMode()（省电模式）；变化事件走 usual.event.BATTERY_CHANGED / POWER_SAVE_MODE_CHANGED commonEvent 订阅（回调内重读属性+去重） | ✅ 2026-09-13（模拟器实测 level/state/source/saver；chargingStatus=0 → 按 Discharging 处理） |
| IVibration | SetDefault | vibrator | 待做（模块已转正） |
| IConnectivity | SetCurrent | net.connection（灰度，需转正；含事件） | 待做 |
| IFileSystem | SetCurrent | file.fs（灰度） | 待做 |
| ILauncher / IShare / IEmail / IPhoneDialer / IBrowser | SetDefault | want/startAbility（abilityContext 通道） | 待做 |
| IMainThread | SetCustomImplementation（双委托） | JS 线程 == UI 线程，直接执行 | 待做 |
| ISecureStorage | SetDefault | security.asset | 待做 |
| ITextToSpeech / IHapticFeedback / IFlashlight | SetDefault | textToSpeech / vibrator / brightness | 远期 |
| IGeolocation / IMap / ISensors 族 / IMediaPicker / IFilePicker / IScreenshot | SetDefault | 各自子系统 | 远期 |

已实现服务的成员级缺口（非阻塞、按价值补）：AppInfo.RequestedTheme/ShowSettingsUI、
DeviceDisplay.KeepScreenOn、DeviceInfo.Idiom 平板判定——abilityContext 通道已就位。
