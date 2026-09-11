import * as fs from 'fs';
import * as path from 'path';
import * as crypto from 'crypto';
import { AstParser } from './astParser';
import { CodeGenerator } from './codeGenerator';
import { EnumGenerator } from './enumGenerator';
import { NativeCodeGenerator, EnumMetadata, NativeGap } from './nativeCodeGenerator';
import { ApiGenerator } from './apiGenerator';
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
    /** 全局枚举名去重：同一枚举（如 Orientation）可能出现在多个模块的 .d.ts 中 */
    private generatedEnumNames = new Set<string>();

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

    /** 过滤掉已生成的枚举，返回仅新增的枚举 */
    private filterNewEnums(enums: EnumInfo[]): EnumInfo[] {
        const newEnums: EnumInfo[] = [];
        for (const e of enums) {
            if (!this.generatedEnumNames.has(e.name)) {
                this.generatedEnumNames.add(e.name);
                newEnums.push(e);
            }
        }
        return newEnums;
    }

    /** 从生成的 C# 枚举代码中提取枚举名 */
    private extractEnumNamesFromCode(code: string): string[] {
        const names: string[] = [];
        const lines = code.split('\n');
        for (const line of lines) {
            const match = line.match(/^\s*public\s+enum\s+(\w+)/);
            if (match) {
                names.push(match[1]);
            }
        }
        return names;
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
        
        // 解析并生成枚举（去重：同一枚举只写一次）
        const enums = this.filterNewEnums(this.parseEnums(inputPath));
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
        
        // 解析并生成枚举（去重：同一枚举只写一次）
        const enums = this.filterNewEnums(this.parseEnums(inputPath));
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

    // SDK ets 根目录：OHOS_SDK_BASE 优先，其次 OHSDK_HOME/26.0.0，最后 DevEco 内置
    const sdkBase = process.env.OHOS_SDK_BASE
        ?? (process.env.OHSDK_HOME && fs.existsSync(path.join(process.env.OHSDK_HOME, '26.0.0', 'ets'))
            ? path.join(process.env.OHSDK_HOME, '26.0.0')
            : 'C:\\Program Files\\Huawei\\DevEco Studio\\sdk\\default\\openharmony');
    const componentDir = path.join(sdkBase, 'ets', 'component');
    const outputDir = path.join(__dirname, '../../HarmonyOS.Bindings/Nodes');

    // 组件清单：全量扫描 component 目录。非 C API 节点组件（如 alert_dialog）
    // 由生成器自动登记 gap/跳过；属性形态未登记的成员进 native-gaps.json 待补。
    const pilotFiles = fs.readdirSync(componentDir)
        .filter(f => f.endsWith('.d.ts'))
        .sort();

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

/**
 * 探测 HarmonyOS SDK 基础路径。
 * 优先级：--sdk <path> > OHOS_SDK_HOME > OHOS_SDK_BASE > 默认路径
 */
function detectSdkBase(cliSdkArg?: string): string {
    if (cliSdkArg) return cliSdkArg;

    const envPaths = [
        process.env.OHOS_SDK_HOME,
        process.env.OHOS_SDK_BASE,
        process.env.OHSDK_HOME,
    ].filter(Boolean) as string[];

    for (const p of envPaths) {
        // env 可能指向 .../openharmony 或 .../Sdk/<version>
        if (fs.existsSync(path.join(p, 'ets', 'api'))) return p;
        // 尝试下一层
        const dirs = fs.readdirSync(p, { withFileTypes: true })
            .filter(d => d.isDirectory()).map(d => d.name);
        for (const d of dirs) {
            if (fs.existsSync(path.join(p, d, 'ets', 'api'))) return path.join(p, d);
        }
    }

    // 默认路径
    const defaults = [
        'C:\\Program Files\\Huawei\\DevEco Studio\\sdk\\default\\openharmony',
        'D:\\Harmony\\OpenHarmony\\Sdk\\26.0.0',
    ];
    for (const d of defaults) {
        if (fs.existsSync(path.join(d, 'ets', 'api'))) return d;
    }

    throw new Error(
        'HarmonyOS SDK not found. Use --sdk <path>, set OHOS_SDK_HOME, or install DevEco Studio.'
    );
}

/** pilot 模式：第一批绑定目标模块 */
const PILOT_MODULES = [
    // M2.3 第一批：基础系统信息
    '@ohos.deviceInfo',
    '@ohos.batteryInfo',
    '@ohos.display',
    '@ohos.settings',
    '@ohos.pasteboard',
    '@ohos.vibrator',
    '@ohos.sensor',
    '@ohos.geolocation',
    '@ohos.geoLocationManager',
    // M2.3 第二批：常用系统 API
    '@ohos.net.http',
    '@ohos.net.connection',
    '@ohos.file.fs',
    '@ohos.file.picker',
    '@ohos.multimedia.media',
    '@ohos.multimedia.image',
    '@ohos.multimedia.camera',
    '@ohos.window',
    '@ohos.router',
    '@ohos.data.preferences',
    '@ohos.request',
    '@ohos.promptAction',
];

/** 全局枚举名去重（processFullSDK 作用域内） */
const writtenEnumNames = new Set<string>();

/** 从生成的 C# 枚举代码中提取枚举名 */
function extractEnumNamesFromCode(code: string): string[] {
    const names: string[] = [];
    for (const line of code.split('\n')) {
        const match = line.match(/^\s*public\s+enum\s+(\w+)/);
        if (match) names.push(match[1]);
    }
    return names;
}

export async function processFullSDK(sdkArg?: string): Promise<void> {
    const sdkBase = detectSdkBase(sdkArg);
    const componentDir = path.join(sdkBase, 'ets', 'component');
    const apiDir = path.join(sdkBase, 'ets', 'api');

    const parser = new ArkTsParser();
    const context = createParseContext();

    // 输出到 HarmonyOS.Bindings 项目
    const bindingsDir = path.join(__dirname, '../../HarmonyOS.Bindings');
    const apiOutputDir = path.join(bindingsDir, 'Api');

    console.log('=== Processing HarmonyOS SDK ===');
    console.log(`SDK:     ${sdkBase}`);
    console.log(`API dir: ${apiDir}`);
    console.log(`Output:  ${apiOutputDir}`);

    if (!fs.existsSync(apiOutputDir)) {
        fs.mkdirSync(apiOutputDir, { recursive: true });
    }

    // pilot 模式：只处理指定模块；否则全量
    const pilotSet = new Set(PILOT_MODULES.map(m => m + '.d.ts'));
    const allApiFiles = fs.existsSync(apiDir)
        ? fs.readdirSync(apiDir).filter(f => f.endsWith('.d.ts'))
        : [];
    const apiFiles = allApiFiles.filter(f => pilotSet.has(f));

    console.log(`\n--- APIs (${apiFiles.length} pilot modules) ---`);

    const apiGen = new ApiGenerator();
    let apiSuccess = 0;
    let apiSkipped = 0;
    const boundModules: { module: string; local: string }[] = [];
    const allPermissions = new Set<string>();

    for (const file of apiFiles) {
        const apiPath = path.join(apiDir, file);
        const moduleInfo = ApiGenerator.dtsToModuleInfo(file);
        const source = fs.readFileSync(apiPath, 'utf-8');
        const permissions = ApiGenerator.extractPermissions(source);
        permissions.forEach(p => allPermissions.add(p));

        try {
            const result = parser.parseFile(apiPath);
            if (!result.component.name) {
                console.log(`  Skipped (no namespace): ${file}`);
                apiSkipped++;
                continue;
            }

            // 过滤已生成的枚举，避免不同模块的同名枚举重复定义
            const newEnums = result.enums.filter(e => !writtenEnumNames.has(e.name));
            newEnums.forEach(e => writtenEnumNames.add(e.name));

            const gen = apiGen.generate(
                result.component,
                moduleInfo,
                permissions,
                newEnums,
                result.enums
            );

            const csPath = path.join(apiOutputDir, `${gen.className}.cs`);
            fs.writeFileSync(csPath, gen.csharp);
            console.log(`  Generated: ${gen.className}.cs (${gen.permissions.length} perms)`);
            apiSuccess++;

            if (gen.enums) {
                const enumCsPath = path.join(apiOutputDir, `${gen.className}.Enums.cs`);
                fs.writeFileSync(enumCsPath, gen.enums);
            }

            boundModules.push({ module: moduleInfo.module, local: moduleInfo.local });
        } catch (e: any) {
            console.error(`  Error: ${file} - ${e.message}`);
            apiSkipped++;
        }
    }

    // 写 ohosImports.ets
    writeOhosImports(boundModules);

    // 写 module.json5 的 requestPermissions（过滤掉 SDK 中不存在的权限）
    const validPerms = filterSdkPermissions([...allPermissions].sort(), sdkBase);
    writeModuleJson5Permissions(validPerms);

    // 写灰度策略到 csproj
    writeGrayscaleCompileRemove(boundModules);

    console.log(`\n  Generated: ${apiSuccess}, Skipped: ${apiSkipped}`);
    console.log(`  Permissions: ${allPermissions.size} unique → module.json5`);
    console.log('');
    console.log('=== SDK Processing Complete ===');
}

/**
 * 从 SDK permissions.d.ts 加载合法权限列表，过滤掉不存在的权限。
 */
function filterSdkPermissions(perms: string[], sdkBase: string): string[] {
    const sdkPermissionsPath = path.join(sdkBase, 'ets', 'api', 'permissions.d.ts');
    if (!fs.existsSync(sdkPermissionsPath)) {
        console.log(`  permissions.d.ts not found at ${sdkPermissionsPath}, skipping filter`);
        return perms;
    }
    const content = fs.readFileSync(sdkPermissionsPath, 'utf-8');
    const validPerms = new Set<string>();
    const regex = /'(ohos\.permission\.[^']+)'/g;
    let match: RegExpExecArray | null;
    while ((match = regex.exec(content)) !== null) {
        validPerms.add(match[1]);
    }
    const filtered = perms.filter(p => validPerms.has(p));
    const removed = perms.length - filtered.length;
    if (removed > 0) {
        console.log(`  Filtered out ${removed} permissions not in SDK`);
    }
    return filtered;
}

/**
 * 更新 module.json5：注入 requestPermissions 清单。
 * 策略：在 "module": { 之后插入 requestPermissions 数组（或替换已有块）。
 */
function writeModuleJson5Permissions(perms: string[]): void {
    const moduleJson5Path = path.join(
        __dirname, '../../samples/HarmonyHost/entry/src/main/module.json5'
    );

    if (!fs.existsSync(moduleJson5Path)) {
        console.log(`  module.json5 not found at ${moduleJson5Path}, skipping permissions`);
        return;
    }

    if (perms.length === 0) {
        console.log('  No permissions needed, module.json5 unchanged');
        return;
    }

    let content = fs.readFileSync(moduleJson5Path, 'utf-8');

    // 构造 requestPermissions 块
    const permEntries = perms
        .map(p => [
            '      {',
            `        "name": "${p}",`,
            `        "reason": "$string:permission_${p.replace(/.*\./, '')}_reason",`,
            '        "usedScene": {',
            '          "abilities": ["EntryAbility"],',
            '          "when": "always"',
            '        }',
            '      }',
        ].join('\n'))
        .join(',\n');

    const permBlock = [
        '    "requestPermissions": [',
        permEntries,
        '    ]',
    ].join('\n');

    if (content.includes('"requestPermissions"')) {
        // 替换已有的 requestPermissions 块（用括号计数匹配到正确的闭合 ]）
        const startIdx = content.indexOf('"requestPermissions"');
        if (startIdx !== -1) {
            // 找到 [ 的位置
            let bracketStart = content.indexOf('[', startIdx);
            if (bracketStart !== -1) {
                let depth = 0;
                let bracketEnd = -1;
                for (let i = bracketStart; i < content.length; i++) {
                    if (content[i] === '[') depth++;
                    else if (content[i] === ']') {
                        depth--;
                        if (depth === 0) { bracketEnd = i; break; }
                    }
                }
                if (bracketEnd !== -1) {
                    // 找到 ] 前面的冒号和空白
                    let deleteStart = startIdx;
                    while (deleteStart > 0 && content[deleteStart - 1] !== '\n' && content[deleteStart - 1] !== '\r') {
                        deleteStart--;
                    }
                    // 从 "requestPermissions" 行首到 ] 之后全部替换
                    let deleteEnd = bracketEnd + 1;
                    // 跳过 ] 后可能的逗号
                    while (deleteEnd < content.length && (content[deleteEnd] === ',' || content[deleteEnd] === ' ' || content[deleteEnd] === '\t')) {
                        deleteEnd++;
                    }
                    content = content.substring(0, deleteStart) + permBlock + ',' + content.substring(deleteEnd);
                }
            }
        }
    } else {
        // 在 "module": { 之后插入
        content = content.replace(
            /("module"\s*:\s*\{)(\r?\n)/,
            `$1$2${permBlock},\n`
        );
    }

    fs.writeFileSync(moduleJson5Path, content);
    console.log(`  module.json5 updated: ${perms.length} permissions`);

    writePermissionReasonStrings(perms);
}

/**
 * 同步写入权限 reason 字符串资源。
 * module.json5 引用 $string:permission_XXX_reason，若 string.json 缺少对应条目，
 * hvigor CompileResource 会直接报错（"resource reference is not defined"）。
 * 策略：解析 string.json，移除旧的 permission_* 条目后写入当前集合，其余条目保留。
 */
function writePermissionReasonStrings(perms: string[]): void {
    const stringJsonPath = path.join(
        __dirname, '../../samples/HarmonyHost/entry/src/main/resources/base/element/string.json'
    );

    if (!fs.existsSync(stringJsonPath)) {
        console.log(`  string.json not found at ${stringJsonPath}, skipping permission reasons`);
        return;
    }

    let doc: { string: Array<{ name: string; value: string }> };
    try {
        doc = JSON.parse(fs.readFileSync(stringJsonPath, 'utf-8'));
    } catch (e: any) {
        console.error(`  string.json parse failed: ${e.message}, skipping permission reasons`);
        return;
    }
    if (!Array.isArray(doc.string)) {
        doc.string = [];
    }

    // 移除旧 permission_* 条目，保留其它资源
    doc.string = doc.string.filter(s => !s.name.startsWith('permission_'));

    for (const p of perms) {
        const short = p.replace(/.*\./, '');
        doc.string.push({
            name: `permission_${short}_reason`,
            value: `Allow the app to use ${p}`,
        });
    }

    fs.writeFileSync(stringJsonPath, JSON.stringify(doc, null, 2) + '\n');
    console.log(`  string.json updated: ${perms.length} permission reasons`);
}

/** 已转正的模块（参与编译，不生成 Compile Remove） */
const APPROVED_MODULES = new Set([
    'DeviceInfo',    // 手写
    'BatteryInfo',
    'Display',
    'Settings',
    'Vibrator',
    // M2.3 转正（17 个可编译模块）
    'Camera',
    'Connection',
    // 'Fs',        // 含 ArrayBuffer/WriteOptions/DfsListeners 等未映射类型，待修复
    'Geolocation',
    'Http',
    'Image',
    'Media',
    'Pasteboard',
    'Picker',
    'Preferences',
    'PromptAction',
    'Request',
    'Router',
    // 'Sensor',    // 含 SensorId/SensorType/SensorInfoParam 等复杂类型，待修复
    // 'Settings',  // 含 Context/DataAbilityHelper 等未映射类型，待修复
    'Window',
]);

/**
 * 灰度策略：生成的 Api/*.cs 默认不参与编译（Compile Remove），
 * 逐个转正时从 APPROVED_MODULES 中添加模块名。
 */
function writeGrayscaleCompileRemove(modules: { module: string; local: string }[]): void {
    const csprojPath = path.join(__dirname, '../../HarmonyOS.Bindings/HarmonyOS.Bindings.csproj');
    let content = fs.readFileSync(csprojPath, 'utf-8');

    const classNames = modules
        .map(m => {
            const parts = m.local.split('.');
            return parts[parts.length - 1].charAt(0).toUpperCase()
                + parts[parts.length - 1].slice(1);
        })
        .sort();

    const pending = classNames.filter(n => !APPROVED_MODULES.has(n));

    const removeItems = pending
        .map(n => `    <Compile Remove="Api\\${n}.cs" />`)
        .join('\n');
    const enumRemoveItems = pending
        .map(n => `    <Compile Remove="Api\\${n}.Enums.cs" />`)
        .join('\n');

    const approvedList = [...APPROVED_MODULES].sort().join(', ');
    const block = `  <!-- 灰度策略：已转正 ${approvedList}（共 ${APPROVED_MODULES.size} 个）。
       未转正模块默认不参与编译，逐个验证后加入 APPROVED_MODULES。 -->
  <ItemGroup Condition="'$(SkipGeneratedApi)' != 'false'">
${removeItems}
${enumRemoveItems}
  </ItemGroup>`;

    // 替换已有的灰度块或追加
    if (content.includes('灰度策略')) {
        content = content.replace(
            /  <!-- 灰度策略[\s\S]*?<\/ItemGroup>/,
            block
        );
    } else {
        content = content.replace('</Project>', `${block}\n</Project>`);
    }

    fs.writeFileSync(csprojPath, content);
    const approved = classNames.length - pending.length;
    console.log(`  HarmonyOS.Bindings.csproj: ${approved}/${classNames.length} approved, ${pending.length} in grayscale`);
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
        const sdkIdx = args.indexOf('--sdk');
        const sdkArg = args[sdkIdx + 1] && !args[sdkIdx + 1].startsWith('--')
            ? args[sdkIdx + 1] : undefined;
        processFullSDK(sdkArg).catch(console.error);
    } else {
        main().catch(console.error);
    }
}
