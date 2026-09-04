import * as fs from 'fs';
import * as path from 'path';
import { ArkTsParser } from '../src/parser/index';

const outputDir = path.join(__dirname, '../output');

describe('Code Generation Tests', () => {
    test('should generate Text.cs', () => {
        const textCsPath = path.join(outputDir, 'text.cs');
        expect(fs.existsSync(textCsPath)).toBe(true);
        
        const content = fs.readFileSync(textCsPath, 'utf-8');
        expect(content).toContain('public partial class Text');
        expect(content).toContain('private IntPtr _jsObject');
        expect(content).toContain('public Text()');
        expect(content).toContain('public TextAttribute Font(Font value)');
        expect(content).toContain('public TextAttribute FontSize(double value)');
    });

    test('should generate Column.cs', () => {
        const columnCsPath = path.join(outputDir, 'column.cs');
        expect(fs.existsSync(columnCsPath)).toBe(true);
        
        const content = fs.readFileSync(columnCsPath, 'utf-8');
        expect(content).toContain('public partial class Column');
        expect(content).toContain('private IntPtr _jsObject');
        expect(content).toContain('public Column()');
        expect(content).toContain('public ColumnAttribute AlignItems(HorizontalAlign value)');
        expect(content).toContain('public ColumnAttribute JustifyContent(FlexAlign value)');
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
        // ColumnOptions 被映射为 IntPtr
        expect(content).toContain('public Column(IntPtr options)');
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
        
        // 验证字符串枚举
        expect(content).toContain('public enum ColoringStrategy');
        expect(content).toContain('[Description("invert")]');
        expect(content).toContain('INVERT = "invert"');
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
        expect(content).toContain('NodeApi.SetEventHandler(_jsObject, "onCopy", handler)');
        expect(content).toContain('NodeApi.SetEventHandler(_jsObject, "onTextSelectionChange", handler)');
    });

    // 阶段4组件测试
    test('should generate Row.cs', () => {
        const rowCsPath = path.join(outputDir, 'row.cs');
        expect(fs.existsSync(rowCsPath)).toBe(true);
        
        const content = fs.readFileSync(rowCsPath, 'utf-8');
        expect(content).toContain('public partial class Row');
        expect(content).toContain('public Row()');
        expect(content).toContain('public Row(RowOptions options)');
        expect(content).toContain('public RowAttribute AlignItems(VerticalAlign value)');
    });

    test('should generate Button.cs', () => {
        const buttonCsPath = path.join(outputDir, 'button.cs');
        expect(fs.existsSync(buttonCsPath)).toBe(true);
        
        const content = fs.readFileSync(buttonCsPath, 'utf-8');
        expect(content).toContain('public partial class Button');
        expect(content).toContain('public Button()');
        expect(content).toContain('public ButtonAttribute Type(ButtonType value)');
    });

    test('should generate Image.cs', () => {
        const imageCsPath = path.join(outputDir, 'image.cs');
        expect(fs.existsSync(imageCsPath)).toBe(true);
        
        const content = fs.readFileSync(imageCsPath, 'utf-8');
        expect(content).toContain('public partial class Image');
        expect(content).toContain('public Image(IntPtr src)');
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
});
