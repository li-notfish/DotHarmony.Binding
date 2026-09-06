# ArkTsBinding

.NET (NativeAOT) 与 HarmonyOS (ArkUI) 的高性能绑定库 —— 为 .NET MAUI 鸿蒙 Handler 提供 platform view 层。

**当前状态：端到端验证通过。** C# (NativeAOT) 在鸿蒙模拟器/真机上完成：运行时初始化 → 创建原生 ArkUI 节点树上屏 → 属性设置 → 点击事件回调 → 回写 UI，全链路闭环。

## 这是什么

对标 Mono.Android（Java 绑定）与 Microsoft.iOS（ObjC 绑定）的平台绑定库：为上层 UI 框架（dotnet/maui fork 的 HarmonyOS Handler，下一里程碑）提供**可命令式创建、持稳定句柄、可挂载、可响应事件**的平台视图类型。

```
[MAUI HarmonyOS Handler —— 下一里程碑]
    │ 消费 platformView
[HarmonyOS.Bindings]
    ├─ Nodes/       ArkUINodeBase : { ArkUI_NodeHandle } 包装类（解析器自动生成）
    ├─ NativeNode/  ArkUI NDK C API 互操作（ArkUI_NativeNodeAPI_1 函数表镜像、事件总线）
    ├─ Runtime/     napi 互操作（非 UI 的 @ohos.* 服务调用；env 注入、hilog 桥）
    └─ Hosting/     Host 入口（libapp.so 导出 HarmonyInit / HarmonyBuildUI）
    │ P/Invoke / dlopen
[HarmonyHost 宿主]  ArkTS 页面 (ContentSlot) + C shim (libentry.so) + libapp.so (NativeAOT)
```

## 关键架构决策

1. **UI 走 ArkUI NDK C API，不走 napi 调 ArkTS 对象。**
   ArkUI 声明式组件由 ArkTS 编译器在 build() 内构建，napi 创建的裸对象没有挂进 UI 树的路径；
   命令式 UI 的官方原生路径是 `ArkUI_NativeNodeAPI_1`（createNode/setAttribute/setEventListener），
   经 NodeContent/ContentSlot 挂载。`ArkUI_NodeHandle` 是稳定句柄（对齐 UIKit 指针语义）。
   早期白皮书（ArkTsBinding.md，文末附决策记录）的「napi 直调 ArkTS 影子对象」方案已被此路线取代。
2. **非 UI 的 @ohos.\* 服务**（传感器/定位等）继续走 napi，env 由宿主 shim 在入口注入（`NapiEnv.Initialize`）。
3. **运行时**：鸿蒙禁 JIT，唯一路线是 NativeAOT（linux-musl）产出 libapp.so，由 C shim dlopen。
   Windows 不支持 cross-OS NativeAOT，交叉编译在远程 Linux（WSL2）完成。
4. **入口程序集导出**：ILC 只导出入口程序集内的 `[UnmanagedCallersOnly]`，
   因此每个 libapp.so 应用需要薄转发层（见 HelloApp 的 `NativeExports`）。

## 快速开始

环境：Node.js + npm（解析器）、Python 3（头文件提取）、.NET 10 SDK（绑定库）、
DevEco Studio（内置 HarmonyOS SDK/NDK/hvigor）、可 SSH 的 Linux（NativeAOT 交叉编译）。

```bash
# 1. 解析器构建 + 测试（45 用例）
npm install && npm test

# 2. 从 NDK 头文件生成 C# 枚举（ArkUINodeTypes.g.cs + .json 元数据）
python src/nativeBinding/extract_arkui_types.py --dump-json HarmonyOS.Bindings/NativeNode/ArkUINodeTypes.json

# 3. 从 SDK 组件 .d.ts 生成 NodeHandle 包装类 → HarmonyOS.Bindings/Nodes/
npx ts-node src/parser/index.ts --native

# 4. Windows 上构建绑定库 + 样例
dotnet build ArkTsBinding.slnx

# 5. 远程交叉编译 libapp.so（双架构），配置见 scripts/remote-build.sh 顶部
bash scripts/remote-build.sh

# 6. 打 HAP（使用 DevEco 内置 hvigor，签名需在 DevEco 中配置自动签名一次）
cmd //c scripts\build-hap.cmd

# 7. 部署到模拟器/真机并抓取日志
bash scripts/deploy-hap.sh
```

## 类型映射（Native 模式）

| ArkTS (.d.ts) | C# (Nodes/) | 封送 |
|---|---|---|
| 组件 `TextInterface`/`TextAttribute` | `class Text : ArkUINodeBase` | `ARKUI_NODE_TEXT` |
| 构造参数 `Text(content)` | `string Content` | `SetStringAttribute(NODE_TEXT_CONTENT)` |
| `textAlign(...)` | `ArkUI_TextAlignment TextAlign` | i32 单值 |
| `fontColor(...)` | `void SetFontColor(r,g,b,a=255)` | u32 `0xAARRGGBB` |
| `onClick(...)` | `event Action<ArkUINodeEvent>? Click` | `NODE_ON_CLICK` + 事件总线 |
| 点击参数 | `ev.ClickX/.ClickY/.ClickDevice/...` | `NodeComponentEvent.data[]` 直读 |
| `Promise<T>` | `IntPtr`（napi promise 句柄） | async 待 TSFN 层 |
| 未映射属性 | —— | 记入 `Nodes/native-gaps.json` |

生成器不猜属性形态：只有登记在 `nativeCodeGenerator.ts` shape 表中的属性才生成代码，
其余进入 **gap 清单**（当前 1182 条），这是 C API 覆盖度的实时地图，也是扩展组件的待办清单。

## 运行时工具链要点（踩坑记录）

- **musl 交叉**：arm64 用 musl.cc gcc（`naot-driver` wrapper 过滤 clang 风格 `--target`）；
  x64 用 zig cc（wrapper 需过滤 `-Wl,--gc-sections`——它会收割 ILC 的 `__modules` section，
  导致 dlopen 时 `__start___modules` 重定位失败）。
- **符号分离**：arm64 用工具链 objcopy；x64 用 `zig objcopy` 的 GNU 兼容 wrapper（`objcopy-gnu`）。
- **引导参数**：shim 在 dlopen 前 setenv `DOTNET_GCHeapHardLimit`（默认 256G 虚拟预留超限）与 `ICU_DATA`。
- **调试**：C# 经 `HiLog` 直写 hilog（含托管堆栈）；`hilog -x | grep HarmonyHost`。

## 项目结构

```
src/parser/                  解析器（TS Compiler API）
  ├─ astParser.ts            AST → 组件/API 中间模型
  ├─ nativeCodeGenerator.ts  C API 目标生成器（shape 表 + gap 登记）
  ├─ codeGenerator.ts        napi 目标生成器（@ohos.* 服务层）
  └─ typeMapper.ts           类型映射
src/nativeBinding/
  └─ extract_arkui_types.py  NDK 头文件 → C# 枚举 + JSON 元数据
HarmonyOS.Bindings/          绑定库（net10.0, AOT/trim 友好）
  ├─ NativeNode/             ArkUI C API 互操作 + ArkUINodeBase + 事件总线
  ├─ Nodes/                  生成的组件包装类 + native-gaps.json
  ├─ Runtime/                napi 互操作（env 注入、INapiRecord、HiLog）
  └─ Hosting/Host.cs         libapp.so 导出入口
samples/HarmonyHost/         鸿蒙宿主工程（ArkTS + C shim + CMake）
samples/dotnet/HelloApp/     .NET 样例应用（NativeAOT → libapp.so）
scripts/                     remote-build / build-hap / deploy-hap 一键工具链
tests/                       jest（解析器/生成器 45 用例）
```

## 已知限制 / 下一步

- [ ] fork dotnet/maui：HarmonyOS Handler 基础设施（IViewHandler → ArkUINodeBase）
- [ ] `object` 型复杂属性（ArkUI_TextStyle 等）需头文件结构体提取后再开放（禁止猜测 ABI）
- [ ] 组件覆盖面扩展（按 native-gaps.json 与 shape 表逐步放开）
- [ ] 触摸事件详细解析（ui_input_event.h 访问器）、ThreadSafeFunction（后台线程 → UI 线程）
- [ ] 真机（arm64）验证
