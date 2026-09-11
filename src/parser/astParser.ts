import * as ts from 'typescript';
import * as path from 'path';
import * as fs from 'fs';
import { ComponentInfo, MethodInfo, ParameterInfo, ConstructorOverload, EnumInfo, EnumMemberInfo, EventInfo, DelegateInfo, InheritanceInfo, ImportInfo, ParseResult, ParseContext, createParseContext, ModuleInfo, InterfaceInfo, PropertyInfo, ClassInfo } from './models';
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
            } else if (ts.isModuleDeclaration(node) && node.name) {
                // @ohos.* 模块：declare namespace xxx { const/function/enum }
                this.parseNamespace(node, component, warnings, enums);
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
            } else if (ts.isFunctionDeclaration(node)) {
                this.parseFunctionDeclaration(node, component, warnings);
            }
        });

        // 如果没有找到标准 Interface 模式，尝试解析 API 格式
        if (!component.name) {
            this.parseApiFile(sourceFile, component, warnings);
        }

        return {
            component,
            enums,
            imports,
            warnings
        };
    }

    /**
     * 解析 declare namespace xxx { ... } 块（@ohos.* API 模块格式）。
     * 提取 const（属性）、function（方法）、export enum（枚举）。
     */
    private parseNamespace(
        node: ts.ModuleDeclaration,
        component: ComponentInfo,
        warnings: string[],
        enums: EnumInfo[]
    ): void {
        const nsName = node.name.text;
        if (!component.name) {
            component.name = nsName;
            component.interfaceName = nsName;
        }

        const body = node.body;
        if (!body || !ts.isModuleBlock(body)) return;

        ts.forEachChild(body, (child) => {
            if (ts.isVariableStatement(child)) {
                for (const decl of child.declarationList.declarations) {
                    if (decl.name && ts.isIdentifier(decl.name)) {
                        const propType = this.getTypeName(decl.type);
                        const mappedType = TypeMapper.mapType(propType);
                        // const → C# static property (getter)
                        const method: MethodInfo = {
                            name: decl.name.text,
                            returnType: mappedType,
                            parameters: [],
                            isChained: true // 标记为属性（单参数链式路径在生成器里产 property getter）
                        };
                        component.methods.push(method);
                    }
                }
            } else if (ts.isFunctionDeclaration(child) && child.name) {
                this.parseMethod(child as unknown as ts.MethodDeclaration, component, warnings);
            } else if (ts.isEnumDeclaration(child)) {
                const enumInfo = this.parseEnum(child);
                if (enumInfo) {
                    enums.push(enumInfo);
                }
            } else if (ts.isInterfaceDeclaration(child)) {
                // namespace 内部嵌套接口（如 Display、DisplayInfo 等）
                this.parseInterface(child, component, warnings);
            } else if (ts.isModuleDeclaration(child) && child.name) {
                // 嵌套 namespace（如 settings.date、settings.general）
                this.parseNamespace(child, component, warnings, enums);
            }
        });
    }

    private parseApiFile(sourceFile: ts.SourceFile, component: ComponentInfo, warnings: string[]): void {
        ts.forEachChild(sourceFile, (node) => {
            if (ts.isClassDeclaration(node) && node.name) {
                const className = node.name.text;
                if (!component.name) {
                    component.name = className;
                    component.interfaceName = className + 'Interface';
                }
                
                node.members.forEach(member => {
                    if (ts.isMethodDeclaration(member)) {
                        const method: MethodInfo = {
                            name: member.name.getText(),
                            returnType: TypeMapper.mapType(this.getTypeName(member.type)),
                            parameters: member.parameters.map(p => ({
                                name: p.name.getText(),
                                type: TypeMapper.mapType(this.getTypeName(p.type)),
                                optional: !!p.questionToken,
                                defaultValue: p.initializer ? p.initializer.getText() : undefined
                            })),
                            isChained: false
                        };

                        // AsyncCallback 检测：检查原始 TypeScript AST
                        if (member.parameters && member.parameters.length > 0) {
                            const lastParam = member.parameters[member.parameters.length - 1];
                            if (lastParam.type && ts.isFunctionTypeNode(lastParam.type)) {
                                const asyncCallbackInfo = this.detectAsyncCallbackFromAST(lastParam.type);
                                if (asyncCallbackInfo) {
                                    const mappedResultType = TypeMapper.mapType(asyncCallbackInfo.resultType);
                                    method.returnType = mappedResultType === 'void' ? 'Task' : `Task<${mappedResultType}>`;
                                    method.parameters.pop(); // 移除回调参数
                                }
                            }
                        }

                        component.methods.push(method);
                    } else if (ts.isConstructorDeclaration(member)) {
                        const ctor: ConstructorOverload = {
                            parameters: member.parameters.map(p => ({
                                name: p.name.getText(),
                                type: TypeMapper.mapType(this.getTypeName(p.type)),
                                optional: !!p.questionToken,
                                defaultValue: p.initializer ? p.initializer.getText() : undefined
                            }))
                        };
                        component.constructorOverloads.push(ctor);
                    }
                });
            } else if (ts.isInterfaceDeclaration(node) && node.name) {
                const interfaceName = node.name.text;
                if (!component.name) {
                    component.name = interfaceName;
                    component.interfaceName = interfaceName;
                }
                
                node.members.forEach(member => {
                    if (ts.isMethodSignature(member)) {
                        const method: MethodInfo = {
                            name: member.name.getText(),
                            returnType: TypeMapper.mapType(this.getTypeName(member.type)),
                            parameters: member.parameters.map(p => ({
                                name: p.name.getText(),
                                type: TypeMapper.mapType(this.getTypeName(p.type)),
                                optional: !!p.questionToken,
                                defaultValue: p.initializer ? p.initializer.getText() : undefined
                            })),
                            isChained: false
                        };
                        component.methods.push(method);
                    }
                });
            } else if (ts.isFunctionDeclaration(node) && node.name) {
                const funcName = node.name.text;
                if (!component.name) {
                    component.name = funcName;
                    component.interfaceName = funcName + 'Interface';
                }

                const method: MethodInfo = {
                    name: funcName,
                    returnType: TypeMapper.mapType(this.getTypeName(node.type)),
                    parameters: node.parameters.map(p => ({
                        name: p.name.getText(),
                        type: TypeMapper.mapType(this.getTypeName(p.type)),
                        optional: !!p.questionToken,
                        defaultValue: p.initializer ? p.initializer.getText() : undefined
                    })),
                    isChained: false
                };
                component.methods.push(method);
            } else if (ts.isExportAssignment(node)) {
                const exportName = node.expression?.getText();
                if (exportName) {
                    component.name = exportName;
                }
            }
        });
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

        // AsyncCallback 检测：检查原始 TypeScript AST
        // 如果最后一个参数是 (result: T, err?: Error) => void 格式
        if (node.parameters && node.parameters.length > 0) {
            const lastParam = node.parameters[node.parameters.length - 1];
            if (lastParam.type && ts.isFunctionTypeNode(lastParam.type)) {
                const asyncCallbackInfo = this.detectAsyncCallbackFromAST(lastParam.type);
                if (asyncCallbackInfo) {
                    const mappedResultType = TypeMapper.mapType(asyncCallbackInfo.resultType);
                    method.returnType = mappedResultType === 'void' ? 'Task' : `Task<${mappedResultType}>`;
                    method.parameters.pop(); // 移除回调参数
                }
            }
        }

        component.methods.push(method);
    }

    private parseFunctionDeclaration(node: ts.FunctionDeclaration, component: ComponentInfo, warnings: string[]): void {
        const funcName = node.name?.getText();
        if (!funcName) return;

        const returnType = this.getTypeName(node.type);

        const method: MethodInfo = {
            name: funcName,
            returnType: TypeMapper.mapType(returnType),
            parameters: [],
            isChained: false
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

        // AsyncCallback 检测：检查原始 TypeScript AST
        if (node.parameters && node.parameters.length > 0) {
            const lastParam = node.parameters[node.parameters.length - 1];
            if (lastParam.type && ts.isFunctionTypeNode(lastParam.type)) {
                const asyncCallbackInfo = this.detectAsyncCallbackFromAST(lastParam.type);
                if (asyncCallbackInfo) {
                    const mappedResultType = TypeMapper.mapType(asyncCallbackInfo.resultType);
                    method.returnType = mappedResultType === 'void' ? 'Task' : `Task<${mappedResultType}>`;
                    method.parameters.pop(); // 移除回调参数
                }
            }
        }

        component.methods.push(method);
    }

    /**
     * 从 TypeScript AST 检测 AsyncCallback 模式
     * 匹配 (result: T, err?: Error) => void 或 (err?: Error) => void
     */
    private detectAsyncCallbackFromAST(funcType: ts.FunctionTypeNode): { resultType: string; callbackIndex: number } | null {
        const params = funcType.parameters;
        const returnType = this.getTypeName(funcType.type);

        // 返回类型必须是 void
        if (returnType !== 'void') return null;

        // 模式1: (result: T, err?: Error) => void
        if (params.length === 2) {
            const firstParamType = this.getTypeName(params[0].type);
            const secondParamType = this.getTypeName(params[1].type);
            const secondParamOptional = !!params[1].questionToken;

            if (secondParamOptional && this.isErrorType(secondParamType)) {
                return { resultType: firstParamType, callbackIndex: 0 };
            }
        }

        // 模式2: (err?: Error) => void - 仅错误回调，结果为 void
        if (params.length === 1) {
            const firstParamType = this.getTypeName(params[0].type);
            const firstParamOptional = !!params[0].questionToken;

            if (firstParamOptional && this.isErrorType(firstParamType)) {
                return { resultType: 'void', callbackIndex: -1 };
            }
        }

        return null;
    }

    /**
     * 检查类型是否是 Error 类型
     */
    private isErrorType(typeName: string): boolean {
        return typeName === 'Error' || typeName === 'error' || 
               typeName.endsWith('Error') || typeName.endsWith('error');
    }

    private isEventMethod(methodName: string): boolean {
        return methodName.startsWith('on') && methodName.length > 2 && methodName[2] === methodName[2].toUpperCase();
    }

    private parseEventMethod(node: ts.MethodDeclaration, methodName: string, component: ComponentInfo): EventInfo | null {
        const delegateName = `${this.capitalizeFirst(methodName.slice(2))}Handler`;
        
        const parameters: ParameterInfo[] = [];
        
        if (node.parameters) {
            node.parameters.forEach((param) => {
                const rawType = this.getTypeName(param.type);
                const paramType = TypeMapper.mapType(rawType);
                parameters.push({
                    name: param.name.getText(),
                    type: paramType,
                    optional: !!param.questionToken
                });
            });
        }
        
        // 尝试提取函数类型参数的内部参数
        // 注意：getTypeName 已将 (param: type) => returnType 转换为 Action<type> 或 Func<type, R>
        // 对于这种情况，我们需要从原始 TypeScript AST 重新提取
        if (parameters.length === 1) {
            const firstParam = node.parameters?.[0];
            if (firstParam && firstParam.type && ts.isFunctionTypeNode(firstParam.type)) {
                // 从原始 AST 提取函数参数并递归映射
                const extractedParams = firstParam.type.parameters.map((p, index) => {
                    const innerRawType = this.getTypeName(p.type);
                    const innerParamType = TypeMapper.mapType(innerRawType);
                    return {
                        name: p.name.getText() || `param${index}`,
                        type: innerParamType,
                        optional: !!p.questionToken
                    };
                });
                
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
                const rawType = parts.length > 1 ? parts[1].trim() : 'IntPtr';
                const paramType = TypeMapper.mapType(rawType);
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
        
        // 记录类型别名（用于复杂类型处理）
        if (ts.isUnionTypeNode(typeNode)) {
            const types = typeNode.types.map(t => this.getTypeName(t));
            // 检查是否可空
            const isNullable = types.some(t => t === 'null' || t === 'undefined');
        } else if (ts.isIntersectionTypeNode(typeNode)) {
            const types = typeNode.types.map(t => this.getTypeName(t));
        } else if (ts.isConditionalTypeNode(typeNode)) {
            const checkType = this.getTypeName(typeNode.checkType);
            const extendsType = this.getTypeName(typeNode.extendsType);
            const trueType = this.getTypeName(typeNode.trueType);
            const falseType = this.getTypeName(typeNode.falseType);
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
            // 过滤掉 null 和 undefined，单独处理可空性
            const nonNullTypes = types.filter(t => t !== 'null' && t !== 'undefined');
            const isNullable = types.length !== nonNullTypes.length;
            
            if (nonNullTypes.length === 1) {
                return isNullable ? `${nonNullTypes[0]}?` : nonNullTypes[0];
            }
            return types.join(' | ');
        }

        if (ts.isIntersectionTypeNode(typeNode)) {
            const types = typeNode.types.map(t => this.getTypeName(t));
            return types.join(' & ');
        }

        if (ts.isConditionalTypeNode(typeNode)) {
            const checkType = this.getTypeName(typeNode.checkType);
            const extendsType = this.getTypeName(typeNode.extendsType);
            const trueType = this.getTypeName(typeNode.trueType);
            const falseType = this.getTypeName(typeNode.falseType);
            // 映射为 C# 的条件类型或动态类型
            return `dynamic /* ${checkType} extends ${extendsType} ? ${trueType} : ${falseType} */`;
        }

        if (ts.isMappedTypeNode(typeNode)) {
            return 'dynamic /* MappedType */';
        }

        if (ts.isArrayTypeNode(typeNode)) {
            return `${this.getTypeName(typeNode.elementType)}[]`;
        }

        if (ts.isTupleTypeNode(typeNode)) {
            const elements = typeNode.elements.map(e => this.getTypeName(e));
            return `(${elements.join(', ')})`;
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

        if (ts.isTypeOperatorNode(typeNode)) {
            const operator = typeNode.operator;
            const typeName = this.getTypeName(typeNode.type);
            if (operator === ts.SyntaxKind.ReadonlyKeyword) {
                return `readonly ${typeName}`;
            }
            if (operator === ts.SyntaxKind.KeyOfKeyword) {
                return `keyof ${typeName}`;
            }
            if (operator === ts.SyntaxKind.UniqueKeyword) {
                return `unique ${typeName}`;
            }
        }

        if (ts.isIndexedAccessTypeNode(typeNode)) {
            const objectType = this.getTypeName(typeNode.objectType);
            const indexType = this.getTypeName(typeNode.indexType);
            return `${objectType}[${indexType}]`;
        }

        if (ts.isTypeLiteralNode(typeNode)) {
            return 'IntPtr';
        }

        return typeNode.getText();
    }

    parseModule(filePath: string, context: ParseContext): ModuleInfo | null {
        const sourceFile = ts.createSourceFile(
            filePath,
            fs.readFileSync(filePath, 'utf-8'),
            ts.ScriptTarget.Latest,
            true
        );

        const moduleName = path.basename(filePath, '.d.ts');
        const moduleInfo: ModuleInfo = {
            name: moduleName,
            path: filePath,
            exports: [],
            interfaces: [],
            classes: [],
            enums: [],
            typeAliases: {}
        };

        ts.forEachChild(sourceFile, (node) => {
            if (ts.isInterfaceDeclaration(node)) {
                const iface = this.parseInterfaceFull(node, filePath);
                if (iface) {
                    moduleInfo.interfaces.push(iface);
                    moduleInfo.exports.push(iface.name);
                    context.interfaces.set(iface.name, iface);
                }
            } else if (ts.isClassDeclaration(node)) {
                const cls = this.parseClassFull(node, filePath);
                if (cls) {
                    moduleInfo.classes.push(cls);
                    moduleInfo.exports.push(cls.name);
                    context.classes.set(cls.name, cls);
                }
            } else if (ts.isEnumDeclaration(node)) {
                const enumInfo = this.parseEnum(node);
                if (enumInfo) {
                    moduleInfo.enums.push(enumInfo);
                    moduleInfo.exports.push(enumInfo.name);
                    context.enums.set(enumInfo.name, enumInfo);
                }
            } else if (ts.isTypeAliasDeclaration(node)) {
                const typeName = node.name.getText();
                const typeStr = this.getTypeName(node.type);
                moduleInfo.typeAliases[typeName] = typeStr;
                context.types.set(typeName, typeStr);
            } else if (ts.isVariableStatement(node)) {
                node.declarationList.declarations.forEach(decl => {
                    const varName = decl.name.getText();
                    if (decl.type && ts.isTypeLiteralNode(decl.type)) {
                        const props = this.parsePropertySignatures([...decl.type.members]);
                        const iface: InterfaceInfo = {
                            name: varName,
                            properties: props,
                            methods: [],
                            sourceFile: filePath
                        };
                        moduleInfo.interfaces.push(iface);
                        moduleInfo.exports.push(iface.name);
                        context.interfaces.set(iface.name, iface);
                    }
                });
            }
        });

        context.modules.set(moduleName, moduleInfo);
        return moduleInfo;
    }

    parseCommonMethod(filePath: string, context: ParseContext): InterfaceInfo | null {
        const sourceFile = ts.createSourceFile(
            filePath,
            fs.readFileSync(filePath, 'utf-8'),
            ts.ScriptTarget.Latest,
            true
        );

        let commonMethod: InterfaceInfo | null = null;

        ts.forEachChild(sourceFile, (node) => {
            if (ts.isClassDeclaration(node) && node.name?.text === 'CommonMethod') {
                commonMethod = this.parseClassAsInterface(node, filePath);
                if (commonMethod) {
                    context.commonMethodInterface = commonMethod;
                    context.interfaces.set('CommonMethod', commonMethod);
                }
            }
        });

        return commonMethod;
    }

    mergeCommonMethod(component: ComponentInfo, context: ParseContext): void {
        if (!context.commonMethodInterface) {
            return;
        }

        const commonMethod = context.commonMethodInterface;
        const attrName = component.attributeName;

        commonMethod.methods.forEach(method => {
            const exists = component.methods.some(m => m.name === method.name);
            if (!exists) {
                const mappedMethod: MethodInfo = {
                    name: method.name,
                    returnType: method.returnType === 'CommonMethod<T>' ? attrName : TypeMapper.mapType(method.returnType),
                    parameters: method.parameters.map(p => ({
                        name: p.name,
                        type: TypeMapper.mapType(p.type),
                        optional: p.optional,
                        defaultValue: p.defaultValue
                    })),
                    isChained: method.returnType.includes('CommonMethod') || method.returnType.includes('this')
                };
                component.methods.push(mappedMethod);
            }
        });
    }

    private parseInterfaceFull(node: ts.InterfaceDeclaration, sourceFile: string): InterfaceInfo {
        const name = node.name.text;
        const properties: PropertyInfo[] = [];
        const methods: MethodInfo[] = [];
        const typeParameters = node.typeParameters?.map(tp => tp.name.text);
        const extendsList: string[] = [];

        if (node.heritageClauses) {
            node.heritageClauses.forEach(clause => {
                if (clause.token === ts.SyntaxKind.ExtendsKeyword) {
                    clause.types.forEach(type => {
                        extendsList.push(type.expression.getText());
                    });
                }
            });
        }

        node.members.forEach(member => {
            if (ts.isPropertySignature(member)) {
                const prop: PropertyInfo = {
                    name: member.name.getText(),
                    type: this.getTypeName(member.type),
                    optional: !!member.questionToken,
                    readonly: !!member.modifiers?.some(m => m.kind === ts.SyntaxKind.ReadonlyKeyword)
                };
                properties.push(prop);
            } else if (ts.isMethodSignature(member)) {
                const method: MethodInfo = {
                    name: member.name.getText(),
                    returnType: TypeMapper.mapType(this.getTypeName(member.type)),
                    parameters: member.parameters.map(p => ({
                        name: p.name.getText(),
                        type: TypeMapper.mapType(this.getTypeName(p.type)),
                        optional: !!p.questionToken,
                        defaultValue: p.initializer ? p.initializer.getText() : undefined
                    })),
                    isChained: false
                };
                methods.push(method);
            } else if (ts.isCallSignatureDeclaration(member)) {
                const method: MethodInfo = {
                    name: '__call__',
                    returnType: TypeMapper.mapType(this.getTypeName(member.type)),
                    parameters: member.parameters.map(p => ({
                        name: p.name.getText(),
                        type: TypeMapper.mapType(this.getTypeName(p.type)),
                        optional: !!p.questionToken,
                        defaultValue: p.initializer ? p.initializer.getText() : undefined
                    })),
                    isChained: false
                };
                methods.push(method);
            }
        });

        return {
            name,
            properties,
            methods,
            typeParameters,
            extends: extendsList.length > 0 ? extendsList : undefined,
            sourceFile
        };
    }

    private parseClassFull(node: ts.ClassDeclaration, sourceFile: string): ClassInfo {
        const name = node.name?.getText() || '';
        const properties: PropertyInfo[] = [];
        const methods: MethodInfo[] = [];
        const constructors: ConstructorOverload[] = [];
        const typeParameters = node.typeParameters?.map(tp => tp.name.text);
        let extendsClass: string | undefined;
        const implementsList: string[] = [];
        const isAbstract = !!node.modifiers?.some(m => m.kind === ts.SyntaxKind.AbstractKeyword);

        if (node.heritageClauses) {
            node.heritageClauses.forEach(clause => {
                if (clause.token === ts.SyntaxKind.ExtendsKeyword) {
                    clause.types.forEach(type => {
                        extendsClass = type.expression.getText();
                    });
                } else if (clause.token === ts.SyntaxKind.ImplementsKeyword) {
                    clause.types.forEach(type => {
                        implementsList.push(type.expression.getText());
                    });
                }
            });
        }

        node.members.forEach(member => {
            if (ts.isPropertyDeclaration(member)) {
                const prop: PropertyInfo = {
                    name: member.name?.getText() || '',
                    type: this.getTypeName(member.type),
                    optional: !!member.questionToken,
                    readonly: !!(member.modifiers && member.modifiers.some(m => !ts.isDecorator(m) && m.kind === ts.SyntaxKind.ReadonlyKeyword))
                };
                properties.push(prop);
            } else if (ts.isMethodDeclaration(member)) {
                const method: MethodInfo = {
                    name: member.name.getText(),
                    returnType: TypeMapper.mapType(this.getTypeName(member.type)),
                    parameters: member.parameters.map(p => ({
                        name: p.name.getText(),
                        type: TypeMapper.mapType(this.getTypeName(p.type)),
                        optional: !!p.questionToken,
                        defaultValue: p.initializer ? p.initializer.getText() : undefined
                    })),
                    isChained: false
                };
                methods.push(method);
            } else if (ts.isConstructorDeclaration(member)) {
                const ctor: ConstructorOverload = {
                    parameters: member.parameters.map(p => ({
                        name: p.name.getText(),
                        type: TypeMapper.mapType(this.getTypeName(p.type)),
                        optional: !!p.questionToken,
                        defaultValue: p.initializer ? p.initializer.getText() : undefined
                    }))
                };
                constructors.push(ctor);
            }
        });

        return {
            name,
            properties,
            methods,
            constructors,
            typeParameters,
            extends: extendsClass,
            implements: implementsList.length > 0 ? implementsList : undefined,
            isAbstract,
            sourceFile
        };
    }

    private parsePropertySignatures(members: ts.TypeElement[]): PropertyInfo[] {
        const properties: PropertyInfo[] = [];
        members.forEach(member => {
            if (ts.isPropertySignature(member)) {
                properties.push({
                    name: member.name.getText(),
                    type: this.getTypeName(member.type),
                    optional: !!member.questionToken,
                    readonly: !!member.modifiers?.some(m => m.kind === ts.SyntaxKind.ReadonlyKeyword)
                });
            }
        });
        return properties;
    }

    private parseClassAsInterface(node: ts.ClassDeclaration, sourceFile: string): InterfaceInfo {
        const name = node.name?.getText() || 'CommonMethod';
        const properties: PropertyInfo[] = [];
        const methods: MethodInfo[] = [];
        const typeParameters = node.typeParameters?.map(tp => tp.name.text);

        node.members.forEach(member => {
            if (ts.isMethodDeclaration(member)) {
                const method: MethodInfo = {
                    name: member.name.getText(),
                    returnType: TypeMapper.mapType(this.getTypeName(member.type)),
                    parameters: member.parameters.map(p => ({
                        name: p.name.getText(),
                        type: TypeMapper.mapType(this.getTypeName(p.type)),
                        optional: !!p.questionToken,
                        defaultValue: p.initializer ? p.initializer.getText() : undefined
                    })),
                    isChained: false
                };
                methods.push(method);
            } else if (ts.isPropertyDeclaration(member)) {
                properties.push({
                    name: member.name?.getText() || '',
                    type: this.getTypeName(member.type),
                    optional: !!member.questionToken,
                    readonly: !!(member.modifiers && member.modifiers.some(m => !ts.isDecorator(m) && m.kind === ts.SyntaxKind.ReadonlyKeyword))
                });
            }
        });

        return {
            name,
            properties,
            methods,
            typeParameters,
            sourceFile
        };
    }

    collectOptionsInterfaces(filePath: string): InterfaceInfo[] {
        const sourceFile = ts.createSourceFile(
            filePath,
            fs.readFileSync(filePath, 'utf-8'),
            ts.ScriptTarget.Latest,
            true
        );

        const options: InterfaceInfo[] = [];
        ts.forEachChild(sourceFile, (node) => {
            if (ts.isInterfaceDeclaration(node) && node.name.text.endsWith('Options')) {
                options.push(this.parseInterfaceFull(node, filePath));
            }
        });
        return options;
    }

    collectAllInterfaces(filePath: string): InterfaceInfo[] {
        const sourceFile = ts.createSourceFile(
            filePath,
            fs.readFileSync(filePath, 'utf-8'),
            ts.ScriptTarget.Latest,
            true
        );

        const interfaces: InterfaceInfo[] = [];
        ts.forEachChild(sourceFile, (node) => {
            if (ts.isInterfaceDeclaration(node)) {
                interfaces.push(this.parseInterfaceFull(node, filePath));
            }
        });
        return interfaces;
    }

    extractReferencedTypes(interfaces: InterfaceInfo[]): string[] {
        const referenced = new Set<string>();

        for (const iface of interfaces) {
            for (const prop of iface.properties) {
                const types = this.extractTypeNames(prop.type);
                types.forEach(t => referenced.add(t));
            }
            if (iface.extends) {
                iface.extends.forEach(e => referenced.add(e));
            }
        }

        referenced.delete('string');
        referenced.delete('number');
        referenced.delete('boolean');
        referenced.delete('void');
        referenced.delete('object');
        referenced.delete('any');
        referenced.delete('unknown');
        referenced.delete('undefined');
        referenced.delete('null');

        return Array.from(referenced);
    }

    private extractTypeNames(typeStr: string): string[] {
        const cleaned = typeStr.replace(/Optional<(.+)>/g, '$1')
                               .replace(/\(\s*\)\s*=>\s*.+/g, '')
                               .replace(/\([^)]*\)\s*=>\s*.+/g, '');

        const matches = cleaned.match(/[A-Z][a-zA-Z0-9_]*/g);
        return matches ? matches.filter(m => m.length > 1) : [];
    }
}
