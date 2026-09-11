import { TypeMapping } from './models';

export class TypeMapper {
    private static readonly TYPE_MAP: Record<string, TypeMapping> = {
        'string': { typescript: 'string', csharp: 'string', isNative: false },
        'number': { typescript: 'number', csharp: 'double', isNative: false },
        'boolean': { typescript: 'boolean', csharp: 'bool', isNative: false },
        'void': { typescript: 'void', csharp: 'void', isNative: false },
        // 数值类型扩展
        'long': { typescript: 'long', csharp: 'long', isNative: false },
        'ulong': { typescript: 'ulong', csharp: 'ulong', isNative: false },
        'uint': { typescript: 'uint', csharp: 'uint', isNative: false },
        'byte': { typescript: 'byte', csharp: 'byte', isNative: false },
        'Resource': { typescript: 'Resource', csharp: 'IntPtr', isNative: true },
        'ResourceColor': { typescript: 'ResourceColor', csharp: 'IntPtr', isNative: true },
        'ResourceStr': { typescript: 'ResourceStr', csharp: 'IntPtr', isNative: true },
        'Font': { typescript: 'Font', csharp: 'Font', isNative: true },
        'Length': { typescript: 'Length', csharp: 'IntPtr', isNative: true },
        'Dimension': { typescript: 'Dimension', csharp: 'double', isNative: false },
        'TextAttribute': { typescript: 'TextAttribute', csharp: 'TextAttribute', isNative: false },
        'ColumnAttribute': { typescript: 'ColumnAttribute', csharp: 'ColumnAttribute', isNative: false },
        'RowAttribute': { typescript: 'RowAttribute', csharp: 'RowAttribute', isNative: false },
        'ButtonAttribute': { typescript: 'ButtonAttribute', csharp: 'ButtonAttribute', isNative: false },
        'CommonMethod': { typescript: 'CommonMethod', csharp: 'CommonMethod', isNative: false },
        'Optional': { typescript: 'Optional', csharp: 'IntPtr', isNative: true },
        'Array': { typescript: 'Array', csharp: 'IntPtr[]', isNative: true },
        'Callback': { typescript: 'Callback', csharp: 'IntPtr', isNative: true },
        'Function': { typescript: 'Function', csharp: 'IntPtr', isNative: true },
        // System. 全限定：产物 using HarmonyOS.ArkUI 中可能有同名类型（如枚举 Action）造成 CS0104
        'Action': { typescript: 'Action', csharp: 'System.Action', isNative: false },
        'Func': { typescript: 'Func', csharp: 'System.Func', isNative: false },
        // 特殊类型映射
        'object': { typescript: 'object', csharp: 'object', isNative: false },
        'any': { typescript: 'any', csharp: 'object', isNative: false },
        'unknown': { typescript: 'unknown', csharp: 'object', isNative: false },
        'never': { typescript: 'never', csharp: 'void', isNative: false },
        'null': { typescript: 'null', csharp: 'null', isNative: false },
        'undefined': { typescript: 'undefined', csharp: 'null', isNative: false },
    };

    private static readonly CS_PRIMITIVES = new Set(['string', 'bool', 'double', 'int', 'uint', 'long', 'byte', 'nint', 'IntPtr', 'object', 'void', 'Task', 'ValueTask']);

    static mapType(typescriptType: string): string {
        // 0. 已是合法 C# 基本类型，直接返回（避免二次映射把 double 变 IntPtr）
        if (this.CS_PRIMITIVES.has(typescriptType)) {
            return typescriptType;
        }

        // 1. 处理 Optional<T> 类型
        if (typescriptType.startsWith('Optional<')) {
            return this.mapOptionalType(typescriptType);
        }
        
        // 2. 处理函数类型 ((param: type) => returnType) 或 Action/Func
        if (typescriptType.startsWith('(') && typescriptType.includes('=>')) {
            return this.mapFunctionTypeFromStr(typescriptType);
        }
        
        // 3. 处理联合类型 (string | Resource)
        if (typescriptType.includes('|')) {
            return this.mapUnionType(typescriptType);
        }
        
        // 4. 处理交叉类型 (A & B)
        if (typescriptType.includes('&')) {
            return this.mapIntersectionType(typescriptType);
        }
        
        // 5. 处理数组类型 (Array<T>) 和 (T[])
        //    注意：只匹配以 Array< 开头的类型，避免 Promise<Array<...>> 被误匹配
        if (/^Array<.+>$/.test(typescriptType.trim())) {
            return this.mapArrayType(typescriptType);
        }
        if (/\w+\[\]$/.test(typescriptType)) {
            const baseType = typescriptType.replace('[]', '');
            const mapped = this.mapType(baseType);
            return `${mapped}[]`;
        }
        
        // 6. 处理映射类型 ([K in keyof T]) - 必须在泛型检测之前
        if (typescriptType.includes('[K in keyof') || typescriptType.includes('keyof')) {
            return 'object';
        }

        // 7. 处理泛型类型 (Callback<T1, T2>, Action<T>, Func<T, R>)
        if (typescriptType.includes('<') && typescriptType.includes('>')) {
            return this.mapGenericType(typescriptType);
        }

        // 8. 处理元组类型 (T1, T2, ...) 和 [T1, T2, ...]
        if (typescriptType.startsWith('(') && !typescriptType.includes('=>')) {
            return this.mapTupleType(typescriptType);
        }
        if (typescriptType.startsWith('[') && typescriptType.endsWith(']')) {
            return this.mapTupleType(typescriptType);
        }
        
        // 9. 处理条件类型 (T extends U ? X : Y)
        if (typescriptType.includes('extends') && typescriptType.includes('?') && typescriptType.includes(':')) {
            return this.mapConditionalType(typescriptType);
        }
        
        // 10. 处理 readonly 类型
        if (typescriptType.startsWith('readonly ')) {
            return this.mapType(typescriptType.slice(9));
        }

        // 11. 处理内联对象字面量 { ... } → fallback 为 object
        if (typescriptType.trim().startsWith('{') && typescriptType.trim().endsWith('}')) {
            return 'object';
        }
        
        // 11. Options 类型由 codeGenerator 生成 record，此处不再特殊处理

        // 11.5. 处理字符串字面量类型（'literal'）→ string
        if (/^['"].+['"]$/.test(typescriptType.trim())) {
            return 'string';
        }

        // 12. 直接映射：已知类型返回 C# 类型名，未知类型退回 IntPtr 句柄
        const mapping = this.TYPE_MAP[typescriptType];
        return mapping ? mapping.csharp : 'IntPtr';
    }

    private static mapOptionalType(optionalType: string): string {
        const match = optionalType.match(/Optional<(.+)>/);
        if (match) {
            const innerType = this.mapType(match[1]);
            // 对于值类型，添加可空标记
            if (innerType === 'double' || innerType === 'bool' || innerType === 'int') {
                return `${innerType}?`;
            }
            // 对于引用类型，直接使用
            return innerType;
        }
        return 'IntPtr';
    }

    private static mapFunctionTypeFromStr(funcType: string): string {
        // 解析 "(param1: type1, param2: type2) => returnType"
        const match = funcType.match(/\(([^)]*)\)\s*=>\s*(.+)/);
        if (match) {
            const paramsStr = match[1];
            const returnType = match[2].trim();
            
            const params = paramsStr.split(',').map(p => {
                const parts = p.trim().split(':');
                return parts.length > 1 ? this.mapType(parts[1].trim()) : 'IntPtr';
            });
            
            const mappedReturnType = this.mapType(returnType);
            
            // AsyncCallback 检测：(result: T, err?: Error) => void → Task<T>
            if (mappedReturnType === 'void' && this.isAsyncCallback(funcType)) {
                const resultType = this.extractAsyncCallbackResultType(funcType);
                if (resultType) {
                    const mapped = this.mapType(resultType);
                    return mapped === 'void' ? 'Task' : `Task<${mapped}>`;
                }
            }
            
            if (mappedReturnType === 'void') {
                return params.length > 0 ? `System.Action<${params.join(', ')}>` : 'System.Action';
            } else {
                return params.length > 0 ? `System.Func<${params.join(', ')}, ${mappedReturnType}>` : `System.Func<${mappedReturnType}>`;
            }
        }
        return 'IntPtr';
    }

    private static mapUnionType(unionType: string): string {
        const parts = unionType.split('|').map(p => p.trim());

        // 特殊处理：如果包含 Optional，优先处理
        const optionalPart = parts.find(p => p.startsWith('Optional<'));
        if (optionalPart) {
            return this.mapOptionalType(optionalPart);
        }

        // 特殊处理：如果包含 undefined 或 null，生成可空类型
        const hasUndefined = parts.some(p => p === 'undefined' || p === 'null');
        const nonNullParts = parts.filter(p => p !== 'undefined' && p !== 'null');

        if (nonNullParts.length === 1) {
            const mappedType = this.mapType(nonNullParts[0]);
            if (hasUndefined && (mappedType === 'double' || mappedType === 'bool' || mappedType === 'int')) {
                return `${mappedType}?`;
            }
            return mappedType;
        }

        // 多个分支时，对每个分支递归 mapType，然后选择第一个非 IntPtr 的结果
        for (const part of nonNullParts) {
            const mapped = this.mapType(part);
            if (mapped !== 'IntPtr') {
                return mapped;
            }
        }

        // 所有分支都是 IntPtr，返回 IntPtr
        return 'IntPtr';
    }

    private static mapIntersectionType(intersectionType: string): string {
        const parts = intersectionType.split('&').map(p => p.trim());
        
        // 对于交叉类型，返回第一个类型的映射
        // 在 C# 中可以使用接口实现
        if (parts.length > 0) {
            return this.mapType(parts[0]);
        }
        
        return 'IntPtr';
    }

    private static mapTupleType(tupleType: string): string {
        // 元组类型 fallback 为 object（record 字段不支持内联元组）
        return 'object';
    }

    private static mapConditionalType(conditionalType: string): string {
        // 条件类型无法静态确定，映射为 object（AOT 下不用 dynamic）
        return 'object';
    }

    private static mapArrayType(arrayType: string): string {
        const match = arrayType.match(/Array<(.+)>/);
        if (match) {
            const elementType = this.mapType(match[1]);
            return `${elementType}[]`;
        }
        return 'IntPtr[]';
    }

    private static mapGenericType(genericType: string): string {
        // 匹配泛型类型，如 Callback<string, boolean>, Action<number>, Func<number, boolean>
        // baseType 只取首个 '<' 前的限定标识符——贪婪 ([^(]+)< 会把 Promise<Array<T>> 误切为 baseType='Promise<Array>'
        const match = genericType.match(/^([A-Za-z_]\w*)<(.+)>$/);
        if (match) {
            const baseType = match[1].trim();
            const typeArgsStr = match[2];
            
            // 递归映射每个类型参数
            const typeArgs = this.splitTypeArguments(typeArgsStr).map(t => this.mapType(t.trim()));
            
            // 检查基础类型是否有映射
            const mapping = this.TYPE_MAP[baseType];
            if (mapping) {
                // 对于已映射的泛型类型，需要区分两种情况：
                // 1. 需要递归映射内部参数的类型（如 Action<T>, Func<T, R>）
                // 2. 直接映射为 IntPtr 的类型（如 Callback<T>, Optional<T>）
                
                // Action 和 Func 需要递归映射内部参数
                if (baseType === 'Action' || baseType === 'Func') {
                    if (typeArgs.length === 0) {
                        return `System.${baseType}`;
                    } else if (baseType === 'Action') {
                        return `System.Action<${typeArgs.join(', ')}>`;
                    } else {
                        // Func<T, R> 或 Func<T1, T2, ..., R>
                        return `System.Func<${typeArgs.join(', ')}>`;
                    }
                }
                
                // Array 需要递归映射
                if (baseType === 'Array' || baseType === 'array') {
                    return `${typeArgs[0]}[]`;
                }
                
                // 其他泛型类型（如 Callback, Optional）直接返回 IntPtr
                return mapping.csharp;
            }
            
            // Promise<T>：基础类型映射为 Task<T>（运行时经 TSFN 异步层完成，见 ROADMAP 2.1）；
            // 内层类型映射成功（含包装类/数组）直接用，仍为 IntPtr 的不可封送类型退回 Task<IntPtr> 句柄
            if (baseType === 'Promise' || baseType === 'promise') {
                const innerMatch = /Promise<(.+)>\s*$/.exec(genericType.trim());
                if (innerMatch) {
                    const inner = this.mapType(this.cleanOptional(innerMatch[1].trim()));
                    if (inner === 'void') {
                        return 'Task';
                    }
                    if (inner !== 'IntPtr' && inner !== 'object') {
                        return `Task<${inner}>`;
                    }
                    // 不可封送的复杂类型：返回 Task<IntPtr>（句柄）
                    return 'Task<IntPtr>';
                }
                return 'IntPtr';
            }

            // Record/Map/Set 等 JS 运行时容器没有对应 C# 泛型映射，统一封送为对象句柄
            if (baseType === 'Record' || baseType === 'Map' || baseType === 'Set') {
                return 'IntPtr';
            }

            // 对于未映射的泛型类型，检查是否已经是有效的 C# 泛型类型
            // （如 Task<T>, Action<T>, Func<T,R> 等由先前映射产生的类型）
            if (baseType === 'Task' || baseType === 'Action' || baseType === 'Func') {
                const csBase = baseType === 'Task' ? 'Task' : `System.${baseType}`;
                return `${csBase}<${typeArgs.join(', ')}>`;
            }
            // 其他未映射的泛型类型退回 IntPtr
            return 'IntPtr';
        }
        return 'IntPtr';
    }

    private static splitTypeArguments(typeArgsStr: string): string[] {
        // 按逗号分割，但要考虑嵌套的尖括号
        const result: string[] = [];
        let depth = 0;
        let current = '';
        
        for (const char of typeArgsStr) {
            if (char === '<') {
                depth++;
                current += char;
            } else if (char === '>') {
                depth--;
                current += char;
            } else if (char === ',' && depth === 0) {
                result.push(current);
                current = '';
            } else {
                current += char;
            }
        }
        
        if (current.trim()) {
            result.push(current);
        }
        
        return result;
    }

    static isNativeType(typescriptType: string): boolean {
        const mapping = this.TYPE_MAP[typescriptType];
        return mapping ? mapping.isNative : false;
    }

    static addMapping(typescript: string, csharp: string, isNative: boolean = false): void {
        this.TYPE_MAP[typescript] = { typescript, csharp, isNative };
    }

    /** 移除映射（record 可封送性收敛时调用，移除后类型退回 IntPtr 句柄） */
    static removeMapping(typescript: string): void {
        delete this.TYPE_MAP[typescript];
    }

    static extractBaseType(typeWithUnion: string): string {
        const parts = typeWithUnion.split('|').map(p => p.trim());
        if (parts.length > 0) {
            return parts[0];
        }
        return typeWithUnion;
    }

    static isOptional(typeStr: string): boolean {
        return typeStr.includes('undefined') || typeStr.includes('?') || typeStr.startsWith('Optional<');
    }

    static cleanOptional(typeStr: string): string {
        // 移除 Optional<> 包装
        if (typeStr.startsWith('Optional<')) {
            const match = typeStr.match(/Optional<(.+)>/);
            if (match) {
                return match[1];
            }
        }
        return typeStr.replace('?', '').replace(' | undefined', '').trim();
    }

    static isUnionType(typeStr: string): boolean {
        return typeStr.includes('|') && !typeStr.includes('=>');
    }

    static isIntersectionType(typeStr: string): boolean {
        return typeStr.includes('&') && !typeStr.includes('=>');
    }

    static isConditionalType(typeStr: string): boolean {
        return typeStr.includes('extends') && typeStr.includes('?') && typeStr.includes(':');
    }

    static isMappedType(typeStr: string): boolean {
        return typeStr.includes('[K in keyof') || typeStr.includes('keyof');
    }

    static isOptionsType(typeStr: string): boolean {
        return typeStr.endsWith('Options') && !typeStr.includes('|') && !typeStr.includes('<');
    }

    // AsyncCallback 相关方法

    /**
     * 检测是否是 AsyncCallback 模式
     * 模式：(result: T, err?: Error) => void
     */
    static isAsyncCallback(funcType: string): boolean {
        // 匹配 (result: T, err?: Error) => void 模式
        const pattern = /^\(([^,]+),\s*[^,]*(?:Error|error)\?\s*:\s*(?:Error|error)\)\s*=>\s*void$/;
        return pattern.test(funcType.trim());
    }

    /**
     * 从 AsyncCallback 提取结果类型
     * (result: string, err?: Error) => void → string
     */
    static extractAsyncCallbackResultType(funcType: string): string | null {
        const match = funcType.match(/^\(([^,]+),\s*(?:[^,]*(?:Error|error)\?)\s*:\s*(?:Error|error)\)\s*=>\s*void$/);
        if (match) {
            const paramStr = match[1].trim();
            const colonIndex = paramStr.indexOf(':');
            if (colonIndex > 0) {
                return paramStr.substring(colonIndex + 1).trim();
            }
        }
        return null;
    }

    /**
     * 检测方法参数中是否包含 AsyncCallback
     * 返回 { isAsync, resultType, callbackIndex }
     */
    static detectAsyncCallbackParam(params: { type: string }[]): { isAsync: boolean; resultType: string | null; callbackIndex: number } {
        for (let i = 0; i < params.length; i++) {
            const param = params[i];
            if (this.isAsyncCallback(param.type)) {
                return {
                    isAsync: true,
                    resultType: this.extractAsyncCallbackResultType(param.type),
                    callbackIndex: i
                };
            }
        }
        return { isAsync: false, resultType: null, callbackIndex: -1 };
    }

    /** C# 保留字列表（用作参数名时需加 @ 前缀） */
    private static readonly CSHARP_KEYWORDS = new Set([
        'abstract', 'as', 'base', 'bool', 'break', 'byte', 'case', 'catch', 'char',
        'checked', 'class', 'const', 'continue', 'decimal', 'default', 'delegate', 'do',
        'double', 'else', 'enum', 'event', 'explicit', 'extern', 'false', 'finally',
        'fixed', 'float', 'for', 'foreach', 'goto', 'if', 'implicit', 'in', 'int',
        'interface', 'internal', 'is', 'lock', 'long', 'namespace', 'new', 'null',
        'object', 'operator', 'out', 'override', 'params', 'private', 'protected',
        'public', 'readonly', 'ref', 'return', 'sbyte', 'sealed', 'short', 'sizeof',
        'stackalloc', 'static', 'string', 'struct', 'switch', 'this', 'throw', 'true',
        'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe', 'ushort', 'using',
        'virtual', 'void', 'volatile', 'while',
    ]);

    /** 转义 C# 保留字：params → @params */
    static escapeCSharpKeyword(name: string): string {
        return this.CSHARP_KEYWORDS.has(name) ? `@${name}` : name;
    }
}
