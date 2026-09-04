import { EnumInfo, EnumMemberInfo } from './models';

export class EnumGenerator {
    generate(enumInfo: EnumInfo): string {
        const lines: string[] = [];
        
        lines.push('using System;');
        lines.push('');
        lines.push('namespace HarmonyOS.ArkUI;');
        lines.push('');
        
        this.generateEnum(enumInfo, lines);
        
        return lines.join('\n');
    }

    private generateEnum(enumInfo: EnumInfo, lines: string[]): void {
        // 添加 [Flags] 特性（如果是位标志枚举）
        if (enumInfo.isFlags) {
            lines.push('[Flags]');
        }
        
        lines.push(`/// <summary>`);
        lines.push(`/// ${enumInfo.name} 枚举`);
        lines.push(`/// </summary>`);
        lines.push(`public enum ${enumInfo.name}`);
        lines.push('{');
        
        enumInfo.members.forEach((member, index) => {
            this.generateEnumMember(member, enumInfo, lines, index === enumInfo.members.length - 1);
        });
        
        lines.push('}');
    }

    private generateEnumMember(member: EnumMemberInfo, enumInfo: EnumInfo, lines: string[], isLast: boolean): void {
        const comma = isLast ? '' : ',';
        
        // 添加描述特性（如果是字符串枚举）
        if (enumInfo.isStringEnum && member.description) {
            lines.push(`    [Description("${member.description}")]`);
        }
        
        if (enumInfo.isStringEnum && typeof member.value === 'string') {
            // 字符串枚举：不使用显式值，依赖 [Description] 存储字符串
            // C# 枚举必须使用整数值
            lines.push(`    ${member.name}${comma}`);
        } else if (member.value !== undefined) {
            // 有显式值（数值）
            const valueStr = member.value.toString();
            lines.push(`    ${member.name} = ${valueStr}${comma}`);
        } else {
            // 无显式值
            lines.push(`    ${member.name}${comma}`);
        }
    }

    generateMultipleEnums(enums: EnumInfo[]): string {
        const lines: string[] = [];
        
        lines.push('using System;');
        lines.push('');
        
        // 检查是否需要 Description 特性
        const needsDescription = enums.some(e => e.isStringEnum);
        if (needsDescription) {
            lines.push('using System.ComponentModel;');
            lines.push('');
        }
        
        lines.push('namespace HarmonyOS.ArkUI;');
        lines.push('');
        
        enums.forEach((enumInfo, index) => {
            this.generateEnum(enumInfo, lines);
            if (index < enums.length - 1) {
                lines.push('');
            }
        });
        
        return lines.join('\n');
    }
}
