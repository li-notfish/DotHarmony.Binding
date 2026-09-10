import { TypeMapping } from './models';

export class TypeMapper {
    private static readonly TYPE_MAP: Record<string, TypeMapping> = {
        'string': { typescript: 'string', csharp: 'string', isNative: false },
        'number': { typescript: 'number', csharp: 'double', isNative: false },
        'boolean': { typescript: 'boolean', csharp: 'bool', isNative: false },
        'void': { typescript: 'void', csharp: 'void', isNative: false },
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
        'Action': { typescript: 'Action', csharp: 'Action', isNative: false },
        'Func': { typescript: 'Func', csharp: 'Func', isNative: false },
        // 特殊类型映射
        'object': { typescript: 'object', csharp: 'object', isNative: false },
        'any': { typescript: 'any', csharp: 'object', isNative: false },
        'unknown': { typescript: 'unknown', csharp: 'object', isNative: false },
        'never': { typescript: 'never', csharp: 'void', isNative: false },
        'null': { typescript: 'null', csharp: 'null', isNative: false },
        'undefined': { typescript: 'undefined', csharp: 'null', isNative: false },
    };

    static mapType(typescriptType: string): string {
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
        if (typescriptType.includes('Array<')) {
            return this.mapArrayType(typescriptType);
        }
        if (/\w+\[\]$/.test(typescriptType)) {
            const baseType = typescriptType.replace('[]', '');
            const mapped = this.mapType(baseType);
            return `${mapped}[]`;
        }
        
        // 6. 处理映射类型 ([K in keyof T]) - 必须在泛型检测之前
        if (typescriptType.includes('[K in keyof') || typescriptType.includes('keyof')) {
            return 'dynamic';
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
        
        // 11. 处理 Options 类型（直接返回类名，会生成真实的 C# record）
        if (this.isOptionsType(typescriptType)) {
            return typescriptType;
        }
        
        // 12. 直接映射
        const mapping = this.TYPE_MAP[typescriptType];
        return mapping ? mapping.csharp : typescriptType;
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
            
            if (mappedReturnType === 'void') {
                return params.length > 0 ? `Action<${params.join(', ')}>` : 'Action';
            } else {
                return params.length > 0 ? `Func<${params.join(', ')}, ${mappedReturnType}>` : `Func<${mappedReturnType}>`;
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
        // 条件类型映射为 dynamic
        // 在 C# 中可以使用泛型约束或运行时类型判断
        return 'dynamic';
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
        const match = genericType.match(/^([^(]+)<(.+)>$/);
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
                        return baseType;
                    } else if (baseType === 'Action') {
                        return `Action<${typeArgs.join(', ')}>`;
                    } else {
                        // Func<T, R> 或 Func<T1, T2, ..., R>
                        return `Func<${typeArgs.join(', ')}>`;
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
            // 不可封送的内层类型退回 IntPtr 句柄
            if (baseType === 'Promise' || baseType === 'promise') {
                const innerMatch = /Promise<(.+)>\s*$/.exec(genericType.trim());
                if (innerMatch) {
                    const inner = this.mapType(this.cleanOptional(innerMatch[1].trim()));
                    if (['string', 'double', 'bool', 'int', 'IntPtr'].includes(inner)) {
                        return inner === 'double' ? 'Task<double>' : `Task<${inner}>`;
                    }
                }
                return 'IntPtr';
            }

            // Record/Map/Set 等 JS 运行时容器没有对应 C# 泛型映射，统一封送为对象句柄
            if (baseType === 'Record' || baseType === 'Map' || baseType === 'Set') {
                return 'IntPtr';
            }

            // 对于未映射的泛型类型，尝试保持结构
            // 例如 MyType<number, boolean> → MyType<double, bool>
            return `${baseType}<${typeArgs.join(', ')}>`;
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
}
