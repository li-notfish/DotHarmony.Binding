import { ComponentInfo, MethodInfo, ParameterInfo, ConstructorOverload, EventInfo, DelegateInfo, InheritanceInfo, ImportInfo, ParseResult } from './models';
import { TypeMapper } from './typeMapper';

export class CodeGenerator {
    generate(result: ParseResult): string {
        const { component, imports, warnings } = result;
        const lines: string[] = [];
        
        lines.push('using System;');
        lines.push('using System.Runtime.InteropServices;');
        lines.push('');
        
        // 生成命名空间
        lines.push(`namespace ${component.namespace};`);
        lines.push('');
        
        // 生成类（带继承支持）
        this.generateClass(component, lines);
        
        return lines.join('\n');
    }

    private generateClass(component: ComponentInfo, lines: string[]): void {
        lines.push(`/// <summary>`);
        lines.push(`/// ${component.name} 组件的 C# 绑定`);
        lines.push(`/// </summary>`);
        
        // 支持继承（目前暂不支持基类，只记录信息）
        const baseClass = this.getBaseClass(component);
        if (baseClass) {
            lines.push(`public partial class ${component.name} : ${baseClass}, IDisposable`);
        } else {
            lines.push(`public partial class ${component.name} : IDisposable`);
        }
        
        lines.push('{');
        
        lines.push('    /// <summary>');
        lines.push('    /// JS对象指针');
        lines.push('    /// </summary>');
        lines.push('    private IntPtr _jsObject;');
        lines.push('');
        lines.push('    /// <summary>');
        lines.push('    /// 指示是否已释放资源');
        lines.push('    /// </summary>');
        lines.push('    private bool _disposed = false;');
        lines.push('');
        
        this.generateConstructors(component, lines);
        this.generateMethods(component, lines);
        this.generateEvents(component, lines);
        this.generateDisposePattern(component.name, lines);
        
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
        lines.push(`    public ${component.name}()`);
        lines.push('    {');
        lines.push(`        _jsObject = NodeApi.CreateComponent("${component.name}");`);
        lines.push('    }');
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
        
        // 参数名列表（用于方法体）
        const paramNames = params.map(p => p.name).join(', ');
        
        lines.push(`    /// <summary>`);
        lines.push(`    /// 创建 ${className} 组件`);
        lines.push(`    /// </summary>`);
        lines.push(`    public ${className}(${paramStr})`);
        lines.push('    {');
        lines.push(`        _jsObject = NodeApi.CreateComponent("${className}", ${paramNames});`);
        lines.push('    }');
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
        const generatedSignatures = new Set<string>();
        
        component.methods.forEach(method => {
            // Build C# signature for dedup (name + mapped param types, ignoring param names)
            const paramTypes = method.parameters.map(p => {
                const cleanType = TypeMapper.cleanOptional(p.type);
                return TypeMapper.mapType(cleanType);
            }).join(', ');
            const signature = `${method.name}(${paramTypes})`;
            
            if (generatedSignatures.has(signature)) {
                return; // Skip duplicate method
            }
            generatedSignatures.add(signature);
            
            this.generateMethod(method, component.attributeName, lines);
        });
    }

    private generateMethod(method: MethodInfo, attributeName: string, lines: string[]): void {
        const params = method.parameters.map(p => this.formatParameter(p));
        
        const paramStr = params.join(', ');
        const returnTypeName = method.isChained ? attributeName : method.returnType;
        
        lines.push(`    /// <summary>`);
        lines.push(`    /// 设置 ${method.name} 属性`);
        lines.push(`    /// </summary>`);
        lines.push(`    public ${returnTypeName} ${this.capitalizeFirst(method.name)}(${paramStr})`);
        lines.push('    {');
        
        if (method.isChained) {
            const paramNames = method.parameters.map(p => p.name).join(', ');
            lines.push(`        NodeApi.SetAttribute(_jsObject, "${method.name}", ${paramNames});`);
            lines.push(`        return new ${attributeName}(_jsObject);`);
        } else {
            const paramNames = method.parameters.map(p => p.name).join(', ');
            lines.push(`        return NodeApi.CallMethod<${returnTypeName}>(_jsObject, "${method.name}", ${paramNames});`);
        }
        
        lines.push('    }');
        lines.push('');
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
    }

    private generateDelegate(delegate: DelegateInfo, lines: string[]): void {
        const params = delegate.parameters.map(p => {
            const type = TypeMapper.mapType(p.type);
            return `${type} ${p.name}`;
        });
        
        const paramStr = params.join(', ');
        const returnType = TypeMapper.mapType(delegate.returnType);
        
        lines.push(`    /// <summary>`);
        lines.push(`    /// ${delegate.name} 委托`);
        lines.push(`    /// </summary>`);
        lines.push(`    public delegate ${returnType} ${delegate.name}(${paramStr});`);
        lines.push('');
    }

    private generateEventMethod(event: EventInfo, lines: string[]): void {
        const paramName = event.parameters.length > 0 ? 'handler' : 'handler';
        
        lines.push(`    /// <summary>`);
        lines.push(`    /// ${event.description || '设置 ' + event.name + ' 事件处理器'}`);
        lines.push(`    /// </summary>`);
        lines.push(`    public ${event.returnType} ${this.capitalizeFirst(event.name)}(${event.delegateName} ${paramName})`);
        lines.push('    {');
        lines.push(`        NodeApi.SetEventHandler(_jsObject, "${event.name}", ${paramName});`);
        lines.push(`        return this;`);
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
}
