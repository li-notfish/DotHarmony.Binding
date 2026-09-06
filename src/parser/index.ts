import * as fs from 'fs';
import * as path from 'path';
import * as crypto from 'crypto';
import { AstParser } from './astParser';
import { CodeGenerator } from './codeGenerator';
import { EnumGenerator } from './enumGenerator';
import { NativeCodeGenerator, EnumMetadata, NativeGap } from './nativeCodeGenerator';
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
    private nativeGenerator: NativeCodeGenerator | null = null;
    private nativeGaps: NativeGap[] = [];

    constructor(enumMetadata?: EnumMetadata) {
        this.parser = new AstParser();
        this.generator = new CodeGenerator();
        this.enumGenerator = new EnumGenerator();
        if (enumMetadata) {
            this.nativeGenerator = new NativeCodeGenerator(enumMetadata);
        }
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

    /** 解析 CommonMethod<T> 共享接口到上下文（--native 模式用） */
    parseCommonMethodInto(commonPath: string, context: ParseContext): void {
        this.parser.parseCommonMethod(commonPath, context);
    }

    /** 将 CommonMethod<T> 的方法内联合并进组件（--native 模式用） */
    mergeCommonMethodInto(component: ComponentInfo, context: ParseContext): void {
        this.parser.mergeCommonMethod(component, context);
    }

    generateCode(result: ParseResult): string {
        return this.generator.generate(result);
    }

    generateEnumCode(enumInfo: EnumInfo): string {
        return this.enumGenerator.generate(enumInfo);
    }

    /** C API（Native Node）目标生成；返回 null 表示该组件无 C API 节点类型 */
    generateNativeCode(result: ParseResult): { csharp: string | null; gaps: NativeGap[] } {
        if (!this.nativeGenerator) {
            throw new Error('NativeCodeGenerator not initialized (missing enum metadata)');
        }
        const r = this.nativeGenerator.generate(result);
        this.nativeGaps.push(...r.gaps);
        return { csharp: r.csharp, gaps: r.gaps };
    }

    getNativeGaps(): NativeGap[] {
        return this.nativeGaps;
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

/**
 * C API（Native Node）模式：从 SDK 组件 .d.ts 生成 NodeHandle 包装类。
 * 产出 HarmonyOS.Bindings/Nodes/*.cs 与 native-gaps.json（C API 覆盖缺口清单）。
 */
export async function processNativeSDK(): Promise<void> {
    const enumMetaPath = path.join(__dirname, '../../HarmonyOS.Bindings/NativeNode/ArkUINodeTypes.json');
    if (!fs.existsSync(enumMetaPath)) {
        throw new Error(`enum metadata not found: ${enumMetaPath}（先运行 extract_arkui_types.py --dump-json）`);
    }
    const enumMetadata = JSON.parse(fs.readFileSync(enumMetaPath, 'utf-8')) as EnumMetadata;
    const parser = new ArkTsParser(enumMetadata);
    const context = createParseContext();

    const sdkBase = 'C:\\Program Files\\Huawei\\DevEco Studio\\sdk\\default\\openharmony';
    const componentDir = path.join(sdkBase, 'ets', 'component');
    const outputDir = path.join(__dirname, '../../HarmonyOS.Bindings/Nodes');

    // 试点组件集合：shape 表覆盖范围内先跑通，扩展组件随 shape 表成长
    const pilotFiles = ['text.d.ts', 'button.d.ts', 'column.d.ts', 'row.d.ts', 'stack.d.ts', 'flex.d.ts'];

    console.log('=== Native (C API) Generation ===');
    console.log(`Input:  ${componentDir}`);
    console.log(`Output: ${outputDir}`);

    // common.d.ts 先解析（CommonMethod 内联）
    const commonPath = path.join(componentDir, 'common.d.ts');
    if (fs.existsSync(commonPath)) {
        parser.parseCommonMethodInto(commonPath, context);
    }

    fs.mkdirSync(outputDir, { recursive: true });
    let generated = 0;
    let skipped = 0;

    for (const file of pilotFiles) {
        const inputPath = path.join(componentDir, file);
        if (!fs.existsSync(inputPath)) {
            console.log(`  (missing in SDK: ${file})`);
            skipped++;
            continue;
        }
        const result = parser.parseFile(inputPath);
        parser.mergeCommonMethodInto(result.component, context);
        result.component.namespace = 'HarmonyOS.ArkUI';

        const { csharp } = parser.generateNativeCode(result);
        if (csharp) {
            const outPath = path.join(outputDir, file.replace('.d.ts', '.cs'));
            fs.writeFileSync(outPath, csharp);
            console.log(`  Generated: ${outPath}`);
            generated++;
        } else {
            console.log(`  Skipped (no native node): ${file}`);
            skipped++;
        }
    }

    const gapsPath = path.join(outputDir, 'native-gaps.json');
    fs.writeFileSync(gapsPath, JSON.stringify(parser.getNativeGaps(), null, 2));
    console.log(`Generated: ${generated}, Skipped: ${skipped}, Gaps: ${parser.getNativeGaps().length} -> ${gapsPath}`);
    console.log('=== Native Generation Complete ===');
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
    const boundModules: { module: string; local: string }[] = [];

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
                boundModules.push(dtsToModuleName(file));
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

    writeOhosImports(boundModules);

    console.log(`  Generated: ${apiSuccess}, Skipped: ${apiSkipped}`);
    console.log('');
    console.log('=== SDK Processing Complete ===');
}

/**
 * @ohos.deviceInfo.d.ts → { module: "@ohos.deviceInfo", local: "deviceInfo" }
 */
function dtsToModuleName(dtsFileName: string): { module: string; local: string } {
    const stem = dtsFileName.replace(/\.d\.ts$/, '');
    return { module: stem, local: stem.replace(/^@ohos\./, '').replace(/^@system\./, 'system.') };
}

/**
 * 输出宿主 ArkTS 工程的模块登记清单（ohosImports.ets）。
 * native 侧经 napi_load_module 动态加载的 @ohos.* 模块，
 * 必须经此 re-export 编入 HAP 模块表，否则运行时解析失败（jscrash）。
 */
function writeOhosImports(modules: { module: string; local: string }[]): void {
    const hostEtsDir = path.join(
        __dirname, '../../samples/HarmonyHost/entry/src/main/ets');
    const outPath = path.join(hostEtsDir, 'ohosImports.ets');

    const lines = [
        '// <auto-generated>',
        '// 由 ArkTsBinding 生成器维护的 @ohos.* 模块登记清单（npx ts-node src/parser/index.ts --sdk）。',
        '// native 侧经 napi_load_module 动态加载的模块，必须在此 re-export（编入 HAP 模块表）。',
        '// </auto-generated>',
        '',
    ];
    for (const m of [...modules].sort((a, b) => a.local.localeCompare(b.local))) {
        lines.push(`export { default as ${m.local.replace(/\./g, '_')} } from '${m.module}';`);
    }
    fs.mkdirSync(hostEtsDir, { recursive: true });
    fs.writeFileSync(outPath, lines.join('\n') + '\n');
    console.log(`  ohosImports.ets updated: ${modules.length} modules -> ${outPath}`);
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
    if (args.includes('--native')) {
        processNativeSDK().catch(console.error);
    } else if (args.includes('--sdk')) {
        processFullSDK().catch(console.error);
    } else {
        main().catch(console.error);
    }
}
