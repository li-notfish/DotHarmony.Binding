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
        // 选项类型映射
        'ColumnOptions': { typescript: 'ColumnOptions', csharp: 'IntPtr', isNative: true },
        'ColumnOptionsV2': { typescript: 'ColumnOptionsV2', csharp: 'IntPtr', isNative: true },
        'TextOptions': { typescript: 'TextOptions', csharp: 'IntPtr', isNative: true },
        'ButtonOptions': { typescript: 'ButtonOptions', csharp: 'IntPtr', isNative: true },
        'ImageOptions': { typescript: 'ImageOptions', csharp: 'IntPtr', isNative: true },
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
        
        // 4. 处理数组类型 (Array<T>)
        if (typescriptType.includes('Array<')) {
            return this.mapArrayType(typescriptType);
        }
        
        // 5. 直接映射
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
        
        // 找到第一个有映射的类型
        for (const part of nonNullParts) {
            const mapping = this.TYPE_MAP[part];
            if (mapping) {
                return mapping.csharp;
            }
        }
        
        // 如果都没有映射，返回第一个非空类型
        if (nonNullParts.length > 0) {
            return this.mapType(nonNullParts[0]);
        }
        
        return 'IntPtr';
    }

    private static mapArrayType(arrayType: string): string {
        const match = arrayType.match(/Array<(.+)>/);
        if (match) {
            const elementType = this.mapType(match[1]);
            return `${elementType}[]`;
        }
        return 'IntPtr[]';
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
}
