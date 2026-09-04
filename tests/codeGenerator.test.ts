import * as fs from 'fs';
import * as path from 'path';

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
});
