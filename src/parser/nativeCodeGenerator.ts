/**
 * C API 目标生成器（ArkUI Native Node 路线）
 *
 * 输入：组件 .d.ts 的 ParseResult + NDK 头文件枚举元数据（ArkUINodeTypes.json）
 * 输出：包装 ArkUI_NodeHandle 的平台视图类（对齐 Mono.Android / UIKit 包装模式）
 *
 * 生成策略：
 *   - 组件名 → ArkUI_NodeType 枚举（Text → ARKUI_NODE_TEXT）
 *   - 属性方法 → NODE_<组件>_<属性> 形态查表（string/int/float/enum）→ SetXxxAttribute
 *   - onXxx 事件 → NODE_ON_XXX 事件枚举 → 基类 On()
 *   - 头文件枚举中不存在、或形态未登记的成员 → 进入 gap 清单（native-gaps）
 */
import { ParseResult, MethodInfo, EventInfo } from './models';

export interface AttributeShape {
    kind: 'string' | 'int' | 'float' | 'enum' | 'color';
    enumType?: string;
}

export type EnumMetadata = Record<string, Record<string, number>>;
export type ShapeTable = Record<string, AttributeShape>;

export interface NativeGap {
    component: string;
    member: string;
    kind: 'attribute' | 'event' | 'nodeType';
    reason: string;
}

export interface NativeGenerationResult {
    className: string;
    csharp: string | null;
    gaps: NativeGap[];
}

/**
 * 已知形态的属性表（shape 未知但枚举存在的属性会被登记为 gap 待补）
 * key = NODE_<COMPONENT>_<ATTR>
 */
export const DEFAULT_SHAPES: ShapeTable = {
    NODE_TEXT_CONTENT: { kind: 'string' },
    NODE_TEXT_ALIGN: { kind: 'enum', enumType: 'ArkUI_TextAlignment' },
    NODE_BUTTON_LABEL: { kind: 'string' },
    NODE_BUTTON_TYPE: { kind: 'enum', enumType: 'ArkUI_ButtonType' },
    NODE_COLUMN_ALIGN_ITEMS: { kind: 'enum', enumType: 'ArkUI_HorizontalAlignment' },
    NODE_COLUMN_JUSTIFY_CONTENT: { kind: 'enum', enumType: 'ArkUI_FlexAlignment' },
    NODE_ROW_ALIGN_ITEMS: { kind: 'enum', enumType: 'ArkUI_ItemAlignment' },
    NODE_ROW_JUSTIFY_CONTENT: { kind: 'enum', enumType: 'ArkUI_FlexAlignment' },
    // 全局通用属性段
    NODE_FONT_COLOR: { kind: 'color' },
    NODE_FONT_SIZE: { kind: 'float' },
    // Refresh（RefreshView）
    NODE_REFRESH_REFRESHING: { kind: 'int' },
};

/** CommonMethod 内联展开后与基类公开成员重名的属性，生成器跳过 */
const BASE_CLASS_MEMBERS = new Set([
    'width', 'height', 'padding', 'margin', 'backgroundColor', 'visibility', 'opacity', 'enabled',
]);

/** 事件名特殊映射（onXxx → C 枚举后缀） */
const EVENT_NAME_OVERRIDES: Record<string, string> = {
    onTouch: 'TOUCH_EVENT',
    onClick: 'ON_CLICK',
    // ArkUI 部分组件事件名与 d.ts Evo 方法名不同构（NODE_REFRESH_ON_REFRESH 无 ING、
    // NODE_REFRESH_STATE_CHANGE 无 ON_），逐一登记：
    onRefreshing: 'ON_REFRESH',
    onStateChange: 'STATE_CHANGE',
    onOffsetChange: 'ON_OFFSET_CHANGE',
};

function toSnakeUpper(name: string): string {
    return name
        .replace(/([a-z0-9])([A-Z])/g, '$1_$2')
        .replace(/([A-Z]+)([A-Z][a-z])/g, '$1_$2')
        .toUpperCase();
}

function toPascal(name: string): string {
    return name.charAt(0).toUpperCase() + name.slice(1);
}

/** C# 保留字转义（旧生成器曾生成 `Action event` 等非法代码，此处集中防御） */
const CS_KEYWORDS = new Set([
    'event', 'delegate', 'params', 'object', 'string', 'int', 'float', 'double', 'bool',
    'class', 'interface', 'namespace', 'public', 'private', 'protected', 'static', 'new',
    'base', 'this', 'void', 'ref', 'out', 'in', 'is', 'as', 'switch', 'lock',
]);

function escapeIdentifier(name: string): string {
    return CS_KEYWORDS.has(name) ? `@${name}` : name;
}

export class NativeCodeGenerator {
    private enumMetadata: EnumMetadata;
    private shapes: ShapeTable;

    constructor(enumMetadata: EnumMetadata, shapes?: ShapeTable) {
        this.enumMetadata = enumMetadata;
        this.shapes = shapes ? { ...DEFAULT_SHAPES, ...shapes } : { ...DEFAULT_SHAPES };
    }

    generate(result: ParseResult): NativeGenerationResult {
        const comp = result.component;
        const gaps: NativeGap[] = [];
        if (!comp.name) {
            return { className: '', csharp: null, gaps };
        }

        // 1. ArkUI_NodeType
        const nodeTypeName = `ARKUI_NODE_${comp.name.toUpperCase()}`;
        const nodeTypes = this.enumMetadata['ArkUI_NodeType'] || {};
        if (!(nodeTypeName in nodeTypes)) {
            gaps.push({
                component: comp.name, member: nodeTypeName, kind: 'nodeType',
                reason: `missing in ArkUI_NodeType (C API does not expose this component as a native node)`,
            });
            return { className: comp.name, csharp: null, gaps };
        }

        // 2. 属性（onXxx 方法归入事件通道）
        const isEventLike = (m: MethodInfo) => /^on[A-Z]/.test(m.name);
        const attributeBlocks: string[] = [];
        for (const method of comp.methods) {
            if (BASE_CLASS_MEMBERS.has(method.name)) continue;
            if (isEventLike(method)) continue;
            const block = this.generateAttribute(comp.name, method, gaps);
            if (block) attributeBlocks.push(block);
        }

        // 2b. 构造参数映射：Text(content) / Button(label) 这类内容参数对应
        //     NODE_<组件>_<参数名>（如 NODE_TEXT_CONTENT / NODE_BUTTON_LABEL）
        for (const param of comp.constructorParams) {
            if (!param.type.includes('string') && !param.type.includes('ResourceStr')) continue;
            const block = this.generateConstructorParamAttribute(comp.name, param.name, gaps);
            if (block) attributeBlocks.push(block);
        }

        // 3. 事件：显式 events + methods 中 on 开头的方法（CommonMethod<T> 合并产物）
        const allEvents: EventInfo[] = [...comp.events];
        for (const method of comp.methods) {
            if (isEventLike(method) && !allEvents.some(e => e.name === method.name)) {
                allEvents.push({
                    name: method.name,
                    delegateName: `${toPascal(method.name)}Handler`,
                    parameters: method.parameters,
                    returnType: method.returnType,
                });
            }
        }
        const eventBlocks: string[] = [];
        for (const evt of allEvents) {
            const block = this.generateEvent(comp.name, evt, gaps);
            if (block) eventBlocks.push(block);
        }

        // 4. 产出
        const csharp = this.emit(comp.name, nodeTypeName, attributeBlocks, eventBlocks, comp.methods, comp.events);
        return { className: comp.name, csharp, gaps };
    }

    /**
     * 属性枚举候选键：
     *   textAlign + Text → 候选 [NODE_TEXT_TEXT_ALIGN, NODE_TEXT_ALIGN]（TS 属性名自带组件前缀时去重）
     *   label + Button   → 候选 [NODE_BUTTON_LABEL]
     */
    private candidateAttrKeys(componentName: string, snake: string): string[] {
        const compPrefix = `${componentName.toUpperCase()}_`;
        const keys = [`NODE_${compPrefix}${snake}`];
        if (snake.startsWith(compPrefix)) {
            keys.push(`NODE_${snake}`);
        }
        // 全局通用属性段（如 NODE_FONT_COLOR / NODE_OPACITY）
        keys.push(`NODE_${snake}`);
        return keys;
    }

    private resolveShape(componentName: string, snake: string): { attrKey: string; shape: AttributeShape } | null {
        for (const key of this.candidateAttrKeys(componentName, snake)) {
            const shape = this.shapes[key];
            if (shape) return { attrKey: key, shape };
        }
        return null;
    }

    private shapeExistsInCApi(componentName: string, snake: string): boolean {
        const attrTypes = this.enumMetadata['ArkUI_NodeAttributeType'] || {};
        return this.candidateAttrKeys(componentName, snake).some(key => key in attrTypes);
    }

    private generateConstructorParamAttribute(
        componentName: string, paramName: string, gaps: NativeGap[]
    ): string | null {
        const snake = toSnakeUpper(paramName);
        const resolved = this.resolveShape(componentName, snake);
        if (!resolved) {
            if (this.shapeExistsInCApi(componentName, snake)) {
                gaps.push({
                    component: componentName, member: `${paramName} (constructor)`, kind: 'attribute',
                    reason: `${snake} exists in C API but shape is not registered`,
                });
            }
            return null;
        }
        if (resolved.shape.kind !== 'string') return null; // 试点只处理内容型构造参数

        const prop = toPascal(paramName);
        return `    /// <summary>${prop}（构造参数映射，${resolved.attrKey}）</summary>\n` +
            `    public string ${prop}\n    {\n` +
            `        set => SetStringAttribute(ArkUI_NodeAttributeType.${resolved.attrKey}, value);\n    }`;
    }

    private generateAttribute(componentName: string, method: MethodInfo, gaps: NativeGap[]): string | null {
        const snake = toSnakeUpper(method.name);
        const resolved = this.resolveShape(componentName, snake);

        if (!resolved) {
            const exists = this.shapeExistsInCApi(componentName, snake);
            gaps.push({
                component: componentName, member: method.name, kind: 'attribute',
                reason: exists
                    ? `${snake} exists in C API but shape is not registered`
                    : `${snake} not present in C API`,
            });
            return null;
        }

        const { attrKey, shape } = resolved;
        const prop = toPascal(method.name);
        const attrRef = `ArkUI_NodeAttributeType.${attrKey}`;

        switch (shape.kind) {
            case 'string':
                return `    /// <summary>${method.name}（${attrKey}）</summary>\n` +
                    `    public string ${prop}\n    {\n` +
                    `        set => SetStringAttribute(${attrRef}, value);\n    }`;
            case 'int':
                return `    /// <summary>${method.name}（${attrKey}）</summary>\n` +
                    `    public int ${prop}\n    {\n` +
                    `        set => SetNumericAttribute(${attrRef}, ArkUIValue.I(value));\n    }`;
            case 'float':
                return `    /// <summary>${method.name}（${attrKey}）</summary>\n` +
                    `    public float ${prop}\n    {\n` +
                    `        set => SetNumericAttribute(${attrRef}, ArkUIValue.F(value));\n    }`;
            case 'enum': {
                const et = shape.enumType || 'int';
                return `    /// <summary>${method.name}（${attrKey}）</summary>\n` +
                    `    public ${et} ${prop}\n    {\n` +
                    `        set => SetNumericAttribute(${attrRef}, ArkUIValue.I((int)value));\n    }`;
            }
            case 'color':
                return `    /// <summary>${method.name}（${attrKey}，单值 u32，0xAARRGGBB 格式）</summary>\n` +
                    `    public void Set${prop}(byte r, byte g, byte b, byte a = 255)\n    {\n` +
                    `        SetNumericAttribute(${attrRef}, ArkUIValue.U((uint)((a << 24) | (r << 16) | (g << 8) | b)));\n    }`;
            default:
                return null;
        }
    }

    private generateEvent(componentName: string, evt: EventInfo, gaps: NativeGap[]): string | null {
        const suffix = EVENT_NAME_OVERRIDES[evt.name] || toSnakeUpper(evt.name);
        // 候选枚举键：全局事件（NODE_ON_CLICK）与组件专属事件（NODE_REFRESH_ON_REFRESH）双形态
        const compPrefix = componentName.toUpperCase();
        const candidateKeys = [`NODE_${suffix}`, `NODE_${compPrefix}_${suffix}`];
        const eventTypes = this.enumMetadata['ArkUI_NodeEventType'] || {};
        const evtKey = candidateKeys.find(key => key in eventTypes);
        if (!evtKey) {
            gaps.push({
                component: componentName, member: evt.name, kind: 'event',
                reason: `none of ${candidateKeys.join('/')} present in ArkUI_NodeEventType`,
            });
            return null;
        }

        const csEventName = toPascal(evt.name.replace(/^on/, ''));
        return `    /// <summary>${evt.name} 事件（${evtKey}）</summary>\n` +
            `    public event Action<ArkUINodeEvent>? ${csEventName}\n    {\n` +
            `        add => On(ArkUI_NodeEventType.${evtKey}, value!);\n` +
            `        remove => Off(ArkUI_NodeEventType.${evtKey});\n    }`;
    }

    private emit(
        componentName: string,
        nodeTypeName: string,
        attributeBlocks: string[],
        eventBlocks: string[],
        _methods: MethodInfo[],
        _events: EventInfo[]
    ): string {
        const body = [...attributeBlocks, ...eventBlocks].join('\n\n');
        return `// <auto-generated>由 ArkTsBinding 解析器（NativeCodeGenerator）从组件 .d.ts 生成，请勿手工编辑</auto-generated>
#nullable enable
using System;
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

/// <summary>${componentName} 组件（${nodeTypeName}）</summary>
public unsafe class ${escapeIdentifier(componentName)} : ArkUINodeBase
{
    public ${escapeIdentifier(componentName)}() : base(ArkUI_NodeType.${nodeTypeName}) { }

${body || '    // （无可映射到 C API 的属性/事件）'}
}
`;
    }
}
