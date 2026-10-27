# 绑定 API 权限推导（交接 Backlog）

状态：待实施 ｜ 提出日期：2026-10-28 ｜ 优先级：中

## 问题陈述

权限推导器的三类来源（见 `docs/PERMISSIONS.md`）全部只覆盖 MAUI API 面：

1. `HarmonyOS.Maui.Permissions` 的 `RequestAsync<T>` 映射；
2. `harmony-permissions.capabilities.json` 库能力契约；
3. 应用工程显式 `<HarmonyPermission>`。

应用直接调用 `HarmonyOS.Bindings.Api.*`（生成的 @ohos 绑定）不在任何来源的扫描范围内，
所需权限进不了 `module.json5`，运行时返回 `BusinessError 201: Permission denied`。

## 证据（2026-10-28，ApiDemo，模拟器 127.0.0.1:5555）

- `samples/dotnet/ApiDemo/ModuleVerifyPage.xaml.cs:101` 调用
  `HarmonyOS.Bindings.Api.Sensor.Accelerometer += ...`，Sensor 按钮报
  `Error: NAPI call 'InvokeMethod' failed with status napi_pending_exception(10) code 201: Permission denied`。
- 修复前 `obj/harmony/permissions.report.md` 仅有 4 条 `HardwarePage.xaml` 来源的权限
  （LOCATION / APPROXIMATELY_LOCATION / READ_MEDIA / CAMERA），Explicitly declared: None。
- `src/HarmonyOS.Bindings/Api/Sensor.cs` 类头注释已列明
  `/// 所需权限：ohos.permission.ACCELEROMETER, ...`——信息在源头已存在，但没有结构化形式。

## 断点分析

`tools/api-generator/apiGenerator.ts:127` 的 `extractPermissions` 已经解析 SDK `.d.ts` 的
`@permission` 标签（文件级粒度），`tools/api-generator/index.ts:727` 也已按模块聚合
（`allPermissions`），但当前只落到生成代码的 `/// 所需权限：` 文档注释里。
MSBuild 权限管线（`HarmonyPermissionGenerator`）无法消费文本注释，聚合链在此中断。

## 目标

- 绑定 API 的权限自动进入 `obj/harmony/permissions.json` → `module.json5`，
  应用工程无需为直接调用 `HarmonyOS.Bindings.Api.*` 手写 `<HarmonyPermission>`。
- `permissions.report.md` 给出绑定来源条目（来源生成文件），审计轨迹完整。

## 方案 A（推荐）：生成器产出权限 sidecar，复用聚合管线

api-generator 在生成 `Api/*.cs` 的同时，产出结构化权限清单并被 MSBuild 自动发现：

- **起步（文件级）**：每个生成模块输出一份清单（JSON 或 generated `.cs` 中的
  `[assembly: HarmonyBindingPermission(...)]` 特性），内容为
  `module（@ohos.sensor）→ permissions[]`。
  `HarmonyOS.Bindings.csproj` 通过现有 capabilities.json 发现链
  （`build/HarmonyOS.Essentials.props` 同款 `buildTransitive` 机制）或编译期特性扫描
  把清单带入聚合。
- **后续（成员级）**：`astParser` 把 `@permission` JSDoc 关联到所在签名，
  清单升级为 `module + member → permission + when`，与 capabilities.json schema v3 的
  `mauiMethods`/`mauiMembers` 对齐（可扩展为 v4 增加 `apiMethods` section）。
  聚合时按应用实际调用的成员收窄，避免清单膨胀。

优点：权限元数据留在源头（.d.ts），SDK 升级重跑生成器即刷新；
复用现有三级并集与报告管线，改动集中。

## 方案 B（备选）：扫描 Bindings.Api 调用点

Roslyn 分析应用工程中对 `HarmonyOS.Bindings.Api.*` 的调用点，把 C# 绑定成员映射回
@ohos 函数再查权限表。缺点：需要维护跨程序集成员映射；事件订阅（`+=`）等模式需
额外分析；权限事实仍不在生成源头，SDK 演进时映射表易漂移。仅作为方案 A 不可行时的退路。

## 验收标准

1. ApiDemo 删除显式 ACCELEROMETER `<HarmonyPermission>` 后重建：
   `module.json5` 含 `ohos.permission.ACCELEROMETER`，报告来源标注为绑定 sidecar/特性；
   模拟器 Sensor 按钮不再 201（正常收到 5 次加速度事件并自动退订）。
2. `HarmonyPermissionCheck` target 通过；`permissions.report.md` 新增绑定来源段。
3. 既有 XAML/Essentials 推导与 `HarmonyGestureTests` 权限相关测试不回归。
4. 显式声明与推导结果重复时去重，不触发 `HMP003`。

## 风险与边界

- `user_grant` 权限（如 Sensor 绑定同时涉及的 `ACTIVITY_MOTION`）仅声明不免运行时申请；
  本推导只保证清单完整，运行时授权仍需 MAUI Permissions 门面。
- `.d.ts` 的 `@permission` 标签是唯一事实来源；生成注释为手工文案的模块不在覆盖范围。
- 文件级起步方案会把该 .d.ts 全部权限并入清单（如 Sensor 6 条），
  对 `system_grant` 权限无害，但成员级收窄应作为后续项跟进。
- capabilities.json 若升级到 v4，需定义旧 v3 文件的兼容读取策略。

## 当前临时缓解（已落地）

`samples/dotnet/ApiDemo/ApiDemo.csproj` 已按第三类来源显式声明：

```xml
<HarmonyPermission Include="ohos.permission.ACCELEROMETER" Usage="inuse" />
```

其他 sample 若直接调用需要权限的绑定 API，需同样显式声明直至本 backlog 落地。

## 相关文件

- `docs/PERMISSIONS.md` — 推导器三类来源与诊断码
- `src/HarmonyOS.Maui.Permissions/HarmonyPermissionGenerator.cs` — 聚合实现
- `src/HarmonyOS.Maui/build/HarmonyOS.Maui.App.targets` — 报告产出与 `HarmonyPermissionCheck`
- `tools/api-generator/apiGenerator.ts` — `extractPermissions`（改造起点）
- `tools/api-generator/index.ts` — 模块循环与 `allPermissions` 聚合
- `src/HarmonyOS.Essentials/harmony-permissions.capabilities.json` — 能力契约示例
