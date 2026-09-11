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
    interfaces: InterfaceInfo[];     // namespace 内嵌套接口（@ohos 服务模块的实例类型）
    classes: ClassInfo[];            // namespace 内嵌套类（如 photoViewPicker）
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
    /** 服务模块：inline AsyncCallback 检测到的原始结果类型（ApiGenerator 负责映射为 Task<T>） */
    asyncResultType?: string;
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

export interface UnionTypeInfo {
    types: string[];                  // 联合类型列表
    isNullable: boolean;              // 是否可空 (包含 null/undefined)
}

export interface IntersectionTypeInfo {
    types: string[];                  // 交叉类型列表
}

export interface ConditionalTypeInfo {
    checkType: string;                // 检查类型
    extendsType: string;              // extends 类型
    trueType: string;                 // 条件为真时的类型
    falseType: string;                // 条件为假时的类型
}

export interface MappedTypeInfo {
    keyType: string;                  // 键类型
    valueType: string;                // 值类型
    readOnly: boolean;                // 是否只读
    optional: boolean;                // 是否可选
}

export interface ArrayTypeInfo {
    elementType: string;              // 元素类型
    dimensions: number;               // 数组维度
}

export interface InterfaceInfo {
    name: string;                     // 接口名称
    properties: PropertyInfo[];       // 属性列表
    methods: MethodInfo[];            // 方法列表
    typeParameters?: string[];        // 泛型参数
    extends?: string[];               // 继承的接口
    sourceFile?: string;              // 来源文件
}

export interface PropertyInfo {
    name: string;                     // 属性名称
    type: string;                     // 属性类型
    optional: boolean;                // 是否可选
    readonly: boolean;                // 是否只读
    description?: string;             // 描述
    defaultValue?: string;            // 默认值
}

export interface ClassInfo {
    name: string;                     // 类名
    properties: PropertyInfo[];       // 属性列表
    methods: MethodInfo[];            // 方法列表
    constructors: ConstructorOverload[]; // 构造函数重载
    typeParameters?: string[];        // 泛型参数
    extends?: string;                 // 继承的类
    implements?: string[];            // 实现的接口
    isAbstract?: boolean;             // 是否抽象类
    sourceFile?: string;              // 来源文件
}

export interface ModuleInfo {
    name: string;                     // 模块名称
    path: string;                     // 文件路径
    exports: string[];                // 导出的类型名称
    interfaces: InterfaceInfo[];      // 导出的接口
    classes: ClassInfo[];             // 导出的类
    enums: EnumInfo[];                // 导出的枚举
    typeAliases: Record<string, string>; // 类型别名
    reExports?: Record<string, string>; // 重导出映射
}

export interface ParseContext {
    modules: Map<string, ModuleInfo>; // 模块名称 -> 模块信息
    types: Map<string, string>;       // 类型名称 -> C# 类型
    enums: Map<string, EnumInfo>;     // 枚举名称 -> 枚举信息
    interfaces: Map<string, InterfaceInfo>; // 接口名称 -> 接口信息
    classes: Map<string, ClassInfo>;   // 类名 -> 类信息
    commonMethodInterface?: InterfaceInfo; // CommonMethod 接口
    warnings: string[];               // 解析警告
}

export function createParseContext(): ParseContext {
    return {
        modules: new Map(),
        types: new Map(),
        enums: new Map(),
        interfaces: new Map(),
        classes: new Map(),
        warnings: []
    };
}
