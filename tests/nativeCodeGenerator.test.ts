import { ArkTsParser } from '../src/parser/index';
import { NativeCodeGenerator, EnumMetadata } from '../src/parser/nativeCodeGenerator';
import { ParseResult, ParameterInfo, EventInfo, MethodInfo } from '../src/parser/models';
import * as fs from 'fs';
import * as path from 'path';

const ENUM_META_PATH = path.join(__dirname, '../HarmonyOS.Bindings/NativeNode/ArkUINodeTypes.json');

function loadEnumMetadata(): EnumMetadata {
    return JSON.parse(fs.readFileSync(ENUM_META_PATH, 'utf-8')) as EnumMetadata;
}

function makeResult(name: string, options: {
    constructorParams?: ParameterInfo[];
    methods?: MethodInfo[];
    events?: EventInfo[];
}): ParseResult {
    return {
        component: {
            name,
            interfaceName: `${name}Interface`,
            attributeName: `${name}Attribute`,
            constructorParams: options.constructorParams || [],
            constructorOverloads: [],
            methods: options.methods || [],
            events: options.events || [],
            delegates: [],
            namespace: 'HarmonyOS.ArkUI',
        },
        enums: [],
        imports: [],
        warnings: [],
    };
}

describe('NativeCodeGenerator（ArkUI C API 生成模式）', () => {
    const enumMetadata = loadEnumMetadata();

    test('ArkUINodeTypes.json 元数据存在且包含核心枚举', () => {
        expect(enumMetadata['ArkUI_NodeType']['ARKUI_NODE_TEXT']).toBeDefined();
        expect(enumMetadata['ArkUI_NodeType']['ARKUI_NODE_BUTTON']).toBeDefined();
        expect(enumMetadata['ArkUI_NodeAttributeType']['NODE_TEXT_CONTENT']).toBeDefined();
        expect(enumMetadata['ArkUI_NodeEventType']['NODE_ON_CLICK']).toBeDefined();
        expect(enumMetadata['ArkUI_NodeEventType']['NODE_TOUCH_EVENT']).toBeDefined();
    });

    test('Text 组件：真实 .d.ts 解析 → string/enum 属性生成', () => {
        const parser = new ArkTsParser(enumMetadata);
        const result = parser.parseFile(path.join(__dirname, '../tests/fixtures/text.d.ts'));
        const { csharp, gaps } = parser.generateNativeCode(result);

        expect(csharp).not.toBeNull();
        expect(csharp).toContain('public unsafe class Text : ArkUINodeBase');
        expect(csharp).toContain('ArkUI_NodeType.ARKUI_NODE_TEXT');
        // shape 表覆盖：textAlign → NODE_TEXT_ALIGN
        expect(csharp).toContain('SetNumericAttribute(ArkUI_NodeAttributeType.NODE_TEXT_ALIGN, ArkUIValue.I((int)value))');
        // 全局通用段：fontColor → NODE_FONT_COLOR（color 形态）
        expect(csharp).toContain('public void SetFontColor(byte r, byte g, byte b, byte a = 255)');
        // gap：C API 未暴露的属性被记录而非静默丢失
        const gapMembers = gaps.filter(g => g.kind === 'attribute').map(g => g.member);
        expect(gapMembers.length).toBeGreaterThan(0);
        expect(gapMembers).toContain('font');
    });

    test('Text 构造参数 content → NODE_TEXT_CONTENT（string）', () => {
        const gen = new NativeCodeGenerator(enumMetadata);
        const result = makeResult('Text', {
            constructorParams: [{ name: 'content', type: 'string | Resource', optional: true }],
        });
        const { csharp } = gen.generate(result);
        expect(csharp).toContain('public string Content');
        expect(csharp).toContain('SetStringAttribute(ArkUI_NodeAttributeType.NODE_TEXT_CONTENT, value)');
    });

    test('Button：构造参数 label → NODE_BUTTON_LABEL，onClick → NODE_ON_CLICK 事件', () => {
        const gen = new NativeCodeGenerator(enumMetadata);
        const result = makeResult('Button', {
            constructorParams: [{ name: 'label', type: 'string', optional: true }],
            methods: [{ name: 'type', returnType: 'ButtonAttribute', parameters: [], isChained: true }],
            events: [{ name: 'onClick', delegateName: 'ClickEventHandler', parameters: [], returnType: 'void' }],
        });
        const { csharp } = gen.generate(result);
        expect(csharp).toContain('ArkUI_NodeType.ARKUI_NODE_BUTTON');
        expect(csharp).toContain('SetStringAttribute(ArkUI_NodeAttributeType.NODE_BUTTON_LABEL, value)');
        expect(csharp).toContain('SetNumericAttribute(ArkUI_NodeAttributeType.NODE_BUTTON_TYPE, ArkUIValue.I((int)value))');
        expect(csharp).toContain('public event Action<ArkUINodeEvent>? Click');
        expect(csharp).toContain('On(ArkUI_NodeEventType.NODE_ON_CLICK, value!)');
    });

    test('Column/Row：真实 .d.ts 解析生成容器属性', () => {
        const parser = new ArkTsParser(enumMetadata);
        const column = parser.parseFile(path.join(__dirname, '../tests/fixtures/column.d.ts'));
        const colResult = parser.generateNativeCode(column);
        expect(colResult.csharp).toContain('ArkUI_NodeType.ARKUI_NODE_COLUMN');

        const row = parser.parseFile(path.join(__dirname, '../tests/fixtures/row.d.ts'));
        const rowResult = parser.generateNativeCode(row);
        expect(rowResult.csharp).toContain('ArkUI_NodeType.ARKUI_NODE_ROW');
    });

    test('C API 中不存在的组件进入 nodeType gap 并跳过生成', () => {
        const gen = new NativeCodeGenerator(enumMetadata);
        const { csharp, gaps } = gen.generate(makeResult('NoSuchComponent', {}));
        expect(csharp).toBeNull();
        expect(gaps[0].kind).toBe('nodeType');
    });

    test('事件名特殊映射：onTouch → NODE_TOUCH_EVENT', () => {
        const gen = new NativeCodeGenerator(enumMetadata);
        const result = makeResult('Button', {
            events: [{ name: 'onTouch', delegateName: 'TouchEventHandler', parameters: [], returnType: 'void' }],
        });
        const { csharp, gaps } = gen.generate(result);
        expect(csharp).toContain('On(ArkUI_NodeEventType.NODE_TOUCH_EVENT, value!)');
        expect(gaps.filter(g => g.kind === 'event' && g.member === 'onTouch')).toHaveLength(0);
    });
});
