import * as fs from 'fs';
import * as path from 'path';
import * as crypto from 'crypto';
import { AstParser } from './astParser';
import { CodeGenerator } from './codeGenerator';
import { EnumGenerator } from './enumGenerator';
import { ComponentInfo, EnumInfo, ParseResult, ParseContext, createParseContext, InterfaceInfo } from './models';

interface CacheEntry {
    hash: string;
    mtime: string;
    outputPath: string;
    enumOutputPath: string;
}

interface GenerationCache {
    version: number;
    files: Record<string, CacheEntry>;
}

export class ArkTsParser {
    private parser: AstParser;
    private generator: CodeGenerator;
    private enumGenerator: EnumGenerator;

    constructor() {
        this.parser = new AstParser();
        this.generator = new CodeGenerator();
        this.enumGenerator = new EnumGenerator();
    }

    parseFile(inputPath: string): ParseResult {
        if (!fs.existsSync(inputPath)) {
            throw new Error(`File not found: ${inputPath}`);
        }
        
        return this.parser.parse(inputPath);
    }

    parseEnums(inputPath: string): EnumInfo[] {
        if (!fs.existsSync(inputPath)) {
            throw new Error(`File not found: ${inputPath}`);
        }
        
        return this.parser.parseEnums(inputPath);
    }

    generateCode(result: ParseResult): string {
        return this.generator.generate(result);
    }

    generateEnumCode(enumInfo: EnumInfo): string {
        return this.enumGenerator.generate(enumInfo);
    }

    generateEnumsCode(enums: EnumInfo[]): string {
        return this.enumGenerator.generateMultipleEnums(enums);
    }

    async processFile(inputPath: string, outputPath: string): Promise<void> {
        console.log(`Processing: ${inputPath}`);
        
        // 解析组件
        const result = this.parseFile(inputPath);
        
        // 输出警告
        if (result.warnings.length > 0) {
            result.warnings.forEach(w => console.log(`  Warning: ${w}`));
        }
        
        // 确保输出目录存在
        const outputDir = path.dirname(outputPath);
        if (!fs.existsSync(outputDir)) {
            fs.mkdirSync(outputDir, { recursive: true });
        }
        
        // 如果有组件内容，生成组件代码
        if (result.component.name) {
            const csharpCode = this.generateCode(result);
            await fs.promises.writeFile(outputPath, csharpCode);
            console.log(`Generated: ${outputPath}`);
        }
        
        // 解析并生成枚举
        const enums = this.parseEnums(inputPath);
        if (enums.length > 0) {
            const enumOutputPath = outputPath.replace('.cs', '.Enums.cs');
            const enumCode = this.generateEnumsCode(enums);
            await fs.promises.writeFile(enumOutputPath, enumCode);
            console.log(`Generated: ${enumOutputPath}`);
        }
        
        // 收集并生成 Options record
        const optionsInterfaces = this.parser.collectOptionsInterfaces(inputPath);
        for (const optionsInfo of optionsInterfaces) {
            const optionsCode = this.generator.generateOptionsRecord(optionsInfo);
            const optionsOutputPath = outputPath.replace('.cs', `.${optionsInfo.name}.cs`);
            await fs.promises.writeFile(optionsOutputPath, optionsCode);
            console.log(`Generated: ${optionsOutputPath}`);
        }
    }

    async processFileWithContext(
        inputPath: string, 
        outputPath: string, 
        context: ParseContext,
        namespace?: string
    ): Promise<void> {
        console.log(`Processing: ${inputPath}`);
        
        // 解析组件
        const result = this.parseFile(inputPath);
        
        // 合并 CommonMethod<T>
        this.parser.mergeCommonMethod(result.component, context);
        
        // 设置命名空间
        if (namespace) {
            result.component.namespace = namespace;
        }
        
        // 输出警告
        if (result.warnings.length > 0) {
            result.warnings.forEach(w => context.warnings.push(w));
        }
        
        // 确保输出目录存在
        const outputDir = path.dirname(outputPath);
        if (!fs.existsSync(outputDir)) {
            fs.mkdirSync(outputDir, { recursive: true });
        }
        
        // 如果有组件内容，生成组件代码
        if (result.component.name) {
            const csharpCode = this.generateCode(result);
            await fs.promises.writeFile(outputPath, csharpCode);
            console.log(`Generated: ${outputPath}`);
        }
        
        // 解析并生成枚举
        const enums = this.parseEnums(inputPath);
        if (enums.length > 0) {
            const enumOutputPath = outputPath.replace('.cs', '.Enums.cs');
            const enumCode = this.generateEnumsCode(enums);
            await fs.promises.writeFile(enumOutputPath, enumCode);
            console.log(`Generated: ${enumOutputPath}`);
        }
        
        // 收集并生成 Options record
        const optionsInterfaces = this.parser.collectOptionsInterfaces(inputPath);

        // 收集所有接口定义（用于解析嵌套引用和继承）
        const allInterfaces = this.parser.collectAllInterfaces(inputPath);
        const interfaceMap = new Map<string, InterfaceInfo>();
        for (const iface of allInterfaces) {
            interfaceMap.set(iface.name, iface);
            context.interfaces.set(iface.name, iface);
        }

        // 递归收集被 Options 引用的所有类型
        const referencedTypes = this.parser.extractReferencedTypes(optionsInterfaces);
        for (const typeName of referencedTypes) {
            if (!interfaceMap.has(typeName)) {
                const ref = allInterfaces.find(i => i.name === typeName);
                if (ref) {
                    interfaceMap.set(ref.name, ref);
                    context.interfaces.set(ref.name, ref);
                }
            }
        }

        // 先生成被引用的接口
        const generatedRecords = new Set<string>();
        for (const optionsInfo of optionsInterfaces) {
            // 递归展开继承链
            const chain = this.collectInheritanceChain(optionsInfo, interfaceMap);
            for (const item of chain) {
                if (!generatedRecords.has(item.name)) {
                    generatedRecords.add(item.name);
                    const recordCode = this.generator.generateOptionsRecord(item, interfaceMap);
                    const outputFile = outputPath.replace('.cs', `.${item.name}.cs`);
                    await fs.promises.writeFile(outputFile, recordCode);
                    console.log(`Generated: ${outputFile}`);
                }
            }
        }
    }

    private collectInheritanceChain(
        optionsInfo: InterfaceInfo,
        interfaceMap: Map<string, InterfaceInfo>
    ): InterfaceInfo[] {
        const visited = new Set<string>();
        const result: InterfaceInfo[] = [];

        const visit = (iface: InterfaceInfo) => {
            if (visited.has(iface.name)) return;
            visited.add(iface.name);

            if (iface.extends) {
                for (const parentName of iface.extends) {
                    const parent = interfaceMap.get(parentName);
                    if (parent && parent.name.endsWith('Options')) {
                        visit(parent);
                    }
                }
            }

            if (iface.name.endsWith('Options')) {
                result.push(iface);
            }
        };

        visit(optionsInfo);
        return result;
    }

    private loadCache(outputDir: string): GenerationCache {
        const cachePath = path.join(outputDir, '.generation-cache.json');
        if (fs.existsSync(cachePath)) {
            try {
                return JSON.parse(fs.readFileSync(cachePath, 'utf-8'));
            } catch {
                return { version: 1, files: {} };
            }
        }
        return { version: 1, files: {} };
    }

    private saveCache(outputDir: string, cache: GenerationCache): void {
        const cachePath = path.join(outputDir, '.generation-cache.json');
        fs.writeFileSync(cachePath, JSON.stringify(cache, null, 2));
    }

    private fileHash(filePath: string): string {
        const content = fs.readFileSync(filePath);
        return crypto.createHash('sha256').update(content).digest('hex');
    }

    private needsRegeneration(filePath: string, cache: GenerationCache): boolean {
        const fileName = path.basename(filePath);
        const stat = fs.statSync(filePath);
        const currentHash = this.fileHash(filePath);
        
        const cached = cache.files[fileName];
        if (!cached) return true;
        if (cached.hash !== currentHash) return true;
        if (new Date(cached.mtime).getTime() !== stat.mtimeMs) return true;
        
        return false;
    }

    async processDirectory(inputDir: string, outputDir: string): Promise<void> {
        if (!fs.existsSync(inputDir)) {
            throw new Error(`Directory not found: ${inputDir}`);
        }
        
        // 确保输出目录存在
        if (!fs.existsSync(outputDir)) {
            fs.mkdirSync(outputDir, { recursive: true });
        }
        
        const files = fs.readdirSync(inputDir).filter(f => f.endsWith('.d.ts'));
        const cache = this.loadCache(outputDir);
        const newCache: GenerationCache = { version: 1, files: {} };
        
        const promises = files.map(file => {
            const inputPath = path.join(inputDir, file);
            const outputFile = file.replace('.d.ts', '.cs');
            const outputPath = path.join(outputDir, outputFile);
            const enumOutputPath = outputPath.replace('.cs', '.Enums.cs');
            
            // 增量生成：跳过未修改的文件
            if (!this.needsRegeneration(inputPath, cache)) {
                console.log(`Skipped (unchanged): ${file}`);
                const cached = cache.files[file];
                if (cached) {
                    newCache.files[file] = cached;
                }
                return Promise.resolve();
            }
            
            return this.processFile(inputPath, outputPath).then(() => {
                // 更新缓存
                const stat = fs.statSync(inputPath);
                newCache.files[file] = {
                    hash: this.fileHash(inputPath),
                    mtime: stat.mtime.toISOString(),
                    outputPath: outputPath,
                    enumOutputPath: enumOutputPath
                };
            }).catch(error => {
                console.error(`Error processing ${file}:`, error);
            });
        });
        
        await Promise.all(promises);
        this.saveCache(outputDir, newCache);
    }

    async processDirectoryWithContext(
        inputDir: string, 
        outputDir: string, 
        context?: ParseContext
    ): Promise<void> {
        if (!fs.existsSync(inputDir)) {
            throw new Error(`Directory not found: ${inputDir}`);
        }
        
        // 确保输出目录存在
        if (!fs.existsSync(outputDir)) {
            fs.mkdirSync(outputDir, { recursive: true });
        }
        
        const ctx = context || createParseContext();
        
        // 1. 先解析 common.d.ts（如果存在）
        const commonPath = path.join(inputDir, 'common.d.ts');
        if (fs.existsSync(commonPath)) {
            console.log('Parsing CommonMethod interface...');
            this.parser.parseCommonMethod(commonPath, ctx);
        }
        
        // 2. 解析所有组件文件
        const files = fs.readdirSync(inputDir).filter(f => f.endsWith('.d.ts'));
        const cache = this.loadCache(outputDir);
        const newCache: GenerationCache = { version: 1, files: {} };
        
        for (const file of files) {
            const inputPath = path.join(inputDir, file);
            const outputFile = file.replace('.d.ts', '.cs');
            const outputPath = path.join(outputDir, outputFile);
            
            // 增量生成：跳过未修改的文件
            if (!this.needsRegeneration(inputPath, cache)) {
                console.log(`Skipped (unchanged): ${file}`);
                const cached = cache.files[file];
                if (cached) {
                    newCache.files[file] = cached;
                }
                continue;
            }
            
            await this.processFileWithContext(inputPath, outputPath, ctx, 'HarmonyOS.ArkUI');
            
            // 更新缓存
            const stat = fs.statSync(inputPath);
            newCache.files[file] = {
                hash: this.fileHash(inputPath),
                mtime: stat.mtime.toISOString(),
                outputPath: outputPath,
                enumOutputPath: outputPath.replace('.cs', '.Enums.cs')
            };
        }
        
        this.saveCache(outputDir, newCache);
        
        if (ctx.warnings.length > 0) {
            console.log(`\nTotal warnings: ${ctx.warnings.length}`);
        }
    }
}

export async function main(): Promise<void> {
    const parser = new ArkTsParser();
    
    // 测试 fixtures 目录
    const inputDir = path.join(__dirname, '../../tests/fixtures');
    const outputDir = path.join(__dirname, '../../output');
    
    await parser.processDirectory(inputDir, outputDir);
    
    console.log('Done!');
}

export async function processFullSDK(): Promise<void> {
    const parser = new ArkTsParser();
    const context = createParseContext();
    
    // HarmonyOS SDK 目录
    const sdkBase = 'C:\\Program Files\\Huawei\\DevEco Studio\\sdk\\default\\openharmony';
    const componentDir = path.join(sdkBase, 'ets', 'component');
    const apiDir = path.join(sdkBase, 'ets', 'api');
    
    // 输出到 HarmonyOS.Bindings 项目
    const bindingsDir = path.join(__dirname, '../../HarmonyOS.Bindings');
    const componentOutputDir = path.join(bindingsDir, 'Components');
    const apiOutputDir = path.join(bindingsDir, 'Api');
    
    console.log('=== Processing HarmonyOS SDK ===');
    
    // 1. 处理组件
    console.log('\n--- Components ---');
    console.log(`Input: ${componentDir}`);
    console.log(`Output: ${componentOutputDir}`);
    
    if (!fs.existsSync(componentOutputDir)) {
        fs.mkdirSync(componentOutputDir, { recursive: true });
    }
    
    await parser.processDirectoryWithContext(componentDir, componentOutputDir, context);
    
    // 2. 处理全部 API
    console.log('\n--- APIs ---');
    console.log(`Input: ${apiDir}`);
    console.log(`Output: ${apiOutputDir}`);
    
    if (!fs.existsSync(apiOutputDir)) {
        fs.mkdirSync(apiOutputDir, { recursive: true });
    }
    
    const apiFiles = fs.readdirSync(apiDir).filter(f => f.endsWith('.d.ts'));
    let apiSuccess = 0;
    let apiSkipped = 0;
    
    for (const file of apiFiles) {
        const apiPath = path.join(apiDir, file);
        // 文件名转换: @ohos.ability.ability.d.ts → Ability.Ability.cs
        const csFileName = convertApiFileName(file);
        const csPath = path.join(apiOutputDir, csFileName);
        
        try {
            const result = parser.parseFile(apiPath);
            if (result.component.name) {
                result.component.namespace = 'HarmonyOS.Bindings.Api';
                const code = parser.generateCode(result);
                fs.mkdirSync(path.dirname(csPath), { recursive: true });
                fs.writeFileSync(csPath, code);
                apiSuccess++;
            } else {
                apiSkipped++;
            }
            const enums = parser.parseEnums(apiPath);
            if (enums.length > 0) {
                const enumCsPath = csFileName.replace('.cs', '.Enums.cs');
                const enumCode = parser.generateEnumsCode(enums);
                fs.writeFileSync(path.join(apiOutputDir, enumCsPath), enumCode);
            }
        } catch (e: any) {
            console.error(`  Error: ${file} - ${e.message}`);
        }
    }
    
    console.log(`  Generated: ${apiSuccess}, Skipped: ${apiSkipped}`);
    console.log('');
    console.log('=== SDK Processing Complete ===');
}

function convertApiFileName(dtsFileName: string): string {
    // @ohos.ability.ability.d.ts → Ability.Ability.cs
    // @ohos.animator.d.ts → Animator.cs
    // @ohos.arkui.dragController.d.ts → Arkui.DragController.cs
    let name = dtsFileName.replace('.d.ts', '');
    
    // 去掉 @ohos. 前缀
    if (name.startsWith('@ohos.')) {
        name = name.substring(6); // 去掉 "@ohos." (6个字符)
    } else if (name.startsWith('@')) {
        name = name.substring(1);
    }
    
    // 按 . 分割并转换每一段
    const parts = name.split('.');
    const converted = parts.map(part => {
        // 首字母大写，其余小写
        return part.charAt(0).toUpperCase() + part.slice(1).toLowerCase();
    });
    
    return converted.join('.') + '.cs';
}

if (require.main === module) {
    const args = process.argv.slice(2);
    if (args.includes('--sdk')) {
        processFullSDK().catch(console.error);
    } else {
        main().catch(console.error);
    }
}
