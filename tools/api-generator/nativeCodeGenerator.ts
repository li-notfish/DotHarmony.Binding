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
    kind: 'string' | 'int' | 'float' | 'enum' | 'color' | 'bool';
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
    // Checkbox
    NODE_CHECKBOX_SELECT: { kind: 'bool' },
    NODE_CHECKBOX_NAME: { kind: 'string' },
    NODE_CHECKBOX_SELECT_COLOR: { kind: 'color' },
    // Radio
    NODE_RADIO_CHECKED: { kind: 'bool' },
    // Image
    NODE_IMAGE_SRC: { kind: 'string' },
    NODE_IMAGE_OBJECT_FIT: { kind: 'enum', enumType: 'ArkUI_ObjectFit' },
    NODE_IMAGE_FILL_COLOR: { kind: 'color' },
    // Progress
    NODE_PROGRESS_TYPE: { kind: 'enum', enumType: 'ArkUI_ProgressType' },
    NODE_PROGRESS_VALUE: { kind: 'float' },
    NODE_PROGRESS_TOTAL: { kind: 'float' },
    NODE_PROGRESS_COLOR: { kind: 'color' },
    // Slider
    NODE_SLIDER_VALUE: { kind: 'float' },
    NODE_SLIDER_MIN_VALUE: { kind: 'float' },
    NODE_SLIDER_MAX_VALUE: { kind: 'float' },
    NODE_SLIDER_STEP: { kind: 'float' },
    NODE_SLIDER_BLOCK_COLOR: { kind: 'color' },
    NODE_SLIDER_TRACK_COLOR: { kind: 'color' },
    NODE_SLIDER_SELECTED_COLOR: { kind: 'color' },
    // Radio
    NODE_RADIO_VALUE: { kind: 'string' },
    NODE_RADIO_GROUP: { kind: 'string' },
    // Scroll
    NODE_SCROLL_SCROLL_DIRECTION: { kind: 'enum', enumType: 'ArkUI_ScrollDirection' },
    NODE_SCROLL_EDGE_EFFECT: { kind: 'int' },
    // Toggle
    NODE_TOGGLE_VALUE: { kind: 'int' },
    NODE_TOGGLE_SELECTED_COLOR: { kind: 'color' },
    // TextArea / TextInput
    NODE_TEXT_AREA_TEXT: { kind: 'string' },
    NODE_TEXT_AREA_PLACEHOLDER: { kind: 'string' },
    NODE_TEXT_AREA_PLACEHOLDER_COLOR: { kind: 'color' },
    NODE_TEXT_INPUT_TEXT: { kind: 'string' },
    NODE_TEXT_INPUT_PLACEHOLDER: { kind: 'string' },
    NODE_TEXT_INPUT_PLACEHOLDER_COLOR: { kind: 'color' },
    NODE_TEXT_INPUT_TYPE: { kind: 'enum', enumType: 'ArkUI_TextInputType' },
    NODE_TEXT_INPUT_MAX_LENGTH: { kind: 'int' },
    NODE_TEXT_INPUT_ENTER_KEY_TYPE: { kind: 'enum', enumType: 'ArkUI_EnterKeyType' },
    NODE_TEXT_INPUT_EDITING: { kind: 'int' },
    // DatePicker / TimePicker / TextPicker（d.ts 链式方法）
    NODE_DATE_PICKER_LUNAR: { kind: 'bool' },
    NODE_TIME_PICKER_USE_MILITARY_TIME: { kind: 'bool' },
    NODE_TEXT_PICKER_CAN_LOOP: { kind: 'bool' },
};

/**
 * TS 方法名与 C 枚举名不同构的显式对照（含形态）。
 * key = "<组件名>:<d.ts 方法名小写>"，优先级高于候选键。
 */
export const ATTR_ALIASES: Record<string, { key: string; shape: AttributeShape }> = {
    // d.ts unselectedColor → C 枚举 NODE_CHECKBOX_UNSELECT_COLOR（无 ED）
    'checkbox:unselectedcolor': { key: 'NODE_CHECKBOX_UNSELECT_COLOR', shape: { kind: 'color' } },
    // d.ts selectedColor → C 枚举 NODE_CHECKBOX_SELECT_COLOR（无 ED）
    'checkbox:selectedcolor': { key: 'NODE_CHECKBOX_SELECT_COLOR', shape: { kind: 'color' } },
    // d.ts scrollable → C 枚举 NODE_SCROLL_SCROLL_DIRECTION（.d.ts 方法名与 C 枚举名不匹配）
    'scroll:scrollable': { key: 'NODE_SCROLL_SCROLL_DIRECTION', shape: { kind: 'enum', enumType: 'ArkUI_ScrollDirection' } },
};

/**
 * 构造参数 / 构造选项中映射到 C API 属性的成员。
 * key = NODE_<组件>_<ATTR>（DEFAULT_SHAPES 中的键名）。
 * 生成器在第 2c 阶段遍历此表，为未被 .d.ts 链式接口覆盖的属性补充生成。
 */
export const CONSTRUCTOR_OPTION_PROPS: Record<string, { component: string; csName: string; shape: AttributeShape; csType?: string }> = {
    // CheckBox — select / name 是构造选项（非链式属性）
    NODE_CHECKBOX_SELECT: { component: 'Checkbox', csName: 'IsSelected', shape: { kind: 'bool' }, csType: 'bool' },
    NODE_CHECKBOX_NAME:   { component: 'Checkbox', csName: 'Name', shape: { kind: 'string' }, csType: 'string' },
    // Radio — group / value 是构造选项
    NODE_RADIO_GROUP: { component: 'Radio', csName: 'Group', shape: { kind: 'string' }, csType: 'string' },
    NODE_RADIO_VALUE: { component: 'Radio', csName: 'Value', shape: { kind: 'string' }, csType: 'string' },
    // Toggle — isOn 是构造选项（NODE_TOGGLE_VALUE → i32 0/1）
    NODE_TOGGLE_VALUE: { component: 'Toggle', csName: 'IsOn', shape: { kind: 'bool' }, csType: 'bool' },
    // Slider — min / max / value / step 是构造选项
    NODE_SLIDER_VALUE:     { component: 'Slider', csName: 'Value', shape: { kind: 'float' }, csType: 'float' },
    NODE_SLIDER_MIN_VALUE: { component: 'Slider', csName: 'MinValue', shape: { kind: 'float' }, csType: 'float' },
    NODE_SLIDER_MAX_VALUE: { component: 'Slider', csName: 'MaxValue', shape: { kind: 'float' }, csType: 'float' },
    NODE_SLIDER_STEP:      { component: 'Slider', csName: 'Step', shape: { kind: 'float' }, csType: 'float' },
    // Refresh — refreshing 是构造选项（RefreshOptions.refreshing）
    NODE_REFRESH_REFRESHING: { component: 'Refresh', csName: 'IsRefreshing', shape: { kind: 'bool' }, csType: 'bool' },
    // DatePicker / TimePicker — 构造选项（DatePickerOptions/TimePickerOptions，载荷 .string "yyyy-MM-dd"/"HH:mm"）
    NODE_DATE_PICKER_SELECTED: { component: 'DatePicker', csName: 'SelectedDate', shape: { kind: 'string' }, csType: 'string' },
    NODE_DATE_PICKER_START:    { component: 'DatePicker', csName: 'StartDate', shape: { kind: 'string' }, csType: 'string' },
    NODE_DATE_PICKER_END:      { component: 'DatePicker', csName: 'EndDate', shape: { kind: 'string' }, csType: 'string' },
    NODE_TIME_PICKER_SELECTED: { component: 'TimePicker', csName: 'SelectedTime', shape: { kind: 'string' }, csType: 'string' },
    // TextPicker — 选中索引（Range 为多值载荷，见 generate() 的组件特例 SetRange）
    NODE_TEXT_PICKER_SELECTED_INDEX: { component: 'TextPicker', csName: 'SelectedIndex', shape: { kind: 'int' }, csType: 'int' },
};

/**
 * 组件名 → ArkUI_NodeType 枚举名的形态修正（驼峰直接大写连写会错）
 * TextArea → TEXT_AREA（而非 TEXTAREA）
 */
const NODE_TYPE_NAME_FIXES: Record<string, string> = {
    // 值为完整 ArkUI_NodeType 枚举名（attrPrefix 推导时剥离 ARKUI_NODE_ 前缀）。
    // 注意 TextArea/TextInput 两条是历史遗留、从未生效（text_area.cs/text_input.cs
    // 为手工对齐文件，勿在此启用——生成器产物会令 Entry/Editor handler 回归）
    TextArea: 'TEXT_AREA',
    TextInput: 'TEXT_INPUT',
    DatePicker: 'ARKUI_NODE_DATE_PICKER',
    TimePicker: 'ARKUI_NODE_TIME_PICKER',
    TextPicker: 'ARKUI_NODE_TEXT_PICKER',
};

/**
 * 组件名 → C# 类名修正（.d.ts 组件名与 handler 期望的类名不一致时）。
 * Checkbox → CheckBox（handler using alias 期望 HarmonyOS.ArkUI.CheckBox）
 * Radio → RadioButton（handler using alias 期望 HarmonyOS.ArkUI.RadioButton）
 */
const CLASS_NAME_FIXES: Record<string, string> = {
    Checkbox: 'CheckBox',
    Radio: 'RadioButton',
};

/** CommonMethod 内联展开后与基类公开成员重名的属性，生成器跳过 */
const BASE_CLASS_MEMBERS = new Set([
    'width', 'height', 'padding', 'margin', 'backgroundColor', 'visibility', 'opacity', 'enabled',
]);

/** 事件名特殊映射（onXxx → C 枚举后缀） */
const EVENT_NAME_OVERRIDES: Record<string, string> = {
    // 枚举匹配修正（ArkUI 事件枚举名与 d.ts 不同构）
    onTouch: 'TOUCH_EVENT',
    onClick: 'ON_CLICK',
    onRefreshing: 'ON_REFRESH',
    onStateChange: 'STATE_CHANGE',
    onOffsetChange: 'ON_OFFSET_CHANGE',
};

/**
 * 组件级事件名覆盖（key = "组件名:事件名"）。
 * 优先级高于 EVENT_NAME_OVERRIDES，用于同一 ArkTS 事件名在不同组件上映射到不同枚举键和/或 MAUI 名称。
 */
const COMPONENT_EVENT_OVERRIDES: Record<string, { enumSuffix: string; csName?: string }> = {
    'checkbox:onChange': { enumSuffix: 'CHECKBOX_EVENT_ON_CHANGE', csName: 'CheckedChanged' },
    'radio:onChange':     { enumSuffix: 'RADIO_EVENT_ON_CHANGE', csName: 'CheckedChanged' },
    'toggle:onChange':    { enumSuffix: 'TOGGLE_ON_CHANGE', csName: 'Toggled' },
    'slider:onChange':    { enumSuffix: 'SLIDER_EVENT_ON_CHANGE', csName: 'ValueChanged' },
    // CS0542：事件成员名不能与封闭类型 Scroll 同名；MAUI IScrollView 事件名为 Scrolled
    'scroll:onScroll':    { enumSuffix: 'SCROLL_EVENT_ON_SCROLL', csName: 'Scrolled' },
    // picker 系：d.ts onChange 与 C 事件枚举名不同构（带 EVENT 中缀 + 不同尾名）
    'datepicker:onChange': { enumSuffix: 'DATE_PICKER_EVENT_ON_DATE_CHANGE', csName: 'OnDateChange' },
    'timepicker:onChange': { enumSuffix: 'TIME_PICKER_EVENT_ON_CHANGE', csName: 'OnTimeChange' },
    'textpicker:onChange': { enumSuffix: 'TEXT_PICKER_EVENT_ON_CHANGE', csName: 'OnChange' },
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
        const nodeTypeName = NODE_TYPE_NAME_FIXES[comp.name] || `ARKUI_NODE_${comp.name.toUpperCase()}`;
        const className = CLASS_NAME_FIXES[comp.name] || comp.name;
        const nodeTypes = this.enumMetadata['ArkUI_NodeType'] || {};
        if (!(nodeTypeName in nodeTypes)) {
            gaps.push({
                component: comp.name, member: nodeTypeName, kind: 'nodeType',
                reason: `missing in ArkUI_NodeType (C API does not expose this component as a native node)`,
            });
            return { className, csharp: null, gaps };
        }

        // 2. 属性（onXxx 方法归入事件通道）；同名成员去重（d.ts 接口与命名空间双声明）
        const isEventLike = (m: MethodInfo) => /^on[A-Z]/.test(m.name);
        const seenMethod = new Set<string>();
        const uniqueMethods = comp.methods.filter(m => {
            if (seenMethod.has(m.name)) return false;
            seenMethod.add(m.name);
            return true;
        });
        // C 枚举前缀用修正后的 nodeTypeName（DATE_PICKER 而非 DATEPICKER），
        // 与 DEFAULT_SHAPES / CONSTRUCTOR_OPTION_PROPS 的多词键名对齐
        const attrPrefix = nodeTypeName.replace(/^ARKUI_NODE_/, '');
        const attributeBlocks: string[] = [];
        for (const method of uniqueMethods) {
            if (BASE_CLASS_MEMBERS.has(method.name)) continue;
            if (isEventLike(method)) continue;
            const block = this.generateAttribute(comp.name, method, gaps, attrPrefix);
            if (block) attributeBlocks.push(block);
        }

        // 2b. 构造参数映射：Text(content) / Button(label) 这类内容参数对应
        //     NODE_<组件>_<参数名>（如 NODE_TEXT_CONTENT / NODE_BUTTON_LABEL）
        for (const param of comp.constructorParams) {
            if (!param.type.includes('string') && !param.type.includes('ResourceStr')) continue;
            const block = this.generateConstructorParamAttribute(comp.name, param.name, gaps, attrPrefix);
            if (block) attributeBlocks.push(block);
        }

        // 2c. 构造选项属性：DEFAULT_SHAPES 中存在但 .d.ts 链式接口未覆盖的属性
        //     （如 Slider.min/max/value、Toggle.isOn、Radio.group/value、CheckBox.isSelected/name）
        const generatedAttrKeys = new Set<string>();
        const seenAttrProp = new Set<string>();
        for (const block of attributeBlocks) {
            const m = block.match(/ArkUI_NodeAttributeType\.(\w+)/);
            if (m) generatedAttrKeys.add(m[1]);
            const pm = block.match(/public\s+\w+\s+(\w+)/);
            if (pm) seenAttrProp.add(pm[1]);
        }
        for (const [attrKey, opt] of Object.entries(CONSTRUCTOR_OPTION_PROPS)) {
            if (generatedAttrKeys.has(attrKey)) continue;
            if (seenAttrProp.has(opt.csName)) continue;
            // 只生成属于本组件的属性（component 显式归属，防前缀碰撞）
            if (opt.component !== comp.name) continue;
            const attrTypes = this.enumMetadata['ArkUI_NodeAttributeType'] || {};
            if (!(attrKey in attrTypes)) continue;
            const attrRef = `ArkUI_NodeAttributeType.${attrKey}`;
            switch (opt.shape.kind) {
                case 'bool':
                    attributeBlocks.push(
                        `    /// <summary>${opt.csName.toLowerCase()}（构造选项，${attrKey}，i32 0/1）</summary>\n` +
                        `    public bool ${opt.csName}\n    {\n` +
                        `        set => SetNumericAttribute(${attrRef}, ArkUIValue.I(value ? 1 : 0));\n    }`);
                    break;
                case 'string':
                    attributeBlocks.push(
                        `    /// <summary>${opt.csName.toLowerCase()}（构造选项，${attrKey}，string）</summary>\n` +
                        `    public string ${opt.csName}\n    {\n` +
                        `        set => SetStringAttribute(${attrRef}, value);\n    }`);
                    break;
                case 'float':
                    attributeBlocks.push(
                        `    /// <summary>${opt.csName.toLowerCase()}（构造选项，${attrKey}，float）</summary>\n` +
                        `    public float ${opt.csName}\n    {\n` +
                        `        set => SetNumericAttribute(${attrRef}, ArkUIValue.F(value));\n    }`);
                    break;
                case 'int':
                    attributeBlocks.push(
                        `    /// <summary>${opt.csName.toLowerCase()}（构造选项，${attrKey}，i32）</summary>\n` +
                        `    public int ${opt.csName}\n    {\n` +
                        `        set => SetNumericAttribute(${attrRef}, ArkUIValue.I(value));\n    }`);
                    break;
                case 'color':
                    attributeBlocks.push(
                        `    /// <summary>${opt.csName.toLowerCase()}（构造选项，${attrKey}，u32 0xAARRGGBB）</summary>\n` +
                        `    public void Set${opt.csName}(byte r, byte g, byte b, byte a = 255)\n    {\n` +
                        `        SetNumericAttribute(${attrRef}, ArkUIValue.U((uint)((a << 24) | (r << 16) | (g << 8) | b)));\n    }`);
                    break;
            }
        }

        // 2d. 组件特有的额外成员（.d.ts 未声明但 C API 支持的方法/属性）
        if (comp.name === 'Scroll') {
            // SetOffset: C API NODE_SCROLL_OFFSET 接受双 f32 参数（水平/垂直偏移）
            const offsetKey = 'NODE_SCROLL_OFFSET';
            const attrTypes = this.enumMetadata['ArkUI_NodeAttributeType'] || {};
            if (offsetKey in attrTypes && !generatedAttrKeys.has(offsetKey)) {
                attributeBlocks.push(
                    `    /// <summary>滚动偏移（${offsetKey}，双值 f32：水平 vp + 垂直 vp）</summary>\n` +
                    `    public void SetOffset(float horizontal, float vertical)\n` +
                    `        => SetNumericAttribute(ArkUI_NodeAttributeType.${offsetKey},\n` +
                    `            ArkUIValue.F(horizontal), ArkUIValue.F(vertical));`);
            }
        }

        // 2e. TextPicker Range 特殊载荷：value[0].i32=RangeType(1=单列) + .string（';' 分隔），
        //     通用形态表无法表达，与 Scroll.SetOffset 同属组件特例
        if (comp.name === 'TextPicker') {
            const rangeKey = 'NODE_TEXT_PICKER_OPTION_RANGE';
            const attrTypes = this.enumMetadata['ArkUI_NodeAttributeType'] || {};
            if (rangeKey in attrTypes && !generatedAttrKeys.has(rangeKey)) {
                attributeBlocks.push(
                    `    /// <summary>单列选项范围（${rangeKey}：value[0].i32=1 单列，string 以 ';' 分隔）</summary>
` +
                    `    public void SetRange(System.Collections.Generic.IReadOnlyList<string> options)
` +
                    `    {
` +
                    `        var utf8 = System.Text.Encoding.UTF8.GetBytes(string.Join(";", options));
` +
                    `        var values = new ArkUI_NumberValue[] { ArkUIValue.I(1) }; // ArkUI_TextPickerRangeType: 1 = 单列字符串
` +
                    `        fixed (byte* p = utf8)
` +
                    `        fixed (ArkUI_NumberValue* v = values)
` +
                    `        {
` +
                    `            var item = new ArkUI_AttributeItem { value = v, size = 1, @string = p };
` +
                    `            var status = ArkUINativeApi.SetAttribute(Handle, ArkUI_NodeAttributeType.${rangeKey}, &item);
` +
                    `            if (status != 0)
` +
                    `                throw new InvalidOperationException("SetAttribute(${rangeKey}) failed: " + status);
` +
                    `        }
` +
                    `    }`);
            }
        }

        // 3. 事件：显式 events + methods 中 on 开头的方法（CommonMethod<T> 合并产物），同名去重
        const allEvents: EventInfo[] = [];
        const seenEvent = new Set<string>();
        for (const evt of [...comp.events, ...uniqueMethods.filter(m => isEventLike(m)).map(m => ({
            name: m.name,
            delegateName: `${toPascal(m.name)}Handler`,
            parameters: m.parameters,
            returnType: m.returnType,
        }))]) {
            if (seenEvent.has(evt.name)) continue;
            seenEvent.add(evt.name);
            allEvents.push(evt);
        }
        const eventBlocks: string[] = [];
        for (const evt of allEvents) {
            const block = this.generateEvent(comp.name, evt, gaps, attrPrefix);
            if (block) eventBlocks.push(block);
        }

        // 4. 产出
        const csharp = this.emit(className, nodeTypeName, attributeBlocks, eventBlocks, comp.methods, comp.events);
        return { className, csharp, gaps };
    }

    /**
     * 属性枚举候选键：
     *   textAlign + Text → 候选 [NODE_TEXT_TEXT_ALIGN, NODE_TEXT_ALIGN]（TS 属性名自带组件前缀时去重）
     *   label + Button   → 候选 [NODE_BUTTON_LABEL]
     */
    private candidateAttrKeys(componentName: string, snake: string, attrPrefix?: string): string[] {
        // 多词组件（TextArea→TEXT_AREA / DatePicker→DATE_PICKER）必须用修正后的 C 枚举前缀
        const prefix = attrPrefix ?? componentName.toUpperCase();
        const compPrefix = `${prefix}_`;
        const keys = [`NODE_${compPrefix}${snake}`];
        if (snake.startsWith(compPrefix)) {
            keys.push(`NODE_${snake}`);
        }
        // 全局通用属性段（如 NODE_FONT_COLOR / NODE_OPACITY）
        keys.push(`NODE_${snake}`);
        return keys;
    }

    private resolveShape(componentName: string, snake: string, tsName?: string, attrPrefix?: string): { attrKey: string; shape: AttributeShape } | null {
        // 显式对照表优先（TS 名与 C 枚举名不同构的成员）
        if (tsName) {
            const alias = ATTR_ALIASES[`${componentName.toLowerCase()}:${tsName.toLowerCase()}`];
            if (alias) return { attrKey: alias.key, shape: alias.shape };
        }
        for (const key of this.candidateAttrKeys(componentName, snake, attrPrefix)) {
            const shape = this.shapes[key];
            if (shape) return { attrKey: key, shape };
        }
        return null;
    }

    private shapeExistsInCApi(componentName: string, snake: string, attrPrefix?: string): boolean {
        const attrTypes = this.enumMetadata['ArkUI_NodeAttributeType'] || {};
        return this.candidateAttrKeys(componentName, snake, attrPrefix).some(key => key in attrTypes);
    }

    private generateConstructorParamAttribute(
        componentName: string, paramName: string, gaps: NativeGap[], attrPrefix?: string
    ): string | null {
        const snake = toSnakeUpper(paramName);
        const resolved = this.resolveShape(componentName, snake, undefined, attrPrefix);
        if (!resolved) {
            if (this.shapeExistsInCApi(componentName, snake, attrPrefix)) {
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

    private generateAttribute(componentName: string, method: MethodInfo, gaps: NativeGap[], attrPrefix?: string): string | null {
        const snake = toSnakeUpper(method.name);
        const resolved = this.resolveShape(componentName, snake, method.name, attrPrefix);

        if (!resolved) {
            const exists = this.shapeExistsInCApi(componentName, snake, attrPrefix);
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
            case 'bool':
                return `    /// <summary>${method.name}（${attrKey}，i32 0/1）</summary>\n` +
                    `    public bool ${prop}\n    {\n` +
                    `        set => SetNumericAttribute(${attrRef}, ArkUIValue.I(value ? 1 : 0));\n    }`;
            default:
                return null;
        }
    }

    private generateEvent(componentName: string, evt: EventInfo, gaps: NativeGap[], attrPrefix?: string): string | null {
        // 1. 枚举匹配：组件级覆盖优先，再全局覆盖，最后默认转换
        const componentKey = `${componentName.toLowerCase()}:${evt.name}`;
        const componentOverride = COMPONENT_EVENT_OVERRIDES[componentKey];
        const globalEnumOverride = EVENT_NAME_OVERRIDES[evt.name];
        const enumSuffix = componentOverride?.enumSuffix || globalEnumOverride || toSnakeUpper(evt.name);

        // 候选枚举键：组件级覆盖直接用完整枚举名（如 NODE_CHECKBOX_EVENT_ON_CHANGE）；
        // 否则走三形态候选（全局 / 组件专属 / 组件专属带 EVENT 中缀）
        const compPrefix = attrPrefix ?? componentName.toUpperCase();
        let candidateKeys: string[];
        if (componentOverride) {
            candidateKeys = [`NODE_${enumSuffix}`];
        } else {
            candidateKeys = [`NODE_${enumSuffix}`, `NODE_${compPrefix}_${enumSuffix}`, `NODE_${compPrefix}_EVENT_${enumSuffix}`];
        }
        const eventTypes = this.enumMetadata['ArkUI_NodeEventType'] || {};
        const evtKey = candidateKeys.find(key => key in eventTypes);
        if (!evtKey) {
            gaps.push({
                component: componentName, member: evt.name, kind: 'event',
                reason: `none of ${candidateKeys.join('/')} present in ArkUI_NodeEventType`,
            });
            return null;
        }

        // 2. C# 事件名：组件级覆盖有 csName 时用它，否则从 ArkTS 名称推导（去 on + PascalCase）
        const csEventName = componentOverride?.csName || toPascal(evt.name.replace(/^on/, ''));
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
public unsafe partial class ${escapeIdentifier(CLASS_NAME_FIXES[componentName] || componentName)} : ArkUINodeBase
{
    public ${escapeIdentifier(CLASS_NAME_FIXES[componentName] || componentName)}() : base(ArkUI_NodeType.${nodeTypeName}) { }

${body || '    // （无可映射到 C API 的属性/事件）'}
}
`;
    }
}
