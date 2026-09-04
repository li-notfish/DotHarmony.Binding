import { EventInfo, DelegateInfo, ParameterInfo } from './models';
import { TypeMapper } from './typeMapper';

export class EventGenerator {
    generateEvents(events: EventInfo[], delegates: DelegateInfo[]): string {
        const lines: string[] = [];
        
        lines.push('using System;');
        lines.push('');
        lines.push('namespace HarmonyOS.ArkUI;');
        lines.push('');
        
        // 生成委托定义
        delegates.forEach(delegate => {
            this.generateDelegate(delegate, lines);
            lines.push('');
        });
        
        return lines.join('\n');
    }

    private generateDelegate(delegate: DelegateInfo, lines: string[]): void {
        // delegate.parameters 中的 type 已在 AST 解析器中映射，直接使用
        const params = delegate.parameters.map(p => {
            return `${p.type} ${p.name}`;
        });
        
        const paramStr = params.join(', ');
        // delegate.returnType 已在 AST 解析器中映射，直接使用
        const returnType = delegate.returnType;
        
        lines.push(`/// <summary>`);
        lines.push(`/// ${delegate.name} 委托`);
        lines.push(`/// </summary>`);
        lines.push(`public delegate ${returnType} ${delegate.name}(${paramStr});`);
    }

    generateEventMethods(events: EventInfo[]): string {
        const lines: string[] = [];
        
        events.forEach(event => {
            this.generateEventMethod(event, lines);
        });
        
        return lines.join('\n');
    }

    private generateEventMethod(event: EventInfo, lines: string[]): void {
        const params = event.parameters.map(p => {
            const type = TypeMapper.mapType(p.type);
            return `${type} ${p.name}`;
        });
        
        const paramStr = params.join(', ');
        const paramName = 'handler';
        
        lines.push(`    /// <summary>`);
        lines.push(`    /// ${event.description || '设置 ' + event.name + ' 事件处理器'}`);
        lines.push(`    /// </summary>`);
        lines.push(`    public ${event.returnType} ${this.capitalizeFirst(event.name)}(${event.delegateName} ${paramName})`);
        lines.push('    {');
        lines.push(`        NodeApi.SetEventHandler(_jsObject, "${event.name}", ${paramName});`);
        if (event.returnType !== 'void') {
            lines.push(`        return this;`);
        }
        lines.push('    }');
        lines.push('');
    }

    private capitalizeFirst(str: string): string {
        if (!str) return str;
        return str.charAt(0).toUpperCase() + str.slice(1);
    }
}
