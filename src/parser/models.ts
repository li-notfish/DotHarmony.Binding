export interface ComponentInfo {
    name: string;                    // "Text"
    interfaceName: string;           // "TextInterface"
    attributeName: string;           // "TextAttribute"
    constructorParams: ParameterInfo[];
    constructorOverloads: ConstructorOverload[];
    methods: MethodInfo[];
    events: EventInfo[];
    delegates: DelegateInfo[];
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

export interface EventInfo {
    name: string;                    // 事件名称，如 "onClick"
    delegateName: string;            // 委托名称，如 "ClickEventHandler"
    parameters: ParameterInfo[];     // 事件参数
    returnType: string;              // 返回类型（通常是 void）
    description?: string;            // 事件描述
}

export interface DelegateInfo {
    name: string;                    // 委托名称
    parameters: ParameterInfo[];     // 参数列表
    returnType: string;              // 返回类型
}

export interface InheritanceInfo {
    baseType: string;                // 基类名称
    typeArguments: string[];         // 泛型参数
}

export interface ImportInfo {
    module: string;                  // 导入模块路径
    imports: string[];               // 导入的类型列表
    isTypeOnly: boolean;             // 是否是类型导入
}

export interface ParseResult {
    component: ComponentInfo;
    enums: EnumInfo[];
    imports: ImportInfo[];
    warnings: string[];
}
