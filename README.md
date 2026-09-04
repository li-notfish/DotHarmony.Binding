# ArkTsBinding - .NET 绑定鸿蒙 ArkTS API

将鸿蒙 ArkTS UI 组件自动转换为 C# 绑定代码。

## 项目结构

```
ArkTsBinding/
├── src/
│   └── parser/
│       ├── index.ts           # 入口
│       ├── astParser.ts       # TypeScript AST 解析器
│       ├── typeMapper.ts      # 类型映射
│       ├── codeGenerator.ts   # C# 代码生成器
│       └── models.ts          # 数据模型
├── tests/
│   └── fixtures/              # 测试用 .d.ts 文件
├── output/                    # 生成的 C# 文件
└── dist/                      # 编译后的 JavaScript
```

## 快速开始

### 1. 安装依赖

```bash
npm install
```

### 2. 生成绑定代码

```bash
# 生成所有组件
npm run generate

# 或者编译后运行
npm run build
node dist/parser/index.js
```

### 3. 查看生成的代码

生成的 C# 文件位于 `output/` 目录。

## 使用示例

### 输入 (text.d.ts)

```typescript
interface TextInterface {
    (content?: string | Resource): TextAttribute;
}

declare class TextAttribute extends CommonMethod<TextAttribute> {
    font(value: Font): TextAttribute;
    fontColor(value: ResourceColor): TextAttribute;
    fontSize(value: number | string | Resource): TextAttribute;
}
```

### 输出 (Text.cs)

```csharp
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Text 组件的 C# 绑定
/// </summary>
public partial class Text
{
    /// <summary>
    /// JS对象指针
    /// </summary>
    private IntPtr _jsObject;

    /// <summary>
    /// 创建 Text 组件
    /// </summary>
    public Text()
    {
        _jsObject = NodeApi.CreateComponent("Text");
    }

    /// <summary>
    /// 创建 Text 组件
    /// </summary>
    public Text(string content, TextOptions value)
    {
        _jsObject = NodeApi.CreateComponent("Text", content);
    }

    /// <summary>
    /// 设置 font 属性
    /// </summary>
    public TextAttribute Font(Font value)
    {
        NodeApi.SetAttribute(_jsObject, "font", value);
        return new TextAttribute(_jsObject);
    }

    /// <summary>
    /// 设置 fontSize 属性
    /// </summary>
    public TextAttribute FontSize(double value)
    {
        NodeApi.SetAttribute(_jsObject, "fontSize", value);
        return new TextAttribute(_jsObject);
    }
}
```

## 类型映射

| TypeScript | C# | 说明 |
|------------|-----|------|
| `string` | `string` | 字符串 |
| `number` | `double` | 数字 |
| `boolean` | `bool` | 布尔值 |
| `Resource` | `IntPtr` | 资源引用 |
| `ResourceColor` | `IntPtr` | 颜色资源 |
| `Font` | `Font` | 字体设置 |
| `Optional` | `IntPtr?` | 可选参数 |

## 鸿蒙 SDK 位置

默认从以下位置读取 .d.ts 文件：

```
C:\Program Files\Huawei\DevEco Studio\sdk\default\openharmony\ets\component\
```

## 下一步

- [ ] 添加更多组件支持 (Button, Image, List 等)
- [ ] 实现枚举类型生成
- [ ] 添加事件回调处理
- [ ] 创建 NuGet 包
- [ ] 集成到鸿蒙项目

## 许可证

ISC
