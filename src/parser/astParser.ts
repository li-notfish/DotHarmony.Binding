import * as ts from 'typescript';
import * as path from 'path';
import { ComponentInfo, MethodInfo, ParameterInfo, ConstructorOverload, EnumInfo, EnumMemberInfo, EventInfo, DelegateInfo } from './models';
import { TypeMapper } from './typeMapper';

export class AstParser {
    private program: ts.Program;
    private checker: ts.TypeChecker;

    constructor() {
        this.program = ts.createProgram([], {});
        this.checker = this.program.getTypeChecker();
    }

    parse(filePath: string): ComponentInfo {
        const sourceFile = ts.createSourceFile(
            filePath,
            require('fs').readFileSync(filePath, 'utf-8'),
            ts.ScriptTarget.Latest,
            true
        );

        const component: ComponentInfo = {
            name: '',
            interfaceName: '',
            attributeName: '',
            constructorParams: [],
            constructorOverloads: [],
            methods: [],
            events: [],
            delegates: [],
            namespace: 'HarmonyOS.ArkUI'
        };

        ts.forEachChild(sourceFile, (node) => {
            if (ts.isInterfaceDeclaration(node)) {
                this.parseInterface(node, component);
            } else if (ts.isClassDeclaration(node)) {
                this.parseClass(node, component);
            } else if (ts.isTypeAliasDeclaration(node)) {
                this.parseTypeAlias(node, component);
            }
        });

        return component;
    }

    parseEnums(filePath: string): EnumInfo[] {
        const sourceFile = ts.createSourceFile(
            filePath,
            require('fs').readFileSync(filePath, 'utf-8'),
            ts.ScriptTarget.Latest,
            true
        );

        const enums: EnumInfo[] = [];

        ts.forEachChild(sourceFile, (node) => {
            if (ts.isEnumDeclaration(node)) {
                const enumInfo = this.parseEnum(node);
                if (enumInfo) {
                    enums.push(enumInfo);
                }
            }
        });

        return enums;
    }

    private parseEnum(node: ts.EnumDeclaration): EnumInfo | null {
        const enumName = node.name.text;
        
        // 跳过内部枚举
        if (enumName.startsWith('_')) {
            return null;
        }

        const members: EnumMemberInfo[] = [];
        let isStringEnum = false;
        let hasExplicitValues = false;

        node.members.forEach((member) => {
            const memberName = member.name.getText();
            
            if (member.initializer) {
                hasExplicitValues = true;
                const value = this.getEnumValue(member.initializer);
                if (typeof value === 'string') {
                    isStringEnum = true;
                    members.push({
                        name: memberName,
                        value: value,
                        description: value  // 对于字符串枚举，description 就是值
                    });
                } else {
                    members.push({
                        name: memberName,
                        value: value
                    });
                }
            } else {
                members.push({
                    name: memberName
                });
            }
        });

        // 检测是否是位标志枚举（包含位运算）
        const isFlags = this.detectFlagsEnum(node);

        return {
            name: enumName,
            members: members,
            isFlags: isFlags,
            isStringEnum: isStringEnum
        };
    }

    private getEnumValue(initializer: ts.Expression): string | number {
        if (ts.isNumericLiteral(initializer)) {
            return parseInt(initializer.text, 10);
        }
        
        if (ts.isStringLiteral(initializer)) {
            return initializer.text;
        }
        
        // 处理位运算: 1 << 0, 1 << 1 等
        if (ts.isBinaryExpression(initializer)) {
            const text = initializer.getText();
            // 简单的位运算评估
            if (text.includes('<<')) {
                const match = text.match(/(\d+)\s*<<\s*(\d+)/);
                if (match) {
                    return parseInt(match[1], 10) << parseInt(match[2], 10);
                }
            }
        }
        
        // 对于其他复杂表达式，返回文本
        return initializer.getText();
    }

    private detectFlagsEnum(node: ts.EnumDeclaration): boolean {
        // 检查是否有位运算或特定的值模式
        for (const member of node.members) {
            if (member.initializer) {
                const text = member.initializer.getText();
                if (text.includes('<<') || text.includes('|')) {
                    return true;
                }
            }
        }
        return false;
    }

    private parseInterface(node: ts.InterfaceDeclaration, component: ComponentInfo): void {
        const interfaceName = node.name.text;
        
        if (interfaceName.endsWith('Interface')) {
            component.interfaceName = interfaceName;
            component.name = interfaceName.replace('Interface', '');
            
            node.members.forEach((member) => {
                if (ts.isCallSignatureDeclaration(member)) {
                    this.parseCallSignature(member, component);
                }
            });
        }
    }

    private parseCallSignature(node: ts.CallSignatureDeclaration, component: ComponentInfo): void {
        // 每个 CallSignature 创建一个新的重载
        const overload: ConstructorOverload = {
            parameters: []
        };
        
        if (node.parameters) {
            node.parameters.forEach((param) => {
                overload.parameters.push({
                    name: param.name.getText(),
                    type: this.getTypeName(param.type),
                    optional: !!param.questionToken,
                    defaultValue: param.initializer ? param.initializer.getText() : undefined
                });
            });
        }
        
        component.constructorOverloads.push(overload);
        
        // 同时更新 constructorParams 以保持兼容性（将所有参数合并）
        // 注意：这里只添加新的参数，避免重复
        overload.parameters.forEach(param => {
            const exists = component.constructorParams.some(p => p.name === param.name);
            if (!exists) {
                component.constructorParams.push(param);
            }
        });
    }

    private parseClass(node: ts.ClassDeclaration, component: ComponentInfo): void {
        const className = node.name?.getText() || '';
        
        if (className.endsWith('Attribute')) {
            component.attributeName = className;
            
            node.members.forEach((member) => {
                if (ts.isMethodDeclaration(member)) {
                    this.parseMethod(member, component);
                }
            });
        }
    }

    private parseMethod(node: ts.MethodDeclaration, component: ComponentInfo): void {
        const methodName = node.name.getText();
        const returnType = this.getTypeName(node.type);
        
        // 检测是否是事件方法（以 "on" 开头）
        if (this.isEventMethod(methodName)) {
            const eventInfo = this.parseEventMethod(node, methodName, component);
            if (eventInfo) {
                component.events.push(eventInfo);
            }
            return;
        }
        
        const method: MethodInfo = {
            name: methodName,
            returnType: TypeMapper.mapType(returnType),
            parameters: [],
            isChained: returnType === component.attributeName
        };

        if (node.parameters) {
            node.parameters.forEach((param) => {
                const paramInfo: ParameterInfo = {
                    name: param.name.getText(),
                    type: this.getTypeName(param.type),
                    optional: !!param.questionToken,
                    defaultValue: param.initializer ? param.initializer.getText() : undefined
                };
                method.parameters.push(paramInfo);
            });
        }

        component.methods.push(method);
    }

    private isEventMethod(methodName: string): boolean {
        // 事件方法通常以 "on" 开头
        return methodName.startsWith('on') && methodName.length > 2 && methodName[2] === methodName[2].toUpperCase();
    }

    private parseEventMethod(node: ts.MethodDeclaration, methodName: string, component: ComponentInfo): EventInfo | null {
        // 从方法名生成委托名称
        const delegateName = `${this.capitalizeFirst(methodName.slice(2))}Handler`;
        
        const parameters: ParameterInfo[] = [];
        
        if (node.parameters) {
            node.parameters.forEach((param) => {
                const paramType = this.getTypeName(param.type);
                parameters.push({
                    name: param.name.getText(),
                    type: paramType,
                    optional: !!param.questionToken
                });
            });
        }
        
        // 如果参数是函数类型，提取其参数作为事件参数
        if (parameters.length === 1 && parameters[0].type.startsWith('(') && parameters[0].type.includes('=>')) {
            const extractedParams = this.extractFunctionParams(parameters[0].type);
            if (extractedParams.length > 0) {
                // 创建委托
                const delegate: DelegateInfo = {
                    name: delegateName,
                    parameters: extractedParams,
                    returnType: 'void'
                };
                component.delegates.push(delegate);
                
                // 创建事件信息
                return {
                    name: methodName,
                    delegateName: delegateName,
                    parameters: extractedParams,
                    returnType: 'void',
                    description: `${methodName} 事件处理器`
                };
            }
        }
        
        // 如果参数不是函数类型，创建简单的委托
        if (parameters.length > 0) {
            const delegate: DelegateInfo = {
                name: delegateName,
                parameters: parameters,
                returnType: 'void'
            };
            component.delegates.push(delegate);
            
            return {
                name: methodName,
                delegateName: delegateName,
                parameters: parameters,
                returnType: 'void',
                description: `${methodName} 事件处理器`
            };
        }
        
        // 无参数的事件
        const simpleDelegate: DelegateInfo = {
            name: delegateName,
            parameters: [],
            returnType: 'void'
        };
        component.delegates.push(simpleDelegate);
        
        return {
            name: methodName,
            delegateName: delegateName,
            parameters: [],
            returnType: 'void',
            description: `${methodName} 事件处理器`
        };
    }

    private extractFunctionParams(funcType: string): ParameterInfo[] {
        // 解析 "(param1: type1, param2: type2) => returnType"
        const match = funcType.match(/\(([^)]*)\)\s*=>\s*(.+)/);
        if (match) {
            const paramsStr = match[1];
            return paramsStr.split(',').map((p, index) => {
                const parts = p.trim().split(':');
                const paramName = parts[0]?.trim() || `param${index}`;
                const paramType = parts.length > 1 ? parts[1].trim() : 'IntPtr';
                return {
                    name: paramName,
                    type: paramType,
                    optional: false
                };
            }).filter(p => p.name); // 过滤空参数
        }
        return [];
    }

    private capitalizeFirst(str: string): string {
        if (!str) return str;
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    private parseTypeAlias(node: ts.TypeAliasDeclaration, component: ComponentInfo): void {
        // Handle union types like: type FlexDirection = 'Row' | 'Column'
        const typeName = node.name.getText();
        const typeNode = node.type;
        
        if (ts.isUnionTypeNode(typeNode)) {
            // Could be used for enum generation
        }
    }

    private getTypeName(typeNode: ts.TypeNode | undefined): string {
        if (!typeNode) {
            return 'void';
        }

        if (ts.isTypeReferenceNode(typeNode)) {
            const typeName = typeNode.typeName.getText();
            // 处理泛型类型，如 Optional<boolean>
            if (typeNode.typeArguments && typeNode.typeArguments.length > 0) {
                const typeArgs = typeNode.typeArguments.map(arg => this.getTypeName(arg)).join(', ');
                return `${typeName}<${typeArgs}>`;
            }
            return typeName;
        }

        if (ts.isUnionTypeNode(typeNode)) {
            const types = typeNode.types.map(t => this.getTypeName(t));
            return types.join(' | ');
        }

        if (ts.isArrayTypeNode(typeNode)) {
            return `${this.getTypeName(typeNode.elementType)}[]`;
        }

        if (ts.isLiteralTypeNode(typeNode)) {
            return typeNode.getText();
        }

        if (ts.isParenthesizedTypeNode(typeNode)) {
            return this.getTypeName(typeNode.type);
        }

        // 处理函数类型: (param: type) => returnType
        if (ts.isFunctionTypeNode(typeNode)) {
            const params = typeNode.parameters.map(p => this.getTypeName(p.type));
            const returnType = this.getTypeName(typeNode.type);
            
            if (returnType === 'void') {
                return params.length > 0 ? `Action<${params.join(', ')}>` : 'Action';
            } else {
                return params.length > 0 ? `Func<${params.join(', ')}, ${returnType}>` : `Func<${returnType}>`;
            }
        }

        // 处理构造签名: new (param: type) => returnType
        if (ts.isConstructorTypeNode(typeNode)) {
            return 'IntPtr';
        }

        return typeNode.getText();
    }
}
