/**
 * napi 目标生成器（@ohos.* 服务层路线）。
 *
 * 注意：UI 组件的生成走 nativeCodeGenerator.ts（ArkUI C API 路线）；
 * 本文件服务于架构决策中的「非 UI 的 @ohos.* 服务走 napi」路线
 * （传感器/定位/文件等系统能力调用），产物输出至 HarmonyOS.Bindings/Api/。
 * 2026-09 清理时移除了过期产物，重出前需先修复 Void 泛型与 using 缺失问题。
 */
import { ComponentInfo, MethodInfo, ParameterInfo, ConstructorOverload, EventInfo, DelegateInfo, InheritanceInfo, ImportInfo, ParseResult, InterfaceInfo } from './models';
import { TypeMapper } from './typeMapper';

export class CodeGenerator {
    generate(result: ParseResult): string {
        const { component, imports, warnings } = result;
        const lines: string[] = [];
        
        lines.push('using System;');
        lines.push('using System.Runtime.InteropServices;');
        lines.push('using HarmonyOS.Bindings.Runtime;');
        if (component.methods.some(m => /Promise/.test(m.returnType || ''))) {
            lines.push('using System.Threading.Tasks;');
        }
        if (component.namespace !== 'HarmonyOS.ArkUI') {
            lines.push('using HarmonyOS.ArkUI;');
        }
        lines.push('');
        
        // 生成命名空间
        lines.push(`namespace ${component.namespace};`);
        lines.push('');
        
        // 生成组件类（带继承支持）
        this.generateClass(component, lines);
        
        // 生成 *Attribute 类
        this.generateAttributeClass(component, lines);
        
        return lines.join('\n');
    }

    private generateClass(component: ComponentInfo, lines: string[]): void {
        lines.push(`/// <summary>`);
        lines.push(`/// ${component.name} 组件的 C# 绑定`);
        lines.push(`/// </summary>`);
        
        // AOT 防裁剪：经 TrimmerRootAssembly（Bindings 程序集级根）保证；
        // DynamicDependency 特性对 class 声明非法（CS0592），不可标注在类上。
        const baseClass = this.getBaseClass(component);
        if (baseClass) {
            lines.push(`public partial class ${component.name} : ${baseClass}, ArkUIComponentBase`);
        } else {
            lines.push(`public partial class ${component.name} : ArkUIComponentBase`);
        }
        
        lines.push('{');
        
        this.generateConstructors(component, lines);
        this.generateMethods(component, lines);
        this.generateEvents(component, lines);
        
        lines.push('}');
    }

    private generateAttributeClass(component: ComponentInfo, lines: string[]): void {
        const attrName = component.attributeName;
        if (!attrName) return;
        
        lines.push('');
        lines.push(`/// <summary>`);
        lines.push(`/// ${component.name} 属性设置器`);
        lines.push(`/// </summary>`);
        lines.push(`public partial class ${attrName} : ArkUIAttributeBase`);
        lines.push('{');
        lines.push(`    internal ${attrName}(IntPtr jsObject) : base(jsObject) { }`);
        lines.push('}');
    }

    private getBaseClass(component: ComponentInfo): string | null {
        // 目前暂不支持自动生成基类
        // 未来可以添加 CommonMethod<T> 基类支持
        return null;
    }

    private generateConstructors(component: ComponentInfo, lines: string[]): void {
        // 始终生成无参构造函数
        lines.push(`    /// <summary>`);
        lines.push(`    /// 创建 ${component.name} 组件`);
        lines.push(`    /// </summary>`);
        lines.push(`    public ${component.name}() : base(NodeApi.CreateComponent("${component.name}")) { }`);
        lines.push('');
        
        // 用于去重的集合
        const generatedSignatures = new Set<string>();
        
        // 为每个重载生成构造函数
        if (component.constructorOverloads && component.constructorOverloads.length > 0) {
            component.constructorOverloads.forEach((overload) => {
                if (overload.parameters.length > 0) {
                    this.generateConstructorOverloadIfUnique(component.name, overload.parameters, lines, generatedSignatures);
                }
            });
        } else if (component.constructorParams.length > 0) {
            // 兼容旧的解析方式
            this.generateConstructorOverloadIfUnique(component.name, component.constructorParams, lines, generatedSignatures);
        }
    }

    private generateConstructorOverloadIfUnique(className: string, params: ParameterInfo[], lines: string[], generatedSignatures: Set<string>): void {
        // 生成签名用于去重检查
        const signature = params.map(p => {
            const cleanType = TypeMapper.cleanOptional(p.type);
            return TypeMapper.mapType(cleanType);
        }).join(', ');
        
        // 检查是否已生成相同的签名
        if (generatedSignatures.has(signature)) {
            return;
        }
        
        generatedSignatures.add(signature);
        this.generateConstructorOverload(className, params, lines);
    }

    private generateConstructorOverload(className: string, params: ParameterInfo[], lines: string[]): void {
        const processedParams = params.map(p => this.formatParameter(p));
        const paramStr = processedParams.join(', ');
        
        const paramNames = params.map(p => p.name).join(', ');
        
        lines.push(`    /// <summary>`);
        lines.push(`    /// 创建 ${className} 组件`);
        lines.push(`    /// </summary>`);
        lines.push(`    public ${className}(${paramStr}) : base(NodeApi.CreateComponent("${className}", ${paramNames})) { }`);
        lines.push('');
    }

    private formatParameter(param: ParameterInfo): string {
        const cleanType = TypeMapper.cleanOptional(param.type);
        const type = TypeMapper.mapType(cleanType);
        const optionalMark = TypeMapper.isOptional(param.type) ? '?' : '';
        const defaultVal = param.defaultValue ? ` = ${param.defaultValue}` : 
                          (TypeMapper.isOptional(param.type) ? ' = null' : '');
        return `${type}${optionalMark} ${param.name}${defaultVal}`;
    }

    private generateMethods(component: ComponentInfo, lines: string[]): void {
        const propNames = new Set<string>();
        
        // 收集所有唯一的属性名
        component.methods.forEach(method => {
            propNames.add(method.name);
        });

        // 生成 UTF8 字节数组常量
        if (propNames.size > 0) {
            for (const name of propNames) {
                const varName = `_${name}`;
                lines.push(`    private static readonly byte[] ${varName} = "${name}"u8.ToArray();`);
            }
            lines.push('');
        }
        
        // 生成属性 + 方法（带去重）
        const generatedSignatures = new Set<string>();
        component.methods.forEach(method => {
            const paramTypes = method.parameters.map(p => {
                const cleanType = TypeMapper.cleanOptional(p.type);
                return TypeMapper.mapType(cleanType);
            }).join(', ');
            const signature = `${method.name}(${paramTypes})`;
            
            if (generatedSignatures.has(signature)) {
                return;
            }
            generatedSignatures.add(signature);
            
            this.generateMethod(method, component.attributeName, lines);
        });
    }

    private generateMethod(method: MethodInfo, attributeName: string, lines: string[]): void {
        const params = method.parameters.map(p => this.formatParameter(p));
        const paramStr = params.join(', ');
        const propNameVar = `_${method.name}`;
        const pascalName = this.capitalizeFirst(method.name);
        
        if (method.isChained && method.parameters.length === 1) {
            // 单参数链式方法 → 生成属性 + SetXxx 方法
            const param = method.parameters[0];
            const cleanType = TypeMapper.cleanOptional(param.type);
            const propType = TypeMapper.mapType(cleanType);
            const getterExpr = this.generateGetterExpression(propType, propNameVar);
            const setterExpr = this.generateSetterExpression(propType, param.name);
            
            lines.push(`    /// <summary>`);
            lines.push(`    /// 获取或设置 ${method.name} 属性`);
            lines.push(`    /// </summary>`);
            lines.push(`    public ${propType} ${pascalName}`);
            lines.push('    {');
            lines.push(`        get => ${getterExpr};`);
            lines.push(`        set => SetProperty(${propNameVar}, ${setterExpr});`);
            lines.push('    }');
            lines.push('');
            
            // SetXxx 链式方法
            lines.push(`    /// <summary>`);
            lines.push(`    /// 设置 ${method.name} 属性（链式调用）`);
            lines.push(`    /// </summary>`);
            lines.push(`    public ${attributeName} Set${pascalName}(${paramStr})`);
            lines.push('    {');
            lines.push(`        ${pascalName} = ${param.name};`);
            lines.push(`        return new ${attributeName}(_jsObject);`);
            lines.push('    }');
            lines.push('');
        } else if (method.isChained) {
            // 多参数链式方法 → 只生成 SetXxx 方法
            lines.push(`    /// <summary>`);
            lines.push(`    /// 设置 ${method.name} 属性（链式调用）`);
            lines.push(`    /// </summary>`);
            lines.push(`    public ${attributeName} Set${pascalName}(${paramStr})`);
            lines.push('    {');
            const paramNames = method.parameters.map(p => p.name).join(', ');
            lines.push(`        NodeApi.SetAttribute(_jsObject, ${propNameVar}, ${paramNames});`);
            lines.push(`        return new ${attributeName}(_jsObject);`);
            lines.push('    }');
            lines.push('');
        } else {
            // 非链式方法：返回类型经 TypeMapper 映射（旧产物曾直接内插原始 TS 类型，
            // 且 void 会产出非法的 CallMethod<void>——void 走 CallMethodVoid）
            const cleanRet = TypeMapper.cleanOptional(method.returnType || 'void');
            const isVoid = !cleanRet || cleanRet === 'void';
            const returnTypeName = isVoid ? 'void' : TypeMapper.mapType(cleanRet);
            lines.push(`    /// <summary>`);
            lines.push(`    /// 调用 ${method.name} 方法`);
            lines.push(`    /// </summary>`);
            lines.push(`    public ${returnTypeName} ${pascalName}(${paramStr})`);
            lines.push('    {');
            const paramNames = method.parameters.map(p => p.name).join(', ');
            const callArgs = paramNames
                ? `_jsObject, ${propNameVar}, ${paramNames}`
                : `_jsObject, ${propNameVar}`;
            if (isVoid) {
                lines.push(`        NodeApi.CallMethodVoid(${callArgs});`);
            } else {
                // Task<T> 返回（Promise<T> 映射）：经 CallMethodAsync 接为 Task（2.1 异步层）
                const taskMatch = /^Task<(.+)>$/.exec(returnTypeName);
                if (taskMatch) {
                    lines.push(`        return NodeApi.CallMethodAsync<${taskMatch[1]}>(${callArgs});`);
                } else {
                    lines.push(`        return NodeApi.CallMethod<${returnTypeName}>(${callArgs});`);
                }
            }
            lines.push('    }');
            lines.push('');
        }
    }

    private generateGetterExpression(csharpType: string, propNameVar: string): string {
        switch (csharpType) {
            case 'bool':
                return `NativeValue.ToBool(GetProperty(${propNameVar}))`;
            case 'double':
                return `NativeValue.ToDouble(GetProperty(${propNameVar}))`;
            case 'string':
                return `NativeValue.ToString(GetProperty(${propNameVar}))`;
            case 'IntPtr':
                return `GetProperty(${propNameVar})`;
            default:
                // 枚举或复杂类型：用 ToInt 转换
                return `(${csharpType})NativeValue.ToInt(GetProperty(${propNameVar}))`;
        }
    }

    private generateSetterExpression(csharpType: string, paramName: string): string {
        return `NativeValue.From(${paramName})`;
    }

    private generateEvents(component: ComponentInfo, lines: string[]): void {
        // 先生成委托定义
        component.delegates.forEach(delegate => {
            this.generateDelegate(delegate, lines);
        });
        
        // 生成事件方法
        component.events.forEach(event => {
            this.generateEventMethod(event, lines);
        });
        
        // 生成 AOT 兼容的跳板类（如果有事件）
        if (component.delegates.length > 0) {
            this.generateTrampolineClass(component, lines);
        }
    }

    private generateDelegate(delegate: DelegateInfo, lines: string[]): void {
        // delegate.parameters 中的 type 已在 AST 解析器中映射，直接使用
        const params = delegate.parameters.map(p => {
            return `${p.type} ${p.name}`;
        });
        
        const paramStr = params.join(', ');
        // delegate.returnType 已在 AST 解析器中映射，直接使用
        const returnType = delegate.returnType;
        
        lines.push(`    /// <summary>`);
        lines.push(`    /// ${delegate.name} 委托`);
        lines.push(`    /// </summary>`);
        lines.push(`    public delegate ${returnType} ${delegate.name}(${paramStr});`);
        lines.push('');
    }

    private generateEventMethod(event: EventInfo, lines: string[]): void {
        const paramName = 'handler';
        
        lines.push(`    /// <summary>`);
        lines.push(`    /// ${event.description || '设置 ' + event.name + ' 事件处理器'}`);
        lines.push(`    /// </summary>`);
        lines.push(`    public ${event.returnType} ${this.capitalizeFirst(event.name)}(${event.delegateName} ${paramName})`);
        lines.push('    {');
        lines.push(`        NodeApi.SetEventHandler(_jsObject, "${event.name}", ${paramName}, ${event.delegateName}Trampoline_Ptr.Ptr);`);
        if (event.returnType !== 'void') {
            lines.push(`        return this;`);
        }
        lines.push('    }');
        lines.push('');
    }

    private generateDisposePattern(className: string, lines: string[]): void {
        lines.push('    /// <summary>');
        lines.push('    /// 释放原生 JS 对象资源');
        lines.push('    /// </summary>');
        lines.push('    public void Dispose()');
        lines.push('    {');
        lines.push('        Dispose(true);');
        lines.push('        GC.SuppressFinalize(this);');
        lines.push('    }');
        lines.push('');
        lines.push('    /// <summary>');
        lines.push('    /// 受保护的释放实现');
        lines.push('    /// </summary>');
        lines.push('    protected virtual void Dispose(bool disposing)');
        lines.push('    {');
        lines.push('        if (!_disposed)');
        lines.push('        {');
        lines.push('            if (disposing)');
        lines.push('            {');
        lines.push('                // Dispose managed resources');
        lines.push('            }');
        lines.push('');
        lines.push('            NodeApi.DestroyComponent(_jsObject);');
        lines.push('            _jsObject = IntPtr.Zero;');
        lines.push('            _disposed = true;');
        lines.push('        }');
        lines.push('    }');
        lines.push('');
        lines.push('    /// <summary>');
        lines.push('    /// 析构函数');
        lines.push('    /// </summary>');
        lines.push(`    ~${className}()`);
        lines.push('    {');
        lines.push('        Dispose(false);');
        lines.push('    }');
        lines.push('');
    }

    private capitalizeFirst(str: string): string {
        if (!str) return str;
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    private generateTrampolineClass(component: ComponentInfo, lines: string[]): void {
        const componentName = component.name;
        
        // 生成每个委托的跳板类
        component.delegates.forEach(delegate => {
            this.generateSingleTrampolineClass(componentName, delegate, lines);
        });
    }

    private generateSingleTrampolineClass(componentName: string, delegate: DelegateInfo, lines: string[]): void {
        const className = `${delegate.name}Trampoline_Ptr`;
        const callbackName = `Callback`;
        
        lines.push('');
        lines.push(`    internal static class ${className}`);
        lines.push('    {');
        lines.push(`        [System.Runtime.InteropServices.UnmanagedCallersOnly]`);
        lines.push(`        internal static IntPtr ${callbackName}(IntPtr env, IntPtr info)`);
        lines.push('        {');
        lines.push('            NativeNodeApi.napi_get_cb_info(env, info, out _, out var argv, out _, out var data);');
        lines.push(`            var handler = (${delegate.name})System.Runtime.InteropServices.GCHandle.FromIntPtr(data).Target!;`);
        
        // 根据参数类型生成解包代码
        delegate.parameters.forEach((param, index) => {
            const readExpr = `Marshal.ReadIntPtr(argv, ${index} * IntPtr.Size)`;
            switch (param.type) {
                case 'double':
                    lines.push(`            double arg${index} = NativeValue.ToDouble(${readExpr});`);
                    break;
                case 'bool':
                    lines.push(`            bool arg${index} = NativeValue.ToBool(${readExpr});`);
                    break;
                case 'int':
                    lines.push(`            int arg${index} = NativeValue.ToInt(${readExpr});`);
                    break;
                case 'string':
                    lines.push(`            string arg${index} = NativeValue.ToString(${readExpr})!;`);
                    break;
                case 'IntPtr':
                    lines.push(`            IntPtr arg${index} = ${readExpr};`);
                    break;
                default:
                    lines.push(`            IntPtr arg${index} = ${readExpr};`);
                    break;
            }
        });
        
        // 调用委托
        const argNames = delegate.parameters.map((_, i) => `arg${i}`).join(', ');
        if (delegate.returnType === 'void') {
            lines.push(`            handler(${argNames});`);
        } else {
            lines.push(`            var result = handler(${argNames});`);
        }
        
        lines.push('            NativeNodeApi.napi_get_undefined(env, out var undefined);');
        lines.push('            return undefined;');
        lines.push('        }');
        lines.push('');
        lines.push(`        internal static readonly IntPtr Ptr = GetPtr();`);
        lines.push('');
        lines.push(`        private static IntPtr GetPtr()`);
        lines.push(`        {`);
        lines.push(`            unsafe`);
        lines.push(`            {`);
        lines.push(`                return (IntPtr)(delegate*<IntPtr, IntPtr, IntPtr>)(&${callbackName});`);
        lines.push(`            }`);
        lines.push(`        }`);
        lines.push('    }');
        lines.push('');
    }

    generateEnum(name: string, values: string[]): string {
        const lines: string[] = [];
        
        lines.push(`/// <summary>`);
        lines.push(`/// ${name} 枚举`);
        lines.push(`/// </summary>`);
        lines.push(`public enum ${name}`);
        lines.push('{');
        
        values.forEach((value, index) => {
            const comma = index < values.length - 1 ? ',' : '';
            lines.push(`    ${value}${comma}`);
        });
        
        lines.push('}');
        
        return lines.join('\n');
    }

    generateRecord(name: string, fields: { name: string; type: string }[]): string {
        const lines: string[] = [];
        
        const fieldStr = fields.map(f => {
            const type = TypeMapper.mapType(f.type);
            return `${type} ${this.capitalizeFirst(f.name)}`;
        }).join(', ');
        
        lines.push(`/// <summary>`);
        lines.push(`/// ${name} 数据类`);
        lines.push(`/// </summary>`);
        lines.push(`public record ${name}(${fieldStr});`);
        
        return lines.join('\n');
    }

    generateOptionsRecord(optionsInfo: InterfaceInfo, allInterfaces: Map<string, InterfaceInfo> = new Map()): string {
        const lines: string[] = [];

        lines.push('namespace HarmonyOS.ArkUI;');
        lines.push('');

        const mergedProps = this.mergeInheritedProperties(optionsInfo, allInterfaces);

        const params = mergedProps.map(p => {
            const type = TypeMapper.mapType(p.type);
            const pascalName = this.capitalizeFirst(p.name);
            if (p.optional) {
                return `    ${type}? ${pascalName} = null`;
            }
            return `    ${type} ${pascalName}`;
        }).join(',\n');

        lines.push(`/// <summary>`);
        lines.push(`/// ${optionsInfo.name} 配置选项`);
        lines.push(`/// </summary>`);
        lines.push(`public record ${optionsInfo.name}(`);
        lines.push(params);
        lines.push(');');

        return lines.join('\n');
    }

    private mergeInheritedProperties(
        optionsInfo: InterfaceInfo,
        allInterfaces: Map<string, InterfaceInfo>
    ): { name: string; type: string; optional: boolean }[] {
        const visited = new Set<string>();
        const result: { name: string; type: string; optional: boolean }[] = [];
        const seenNames = new Set<string>();

        const visit = (iface: InterfaceInfo) => {
            if (visited.has(iface.name)) return;
            visited.add(iface.name);

            if (iface.extends) {
                for (const parentName of iface.extends) {
                    const parent = allInterfaces.get(parentName);
                    if (parent) {
                        visit(parent);
                    }
                }
            }

            for (const prop of iface.properties) {
                if (!seenNames.has(prop.name)) {
                    seenNames.add(prop.name);
                    result.push({
                        name: prop.name,
                        type: prop.type,
                        optional: prop.optional
                    });
                }
            }
        };

        visit(optionsInfo);
        return result;
    }
}
