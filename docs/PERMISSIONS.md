# 权限推导

权限生成器让工程里的 `module.json5` 与实际使用的 MAUI API 保持一致。来源有三类：

1. **内置权限类型**
   `HarmonyOS.Maui.Permissions` 把 MAUI 的 `Permissions.RequestAsync<T>()` 权限类型映射到 HarmonyOS 权限；
   一个权限类型可以映射到多个 HarmonyOS 权限。

2. **库能力契约**
   库可以在工程文件旁放一份 `harmony-permissions.capabilities.json`，把该库的 MAUI 方法、属性、
   事件映射到 HarmonyOS 权限；构建经 `ProjectReference` 或包的 `buildTransitive` props 自动发现
   （示例见 `src/HarmonyOS.Essentials/harmony-permissions.capabilities.json`）。

3. **工程显式声明**
   无法安全推导的 API 在应用工程里声明：

   ```xml
   <ItemGroup>
     <HarmonyPermission Include="ohos.permission.READ_PASTEBOARD" When="inuse" />
   </ItemGroup>
   ```

最终清单是三类来源的并集。`HarmonyPermissionCheck` 是显式 target（**不会**随普通 build 执行），
用于校验 `permissions.json`、`module.json5` 与 `permissions.report.md` 描述的是同一份权限集合：
`dotnet build <project> -t:HarmonyPermissionCheck`。

## 能力契约 schema

```json
{
  "version": 3,
  "mauiMethods": [
    {
      "containingType": "Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard",
      "methodName": "GetTextAsync",
      "permission": "ohos.permission.READ_PASTEBOARD",
      "when": "inuse"
    }
  ],
  "mauiMembers": [
    {
      "containingType": "Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard",
      "memberName": "HasText",
      "memberKind": "property",
      "permission": "ohos.permission.READ_PASTEBOARD",
      "when": "inuse"
    }
  ]
}
```

`memberKind` 取 `property` 或 `event`，方法调用走 `mauiMethods`。
方法、成员与权限类型都允许映射到多个权限；自定义映射与内置映射冲突时须显式写 `override: true`（否则 `HMP005`）。

## 构建产物

- `obj/harmony/permissions.json` — 机器可读清单
- `obj/harmony/permissions.report.md` — 人类可读的推导审计轨迹
- `obj/harmony/host/entry/src/main/module.json5` — 生成的 HAP 清单

报告会给出每条推导权限的来源文件与行号，以及显式声明与 ProjectReference 聚合的来源。

## 诊断

| 代码 | 含义 |
|---|---|
| `HMP001` | 调用了 `Permissions.RequestAsync<T>`，但 `T` 尚未映射到 HarmonyOS 权限 |
| `HMP002` | 调用了已知需要权限的 MAUI API，但映射表尚未提供 HarmonyOS 权限 |
| `HMP003` | 显式声明的 HarmonyOS 权限没有匹配到任何自动推导结果 |
| `HMP004` | `RequestAsync<T>` 的 `T` 存在多个候选权限，需要显式选择 |
| `HMP005` | 自定义映射与内置映射冲突，且未写 `override: true` |
| `HMP006` | `harmony-permissions.custom.json` 不是合法的映射文档 |
