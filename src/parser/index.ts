import * as fs from 'fs';
import * as path from 'path';
import * as crypto from 'crypto';
import { AstParser } from './astParser';
import { CodeGenerator } from './codeGenerator';
import { EnumGenerator } from './enumGenerator';
import { ComponentInfo, EnumInfo, ParseResult } from './models';

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
}

export async function main(): Promise<void> {
    const parser = new ArkTsParser();
    
    const inputDir = path.join(__dirname, '../../tests/fixtures');
    const outputDir = path.join(__dirname, '../../output');
    
    await parser.processDirectory(inputDir, outputDir);
    
    console.log('Done!');
}

if (require.main === module) {
    main();
}
