import * as fs from 'fs';
import * as path from 'path';
import { AstParser } from './astParser';
import { CodeGenerator } from './codeGenerator';
import { EnumGenerator } from './enumGenerator';
import { ComponentInfo, EnumInfo } from './models';

export class ArkTsParser {
    private parser: AstParser;
    private generator: CodeGenerator;
    private enumGenerator: EnumGenerator;

    constructor() {
        this.parser = new AstParser();
        this.generator = new CodeGenerator();
        this.enumGenerator = new EnumGenerator();
    }

    parseFile(inputPath: string): ComponentInfo {
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

    generateCode(component: ComponentInfo): string {
        return this.generator.generate(component);
    }

    generateEnumCode(enumInfo: EnumInfo): string {
        return this.enumGenerator.generate(enumInfo);
    }

    generateEnumsCode(enums: EnumInfo[]): string {
        return this.enumGenerator.generateMultipleEnums(enums);
    }

    processFile(inputPath: string, outputPath: string): void {
        console.log(`Processing: ${inputPath}`);
        
        // 解析组件
        const component = this.parseFile(inputPath);
        
        // 如果有组件内容，生成组件代码
        if (component.name) {
            const csharpCode = this.generateCode(component);
            
            const outputDir = path.dirname(outputPath);
            if (!fs.existsSync(outputDir)) {
                fs.mkdirSync(outputDir, { recursive: true });
            }
            
            fs.writeFileSync(outputPath, csharpCode);
            console.log(`Generated: ${outputPath}`);
        }
        
        // 解析并生成枚举
        const enums = this.parseEnums(inputPath);
        if (enums.length > 0) {
            const enumOutputPath = outputPath.replace('.cs', '.Enums.cs');
            const enumCode = this.generateEnumsCode(enums);
            
            const outputDir = path.dirname(enumOutputPath);
            if (!fs.existsSync(outputDir)) {
                fs.mkdirSync(outputDir, { recursive: true });
            }
            
            fs.writeFileSync(enumOutputPath, enumCode);
            console.log(`Generated: ${enumOutputPath}`);
        }
    }

    processDirectory(inputDir: string, outputDir: string): void {
        if (!fs.existsSync(inputDir)) {
            throw new Error(`Directory not found: ${inputDir}`);
        }
        
        const files = fs.readdirSync(inputDir).filter(f => f.endsWith('.d.ts'));
        
        files.forEach(file => {
            const inputPath = path.join(inputDir, file);
            const outputFile = file.replace('.d.ts', '.cs');
            const outputPath = path.join(outputDir, outputFile);
            
            try {
                this.processFile(inputPath, outputPath);
            } catch (error) {
                console.error(`Error processing ${file}:`, error);
            }
        });
    }
}

export function main(): void {
    const parser = new ArkTsParser();
    
    const inputDir = path.join(__dirname, '../../tests/fixtures');
    const outputDir = path.join(__dirname, '../../output');
    
    parser.processDirectory(inputDir, outputDir);
    
    console.log('Done!');
}

if (require.main === module) {
    main();
}
