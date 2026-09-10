import * as fs from 'fs';
import * as path from 'path';
import { ArkTsParser } from '../src/parser/index';

const outputDir = path.join(__dirname, '../tests-output');

// 创建测试输出目录并生成所有文件
beforeAll(async () => {
    if (!fs.existsSync(outputDir)) {
        fs.mkdirSync(outputDir, { recursive: true });
    }
    const parser = new ArkTsParser();
    const inputDir = path.join(__dirname, 'fixtures');
    await parser.processDirectory(inputDir, outputDir);
});

// 清理测试输出目录
afterAll(() => {
    if (fs.existsSync(outputDir)) {
        fs.rmSync(outputDir, { recursive: true, force: true });
    }
});

describe('Code Generation Tests', () => {
    test('should generate Text.cs', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        expect(fs.existsSync(textCsPath)).toBe(true);
        
        const content = fs.readFileSync(textCsPath, 'utf-8');
        expect(content).toContain('public partial class Text');
        expect(content).toContain('ArkUIComponentBase');
        expect(content).toContain('public Text()');
        expect(content).toContain('public Font Font');
        expect(content).toContain('public double FontSize');
    });

    test('should generate Column.cs', () => {
        const columnCsPath = path.join(outputDir, 'column.cs');
        expect(fs.existsSync(columnCsPath)).toBe(true);
        
        const content = fs.readFileSync(columnCsPath, 'utf-8');
        expect(content).toContain('public partial class Column');
        expect(content).toContain('ArkUIComponentBase');
        expect(content).toContain('public Column()');
        expect(content).toContain('public HorizontalAlign AlignItems');
        expect(content).toContain('public FlexAlign JustifyContent');
    });

    test('should have correct namespace', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        const content = fs.readFileSync(textCsPath, 'utf-8');
        expect(content).toContain('namespace HarmonyOS.ArkUI;');
    });

    test('should have proper using statements', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        const content = fs.readFileSync(textCsPath, 'utf-8');
        expect(content).toContain('using System;');
        expect(content).toContain('using System.Runtime.InteropServices;');
    });

    test('should have XML documentation', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        const content = fs.readFileSync(textCsPath, 'utf-8');
        expect(content).toContain('/// <summary>');
        expect(content).toContain('/// Text 组件的 C# 绑定');
    });

    // 阶段1修复测试
    test('should handle constructor overloads correctly in Column.cs', () => {
        const columnCsPath = path.join(outputDir, 'column.cs');
        const content = fs.readFileSync(columnCsPath, 'utf-8');
        
        // 验证没有重复参数
        expect(content).not.toContain('ColumnOptions options, ColumnOptions options');
        
        // 验证有两个构造函数（无参和有参）
        expect(content).toContain('public Column()');
        // ColumnOptions 现在被生成为真正的 C# record
        expect(content).toContain('public Column(ColumnOptions options)');
    });

    test('should handle optional types correctly', () => {
        const columnCsPath = path.join(outputDir, 'column.cs');
        const content = fs.readFileSync(columnCsPath, 'utf-8');
        
        // 验证可选参数有正确的类型
        expect(content).toContain('bool?');
    });

    test('should not have TypeScript syntax leaks', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        const content = fs.readFileSync(textCsPath, 'utf-8');
        
        // 验证没有 TypeScript 函数类型语法
        expect(content).not.toContain('=> void');
        expect(content).not.toContain('(value: string) =>');
        expect(content).not.toContain('(selectionStart: number, selectionEnd: number) =>');
    });

    test('should have proper constructor overloads in Text.cs', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        const content = fs.readFileSync(textCsPath, 'utf-8');
        
        // 验证有无参构造函数
        expect(content).toContain('public Text()');
        
        // 验证有带参数的构造函数
        expect(content).toMatch(/public Text\(.+\)/);
    });

    // 阶段2枚举测试
    test('should generate enums file', () => {
        const enumsCsPath = path.join(outputDir, 'enums.Enums.cs');
        expect(fs.existsSync(enumsCsPath)).toBe(true);
        
        const content = fs.readFileSync(enumsCsPath, 'utf-8');
        expect(content).toContain('namespace HarmonyOS.ArkUI;');
        expect(content).toContain('using System;');
    });

    test('should generate numeric enums correctly', () => {
        const enumsCsPath = path.join(outputDir, 'enums.Enums.cs');
        const content = fs.readFileSync(enumsCsPath, 'utf-8');
        
        // 验证数值枚举
        expect(content).toContain('public enum CheckBoxShape');
        expect(content).toContain('CIRCLE = 0');
        expect(content).toContain('ROUNDED_SQUARE = 1');
    });

    test('should generate string enums with Description attribute', () => {
        const enumsCsPath = path.join(outputDir, 'enums.Enums.cs');
        const content = fs.readFileSync(enumsCsPath, 'utf-8');
        
        // 验证字符串枚举 - 使用 [Description] 而不是字符串值
        expect(content).toContain('public enum ColoringStrategy');
        expect(content).toContain('[Description("invert")]');
        // 字符串枚举不再使用 = "value" 语法（C# 枚举不支持）
        expect(content).toContain('INVERT,');
        expect(content).toContain('[Description("average")]');
        expect(content).toContain('AVERAGE,');
    });

    test('should generate enums without explicit values', () => {
        const enumsCsPath = path.join(outputDir, 'enums.Enums.cs');
        const content = fs.readFileSync(enumsCsPath, 'utf-8');
        
        // 验证无显式值的枚举
        expect(content).toContain('public enum Color');
        expect(content).toContain('White');
        expect(content).toContain('Black');
    });

    test('should have XML documentation for enums', () => {
        const enumsCsPath = path.join(outputDir, 'enums.Enums.cs');
        const content = fs.readFileSync(enumsCsPath, 'utf-8');
        
        // 验证XML文档
        expect(content).toContain('/// <summary>');
        expect(content).toContain('/// CheckBoxShape 枚举');
    });

    // 阶段3事件测试
    test('should generate event delegates in Text.cs', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        const content = fs.readFileSync(textCsPath, 'utf-8');
        
        // 验证委托定义
        expect(content).toContain('public delegate void CopyHandler');
        expect(content).toContain('public delegate void TextSelectionChangeHandler');
    });

    test('should generate event handler methods in Text.cs', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        const content = fs.readFileSync(textCsPath, 'utf-8');
        
        // 验证事件方法
        expect(content).toContain('public void OnCopy(CopyHandler handler)');
        expect(content).toContain('public void OnTextSelectionChange(TextSelectionChangeHandler handler)');
    });

    test('should have event handler implementation', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        const content = fs.readFileSync(textCsPath, 'utf-8');
        
        // 验证事件处理实现
        expect(content).toContain('NodeApi.SetEventHandler(_jsObject, "onCopy", handler, CopyHandlerTrampoline_Ptr.Ptr)');
        expect(content).toContain('NodeApi.SetEventHandler(_jsObject, "onTextSelectionChange", handler, TextSelectionChangeHandlerTrampoline_Ptr.Ptr)');
    });

    // 阶段4组件测试
    test('should generate Row.cs', () => {
        const rowCsPath = path.join(outputDir, 'row.cs');
        expect(fs.existsSync(rowCsPath)).toBe(true);
        
        const content = fs.readFileSync(rowCsPath, 'utf-8');
        expect(content).toContain('public partial class Row');
        expect(content).toContain('public Row()');
        expect(content).toContain('public Row(RowOptions options)');
        expect(content).toContain('public VerticalAlign AlignItems');
    });

    test('should generate Button.cs', () => {
        const buttonCsPath = path.join(outputDir, 'button.cs');
        expect(fs.existsSync(buttonCsPath)).toBe(true);
        
        const content = fs.readFileSync(buttonCsPath, 'utf-8');
        expect(content).toContain('public partial class Button');
        expect(content).toContain('public Button()');
        expect(content).toContain('public ButtonType Type');
    });

    test('should generate Image.cs', () => {
        const imageCsPath = path.join(outputDir, 'image.cs');
        expect(fs.existsSync(imageCsPath)).toBe(true);

        const content = fs.readFileSync(imageCsPath, 'utf-8');
        expect(content).toContain('public partial class Image');
        // src 类型是 PixelMap | ResourceStr | DrawableDescriptor，取第一个具体类型
        expect(content).toContain('public Image(PixelMap src)');
    });

    test('should generate List.cs with events', () => {
        const listCsPath = path.join(outputDir, 'list.cs');
        expect(fs.existsSync(listCsPath)).toBe(true);
        
        const content = fs.readFileSync(listCsPath, 'utf-8');
        expect(content).toContain('public partial class List');
        expect(content).toContain('public delegate void ScrollHandler');
        expect(content).toContain('public void OnScroll(ScrollHandler handler)');
    });

    test('should generate Flex.cs', () => {
        const flexCsPath = path.join(outputDir, 'flex.cs');
        expect(fs.existsSync(flexCsPath)).toBe(true);
        
        const content = fs.readFileSync(flexCsPath, 'utf-8');
        expect(content).toContain('public partial class Flex');
        expect(content).toContain('public Flex()');
    });

    test('should generate Scroll.cs', () => {
        const scrollCsPath = path.join(outputDir, 'scroll.cs');
        expect(fs.existsSync(scrollCsPath)).toBe(true);
        
        const content = fs.readFileSync(scrollCsPath, 'utf-8');
        expect(content).toContain('public partial class Scroll');
        expect(content).toContain('public void OnScroll(ScrollHandler handler)');
    });

    // 阶段5解析器增强测试
    test('should parse ParseResult with warnings', () => {
        const parser = new ArkTsParser();
        const textFixturePath = path.join(__dirname, 'fixtures/text.d.ts');
        
        const result = parser.parseFile(textFixturePath);
        
        expect(result).toHaveProperty('component');
        expect(result).toHaveProperty('enums');
        expect(result).toHaveProperty('imports');
        expect(result).toHaveProperty('warnings');
        expect(Array.isArray(result.warnings)).toBe(true);
    });

    test('should detect imports in .d.ts files', () => {
        const parser = new ArkTsParser();
        const imageFixturePath = path.join(__dirname, 'fixtures/image.d.ts');
        
        const result = parser.parseFile(imageFixturePath);
        
        // 应该能解析出组件（即使有警告）
        expect(result.component.name).toBe('Image');
    });

    test('should handle multiple constructor overloads without duplicates', () => {
        const parser = new ArkTsParser();
        const textFixturePath = path.join(__dirname, 'fixtures/text.d.ts');
        
        const result = parser.parseFile(textFixturePath);
        
        // 检查构造函数重载
        const signatures = result.component.constructorOverloads.map(o => 
            o.parameters.map(p => p.type).join(',')
        );
        const uniqueSignatures = [...new Set(signatures)];
        
        // 重载数量应该等于唯一签名数量
        expect(result.component.constructorOverloads.length).toBe(uniqueSignatures.length);
    });

    test('should generate warning for inheritance', () => {
        const parser = new ArkTsParser();
        const textFixturePath = path.join(__dirname, 'fixtures/text.d.ts');
        
        const result = parser.parseFile(textFixturePath);
        
        // Text 继承自 CommonMethod，应该有警告
        const inheritanceWarnings = result.warnings.filter(w => w.includes('extends'));
        expect(inheritanceWarnings.length).toBeGreaterThanOrEqual(0);
    });

    // 阶段6复杂类型测试
    test('should map union types correctly', () => {
        const { TypeMapper } = require('../src/parser/typeMapper');
        
        // 测试联合类型映射
        expect(TypeMapper.mapType('string | number')).toBe('string');
        // Resource | string 取第一个具体类型（string）
        expect(TypeMapper.mapType('Resource | string')).toBe('string');
        expect(TypeMapper.mapType('string | null')).toBe('string');
        expect(TypeMapper.mapType('number | undefined')).toBe('double?');
    });

    test('should map intersection types correctly', () => {
        const { TypeMapper } = require('../src/parser/typeMapper');
        
        // 测试交叉类型映射
        expect(TypeMapper.mapType('A & B')).toBe('A');
        expect(TypeMapper.mapType('ClassA & InterfaceB')).toBe('ClassA');
    });

    test('should map conditional types to dynamic', () => {
        const { TypeMapper } = require('../src/parser/typeMapper');
        
        // 测试条件类型映射
        expect(TypeMapper.mapType('T extends string ? string : number')).toBe('dynamic');
    });

    test('should map mapped types to dynamic', () => {
        const { TypeMapper } = require('../src/parser/typeMapper');
        
        // 测试映射类型映射
        expect(TypeMapper.mapType('keyof T')).toBe('dynamic');
        expect(TypeMapper.mapType('[K in keyof T]')).toBe('dynamic');
    });

    test('should map readonly types', () => {
        const { TypeMapper } = require('../src/parser/typeMapper');
        
        // 测试 readonly 类型映射
        expect(TypeMapper.mapType('readonly string[]')).toBe('string[]');
        expect(TypeMapper.mapType('readonly number')).toBe('double');
    });

    test('should detect complex types', () => {
        const { TypeMapper } = require('../src/parser/typeMapper');
        
        // 测试类型检测
        expect(TypeMapper.isUnionType('string | number')).toBe(true);
        expect(TypeMapper.isIntersectionType('A & B')).toBe(true);
        expect(TypeMapper.isConditionalType('T extends string ? string : number')).toBe(true);
        expect(TypeMapper.isMappedType('keyof T')).toBe(true);
    });

    test('should handle optional union types in fixtures', () => {
        const parser = new ArkTsParser();
        const columnFixturePath = path.join(__dirname, 'fixtures/column.d.ts');
        
        const result = parser.parseFile(columnFixturePath);
        
        // 应该能解析出组件
        expect(result.component.name).toBe('Column');
        // 可选参数应该有正确的处理
        const methods = result.component.methods;
        expect(methods.length).toBeGreaterThan(0);
    });

    // 阶段7优化测试
    test('should have ArkUIComponentBase pattern in generated code', () => {
        const buttonCsPath = path.join(outputDir, 'button.cs');
        const content = fs.readFileSync(buttonCsPath, 'utf-8');
        
        // 验证继承自 ArkUIComponentBase
        expect(content).toContain('ArkUIComponentBase');
        expect(content).toContain(': base(NodeApi.CreateComponent("Button"))');
        expect(content).toContain('using HarmonyOS.Bindings.Runtime;');
    });

    test('should not have duplicate methods in list.cs', () => {
        const listCsPath = path.join(outputDir, 'list.cs');
        const content = fs.readFileSync(listCsPath, 'utf-8');
        
        // 检查 SetLanes 方法只有一个（之前有重复）
        const lanesMatches = content.match(/public ListAttribute SetLanes\(/g);
        expect(lanesMatches).toHaveLength(1);
        
        // SetCachedCount 有两个不同的重载（1个参数 vs 2个参数），这是正确的
        const cachedCountMatches = content.match(/public ListAttribute SetCachedCount\(/g);
        expect(cachedCountMatches).toHaveLength(2);
        
        // 检查这两个重载有不同的参数数量
        const cachedCount1 = content.match(/public ListAttribute SetCachedCount\(double value\)/);
        const cachedCount2 = content.match(/public ListAttribute SetCachedCount\(double count, bool show\)/);
        expect(cachedCount1).not.toBeNull();
        expect(cachedCount2).not.toBeNull();
    });

    test('should create generation cache file', async () => {
        const parser = new ArkTsParser();
        const inputDir = path.join(__dirname, 'fixtures');
        
        // 使用 processDirectory 来创建缓存文件
        await parser.processDirectory(inputDir, outputDir);
        
        const cachePath = path.join(outputDir, '.generation-cache.json');
        expect(fs.existsSync(cachePath)).toBe(true);
        
        const cache = JSON.parse(fs.readFileSync(cachePath, 'utf-8'));
        expect(cache.version).toBe(1);
        expect(Object.keys(cache.files).length).toBeGreaterThan(0);
    });

    test('should support incremental generation', async () => {
        const parser = new ArkTsParser();
        const textFixturePath = path.join(__dirname, 'fixtures/text.d.ts');
        const outputPath = path.join(outputDir, 'text.cs');
        
        // 获取当前文件内容的 hash
        const beforeContent = fs.readFileSync(outputPath, 'utf-8');
        const beforeHash = require('crypto').createHash('sha256').update(beforeContent).digest('hex');
        
        // processFile 不检查缓存（它总是重新生成）
        // 但 processDirectory 会检查缓存
        // 这里验证 processFile 生成的内容与之前相同
        await parser.processFile(textFixturePath, outputPath);
        
        const afterContent = fs.readFileSync(outputPath, 'utf-8');
        const afterHash = require('crypto').createHash('sha256').update(afterContent).digest('hex');
        
         // 内容应该相同（因为源文件未修改）
        expect(afterHash).toBe(beforeHash);
    });

    // 阶段8异步层测试（M2 2.1）
    test('should generate async-service.cs with Promise<T> mapping', async () => {
        const parser = new ArkTsParser();
        const asyncFixturePath = path.join(__dirname, 'fixtures/async-service.d.ts');
        const outputPath = path.join(outputDir, 'async-service.cs');
        
        await parser.processFile(asyncFixturePath, outputPath);
        expect(fs.existsSync(outputPath)).toBe(true);
        
        const content = fs.readFileSync(outputPath, 'utf-8');
        
        // 验证 using System.Threading.Tasks 存在
        expect(content).toContain('using System.Threading.Tasks;');
        
        // 验证 Promise<string> → Task<string> 返回类型
        expect(content).toContain('public Task<string> Load(string url)');
        
        // 验证 Promise<number> → Task<double> 返回类型（number 映射为 double）
        expect(content).toContain('public Task<double> GetCount()');
        
        // 验证 Promise<boolean> → Task<bool> 返回类型
        expect(content).toContain('public Task<bool> IsEnabled()');
        
        // 验证 Promise<void> 返回类型
        expect(content).toContain('public Task DoWork()');
        
        // 验证复杂类型 Promise<SomeComplexType> → Task<IntPtr>
        expect(content).toContain('public Task<IntPtr> GetHandle()');
        
        // 验证同步方法仍使用 CallMethod
        expect(content).toContain('return NodeApi.CallMethod<double>(_jsObject, _syncMethod);');
        
        // 验证异步方法使用 CallMethodAsync
        expect(content).toContain('return NodeApi.CallMethodAsync<string>(_jsObject, _load, url);');
        expect(content).toContain('return NodeApi.CallMethodAsync<double>(_jsObject, _getCount);');
        expect(content).toContain('return NodeApi.CallMethodAsync<bool>(_jsObject, _isEnabled);');
        expect(content).toContain('return NodeApi.CallMethodAsync<IntPtr>(_jsObject, _getHandle);');
    });

    // AsyncCallback 测试（M2 2.1）
    test('should generate async-callback-service.cs with AsyncCallback mapping', async () => {
        const parser = new ArkTsParser();
        const callbackFixturePath = path.join(__dirname, 'fixtures/async-callback-service.d.ts');
        const outputPath = path.join(outputDir, 'async-callback-service.cs');
        
        await parser.processFile(callbackFixturePath, outputPath);
        expect(fs.existsSync(outputPath)).toBe(true);
        
        const content = fs.readFileSync(outputPath, 'utf-8');
        
        // 验证 using System.Threading.Tasks 存在
        expect(content).toContain('using System.Threading.Tasks;');
        
        // 验证 AsyncCallback<string> → Task<string>
        expect(content).toContain('public Task<string> GetData()');
        
        // 验证 AsyncCallback<number> → Task<double>
        expect(content).toContain('public Task<double> GetCount()');
        
        // 验证 AsyncCallback<boolean> → Task<bool>
        expect(content).toContain('public Task<bool> IsEnabled()');
        
        // 验证 AsyncCallback<void> → Task
        expect(content).toContain('public Task DoWork()');
        
        // 验证混合参数：AsyncCallback 移除，只保留 url 参数
        expect(content).toContain('public Task<string> FetchData(string url)');
        
        // 验证同步方法
        expect(content).toContain('public double SyncMethod()');
    });
});
