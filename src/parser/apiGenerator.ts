/**
 * @ohos.* namespace 绑定生成器（.NET 风格标准化版）。
 *
 * 生成模式：
 * - static unsafe partial class，ModuleName 常量，napi_load_module 懒加载 + NapiReference
 * - const → static property（PascalCase，缩写词归一）
 * - function → static method（Task 返回自动加 Async 后缀）
 * - namespace 内嵌套接口/类 → JsObject 派生包装类（有行为）或 sealed record + INapiRecord（纯入参数据）
 * - 类型映射单一事实源在 TypeMapper：包装类/枚举名先生成映射登记，再统一映射原始 TS 类型
 *
 * 产物命名空间：HarmonyOS.Bindings.Api
 */
import { ComponentInfo, MethodInfo, ParameterInfo, EnumInfo, InterfaceInfo, ClassInfo, EventMetaInfo } from './models';
import { TypeMapper } from './typeMapper';
import { EnumGenerator } from './enumGenerator';
import { toPascalCase, withAsyncSuffix } from './naming';

export interface ApiGenResult {
    csharp: string;
    enums: string | null;
    className: string;
    moduleName: string;
    permissions: string[];
}

const EVENT_FN = new Set(['on', 'off', 'once']);

/** 待生成的事件方法/访问器（一个 on/off/once 重载一条） */
interface EventEmitInfo {
    fnName: 'on' | 'off' | 'once';
    kind: EventMetaInfo['kind'];
    /** literal: 字面量值；enum: { raw: 'SensorId.ACCELEROMETER', memberCs: 'Accelerometer' } */
    literals: { value?: string; enumFqn?: string; memberCs?: string }[];
    /** 类型化方法的 type 参数 C# 类型（'string' 或枚举 fqn） */
    typeParamCs: string;
    /** 映射后的回调参数 C# 类型 */
    callbackArgs: string[];
    /** 除 type/callback 外的其余已映射参数 */
    extraParams: ParameterInfo[];
}

/** 实例类型规格（namespace 内嵌套接口/类） */
interface TypeSpec {
    tsName: string;
    csharpName: string;
    kind: 'wrapper' | 'record';
    iface?: InterfaceInfo;
    cls?: ClassInfo;
}

/** 映射完成的待生成成员 */
interface EmitMember {
    rawName: string;
    pascalName: string;
    /** 属性（isChained const）或方法 */
    isProperty: boolean;
    /** 映射后的 C# 返回类型（方法） */
    retType: string;
    /** 映射后的参数（方法） */
    params: ParameterInfo[];
    /** 仅 callback 形式（无 Promise 重载）：调用时经 CallbackTaskBridge 创建回调传入 */
    useCallbackBridge?: boolean;
}

/** 跨模块类型名登记：csharpName → 登记模块类名。同名冲突时加模块前缀（如 WindowRect） */
const takenTypeNames = new Map<string, string>();

/** 与 System.* / 产物命名空间常用类型撞名的实例类型，强制加 Object 后缀 */
const FORBIDDEN_CSHARP_NAMES = new Set(['Task', 'ValueTask', 'Action', 'Func', 'Attribute', 'Exception', 'Nullable']);

/** TS 成员名能否映射为 C# 成员名（[Symbol.iterator]、带点号/引号的名字无法映射，直接跳过） */
function isValidCSharpName(tsName: string): boolean {
    return /^[A-Za-z_][A-Za-z0-9_]*$/.test(tsName);
}

const PRIMITIVE_TYPES = new Set(['bool', 'double', 'float', 'int', 'uint', 'long', 'byte', 'string', 'IntPtr']);

/** 全量生成时预登记的所有模块类名：实例类型与任何模块类同名时追加 Object 后缀（避免撞静态类） */
export const reservedModuleClassNames: Set<string> = new Set();

export class ApiGenerator {
    private enumGenerator: EnumGenerator;
    private enumNames = new Set<string>();
    private specs: TypeSpec[] = [];
    private specByCsharp = new Map<string, TypeSpec>();
    private moduleClassName = '';

    constructor() {
        this.enumGenerator = new EnumGenerator();
    }

    /**
     * 从 .d.ts 文件内容提取 @permission 声明
     */
    static extractPermissions(source: string): string[] {
        const perms = new Set<string>();
        const re = /@permission\s+(ohos\.permission\.[\w.]+)/g;
        let m: RegExpExecArray | null;
        while ((m = re.exec(source)) !== null) {
            perms.add(m[1]);
        }
        return [...perms].sort();
    }

    /**
     * @ohos.deviceInfo.d.ts → { module: "@ohos.deviceInfo", className: "DeviceInfo", local: "deviceInfo" }
     */
    static dtsToModuleInfo(dtsFileName: string): { module: string; className: string; local: string } {
        const stem = dtsFileName.replace(/\.d\.ts$/, '');
        // 去掉 @ohos. 或 @system. 前缀
        let local = stem;
        if (local.startsWith('@ohos.')) local = local.substring(6);
        else if (local.startsWith('@system.')) local = 'system.' + local.substring(8);
        else if (local.startsWith('@')) local = local.substring(1);

        // className: 取最后一段，首字母大写
        const parts = local.split('.');
        const className = parts[parts.length - 1].charAt(0).toUpperCase()
            + parts[parts.length - 1].slice(1);

        return { module: stem, className, local };
    }

    generate(
        component: ComponentInfo,
        moduleInfo: { module: string; className: string; local: string },
        permissions: string[],
        enums: EnumInfo[],
        allEnums: EnumInfo[] = enums,
        importedTypeNames: ReadonlySet<string> = new Set()
    ): ApiGenResult {
        this.moduleClassName = moduleInfo.className;
        // 枚举类型引用一律全限定，避免与 System.* 同名类型（如 Action）冲突（CS0104）
        this.enumNames = new Set(allEnums.map(e => `global::HarmonyOS.ArkUI.${e.name}`));
        this.specs = [];
        this.specByCsharp = new Map();

        // 1. 登记映射：枚举（全部参与映射，去重只影响 Enums.cs 写盘）+ 实例类型
        for (const e of allEnums) {
            TypeMapper.addMapping(e.name, `global::HarmonyOS.ArkUI.${e.name}`);
        }
        // 本模块未定义的跨模块导入类型 → IntPtr 句柄（映射注册顺序保证自有类型优先）
        const ownTypeNames = new Set<string>([
            ...allEnums.map(e => e.name),
            ...component.interfaces.map(i => i.name),
            ...component.classes.map(c => c.name),
        ]);
        for (const n of importedTypeNames) {
            if (!ownTypeNames.has(n)) {
                TypeMapper.addMapping(n, 'IntPtr');
            }
        }
        for (const iface of component.interfaces) {
            if (iface.typeParameters && iface.typeParameters.length > 0) continue; // 泛型接口不包装
            const kind = iface.methods.length > 0 ? 'wrapper' : 'record';
            this.registerSpec(iface.name, kind, iface, undefined);
        }
        for (const cls of component.classes) {
            if (cls.typeParameters && cls.typeParameters.length > 0) continue;
            this.registerSpec(cls.name, 'wrapper', undefined, cls);
        }

        // 2. record 可封送性收敛：属性含不可封送类型的 record 移除映射（参数退回 IntPtr），重映射至不动点
        this.convergeRecordMarshaling(component);

        // 3. 映射模块成员
        const members = this.mapMembers(component);

        // 3.5 事件元数据：类型化 On/Off/Once + .NET event（回调参数类型参与可达性/升级扫描）。
        // 注意：包装类（嵌套接口/类）上的实例事件回调参数同样参与扫描，
        // 否则其载荷类型（如 camera 的 Photo）不会被发射导致 CS0246。
        const moduleEvents = this.buildEventInfos(component.methods);
        const allEventMethods = [
            ...component.methods,
            ...component.interfaces.flatMap(i => i.methods),
            ...component.classes.flatMap(c => c.methods),
        ];
        const eventCallbackArgs = this.buildEventInfos(allEventMethods)
            .filter(e => e.fnName === 'on' || e.fnName === 'once')
            .flatMap(e => e.callbackArgs)
            .filter(t => t !== 'IntPtr');

        // 4. 分类收敛：record 被返回位置引用 → 升级为 wrapper
        this.convergeRecordUsage(members, eventCallbackArgs);

        // 5. 可达性：只生成被模块成员（传递）引用的实例类型
        const reachable = this.collectReachable(members, eventCallbackArgs);

        // 6. 生成
        const needsTask = members.some(m => /^Task(<.+>)?$/.test(m.retType));
        const hasWrappers = [...reachable].some(n => this.specByCsharp.get(n)?.kind === 'wrapper');
        const hasRecords = [...reachable].some(n => this.specByCsharp.get(n)?.kind === 'record');
        const lines: string[] = [];
        this.emitHeader(lines, moduleInfo, permissions, needsTask, hasWrappers || hasRecords);
        this.emitModuleClass(lines, component, members, moduleInfo, moduleEvents);
        for (const name of reachable) {
            const spec = this.specByCsharp.get(name)!;
            if (spec.kind === 'wrapper') this.emitWrapper(lines, spec);
            else this.emitRecord(lines, spec);
        }

        const csharp = lines.join('\n');

        let enumCode: string | null = null;
        if (enums.length > 0) {
            enumCode = this.enumGenerator.generateMultipleEnums(enums);
        }

        return {
            csharp,
            enums: enumCode,
            className: moduleInfo.className,
            moduleName: moduleInfo.module,
            permissions
        };
    }

    // ---------- 映射登记 ----------

    private registerSpec(tsName: string, kind: 'wrapper' | 'record', iface?: InterfaceInfo, cls?: ClassInfo): void {
        let csharp = (tsName === this.moduleClassName || FORBIDDEN_CSHARP_NAMES.has(tsName)
            || reservedModuleClassNames.has(tsName))
            ? `${tsName}Object` : tsName;
        const owner = takenTypeNames.get(csharp);
        if (owner !== undefined && owner !== this.moduleClassName) {
            // 跨模块同名（如 Rect/Size）：加模块前缀避免跨文件重复定义
            csharp = `${this.moduleClassName}${tsName}`;
            takenTypeNames.set(csharp, this.moduleClassName);
        } else if (owner === undefined) {
            takenTypeNames.set(csharp, this.moduleClassName);
        }
        const spec: TypeSpec = { tsName, csharpName: csharp, kind, iface, cls };
        this.specs.push(spec);
        this.specByCsharp.set(csharp, spec);
        TypeMapper.addMapping(tsName, csharp);
    }

    /** record 属性是否全部可封送（基元/枚举/字符串/wrapper/可封送嵌套 record） */
    private isRecordMarshaling(spec: TypeSpec, marshalingOk: Set<string>): boolean {
        const props = spec.iface ? this.mergedProperties(spec.iface) : (spec.cls?.properties ?? []);
        for (const prop of props) {
            const mapped = TypeMapper.mapType(TypeMapper.cleanOptional(prop.type));
            if (mapped === 'IntPtr' || mapped === 'object' || mapped === 'null') return false;
            if (PRIMITIVE_TYPES.has(mapped) || this.enumNames.has(mapped)) continue;
            const m = /^([\w.:]+)\[\]$/.exec(mapped);
            if (m) {
                const elem = m[1];
                if (!PRIMITIVE_TYPES.has(elem) && !this.enumNames.has(elem) && !marshalingOk.has(elem)) return false;
                continue;
            }
            const target = this.specByCsharp.get(mapped);
            if (target && target.kind === 'record' && !marshalingOk.has(mapped)) {
                // 嵌套 record：假定可封送（外层收敛失败会连带移除）
                continue;
            }
            if (target && target.kind === 'wrapper') continue;
            if (!target && !PRIMITIVE_TYPES.has(mapped)) return false;
        }
        return true;
    }

    /**
     * 反复移除属性不可封送的 record 映射，直到不动点。
     * 移除后引用它的参数/返回类型退回 IntPtr（当前行为），保证产物可编译。
     */
    private convergeRecordMarshaling(component: ComponentInfo): void {
        for (let round = 0; round < 5; round++) {
            const marshalingOk = new Set<string>();
            let changed = false;
            for (const spec of this.specs) {
                if (spec.kind !== 'record') continue;
                if (!this.isRecordMarshaling(spec, marshalingOk)) {
                    TypeMapper.removeMapping(spec.tsName);
                    this.specByCsharp.delete(spec.csharpName);
                    changed = true;
                } else {
                    marshalingOk.add(spec.csharpName);
                }
            }
            if (!changed) return;
        }
        // 收敛兜底：仍未登记成功的 record 全部移除
        for (const spec of this.specs) {
            if (spec.kind === 'record' && this.specByCsharp.has(spec.csharpName)
                && !this.isRecordMarshaling(spec, new Set())) {
                TypeMapper.removeMapping(spec.tsName);
                this.specByCsharp.delete(spec.csharpName);
            }
        }
        void component;
    }

    // ---------- 成员映射 ----------

    private mapMembers(component: ComponentInfo): EmitMember[] {
        const members: EmitMember[] = [];
        const generatedSignatures = new Set<string>();

        for (const method of component.methods) {
            if (!isValidCSharpName(method.name)) continue;
            // 属性（const）
            if (method.isChained && method.parameters.length === 0) {
                const retType = this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(method.returnType)));
                if (retType === 'void') continue;
                const pascal = toPascalCase(method.name);
                const sigKey = `${pascal}()`;
                if (generatedSignatures.has(sigKey)) continue;
                generatedSignatures.add(sigKey);
                members.push({ rawName: method.name, pascalName: pascal, isProperty: true, retType, params: [] });
                continue;
            }

            // AsyncCallback 处理：命名形式参数 / 解析期 inline 形式标记
            let params = [...method.parameters];
            // 事件函数：回调参数统一退回 IntPtr 原生重载（类型化 Action 重载由事件生成器单独产出，
            // 否则 inline 回调映射出的 System.Action 会与类型化重载签名撞车 CS0111）。
            // 必须先强制回调为必需参数，再 demoteOptionals——否则回调前的可选参数不会降级（CS1737）。
            if (method.eventMeta) {
                const cbIdx = params.findIndex(p => /^(?:Async)?Callback?</.test(p.type) || p.name === 'callback');
                if (cbIdx >= 0) params[cbIdx] = { ...params[cbIdx], type: 'IntPtr', optional: false };
            }
            params = this.demoteOptionals(params);
            let asyncInner: string | null = null;
            const namedCb = params.find(p => /^AsyncCallback<(.+)>$/.test(p.type));
            if (namedCb) {
                asyncInner = /^AsyncCallback<(.+)>$/.exec(namedCb.type)![1];
                params = params.filter(p => p !== namedCb);
            } else if (method.asyncResultType !== undefined) {
                asyncInner = method.asyncResultType;
            }

            // 双形态判定：同名且剥掉回调后参数一致的 Promise 重载存在时，原生实现按"末参是否
            // 为函数"自适应返回 Promise，按 Promise 通道调用即可；否则该 API 仅 callback 形式，
            // 必须经 CallbackTaskBridge 创建回调传入（否则 JS 侧收不到回调，Task 永不完成）。
            const useCallbackBridge = asyncInner !== null
                && !this.hasMatchingPromiseOverload(method.name, params.map(p => p.type), component.methods);

            // 映射参数
            const mappedParams: ParameterInfo[] = params.map(p => ({
                name: p.name,
                type: this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(p.type))),
                optional: p.optional,
                defaultValue: p.defaultValue
            }));

            // 返回类型
            let retType: string;
            if (asyncInner !== null) {
                const inner = this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(asyncInner)));
                retType = inner === 'void' ? 'Task' : `Task<${inner}>`;
            } else {
                retType = this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(method.returnType)));
            }

            const pascal = withAsyncSuffix(toPascalCase(method.name), retType);
            const sigKey = `${pascal}(${mappedParams.map(p => p.type).join(',')})`;
            if (generatedSignatures.has(sigKey)) continue;
            generatedSignatures.add(sigKey);

            members.push({
                rawName: method.name,
                pascalName: pascal,
                isProperty: false,
                retType,
                params: mappedParams,
                useCallbackBridge: useCallbackBridge || undefined
            });
        }
        return members;
    }

    /**
     * 判断同名方法是否存在"剥掉回调后参数一致"的 Promise 重载
     * （OpenHarmony 原生实现按末参是否为函数在 callback/promise 形态间自适应）。
     */
    private hasMatchingPromiseOverload(name: string, strippedTypes: string[], all: MethodInfo[]): boolean {
        const key = strippedTypes.map(t => TypeMapper.cleanOptional(t)).join(',');
        return all.some(m =>
            m.name === name
            && /^Promise</.test(m.returnType)
            && m.parameters.map(p => TypeMapper.cleanOptional(p.type)).join(',') === key);
    }

    /** 'object'/'null' 等 C# 不可用映射归一为 IntPtr 句柄 */
    private normalize(mapped: string): string {
        if (mapped === 'object' || mapped === 'null' || mapped === 'dynamic') return 'IntPtr';
        return mapped;
    }

    /**
     * C# 可选参数必须在必需参数之后，TS 允许穿插。
     * 可选参数后面还有必需参数时，将该可选参数降级为必需（调用方需传值）。
     */
    private demoteOptionals(params: ParameterInfo[]): ParameterInfo[] {
        return params.map((p, i) => {
            if (!p.optional) return p;
            const hasRequiredAfter = params.slice(i + 1).some(q => !q.optional);
            return hasRequiredAfter ? { ...p, optional: false } : p;
        });
    }

    // ---------- 分类收敛 / 可达性 ----------

    private convergeRecordUsage(members: EmitMember[], extraReturnStrings: string[] = []): void {
        for (let round = 0; round < 5; round++) {
            let changed = false;
            // 返回位置 = 模块方法返回类型 + 全部 wrapper 方法/属性的取值类型 + 事件回调参数类型
            const returnStrings = [
                ...members.filter(m => !m.isProperty).map(m => m.retType),
                ...extraReturnStrings,
            ];
            for (const spec of this.specs) {
                if (spec.kind !== 'wrapper' || !this.specByCsharp.has(spec.csharpName)) continue;
                for (const m of this.wrapperMembers(spec).methods) {
                    returnStrings.push(TypeMapper.mapType(TypeMapper.cleanOptional(m.returnType)));
                }
                for (const p of this.wrapperMembers(spec).properties) {
                    returnStrings.push(TypeMapper.mapType(TypeMapper.cleanOptional(p.type)));
                }
            }
            for (const name of this.collectReferencedNames(returnStrings)) {
                const spec = this.specByCsharp.get(name);
                if (spec && spec.kind === 'record') {
                    spec.kind = 'wrapper';
                    changed = true;
                }
            }
            if (!changed) return;
        }
    }

    /** 从类型字符串提取已登记的实例类型 C# 名 */
    private collectReferencedNames(typeStrings: string[]): Set<string> {
        const result = new Set<string>();
        for (const s of typeStrings) {
            const idents = s.match(/[A-Za-z_]\w*/g) ?? [];
            for (const id of idents) {
                if (this.specByCsharp.has(id)) result.add(id);
            }
        }
        return result;
    }

    private collectReachable(members: EmitMember[], extraSeeds: string[] = []): Set<string> {
        const reachable = new Set<string>();
        const queue: string[] = [];

        const add = (names: Set<string>) => {
            for (const n of names) {
                if (!reachable.has(n)) {
                    reachable.add(n);
                    queue.push(n);
                }
            }
        };

        const memberTypeStrings = (m: EmitMember): string[] =>
            [m.retType, ...m.params.map(p => p.type)];

        add(this.collectReferencedNames(members.flatMap(memberTypeStrings)));
        add(this.collectReferencedNames(extraSeeds));

        // 种子补充：带构造函数的嵌套类是实例化入口（如 PhotoViewPicker），即使模块无成员也生成
        for (const spec of this.specs) {
            if (spec.kind === 'wrapper' && spec.cls && spec.cls.constructors.length > 0
                && this.specByCsharp.has(spec.csharpName) && !reachable.has(spec.csharpName)) {
                reachable.add(spec.csharpName);
                queue.push(spec.csharpName);
            }
        }

        while (queue.length > 0) {
            const name = queue.shift()!;
            const spec = this.specByCsharp.get(name);
            if (!spec) continue;
            const strings: string[] = [];
            if (spec.kind === 'wrapper') {
                for (const m of this.wrapperMembers(spec).methods) {
                    strings.push(TypeMapper.mapType(TypeMapper.cleanOptional(m.returnType)));
                    for (const p of m.parameters) {
                        strings.push(TypeMapper.mapType(TypeMapper.cleanOptional(p.type)));
                    }
                }
                for (const p of this.wrapperMembers(spec).properties) {
                    strings.push(TypeMapper.mapType(TypeMapper.cleanOptional(p.type)));
                }
                // 构造函数参数（输入位，如 intl.Locale(locale, options?: LocaleOptions)）：
                // 引用的 record 必须可达才会生成
                if (spec.cls) {
                    for (const ctor of spec.cls.constructors) {
                        for (const p of ctor.parameters) {
                            strings.push(TypeMapper.mapType(TypeMapper.cleanOptional(p.type)));
                        }
                    }
                }
            } else {
                // record 的属性类型也必须可达（嵌套 record/wrapper/枚举要生成）
                const props = spec.iface ? this.mergedProperties(spec.iface) : (spec.cls?.properties ?? []);
                for (const p of props) {
                    strings.push(TypeMapper.mapType(TypeMapper.cleanOptional(p.type)));
                }
            }
            add(this.collectReferencedNames(strings));
        }
        return reachable;
    }

    /** 合并继承链后的 wrapper 成员 */
    private wrapperMembers(spec: TypeSpec): { properties: { name: string; type: string; optional: boolean; readonly: boolean }[]; methods: MethodInfo[] } {
        const properties: { name: string; type: string; optional: boolean; readonly: boolean }[] = [];
        const methods: MethodInfo[] = [];
        const seenProps = new Set<string>();
        const seenMethods = new Set<string>();
        const visited = new Set<string>();

        const visit = (s: TypeSpec) => {
            if (visited.has(s.tsName)) return;
            visited.add(s.tsName);
            const extendsList = s.iface?.extends ?? (s.cls?.extends ? [s.cls.extends] : []);
            for (const baseName of extendsList) {
                const base = this.specs.find(x => x.tsName === baseName);
                if (base) visit(base);
            }
            const props = s.iface ? s.iface.properties : (s.cls?.properties ?? []);
            for (const p of props) {
                if (!isValidCSharpName(p.name)) continue;
                if (!seenProps.has(p.name)) {
                    seenProps.add(p.name);
                    properties.push(p);
                }
            }
            const ms = s.iface ? s.iface.methods : (s.cls?.methods ?? []);
            for (const m of ms) {
                if (!isValidCSharpName(m.name)) continue; // [Symbol.iterator] 等非标识符成员无法映射
                // 按 名字+参数类型 去重（同名重载必须保留，否则 on 的 30 个字面量重载会被折叠成 1 个）
                const key = `${m.name}(${m.parameters.map(p => p.type).join(',')})`;
                if (!seenMethods.has(key)) {
                    seenMethods.add(key);
                    methods.push(m);
                }
            }
        };
        visit(spec);
        return { properties, methods };
    }

    private mergedProperties(iface: InterfaceInfo): { name: string; type: string; optional: boolean; readonly: boolean }[] {
        return this.wrapperMembers({ tsName: iface.name, csharpName: iface.name, kind: 'wrapper', iface }).properties;
    }

    // ---------- 生成 ----------

    private emitHeader(lines: string[], moduleInfo: { module: string; className: string; local: string }, permissions: string[], needsTask: boolean, needsLinq: boolean): void {
        lines.push('// <auto-generated>');
        lines.push(`// 由 apiGenerator.ts 生成（@ohos.* namespace 绑定路线）。`);
        lines.push('// 生成器输入：HarmonyOS SDK .d.ts → astParser → ComponentInfo → 本生成器');
        lines.push('// </auto-generated>');
        lines.push('#nullable enable');
        lines.push('using System;');
        lines.push('using System.Linq;');
        lines.push('using System.Runtime.InteropServices;');
        lines.push('using System.Text;');
        lines.push('using System.Threading.Tasks;');
        lines.push('using HarmonyOS.Bindings.Runtime;');
        lines.push('using HarmonyOS.ArkUI;');  // 枚举生成在 HarmonyOS.ArkUI 命名空间
        void needsTask;
        void needsLinq;
        lines.push('');
        lines.push('namespace HarmonyOS.Bindings.Api;');
        lines.push('');

        if (permissions.length > 0) {
            lines.push('/// <summary>');
            lines.push(`/// ${moduleInfo.className} 绑定（@ohos.${moduleInfo.local}）。`);
            lines.push(`/// 所需权限：${permissions.join(', ')}`);
            lines.push('/// </summary>');
        } else {
            lines.push('/// <summary>');
            lines.push(`/// ${moduleInfo.className} 绑定（@ohos.${moduleInfo.local}）。`);
            lines.push('/// </summary>');
        }
    }

    private emitModuleClass(
        lines: string[],
        component: ComponentInfo,
        members: EmitMember[],
        moduleInfo: { module: string; className: string; local: string },
        events: EventEmitInfo[]
    ): void {
        lines.push(`public static unsafe partial class ${moduleInfo.className}`);
        lines.push('{');
        lines.push(`    private const string ModuleName = "${moduleInfo.module}";`);
        lines.push('');
        lines.push('    private static NapiReference? _moduleRef;');
        lines.push('    private static bool _loadAttempted;');
        lines.push('');
        lines.push('    /// <summary>懒加载的 @ohos 模块对象（internal：同文件包装类的构造函数需要）</summary>');
        lines.push('    internal static IntPtr Module');

        this.emitModuleLoader(lines, moduleInfo);

        // UTF8 名称常量
        const u8Names = new Set<string>();
        for (const m of members) u8Names.add(m.rawName);
        if (u8Names.size > 0) {
            for (const name of u8Names) {
                lines.push(`    private static ReadOnlySpan<byte> _${name} => "${name}"u8;`);
            }
            lines.push('');
        }

        for (const m of members) {
            if (m.isProperty) {
                this.emitStaticProperty(lines, m);
            } else {
                this.emitStaticMethod(lines, m);
            }
        }
        this.emitEventMembers(lines, events, false,
            new Set(members.map(m => m.pascalName)),
            new Set(members.map(m => `_${m.rawName}`)),
            new Set(members.filter(m => !m.isProperty).map(m => `${m.pascalName}(${m.params.map(p => p.type).join(',')})`)));
        lines.push('}');
        lines.push('');
        void component;
    }

    private emitModuleLoader(lines: string[], moduleInfo: { module: string; className: string; local: string }): void {
        lines.push('    {');
        lines.push('        get');
        lines.push('        {');
        lines.push('            if (_moduleRef != null) return _moduleRef.Value;');
        lines.push('            if (_loadAttempted)');
        lines.push('                throw new InvalidOperationException($"{ModuleName} module failed to load (previous attempt)");');
        lines.push('            _loadAttempted = true;');
        lines.push('');
        lines.push('            var env = NapiEnv.Current;');
        lines.push('            NativeNodeApi.napi_open_handle_scope(env, out var scope).ThrowIfFailed();');
        lines.push('            try');
        lines.push('            {');
        lines.push('                foreach (var name in new[] { "=" + ModuleName, ModuleName })');
        lines.push('                {');
        lines.push('                    var utf8 = System.Text.Encoding.UTF8.GetBytes(name);');
        lines.push('                    fixed (byte* p = utf8)');
        lines.push('                    {');
        lines.push('                        var status = NativeNodeApi.napi_load_module(env, p, out var module);');
        lines.push('                        if (status == NativeNodeApi.napi_status.napi_ok && module != IntPtr.Zero)');
        lines.push('                        {');
        lines.push('                            _moduleRef = new NapiReference(module);');
        lines.push('                            break;');
        lines.push('                        }');
        lines.push('                        NativeNodeApi.napi_get_and_clear_last_exception(env, out _);');
        lines.push('                    }');
        lines.push('                }');
        lines.push('            }');
        lines.push('            finally');
        lines.push('            {');
        lines.push('                NativeNodeApi.napi_close_handle_scope(env, scope).ThrowIfFailed();');
        lines.push('            }');
        lines.push('');
        lines.push('            if (_moduleRef == null)');
        lines.push('                throw new InvalidOperationException(');
        lines.push('                    $"failed to load {ModuleName} via napi_load_module (tried with and without \'=\' prefix)");');
        lines.push('');
        lines.push(`            HiLog.Info("HarmonyHost", $"[${moduleInfo.local}] module loaded via napi_load_module");`);
        lines.push('            return _moduleRef.Value;');
        lines.push('        }');
        lines.push('    }');
        lines.push('');
    }

    private emitStaticProperty(lines: string[], m: EmitMember): void {
        const { type, expr } = this.getterExpr(m.retType, `Module`, `_${m.rawName}`);
        lines.push('    /// <summary>');
        lines.push(`    /// ${m.rawName}`);
        lines.push('    /// </summary>');
        lines.push(`    public static ${type} ${m.pascalName} => ${expr};`);
        lines.push('');
    }

    private emitStaticMethod(lines: string[], m: EmitMember): void {
        const paramStr = m.params.map(p => this.formatParameter(p)).join(', ');
        const paramNames = m.params.map(p => TypeMapper.escapeCSharpKeyword(p.name)).join(', ');
        const callArgs = paramNames ? `, ${paramNames}` : '';

        lines.push('    /// <summary>');
        lines.push(`    /// ${m.rawName}`);
        lines.push('    /// </summary>');
        lines.push(`    public static ${m.retType} ${m.pascalName}(${paramStr})`);
        lines.push('    {');
        const expr = this.callExpr(m.retType, 'Module', `_${m.rawName}`, callArgs, false, !!m.useCallbackBridge);
        lines.push(m.retType === 'void' ? `        ${expr};` : `        return ${expr};`);
        lines.push('    }');
        lines.push('');
    }

    // ---------- 实例类型（包装类 / record）生成 ----------

    private emitWrapper(lines: string[], spec: TypeSpec): void {
        const { properties, methods } = this.wrapperMembers(spec);
        const isClass = spec.cls !== undefined;

        lines.push('/// <summary>');
        lines.push(`/// ${spec.tsName} 实例包装（@ohos 命名空间内嵌套${isClass ? '类' : '接口'}）。`);
        lines.push('/// 由 JsObject 持有 napi 强引用；Dispose 仅释放引用，JS 对象由 ArkTS GC 管理。');
        lines.push('/// </summary>');
        lines.push(`public sealed partial class ${spec.csharpName} : JsObject`);
        lines.push('{');
        lines.push(`    public ${spec.csharpName}(IntPtr handle) : base(handle) { }`);

        // 嵌套类带构造函数：经模块对象 new 实例
        // 注意：(IntPtr) 签名保留给句柄构造函数，映射成该签名的 JS 构造函数跳过
        if (isClass && spec.cls!.constructors.length > 0) {
            lines.push('');
            lines.push(`    private static ReadOnlySpan<byte> _${spec.tsName} => "${spec.tsName}"u8;`);
            const generatedCtorSigs = new Set<string>(['IntPtr']);
            for (const ctor of spec.cls!.constructors) {
                const mappedParams: ParameterInfo[] = this.demoteOptionals(ctor.parameters).map(p => ({
                    name: p.name,
                    type: this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(p.type))),
                    optional: p.optional,
                    defaultValue: p.defaultValue
                }));
                const ctorSig = mappedParams.map(p => p.type).join(',');
                if (generatedCtorSigs.has(ctorSig)) continue;
                generatedCtorSigs.add(ctorSig);
                const paramStr = mappedParams.map(p => this.formatParameter(p)).join(', ');
                const paramNames = mappedParams.map(p => TypeMapper.escapeCSharpKeyword(p.name)).join(', ');
                const ctorArgs = paramNames ? `, ${paramNames}` : '';
                lines.push('');
                lines.push(`    public ${spec.csharpName}(${paramStr})`);
                lines.push(`        : this(NodeApi.CreateInstance(${this.moduleClassName}.Module, _${spec.tsName}${ctorArgs})) { }`);
            }
        }

        // UTF8 名称常量
        const u8Names = new Set<string>();
        for (const p of properties) u8Names.add(p.name);
        for (const m of methods) u8Names.add(m.name);
        u8Names.delete(spec.tsName);
        for (const name of u8Names) {
            lines.push(`    private static ReadOnlySpan<byte> _${name} => "${name}"u8;`);
        }

        // 属性 getter
        for (const p of properties) {
            const mapped = this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(p.type)));
            if (mapped === 'void') continue;
            const pascal = toPascalCase(p.name);
            if (pascal === spec.csharpName) continue; // 与类名冲突的成员降级跳过
            this.emitInstanceProperty(lines, pascal, p, mapped);
        }

        // 实例方法（重载按映射后签名去重：旧 Promise API 与新 *Async API 可能映射到同一 C# 签名）
        const generatedSigs = new Set<string>();
        for (const m of methods) {
            if (m.name === '__call__') continue;
            // 事件函数的回调参数退回 IntPtr（与 mapMembers 同规则），类型化 Action 重载由事件生成器产出
            let rawParams = m.parameters;
            if (m.eventMeta) {
                const cbIdx = rawParams.findIndex(p => /^(?:Async)?Callback?</.test(p.type) || p.name === 'callback');
                if (cbIdx >= 0) rawParams = rawParams.map((p, i) => i === cbIdx ? { ...p, type: 'IntPtr', optional: false } : p);
            }
            // 命名 AsyncCallback 参数：剥掉并接为 Task（双形态判定与 mapMembers 同规则）
            let asyncInner: string | null = m.asyncResultType !== undefined ? m.asyncResultType : null;
            const namedCb = rawParams.find(p => /^AsyncCallback<(.+)>$/.test(p.type));
            if (namedCb) {
                asyncInner = /^AsyncCallback<(.+)>$/.exec(namedCb.type)![1];
                rawParams = rawParams.filter(p => p !== namedCb);
            }
            const useCallbackBridge = asyncInner !== null
                && !this.hasMatchingPromiseOverload(m.name, rawParams.map(p => p.type), methods);
            const mappedParams = this.demoteOptionals(rawParams).map(p => ({
                name: p.name,
                type: this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(p.type))),
                optional: p.optional,
                defaultValue: p.defaultValue
            }));
            const retType = asyncInner !== null
                ? (() => {
                    const inner = this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(asyncInner!)));
                    return inner === 'void' ? 'Task' : `Task<${inner}>`;
                })()
                : this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(m.returnType)));
            const pascal = withAsyncSuffix(toPascalCase(m.name), retType);
            if (pascal === spec.csharpName) continue;
            const paramStr = mappedParams.map(p => this.formatParameter(p)).join(', ');
            const paramNames = mappedParams.map(p => TypeMapper.escapeCSharpKeyword(p.name)).join(', ');
            const callArgs = paramNames ? `, ${paramNames}` : '';

            const sigKey = `${pascal}(${mappedParams.map(p => p.type).join(',')})`;
            if (generatedSigs.has(sigKey)) continue;
            generatedSigs.add(sigKey);

            lines.push('    /// <summary>');
            lines.push(`    /// ${m.name}`);
            lines.push('    /// </summary>');
            lines.push(`    public ${retType} ${pascal}(${paramStr})`);
            lines.push('    {');
            const expr = this.callExpr(retType, 'this.Handle', `_${m.name}`, callArgs, true, useCallbackBridge);
            lines.push(retType === 'void' ? `        ${expr};` : `        return ${expr};`);
            lines.push('    }');
            lines.push('');
        }

        // 实例事件（window/connection/pasteboard 等包装类上的 on/off/once）
        const wrapperEvents = this.buildEventInfos(methods);
        const takenNames = new Set<string>([
            ...properties.map(p => toPascalCase(p.name)),
            ...methods.filter(m => m.name !== '__call__').map(m => toPascalCase(m.name)),
        ]);
        this.emitEventMembers(lines, wrapperEvents, true, takenNames,
            new Set([...u8Names].map(n => `_${n}`)), generatedSigs);

        lines.push('}');
        lines.push('');
    }

    private emitInstanceProperty(lines: string[], pascal: string, p: { name: string; type: string; optional: boolean }, mapped: string): void {
        const u8 = `_${p.name}`;
        lines.push('    /// <summary>');
        lines.push(`    /// ${p.name}`);
        lines.push('    /// </summary>');
        if (mapped === 'IntPtr') {
            lines.push(`    public IntPtr ${pascal} => GetPropertyRaw(${u8});`);
            lines.push('');
            return;
        }
        if (p.optional && (PRIMITIVE_TYPES.has(mapped) || this.enumNames.has(mapped))) {
            // 可选值类型属性：undefined 语义降级为默认值，避免引入 UndefinedValue 概念
            lines.push(`    public ${mapped}? ${pascal} => (${mapped}?)${this.primitiveGetterExpr(mapped, `GetPropertyRaw(${u8})`)};`);
            lines.push('');
            return;
        }
        const wrapperSpec = this.specByCsharp.get(mapped);
        if (p.optional && wrapperSpec) {
            // 可选包装属性：undefined → null
            const raw = `GetPropertyRaw(${u8})`;
            lines.push(`    public ${wrapperSpec.csharpName}? ${pascal} => ${raw} == IntPtr.Zero ? null : new ${wrapperSpec.csharpName}(${raw});`);
            lines.push('');
            return;
        }
        const { type, expr } = this.getterExpr(mapped, 'this', u8, true);
        lines.push(`    public ${type} ${pascal} => ${expr};`);
        lines.push('');
    }

    private emitRecord(lines: string[], spec: TypeSpec): void {
        const props = spec.iface ? this.mergedProperties(spec.iface) : (spec.cls?.properties ?? []);
        const mappedProps = props.map(p => ({
            name: p.name,
            pascal: toPascalCase(p.name),
            type: this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(p.type))),
            optional: p.optional
        })).filter(p => p.type !== 'void');

        lines.push('/// <summary>');
        lines.push(`/// ${spec.tsName}（@ohos 命名空间内嵌套纯数据接口，入参对象）。`);
        lines.push('/// </summary>');
        const params = mappedProps.map((p, i) => {
            // record 位置参数同样要求可选参数在必需参数之后（CS1737）
            const hasRequiredAfter = mappedProps.slice(i + 1).some(q => !q.optional);
            const nullableMark = p.optional ? '?' : '';
            const defaultVal = p.optional && !hasRequiredAfter ? ' = null' : '';
            return `    ${p.type}${nullableMark} ${p.pascal}${defaultVal}`;
        }).join(',\n');
        lines.push(`public sealed record ${spec.csharpName}(`);
        lines.push(params);
        lines.push(') : INapiRecord');
        lines.push('{');
        lines.push('    void INapiRecord.WriteTo(NativeNodeApi.napi_env env, NativeNodeApi.napi_value obj)');
        lines.push('    {');
        for (const p of mappedProps) {
            const utf8Var = this.encodePropVar(p.pascal);
            lines.push(`        var ${utf8Var} = System.Text.Encoding.UTF8.GetBytes("${p.name}");`);
            lines.push(`        var ${utf8Var}V = NativeValue.From(${p.pascal});`);
            lines.push(`        if (${utf8Var}V != IntPtr.Zero)`);
            lines.push(`            NativeNodeApi.napi_set_named_property(env, obj, ${utf8Var}, ${utf8Var}V);`);
        }
        lines.push('    }');
        lines.push('}');
        lines.push('');
    }

    private encodePropVar(pascal: string): string {
        return `_${pascal.charAt(0).toLowerCase()}${pascal.slice(1)}`;
    }

    // ---------- 表达式构造 ----------

    /**
     * 方法调用表达式。instance=true 时目标是 JsObject 派生类自身（this.Handle + 受保护助手），
     * 否则为模块静态类（NodeApi + Module）。
     */
    private callExpr(retType: string, target: string, u8Var: string, callArgs: string, instance: boolean = false, useCallbackBridge: boolean = false): string {
        // 实例模式：JsObject 受保护助手自带句柄，只接收方法名；
        // 静态模式：NodeApi.* + Module 句柄。
        const call = instance
            ? (fn: string, extra: string) => `${fn}(${u8Var}${extra}${callArgs})`
            : (fn: string, extra: string) => `NodeApi.${fn}(${target}, ${u8Var}${extra}${callArgs})`;

        if (retType === 'void') return call('CallMethodVoid', '');
        if (retType === 'Task') {
            return useCallbackBridge ? call('CallMethodAsyncCallbackVoid', '') : call('CallMethodAsyncVoid', '');
        }

        const taskMatch = /^Task<(.+)>$/.exec(retType);
        const inner = taskMatch ? taskMatch[1] : retType;
        const isTask = !!taskMatch;
        const method = isTask
            ? (useCallbackBridge ? 'CallMethodAsyncCallback' : 'CallMethodAsync')
            : 'CallMethod';

        const wrapper = this.specByCsharp.get(inner);
        if (wrapper) {
            // 包装类：调用点显式工厂（AOT 安全，无反射）
            return call(method, `, static h => new ${wrapper.csharpName}(h)`);
        }
        if (inner.startsWith('JsMap<')) {
            return call(method, `, h => ${this.jsMapCtorExpr(inner, 'h')}`);
        }
        const arrMatch = /^([\w.:]+)\[\]$/.exec(inner);
        if (arrMatch) {
            const elem = arrMatch[1];
            const elemWrapper = this.specByCsharp.get(elem);
            const conv = elemWrapper
                ? `static e => new ${elemWrapper.csharpName}(e)`
                : `static e => ValueConverter.Convert<${elem}>(e)`;
            return call(method, `, h => ValueConverter.ConvertArray(h, ${conv})`);
        }
        // 基元/枚举：Promise 通道走泛型 ConvertResult；bridge 通道显式传 null（ValueConverter 运行时路径）
        return call(`${method}<${inner}>`, useCallbackBridge ? ', null' : '');
    }

    /** 属性 getter 表达式，返回（类型, 表达式） */
    private getterExpr(mapped: string, target: string, u8Var: string, instance: boolean = false): { type: string; expr: string } {
        const raw = instance ? `GetPropertyRaw(${u8Var})` : `NodeApi.GetProperty(${target}, ${u8Var})`;
        if (PRIMITIVE_TYPES.has(mapped) || this.enumNames.has(mapped)) {
            return { type: mapped, expr: this.primitiveGetterExpr(mapped, raw) };
        }
        const wrapper = this.specByCsharp.get(mapped);
        if (wrapper) {
            return { type: wrapper.csharpName, expr: `new ${wrapper.csharpName}(${raw})` };
        }
        if (mapped.startsWith('JsMap<')) {
            return { type: mapped, expr: this.jsMapCtorExpr(mapped, raw) };
        }
        const arrMatch = /^([\w.:]+)\[\]$/.exec(mapped);
        if (arrMatch) {
            const elem = arrMatch[1];
            const elemWrapper = this.specByCsharp.get(elem);
            const conv = elemWrapper
                ? `static e => new ${elemWrapper.csharpName}(e)`
                : `static e => ValueConverter.Convert<${elem}>(e)`;
            return { type: mapped, expr: `ValueConverter.ConvertArray(${raw}, ${conv})` };
        }
        // 兜底：原始句柄
        return { type: 'IntPtr', expr: raw };
    }

    /** 基元/枚举类型的取值表达式 */
    private primitiveGetterExpr(mapped: string, rawExpr: string): string {
        switch (mapped) {
            case 'bool': return `NativeValue.ToBool(${rawExpr})`;
            case 'double': return `NativeValue.ToDouble(${rawExpr})`;
            case 'float': return `(float)NativeValue.ToDouble(${rawExpr})`;
            case 'int': return `NativeValue.ToInt(${rawExpr})`;
            case 'uint': return `NativeValue.ToUInt(${rawExpr})`;
            case 'long': return `NativeValue.ToLong(${rawExpr})`;
            case 'byte': return `NativeValue.ToByte(${rawExpr})`;
            case 'string': return `NativeValue.ToString(${rawExpr}) ?? string.Empty`;
            default:
                if (this.enumNames.has(mapped)) return `(${mapped})NativeValue.ToInt(${rawExpr})`;
                return rawExpr;
        }
    }

    private formatParameter(param: ParameterInfo): string {
        const optionalMark = param.optional ? '?' : '';
        const defaultVal = param.defaultValue ? ` = ${param.defaultValue}` :
            (param.optional ? ' = null' : '');
        return `${param.type}${optionalMark} ${TypeMapper.escapeCSharpKeyword(param.name)}${defaultVal}`;
    }

    // ---------- 事件模型（类型化 On/Off/Once + .NET event） ----------

    private buildEventInfos(methods: MethodInfo[]): EventEmitInfo[] {
        const infos: EventEmitInfo[] = [];
        for (const m of methods) {
            const meta = m.eventMeta;
            if (!meta || !EVENT_FN.has(m.name)) continue;

            // 类型参数（literal/plain → string；enum → 枚举 fqn）
            let typeParamCs = 'string';
            const literals: EventEmitInfo['literals'] = [];
            if (meta.kind === 'enum' && meta.enumTypeName) {
                const fqn = `global::HarmonyOS.ArkUI.${meta.enumTypeName}`;
                typeParamCs = this.enumNames.has(fqn) ? fqn : meta.enumTypeName;
            }
            for (const raw of meta.literals) {
                if (meta.kind === 'enum') {
                    const dot = raw.lastIndexOf('.');
                    const memberRaw = dot >= 0 ? raw.substring(dot + 1) : raw;
                    literals.push({ enumFqn: typeParamCs, memberCs: toPascalCase(memberRaw) });
                } else if (meta.kind === 'literal') {
                    literals.push({ value: raw });
                }
            }

            // 其余参数：跳过首参（type）与 callback 参数（具名回调类型如 CameraManager.OnCallback 按参数名兜底）
            const callbackIdx = m.parameters.findIndex(p =>
                /^(?:Async)?Callback?</.test(p.type) || p.name === 'callback');
            const extraParams = m.parameters
                .filter((p, i) => i !== 0 && i !== callbackIdx)
                .map(p => ({
                    name: p.name,
                    type: this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(p.type))),
                    optional: p.optional,
                    defaultValue: p.defaultValue
                }));

            infos.push({
                fnName: m.name as EventEmitInfo['fnName'],
                kind: meta.kind,
                literals,
                typeParamCs,
                // Callback<void> → System.Action；多参中的 void 位丢弃（如 Callback<T, U = void>）。
                // Promise 载荷（Callback<Promise<bool>>）无法在同步适配器里桥接，退回 IntPtr 句柄
                callbackArgs: meta.callbackArgs
                    .map(t => this.normalize(TypeMapper.mapType(TypeMapper.cleanOptional(t))))
                    .map(t => t.startsWith('Task') ? 'IntPtr' : t)
                    .filter(t => t !== 'void'),
                extraParams,
            });
        }
        return infos;
    }

    /** JsMap 构造表达式：值类型为包装类时带 valueFactory（活视图读取包装值） */
    private jsMapCtorExpr(mapped: string, handleExpr: string): string {
        const valueCs = mapped.slice('JsMap<'.length, mapped.length - 1).split(',')[1]?.trim();
        const factory = valueCs && this.specByCsharp.get(valueCs)
            ? `, static v => new ${valueCs}(v)` : '';
        return `new ${mapped}(${handleExpr}${factory})`;
    }

    /** 单个回调参数的转换表达式（适配器闭包内，napi_value → C#） */
    private convertArgExpr(mapped: string, arg: string): string {
        switch (mapped) {
            case 'bool': return `NativeValue.ToBool(${arg})`;
            case 'double': return `NativeValue.ToDouble(${arg})`;
            case 'long': return `NativeValue.ToLong(${arg})`;
            case 'int': return `NativeValue.ToInt(${arg})`;
            case 'uint': return `NativeValue.ToUInt(${arg})`;
            case 'byte': return `NativeValue.ToByte(${arg})`;
            case 'string': return `(NativeValue.ToString(${arg}) ?? string.Empty)`;
            case 'byte[]': return `NativeValue.ToByteArray(${arg})`;
            case 'JsBigInt': return `NativeValue.ToBigInt(${arg})`;
            case 'void': return 'default';
        }
        if (this.enumNames.has(mapped)) return `(${mapped})NativeValue.ToInt(${arg})`;
        const wrapper = this.specByCsharp.get(mapped);
        if (wrapper) return `new ${wrapper.csharpName}(${arg})`;
        if (mapped.startsWith('JsMap<')) return this.jsMapCtorExpr(mapped, arg);
        // 数组载荷（如 Callback<Array<Location>>）：逐元素转换
        const arrMatch = /^([\w.:]+)\[\]$/.exec(mapped);
        if (arrMatch) {
            const elem = arrMatch[1];
            const elemWrapper = this.specByCsharp.get(elem);
            const conv = elemWrapper
                ? `static e => new ${elemWrapper.csharpName}(e)`
                : `static e => ValueConverter.Convert<${elem}>(e)`;
            return `ValueConverter.ConvertArray(${arg}, ${conv})`;
        }
        // 其余类型透传句柄
        return arg;
    }

    /** 适配器闭包：args => userFn(conv(args[0]), conv(args[1]), ...) */
    private adapterExpr(callbackArgs: string[], invoke: string): string {
        if (callbackArgs.length === 0) return `args => ${invoke}()`;
        const convs = callbackArgs.map((t, i) => this.convertArgExpr(t, `args[${i}]`));
        return `args => ${invoke}(${convs.join(', ')})`;
    }

    private actionType(callbackArgs: string[]): string {
        return callbackArgs.length === 0
            ? 'System.Action'
            : `System.Action<${callbackArgs.join(', ')}>`;
    }

    /**
     * 生成类型化 On/Off/Once 方法与 .NET event 访问器（模块类 static / 包装类 instance）。
     * on/off 的 JS 函数实例经 EventListenerRegistry 配对，保证 off 传入同一 JS 函数。
     */
    private emitEventMembers(lines: string[], infos: EventEmitInfo[], instance: boolean, takenNames: Set<string> = new Set(), existingU8Names: Set<string> = new Set(), existingSigs: Set<string> = new Set()): void {
        if (infos.length === 0) return;

        // 类里可能只有 on 没有 off（如 NetConnection）——补齐缺失的方法名 u8 常量。
        // on/once 的事件访问器 remove 分支同样引用 _off。
        const ensuredU8 = new Set<string>();
        const ensure = (fn: string) => {
            if (!ensuredU8.has(fn) && !existingU8Names.has(`_${fn}`)) {
                ensuredU8.add(fn);
                lines.push(`    private static ReadOnlySpan<byte> _${fn} => "${fn}"u8;`);
            }
        };
        for (const info of infos) ensure(info.fnName);
        if (infos.some(i => i.fnName !== 'off' && i.literals.length > 0)) ensure('off');
        if (ensuredU8.size > 0) lines.push('');

        lines.push(instance
            ? '    private readonly EventListenerRegistry _eventListeners = new();'
            : '    private static readonly EventListenerRegistry _eventListeners = new();');
        lines.push('');

        const target = instance ? 'Handle' : 'Module';
        const call = (fnU8: string, args: string) =>
            `NodeApi.CallMethodVoid(${target}, ${fnU8}${args ? `, ${args}` : ''})`;

        // 类型化 On/Off/Once（按签名去重；与普通成员发射的签名也要去重——CS0111 不看参数名）
        const seen = new Set<string>();
        let offAllEmitted = false;
        for (const info of infos) {
            const fn = info.fnName;
            const fnPascal = toPascalCase(fn);
            const action = this.actionType(info.callbackArgs);
            // extra 参数与事件键/callback 撞名时改名（如 relationalStore off 的第二个 type 参数）
            const extraParams = info.extraParams.map(p => {
                const name = TypeMapper.escapeCSharpKeyword(p.name);
                return (name === 'type' || name === 'callback') ? { ...p, name: `${name}2` } : p;
            });
            const extra = extraParams.length > 0
                ? ', ' + extraParams.map(p => this.formatParameter(p)).join(', ') : '';
            const extraArgs = extraParams.length > 0
                ? ', ' + extraParams.map(p => TypeMapper.escapeCSharpKeyword(p.name)).join(', ') : '';
            const mod = instance ? '' : 'static ';
            const extraTypes = extraParams.map(p => p.type).join(',');
            const sigKey = `${fn}|${info.typeParamCs}|${action}|${extraTypes}`;
            if (seen.has(sigKey)) continue;
            seen.add(sigKey);
            const csSig = `${fnPascal}(${[info.typeParamCs, action, ...extraParams.map(p => p.type)].filter(t => t !== '').join(',')})`;
            if (existingSigs.has(csSig)) continue;

            if (fn === 'off') {
                // Off(type) 移除该事件全部 JS 监听（与回调形状无关，只发射一次；
                // 若普通成员已发射同签名方法（如 off(type: string) 无回调重载）则跳过）
                if (!offAllEmitted) {
                    offAllEmitted = true;
                    if (!existingSigs.has(`${fnPascal}(${info.typeParamCs})`)) {
                        lines.push('    /// <summary>');
                        lines.push(`    /// off(type)：移除该事件类型的全部回调`);
                        lines.push('    /// </summary>');
                        lines.push(`    public ${mod}void Off(${info.typeParamCs} type)`);
                        lines.push('    {');
                        lines.push(`        ${call('_off', 'type')};`);
                        lines.push('    }');
                        lines.push('');
                    }
                }
                lines.push('    /// <summary>');
                lines.push(`    /// off(type, callback)：解除订阅（按 handler 匹配）`);
                lines.push('    /// </summary>');
                lines.push(`    public ${mod}void Off(${info.typeParamCs} type, ${action} callback${extra})`);
                lines.push('    {');
                lines.push(`        _eventListeners.Remove((type, callback), js => ${call('_off', `type, js${extraArgs}`)});`);
                lines.push('    }');
                lines.push('');
                continue;
            }

            const u8 = fn === 'on' ? '_on' : '_once';
            lines.push('    /// <summary>');
            lines.push(`    /// ${fn}(type, callback) 的类型化重载（回调经共享跳板进入 C#，任意参数类型自动转换）`);
            lines.push('    /// </summary>');
            lines.push(`    public ${instance ? '' : 'static '}void ${toPascalCase(fn)}(${info.typeParamCs} type, ${action} callback${extra})`);
            lines.push('    {');
            lines.push(`        _eventListeners.Add((type, callback),`);
            lines.push(`            ${this.adapterExpr(info.callbackArgs, 'callback')},`);
            lines.push(`            js => ${call(u8, `type, js${extraArgs}`)});`);
            lines.push('    }');
            lines.push('');
        }

        // .NET event 访问器（仅 literal/enum kind，on）
        const seenEvents = new Set<string>();
        for (const info of infos) {
            if (info.fnName !== 'on') continue;
            const action = this.actionType(info.callbackArgs);
            for (const lit of info.literals) {
                let eventName = lit.memberCs ?? toPascalCase(lit.value!);
                // 与既有成员（如 AudioRecorder.Pause() 方法）撞名时追加 Event 后缀
                if (takenNames.has(eventName)) eventName = `${eventName}Event`;
                if (seenEvents.has(eventName)) continue;
                seenEvents.add(eventName);
                takenNames.add(eventName);
                const typeValue = lit.enumFqn ? `${lit.enumFqn}.${lit.memberCs}` : JSON.stringify(lit.value!);
                lines.push('    /// <summary>');
                lines.push(`    /// 监听 ${lit.value ?? lit.memberCs} 事件（对应 on/off）`);
                lines.push('    /// </summary>');
                lines.push(`    public ${instance ? '' : 'static '}event ${action} ${eventName}`);
                lines.push('    {');
                lines.push('        add');
                lines.push('        {');
                lines.push(`            _eventListeners.Add((${typeValue}, value),`);
                lines.push(`                ${this.adapterExpr(info.callbackArgs, 'value')},`);
                lines.push(`                js => ${call('_on', `${typeValue}, js`)});`);
                lines.push('        }');
                lines.push('        remove');
                lines.push('        {');
                lines.push(`            _eventListeners.Remove((${typeValue}, value), js => ${call('_off', `${typeValue}, js`)});`);
                lines.push('        }');
                lines.push('    }');
                lines.push('');
            }
        }
    }
}
