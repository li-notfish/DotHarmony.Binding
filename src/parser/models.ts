export interface ComponentInfo {
    name: string;                    // "Text"
    interfaceName: string;           // "TextInterface"
    attributeName: string;           // "TextAttribute"
    constructorParams: ParameterInfo[];
    constructorOverloads: ConstructorOverload[];
    methods: MethodInfo[];
    namespace: string;               // "HarmonyOS.ArkUI"
}

export interface ConstructorOverload {
    parameters: ParameterInfo[];
}

export interface ParameterInfo {
    name: string;
    type: string;
    optional: boolean;
    defaultValue?: string;
}

export interface MethodInfo {
    name: string;
    returnType: string;
    parameters: ParameterInfo[];
    isChained: boolean;              // 是否返回this用于链式调用
}

export interface TypeMapping {
    typescript: string;
    csharp: string;
    isNative: boolean;               // 是否需要特殊处理
}

export interface EnumInfo {
    name: string;
    members: EnumMemberInfo[];
    isFlags: boolean;                // 是否是位标志枚举
    isStringEnum: boolean;           // 是否是字符串枚举
}

export interface EnumMemberInfo {
    name: string;
    value?: string | number;         // 枚举值（可选）
    description?: string;            // 描述（用于字符串枚举）
}
