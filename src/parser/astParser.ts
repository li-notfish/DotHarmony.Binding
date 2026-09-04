import * as ts from 'typescript';
import * as path from 'path';
import { ComponentInfo, MethodInfo, ParameterInfo, ConstructorOverload, EnumInfo, EnumMemberInfo, EventInfo, DelegateInfo, InheritanceInfo, ImportInfo, ParseResult } from './models';
import { TypeMapper } from './typeMapper';

export class AstParser {
    private program: ts.Program;
    private checker: ts.TypeChecker;

    constructor() {
        this.program = ts.createProgram([], {});
        this.checker = this.program.getTypeChecker();
    }

    parse(filePath: string): ParseResult {
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

        const enums: EnumInfo[] = [];
        const imports: ImportInfo[] = [];
        const warnings: string[] = [];

        ts.forEachChild(sourceFile, (node) => {
            if (ts.isImportDeclaration(node)) {
                const importInfo = this.parseImport(node);
                if (importInfo) {
                    imports.push(importInfo);
                }
            } else if (ts.isInterfaceDeclaration(node)) {
                this.parseInterface(node, component, warnings);
            } else if (ts.isClassDeclaration(node)) {
                this.parseClass(node, component, warnings);
            } else if (ts.isTypeAliasDeclaration(node)) {
                this.parseTypeAlias(node, component);
            } else if (ts.isEnumDeclaration(node)) {
                const enumInfo = this.parseEnum(node);
                if (enumInfo) {
                    enums.push(enumInfo);
                }
            }
        });

        return {
            component,
            enums,
            imports,
            warnings
        };
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

    private parseImport(node: ts.ImportDeclaration): ImportInfo | null {
        const modulePath = node.moduleSpecifier.getText().replace(/['"]/g, '');
        
        // 跳过系统导入
        if (modulePath.startsWith('@ohos.') || modulePath.startsWith('.')) {
            return null;
        }

        const imports: string[] = [];
        let isTypeOnly = !!node.importClause?.isTypeOnly;

        if (node.importClause?.namedBindings) {
            if (ts.isNamedImports(node.importClause.namedBindings)) {
                node.importClause.namedBindings.elements.forEach(element => {
                    imports.push(element.name.getText());
                });
            }
        }

        if (imports.length === 0) {
            return null;
        }

        return {
            module: modulePath,
            imports,
            isTypeOnly
        };
    }

    private parseInterface(node: ts.InterfaceDeclaration, component: ComponentInfo, warnings: string[]): void {
        const interfaceName = node.name.text;
        
        if (interfaceName.endsWith('Interface')) {
            component.interfaceName = interfaceName;
            component.name = interfaceName.replace('Interface', '');
            
            // 解析继承
            if (node.heritageClauses) {
                node.heritageClauses.forEach(clause => {
                    if (clause.token === ts.SyntaxKind.ExtendsKeyword) {
                        clause.types.forEach(type => {
                            const inheritance = this.parseInheritance(type);
                            if (inheritance) {
                                // 存储继承信息（目前只记录警告）
                                warnings.push(`${interfaceName} extends ${inheritance.baseType}`);
                            }
                        });
                    }
                });
            }
            
            node.members.forEach((member) => {
                if (ts.isCallSignatureDeclaration(member)) {
                    this.parseCallSignature(member, component);
                }
            });
        }
    }

    private parseInheritance(node: ts.ExpressionWithTypeArguments): InheritanceInfo | null {
        if (ts.isIdentifier(node.expression)) {
            const baseType = node.expression.text;
            const typeArguments: string[] = [];
            
            if (node.typeArguments) {
                node.typeArguments.forEach(arg => {
                    typeArguments.push(arg.getText());
                });
            }
            
            return {
                baseType,
                typeArguments
            };
        }
        return null;
    }

    private parseClass(node: ts.ClassDeclaration, component: ComponentInfo, warnings: string[]): void {
        const className = node.name?.getText() || '';
        
        if (className.endsWith('Attribute')) {
            component.attributeName = className;
            
            // 解析继承
            if (node.heritageClauses) {
                node.heritageClauses.forEach(clause => {
                    if (clause.token === ts.SyntaxKind.ExtendsKeyword) {
                        clause.types.forEach(type => {
                            const inheritance = this.parseInheritance(type);
                            if (inheritance) {
                                // 存储继承信息（目前只记录警告）
                                warnings.push(`${className} extends ${inheritance.baseType}`);
                            }
                        });
                    }
                });
            }
            
            node.members.forEach((member) => {
                if (ts.isMethodDeclaration(member)) {
                    this.parseMethod(member, component, warnings);
                }
            });
        }
    }

    private parseCallSignature(node: ts.CallSignatureDeclaration, component: ComponentInfo): void {
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
        
        overload.parameters.forEach(param => {
            const exists = component.constructorParams.some(p => p.name === param.name);
            if (!exists) {
                component.constructorParams.push(param);
            }
        });
    }

    private parseMethod(node: ts.MethodDeclaration, component: ComponentInfo, warnings: string[]): void {
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
        return methodName.startsWith('on') && methodName.length > 2 && methodName[2] === methodName[2].toUpperCase();
    }

    private parseEventMethod(node: ts.MethodDeclaration, methodName: string, component: ComponentInfo): EventInfo | null {
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
        
        if (parameters.length === 1 && parameters[0].type.startsWith('(') && parameters[0].type.includes('=>')) {
            const extractedParams = this.extractFunctionParams(parameters[0].type);
            if (extractedParams.length > 0) {
                const delegate: DelegateInfo = {
                    name: delegateName,
                    parameters: extractedParams,
                    returnType: 'void'
                };
                component.delegates.push(delegate);
                
                return {
                    name: methodName,
                    delegateName: delegateName,
                    parameters: extractedParams,
                    returnType: 'void',
                    description: `${methodName} 事件处理器`
                };
            }
        }
        
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
            }).filter(p => p.name);
        }
        return [];
    }

    private capitalizeFirst(str: string): string {
        if (!str) return str;
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    private parseEnum(node: ts.EnumDeclaration): EnumInfo | null {
        const enumName = node.name.text;
        
        if (enumName.startsWith('_')) {
            return null;
        }

        const members: EnumMemberInfo[] = [];
        let isStringEnum = false;

        node.members.forEach((member) => {
            const memberName = member.name.getText();
            
            if (member.initializer) {
                const value = this.getEnumValue(member.initializer);
                if (typeof value === 'string') {
                    isStringEnum = true;
                    members.push({
                        name: memberName,
                        value: value,
                        description: value
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
        
        if (ts.isBinaryExpression(initializer)) {
            const text = initializer.getText();
            if (text.includes('<<')) {
                const match = text.match(/(\d+)\s*<<\s*(\d+)/);
                if (match) {
                    return parseInt(match[1], 10) << parseInt(match[2], 10);
                }
            }
        }
        
        return initializer.getText();
    }

    private detectFlagsEnum(node: ts.EnumDeclaration): boolean {
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

    private parseTypeAlias(node: ts.TypeAliasDeclaration, component: ComponentInfo): void {
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

        if (ts.isFunctionTypeNode(typeNode)) {
            const params = typeNode.parameters.map(p => this.getTypeName(p.type));
            const returnType = this.getTypeName(typeNode.type);
            
            if (returnType === 'void') {
                return params.length > 0 ? `Action<${params.join(', ')}>` : 'Action';
            } else {
                return params.length > 0 ? `Func<${params.join(', ')}, ${returnType}>` : `Func<${returnType}>`;
            }
        }

        if (ts.isConstructorTypeNode(typeNode)) {
            return 'IntPtr';
        }

        return typeNode.getText();
    }
}
