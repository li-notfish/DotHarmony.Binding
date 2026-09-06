这是一份为你量身定制的 **.NET (NativeAOT) 与 ArkTS (鸿蒙) 高性能 SDK 自动化绑定架构方案** 总结指南。
本指南采用业内最硬核的 **“一阶段影子包欺骗、二阶段源生成器真身、三阶段 AOT 物理缝合”** 战略，完美对齐鸿蒙平台禁止 JIT 的铁律。
# 🛠️ .NET 绑定 ArkTS (Node-API) 架构设计白皮书
## 一、 核心架构三阶段流水线
实现互操作的本质不是修改已有代码，而是通过**静态代码生成技术**在编译期完成类型系统的对账。
```
[ 一阶段：准备期 ]
 ArkTS (.d.ts) ──(AST 解析脚本)──> C# 影子源码 ──> 编译为 NuGet 包 (ArkTsBinding)

[ 二阶段：编译期 ]
 你的 C# 业务代码 + ArkTsBinding NuGet 包
       │
       ▼ (点击 dotnet build)
 .NET Source Generator ──> 自动补全真实的 Node-API 胶水代码 (.g.cs)

[ 隐藏缝合：鸿蒙打包期 ]
 .NET NativeAOT ──> 编译出 libdotnet_core.so ──> 放入鸿蒙项目的 libs 文件夹
       │
       ▼
 鸿蒙 DevEco Studio ──> 打包成最终的 .hap 安装包

```
## 二、 核心痛点与跨语言降维解决方案
由于 TypeScript（ArkTS）属于**结构化/动态类型系统**，而 C# 属于**标称/严格强类型系统**，在自动化生成绑定时，必须通过以下公式进行“概念重组”。
### 1. 基础属性与字段（内存实时代理）
 * **怎么解决：** 绝对不能采用一次性内存复制（会导致数据脱节）。一阶段批量生成 partial 属性空壳；二阶段由 Source Generator 补全，利用 Node-API 的 napi_get_named_property 和 napi_set_named_property 实现每一次读写都实时向方舟虚拟机捞取最新值。
### 2. 泛型 void 参数（空元组占位与重载展开）
 * **怎么解决：** C# 泛型参数不能写 void。在类与接口级别，工具自动将 void 替换为 C# 的空元组 System.ValueTuple；在方法和委托级别，由源生成器自动展开为一个**不带参数**的清洁重载方法，内部用 Lambda 表达式吞掉 ValueTuple，并在 Node-API 层用系统单例 napi_get_undefined 指针填充。
### 3. data class / 纯数据接口（Record 内存拼装）
 * **怎么解决：** TypeScript 里充斥着没有复杂生命周期的纯数据接口（Interface）。在 C# 侧直接将其映射为现代 C# 的 record。在跨界传递时，源生成器在底层将其解包，通过 Node-API 凭空创建一个 JS 空对象 {} 并把属性一项项刷进去。
### 4. 字面量联合类型（String Literal Union）
 * **怎么解决：** 鸿蒙 UI SDK 中极度喜欢用 'Row' | 'Column' 这种字符串当约束。一阶段工具自动将其**强行升格为 C# 强类型 enum**；二阶段源生成器在属性的 set 块里利用 switch 表达式，在底层将其翻译成对应的硬编码字符串指针。
### 5. 匿名对象字面量（匿名结构提纯）
 * **怎么解决：** 方法参数中经常直接写花括号 { accuracy: number } 而没有类名。一阶段工具通过语法树分析，在后台为其**发明一个名字**（如 方法名+Options），在 C# 侧提取为一个独立的强类型类。
### 6. AOT 垃圾裁剪误杀（免死金牌标记）
 * **怎么解决：** NativeAOT 编译器在静态扫描时，可能会误把仅通过 Node-API 间接回调的 C# 方法当成死代码剪掉。源生成器必须在生成的真身方法上贴上 [DynamicDependency] 注解，或者在 NuGet 包中自带 ILLink.Descriptors.xml 控制文件，强制锁死整个命名空间不被裁剪。
## 三、 全场景代码映射样例演练
### 场景 1：基础类、属性与动态字面量联合类型
#### 📝 ArkTS 原生定义 (.d.ts)
```typescript
export type FlexDirection = 'Row' | 'Column';

export class FlexLayout {
    readonly id: string;
    direction: FlexDirection;
}

```
#### 📌 一阶段：自动生成的 C# 影子类 NuGet 包 (ArkTsBinding)
```csharp
namespace HarmonyOS.ArkUI;

public enum FlexDirection
{
    Row,
    Column
}

public partial class FlexLayout
{
    // 影子属性：不写身体，留给二阶段补全
    public partial string Id { get; }
    public partial FlexDirection Direction { get; set; }
}

```
#### ⚡ 二阶段：Source Generator 自动补全的真身 (.g.cs)
```csharp
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.ArkUI;

public partial class FlexLayout
{
    // 每一个 C# 壳对象，内部都死死持有鸿蒙端 JS 对象的指针
    internal IntPtr _jsObjectHandle;

    public partial string Id
    {
        get
        {
            IntPtr env = HarmonyRuntime.GetCurrentEnv();
            IntPtr jsValue = Napi.napi_get_named_property(env, _jsObjectHandle, "id");
            return Napi.ConvertToString(env, jsValue);
        }
    }

    public partial FlexDirection Direction
    {
        get
        {
            IntPtr env = HarmonyRuntime.GetCurrentEnv();
            IntPtr jsValue = Napi.napi_get_named_property(env, _jsObjectHandle, "direction");
            string str = Napi.ConvertToString(env, jsValue);
            return str == "Column" ? FlexDirection.Column : FlexDirection.Row;
        }
        set
        {
            IntPtr env = HarmonyRuntime.GetCurrentEnv();
            string strVal = value == FlexDirection.Column ? "Column" : "Row";
            IntPtr jsStr = Napi.napi_create_string_utf8(env, strVal);
            Napi.napi_set_named_property(env, _jsObjectHandle, "direction", jsStr);
        }
    }
}

```
### 场景 2：泛型 void 事件回调
#### 📝 ArkTS 原生定义 (.d.ts)
```typescript
export class NotifyService {
    // 接受一个没有参数、没有返回值的 void 泛型回调
    static onComplete(callback: () => void): void;
}

```
#### 📌 一阶段：自动生成的 C# 影子类 NuGet 包 (ArkTsBinding)
```csharp
using System;

namespace HarmonyOS.System;

public partial class NotifyService
{
    // 声明无参数的清洁版本供 C# 开发者调用
    public static partial void OnComplete(Action callback);
}

```
#### ⚡ 二阶段：Source Generator 自动补全的真身 (.g.cs)
```csharp
using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace HarmonyOS.System;

public partial class NotifyService
{
    // 1. 防修剪免死金牌：防止 AOT 误杀这个会被 C 层回调的静态方法
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(NotifyService))]
    [UnmanagedCallersOnly(EntryPoint = "CSharp_OnNotifyCompleteBridge")]
    public static void OnNotifyCompleteBridge(IntPtr env, IntPtr managedActionHandle)
    {
        // 从指针中恢复出 C# 的 Action 委托
        var gcHandle = GCHandle.FromIntPtr(managedActionHandle);
        if (gcHandle.Target is Action callback)
        {
            callback.Invoke(); // 安全回到 C# 的上下文执行
        }
    }

    // 2. 补全一阶段的壳方法
    public static partial void OnComplete(Action callback)
    {
        IntPtr env = HarmonyRuntime.GetCurrentEnv();
        
        // 将 C# 的 Action 锁定在内存里拿到指针
        GCHandle handle = GCHandle.Alloc(callback, GCHandleType.Normal);
        IntPtr nativeCallbackRef = GCHandle.ToIntPtr(handle);

        unsafe
        {
            // 拿到上面桥接静态函数的函数指针
            delegate* unmanaged<IntPtr, IntPtr, void> nativeFn = &OnNotifyCompleteBridge;
            
            // 调用底层的底层 C 胶水代码，将其注册给鸿蒙的 Node-API
            // 运行时如果 ArkTS 传回 void(undefined)，桥接函数直接吞掉它并触发 Action
            NativeEngine_RegisterCallback(env, nativeCallbackRef, (IntPtr)nativeFn);
        }
    }

    [DllImport("lib胶水层.so")]
    private static unsafe extern void NativeEngine_RegisterCallback(IntPtr env, IntPtr context, IntPtr fn);
}

```
### 场景 3：纯数据结构（Data Class / Interface）与可选属性
#### 📝 ArkTS 原生定义 (.d.ts)
```typescript
export interface UserInfo {
    username: string;
    age?: number; // 可选属性
}

export class UserService {
    static updateUser(info: UserInfo): void;
}

```
#### 📌 一阶段：自动生成的 C# 影子类 NuGet 包 (ArkTsBinding)
```csharp
namespace HarmonyOS.Model;

// 纯数据结构直接退化为 C# 的 record，可选属性用 C# 可空符号 ? 表达
public record UserInfo(string Username, double? Age);

public partial class UserService
{
    public static partial void UpdateUser(UserInfo info);
}

```
#### ⚡ 二阶段：Source Generator 自动补全的真身 (.g.cs)
```csharp
using System;

namespace HarmonyOS.Model;

public partial class UserService
{
    public static partial void UpdateUser(UserInfo info)
    {
        IntPtr env = HarmonyRuntime.GetCurrentEnv();

        // 1. 动态在方舟编译器堆里 new 一个原生的 JS {} 对象
        IntPtr jsObject = Napi.napi_create_object(env);

        // 2. 封送必填属性 username
        IntPtr jsUsername = Napi.napi_create_string_utf8(env, info.Username);
        Napi.napi_set_named_property(env, jsObject, "username", jsUsername);

        // 3. 处理可选属性 age
        if (info.Age.HasValue)
        {
            IntPtr jsAge = Napi.napi_create_double(env, info.Age.Value);
            Napi.napi_set_named_property(env, jsObject, "age", jsAge);
        }
        else
        {
            // 如果 C# 传入 null，鸿蒙端对齐赋予 undefined
            Napi.napi_set_named_property(env, jsObject, "age", Napi.napi_get_undefined(env));
        }

        // 4. 将组装好的原生 JS 对象指针传给 SDK 的实际方法
        IntPtr serviceModule = Napi.napi_load_module(env, "@ohos.userservice");
        IntPtr updateFn = Napi.napi_get_named_property(env, serviceModule, "updateUser");
        
        Napi.napi_call_function(env, serviceModule, updateFn, new IntPtr[] { jsObject });
    }
}

```
## 四、 工业级工程落地建议
 1. **一阶段脚手架技术选型：** 强烈建议使用 **TypeScript Compiler API** 或 **Antlr4** 编写自动化 CLI 工具来解析 .d.ts 的 AST。遇到不支持的语法时抛出明确警告。
 2. **调试断点技巧：** 由于 NativeAOT 在鸿蒙真机上运行，C# 的断点可以通过 **LLDB 挂载进程** 的方式进行原生底层调试。
 3. **性能优化极限：** 在源生成器生成的 napi_set_named_property 中，字符串 Key（如 "username"）会频繁进行 UTF8 转换。工业级实现中应在 C# 侧将这些常量字符串提前缓存为 ReadOnlySpan<byte> 或固定的 Native 字节数组指针，从而榨干最后一滴 CPU 性能。
 
---

## 架构决策记录（2026-09）

> 本白皮书（"一阶段影子包、二阶段源生成器、三阶段 AOT 缝合"）为项目起点，其中 AOT 防裁剪、
> UTF8 常量缓存、字符串枚举等工程实践已被采纳。但**核心互操作路线已变更**：
>
> 1. UI 控件绑定目标从「napi 直调 ArkTS 声明式对象」改为「ArkUI NDK 原生节点 C API（ArkUI_NativeNodeAPI_1）」。
>    原因：声明式组件无法从 napi 侧命令式创建并挂载进 UI 树；MAUI Handler 需要 UIKit 式的稳定句柄（ArkUI_NodeHandle）。
> 2. 二阶段源生成器不再需要：一阶段解析器直接产出完整 NodeHandle 包装类（生成的即真身）。
> 3. 非 UI 的 @ohos.* 服务保留 napi 路线，env 由宿主 shim 注入。
>
> 详见 README.md「关键架构决策」。
